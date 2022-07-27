using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
/// <summary>
/// Parte de la logica del sistema de combate... diosito tengame en gracia porque aqui voy a hacer un destrozo
/// </summary>

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, BUSY }

public class BattleSystem : MonoBehaviour
{
    #region Variables
	[SerializeField] public static BattleState state;

    [Header("Battle Zone")]

	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject enemyPrefab;

	[SerializeField] Transform playerPosition, enemyPosition;

	BattleUnit playerUnit , enemyUnit;

	[SerializeField] TextMeshProUGUI dialogueText;
	[SerializeField] GameObject ButtonPanel;


	[Header("Spells Zone")]
	[SerializeField] GameObject spellButtonPf;
	[SerializeField] GameObject spellBoxPanel;

    [Header("Miscelanea")]
	[SerializeField] int defenseDuration;

	[SerializeField] float animTransition = 0.26f;
	#endregion

    private void Start()
    {
        state = BattleState.START;
		StartCoroutine(SetupBattle());
		CreateSpellsButtons();
    }

    private void Update()
    {
		// Si pulsas espacio todo el juego va FASTO
		if (Input.GetKey(KeyCode.Space)) Time.timeScale = 2;
		else Time.timeScale = 1;
    }
    IEnumerator SetupBattle()
	{
		GameObject playerGO = Instantiate(playerPrefab, playerPosition);
		playerUnit = playerGO.GetComponent<BattleUnit>();

		GameObject enemyGO = Instantiate(enemyPrefab, enemyPosition);
		enemyUnit = enemyGO.GetComponent<BattleUnit>();

		dialogueText.text = "A wild " + enemyUnit.unitName + " approaches...";

		// Espera hasta que se pulse el raton para continuar con el combate, "() =>" permite crear una funcion ya que introduciendolo sin esto no detecta el booleano
		//yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
		StartCoroutine(EnableActionButtons(false));
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

		state = BattleState.PLAYERTURN;
		PlayerTurn();
	}

    #region Battle Logic

	IEnumerator AttackTarget(BattleUnit attaker, BattleUnit target, int weaponSlot = 0)
	{
		bool isDead = false;

		if (attaker.weapons[weaponSlot].customAnimation != null)
		{
			attaker.animOverride["AttackCustom"] = attaker.weapons[weaponSlot].customAnimation;
			attaker.anim.runtimeAnimatorController = attaker.animOverride;
			attaker.anim.SetTrigger("AttackCustom");
		}
		else attaker.anim.SetTrigger("AttackDefault");

		if (attaker.GetAccuracy() <= target.GetEvasion())
        {
			isDead = DamageTarget(attaker.GetDamage(weaponSlot), target);

			if (isDead) target.anim.SetTrigger("Death");
			else target.anim.SetTrigger("TakeHit");

			dialogueText.text = "The " + attaker.unitName + " attack is successful!";
        }
        else
        {
			target.anim.SetTrigger("MissHit");
			target.PlayMissHitClip();
			dialogueText.text = "The " + attaker.unitName + " attack missed!";
		}

		StartCoroutine(EnableActionButtons(false));

		//	Espera de la transicion de las animaciones
		yield return new WaitForSeconds(animTransition);
		//	Compara cual de las dos animaciones que se estan reproduciendo actualmente es mas larga y devuelve su valor
		yield return new WaitForSeconds(CompareCurrentClipDuration(attaker, target));
		//yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

		TurnSelector(isDead);
	}
	IEnumerator SpellTarget(Spell spell, BattleUnit attaker, BattleUnit target)
    {
		spellBoxPanel.SetActive(false);
		bool isDead = false;

		if (attaker.currentMP >= spell.manaCost)
		{
			ReduceMP(spell.manaCost, attaker);
			attaker.PlaySpellAttackClip(spell.customSound);
			if (spell.customAnimation != null)
			{
				attaker.animOverride["SpellAttackCustom"] = spell.customAnimation;
				attaker.anim.runtimeAnimatorController = attaker.animOverride;
				attaker.anim.SetTrigger("SpellAttackCustom");
			}
			else attaker.anim.SetTrigger("SpellAttackDefault");

			dialogueText.text = attaker.unitName + " used " + spell.spellName;

			switch (spell.spellType)
			{
				case Spell.SpellType.Damage:
					isDead = DamageTarget(spell.GetPower(), target);

					if (isDead) target.anim.SetTrigger("Death");
					else target.anim.SetTrigger("TakeHit");

					switch (spell.spellDebuff)
					{
						case Spell.SpellDebuff.NONE:
							break;
						case Spell.SpellDebuff.Blind:
							target.Blinded(spell.debuffDuration);
							break;
						case Spell.SpellDebuff.Defend:
							attaker.InDefense(spell.debuffDuration);
							break;
					}
					break;

				case Spell.SpellType.Health:
					attaker.Heal(spell.GetPower());
					attaker.HUD.UpdateHP(playerUnit.currentHP);
					break;

				case Spell.SpellType.Debuff:
					switch (spell.spellDebuff)
					{
						case Spell.SpellDebuff.NONE:
							break;
						case Spell.SpellDebuff.Blind:
							target.Blinded(spell.debuffDuration);
							break;
						case Spell.SpellDebuff.Defend:
							attaker.InDefense(spell.debuffDuration);
							break;
					}
					break;
			}

			StartCoroutine(EnableActionButtons(false));

			//	Compara cual de las dos animaciones que se estan reproduciendo actualmente es mas larga y devuelve su valor
			yield return new WaitForSeconds(animTransition);
			yield return new WaitForSeconds(CompareCurrentClipDuration(attaker, target));

			TurnSelector(isDead);
		}
		else if (state == BattleState.ENEMYTURN)
			EnemyIA();
		else dialogueText.text = "You dont have enough mana!";
	}
	
