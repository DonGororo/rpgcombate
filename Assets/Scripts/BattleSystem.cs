using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// Parte de la logica del sistema de combate... diosito tengame en gracia porque aqui voy a hacer un destrozo
/// </summary>

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, BUSY }

public class BattleSystem : MonoBehaviour
{
	[Header("Battle Zone")]
	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject enemyPrefab;

	[SerializeField] Transform playerPosition;
	[SerializeField] Transform enemyPosition;

	BattleUnit playerUnit;
	BattleUnit enemyUnit;

	[SerializeField] TextMeshProUGUI dialogueText;

	[SerializeField] BattleHUD playerHUD;
	[SerializeField] BattleHUD enemyHUD;

	[SerializeField] BattleState state;

	[Header("Spells Zone")]
	[SerializeField] GameObject spellButton;
	[SerializeField] GameObject spellBox;

	[SerializeField] int defenseDuration;

	private void Start()
    {
        state = BattleState.START;
		StartCoroutine(SetupBattle());
		CreateSpellsButtons();
    }
    IEnumerator SetupBattle()
	{
		GameObject playerGO = Instantiate(playerPrefab, playerPosition);
		playerUnit = playerGO.GetComponent<BattleUnit>();

		GameObject enemyGO = Instantiate(enemyPrefab, enemyPosition);
		enemyUnit = enemyGO.GetComponent<BattleUnit>();

		dialogueText.text = "A wild " + enemyUnit.unitName + " approaches...";

		playerUnit.HUD.SetHUD(playerUnit);
		enemyUnit.HUD.SetHUD(enemyUnit);

		// Espera hasta que se pulse el raton para continuar con el combate, "() =>" permite crear una funcion ya que introduciendolo sin esto no detecta el booleano
		//yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

		state = BattleState.PLAYERTURN;
		PlayerTurn();
	}

    #region Battle Logic

	IEnumerator AttackTarget(BattleUnit attaker, BattleUnit target)
	{
		bool isDead = false;

		if (attaker.weapons[0].customAnimation != null)
		{
			attaker.animOverride["AttackCustom"] = attaker.weapons[0].customAnimation;
			attaker.anim.runtimeAnimatorController = attaker.animOverride;
			attaker.anim.SetTrigger("AttackCustom");
		}
		else attaker.anim.SetTrigger("AttackDefault");

		if (attaker.GetAccuracy() <= target.GetEvasion())
        {
			isDead = DamageTarget(attaker.GetDamage(), target);

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

		// Añadir una forma de deshabilitar los comandos del jugador y volver a activar en el inicio del turno de este, igual en el spell

		yield return new WaitForSeconds(0.1f);
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

		TurnSelector(isDead);
	}
	IEnumerator SpellTarget(Spell spell, BattleUnit attaker, BattleUnit target)
    {
		spellBox.SetActive(false);
		bool isDead = false;

        if (attaker.currentMP > spell.manaCost)
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

			dialogueText.text = "You used " + spell.spellName;

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
							enemyUnit.Blinded(spell.debuffDuration);
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
					}
					break;
			}

			// Añadir una forma de deshabilitar los comandos del jugador y volver a activar en el inicio del turno de este, igual en el spell

			yield return new WaitForSeconds(0.1f);
			yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

			TurnSelector(isDead);
		}
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
	}

    public void EnemyTurn()
	{
		enemyUnit.ReduceBuffTurn();

		StartCoroutine(AttackTarget(enemyUnit, playerUnit));
	}

	void EndBattle()
	{
		if (state == BattleState.WON)
		{
			dialogueText.text = "You won the battle!";
		}
		else if (state == BattleState.LOST)
		{
			dialogueText.text = "You were defeated.";
		}
	}

    #endregion

	IEnumerator PlayerDefending()
    {
		playerUnit.InDefense(defenseDuration);

		dialogueText.text = "You are on guard!";

		state = BattleState.ENEMYTURN;
		yield return new WaitForSeconds(0.1f);
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

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
			GameObject button = Instantiate(spellButton, spellBox.transform);
			button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = spell.spellName + "/" + spell.manaCost + "MP";

			button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(SpellTarget(spell, playerUnit, enemyUnit)));
			spellBox.SetActive(false);
		}
    }

	public void OnSpellButton()
    {
		dialogueText.text = null;

		if(!spellBox.activeInHierarchy) spellBox.SetActive(true);
		else spellBox.SetActive(false);
    }

    #region ShortCuts

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

    #endregion

}