	void TurnSelector(bool IsDead)  // Logica para saber de quien es el turno y como avanzar
	{
		if (state == BattleState.PLAYERTURN)
		{
			if (IsDead)
			{
				state = BattleState.WON;
				EndBattle();
			}
			else
			{
				state = BattleState.ENEMYTURN;
				EnemyTurn();
			}
		}
		else if (state == BattleState.ENEMYTURN)
		{
			if (IsDead)
			{
				state = BattleState.LOST;
				EndBattle();
			}
			else
			{
				state = BattleState.PLAYERTURN;
				PlayerTurn();
			}
		}
	}

    void PlayerTurn()
	{
		playerUnit.ReduceBuffTurn();
		dialogueText.text = "Choose an action:";
		StartCoroutine(EnableActionButtons(true));
	}

    public void EnemyTurn()
	{
		enemyUnit.ReduceBuffTurn();

		EnemyIA();
	}

	void EnemyIA()
    {
		int[] chance = { enemyUnit.attackProbability, enemyUnit.spellProbability };
		int counter = 0;
		for (int i = 0; i < chance.Length; i++)     //  Usamos un bucle para crear el numero de probabilidad maxima que debemos crear
		{
			counter += chance[i];
		}

		int num = Random.Range(0, counter);
		for (int i = 0; i < chance.Length; i++)     //  Al numero obtenido vamos restando las probabilidades, hasta que tengamos un numero que sea menor a una de las probabilidades.
		{
			if (num < chance[0])
			{
				StartCoroutine(AttackTarget(enemyUnit, playerUnit, Random.Range(0, enemyUnit.weapons.Length)));
				break;
			}
			if (num < chance[1])
			{
				StartCoroutine(SpellTarget(enemyUnit.spells[Random.Range(0, enemyUnit.spells.Length)], enemyUnit, playerUnit));
				break;
			}
			num -= chance[i];
		}
	}

	void EndBattle()
	{
		SceneManager.LoadScene("EndGameScreen");
		/*
		if (state == BattleState.WON)
			
			dialogueText.text = "You won the battle!";
		}
		else if (state == BattleState.LOST)
		{
			dialogueText.text = "You were defeated.";
		}
		*/
	}

	#endregion

	IEnumerator PlayerDefending()
    {
		playerUnit.InDefense(defenseDuration);

		dialogueText.text = "You are on guard!";

		state = BattleState.ENEMYTURN;
		yield return new WaitForSeconds(animTransition);
		yield return new WaitForSeconds(ReturnCurrentClipDuration(playerUnit));
		//yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

		EnemyTurn();
	}

    public void OnAttackButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(AttackTarget(playerUnit, enemyUnit));
	}

	public void OnDefendButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerDefending());
	}

	void CreateSpellsButtons()
    {
        foreach (Spell spell in playerUnit.spells)
        {
			GameObject button = Instantiate(spellButtonPf, spellBoxPanel.transform);
			button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = spell.spellName + "/" + spell.manaCost + "MP";

			button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(SpellTarget(spell, playerUnit, enemyUnit)));
			spellBoxPanel.SetActive(false);
		}
    }

	public void OnSpellButton()
    {
		dialogueText.text = null;

		if(!spellBoxPanel.activeInHierarchy) spellBoxPanel.SetActive(true);
		else spellBoxPanel.SetActive(false);
    }

    #region Misc

	bool DamageTarget(int dmg, BattleUnit target)
    {
		bool isDead = target.TakeDamage(dmg);
		target.HUD.UpdateHP(target.currentHP);
		return isDead; // Devuelve si el enemigo esta muerto
	}

	void ReduceMP(int mp, BattleUnit target)
    {
		target.ReduceMP(mp);
		target.HUD.UpdateMP(target.currentMP);
	}

	void IfEnemyDead(bool isDead)
    {
		if (isDead)
		{
			state = BattleState.WON;
			EndBattle();
		}
		else
		{
			EnemyTurn();
		}
	}

	IEnumerator EnableActionButtons(bool _bool)
    {
		if(_bool) yield return new WaitForSeconds(0.1f);

		Button[] actionButtons = ButtonPanel.GetComponentsInChildren<Button>();

        for (int i = 0; i < actionButtons.Length; i++)
        {
			actionButtons[i].interactable = _bool;
        }
    }
	float ReturnCurrentClipDuration(BattleUnit target)
    {
		float duration = target.anim.GetCurrentAnimatorStateInfo(0).length;
		print(target.name + " / " + duration + " / " + target.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
		return duration;
    }

	float CompareCurrentClipDuration(BattleUnit first, BattleUnit second)
    {
		float firstDuration = ReturnCurrentClipDuration(first);
		float secodDuration = ReturnCurrentClipDuration(second);


		//print("primero" + " / " + firstDuration + " / " + first.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
		//print("segundo" + " / " + secodDuration + " / " + second.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
		if (firstDuration == secodDuration || firstDuration > secodDuration)
        {
			print("Primero elegido");
			return firstDuration;
        }
		else
        {
			print("Segundo elegido");
			return secodDuration;
        }

    }
    #endregion

}
