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
	[Header("Battle Part")]
	[SerializeField] GameObject playerPrefab;
	[SerializeField] GameObject enemyPrefab;

	[SerializeField] Transform playerPosition;
	[SerializeField] Transform enemyPosition;

	BattleUnit playerUnit;
	BattleUnit enemyUnit;

	Animator playerAnim;
	AnimatorOverrideController playerAnimOverdrive;
	Animator enemyAnim;
	AnimatorOverrideController enemyAnimOverdrive;

	[SerializeField] TextMeshProUGUI dialogueText;

	[SerializeField] BattleHUD playerHUD;
	[SerializeField] BattleHUD enemyHUD;

	[SerializeField] BattleState state;

	[Header("Spells Zone")]
	[SerializeField] GameObject spellButton;
	[SerializeField] GameObject spellBox;

	private void Start()
    {
        state = BattleState.START;
		StartCoroutine(SetupBattle());
		CreateSpellsButtons();
    }

    #region Battle Logic

    IEnumerator SetupBattle()
	{
		GameObject playerGO = Instantiate(playerPrefab, playerPosition);
		playerUnit = playerGO.GetComponent<BattleUnit>();
		playerAnim = playerGO.GetComponent<Animator>();
		playerAnimOverdrive = new AnimatorOverrideController(playerAnim.runtimeAnimatorController);
		playerAnim.runtimeAnimatorController = playerAnimOverdrive;

		GameObject enemyGO = Instantiate(enemyPrefab, enemyPosition);
		enemyUnit = enemyGO.GetComponent<BattleUnit>();
		enemyAnim = enemyGO.GetComponent<Animator>();
		enemyAnimOverdrive = new AnimatorOverrideController(enemyAnim.runtimeAnimatorController);
		enemyAnim.runtimeAnimatorController = enemyAnimOverdrive;

		dialogueText.text = "A wild " + enemyUnit.unitName + " approaches...";

		playerHUD.SetHUD(playerUnit);
		enemyHUD.SetHUD(enemyUnit);

		// Espera hasta que se pulse el raton para continuar con el combate, "() =>" permite crear una funcion ya que introduciendolo sin esto no detecta el booleano
		//yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0));


		state = BattleState.PLAYERTURN;
		PlayerTurn();
	}
	void PlayerTurn()
	{
		playerUnit.ReduceBuffTurn();
		dialogueText.text = "Choose an action:";
	}

	IEnumerator PlayerAttack()
	{
		bool isDead = false;

		if (playerUnit.weapons[0].customAnimation != null)
		{
			playerAnimOverdrive["AttackCustom"] = playerUnit.weapons[0].customAnimation;
			playerAnim.runtimeAnimatorController = playerAnimOverdrive;
			playerAnim.SetTrigger("AttackCustom");
		}
		else playerAnim.SetTrigger("AttackDefault");

		if (playerUnit.GetAccuracy() <= enemyUnit.GetEvasion())
        {
			isDead = DamagePlayerToEnemy(playerUnit.GetDamage());

			if (isDead) enemyAnim.SetTrigger("Death");
			else enemyAnim.SetTrigger("TakeHit");

			dialogueText.text = "The attack is successful!";
        }
        else
        {
			enemyAnim.SetTrigger("MissHit");
			enemyUnit.PlayMissHitClip();
			dialogueText.text = "The attack missed!";
		}

		state = BattleState.ENEMYTURN;

		yield return new WaitForSeconds(0.1f);
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

		IfEnemyDead(isDead);
	}

	IEnumerator PlayerSpell(Spell spell)
    {
		spellBox.SetActive(false);
		bool isDead = false;

        if (playerUnit.currentMP > spell.manaCost)
        {
			ReducePlayerMP(spell.manaCost);
			playerUnit.PlaySpellAttackClip(spell.customSound);
			if (spell.customAnimation != null)
			{
				playerAnimOverdrive["SpellAttackCustom"] = spell.customAnimation;
				playerAnim.runtimeAnimatorController = playerAnimOverdrive;
				playerAnim.SetTrigger("SpellAttackCustom");
			}
			else playerAnim.SetTrigger("SpellAttackDefault");

			dialogueText.text = "You used " + spell.spellName;

			switch (spell.spellType)
			{
				case Spell.SpellType.Damage:
					isDead = DamagePlayerToEnemy(spell.GetPower());

					if (isDead) enemyAnim.SetTrigger("Death");
					else enemyAnim.SetTrigger("TakeHit");

					switch (spell.spellDebuff)
					{
						case Spell.SpellDebuff.NONE:
							break;
						case Spell.SpellDebuff.Blind:
							enemyUnit.Blinded(spell.debuffDuration);
							break;
						default:
							break;
					}
					break;

				case Spell.SpellType.Health:
					playerUnit.Heal(spell.GetPower());
					playerHUD.UpdateHP(playerUnit.currentHP);
					break;

				case Spell.SpellType.Debuff:
					switch (spell.spellDebuff)
					{
						case Spell.SpellDebuff.NONE:
							break;
						case Spell.SpellDebuff.Blind:
							enemyUnit.Blinded(spell.debuffDuration);
							break;
						default:
							break;
					}
					break;
			}

			state = BattleState.ENEMYTURN;
			yield return new WaitForSeconds(0.1f);
			yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
			IfEnemyDead(isDead);
		}
		else dialogueText.text = "You dont have enough mana!";
	}

	IEnumerator EnemyTurn()
	{
		enemyUnit.ReduceBuffTurn();

		bool isDead = false;
		dialogueText.text = enemyUnit.unitName + " attacks!";
		print("Dialogo de ataque");

		if (enemyUnit.weapons[0].customAnimation != null)
		{
			enemyAnimOverdrive["AttackCustom"] = enemyUnit.weapons[0].customAnimation;
			enemyAnim.runtimeAnimatorController = enemyAnimOverdrive;
			enemyAnim.SetTrigger("AttackCustom");
		}
		else enemyAnim.SetTrigger("AttackDefault");

		if (enemyUnit.GetAccuracy() <= playerUnit.GetEvasion())
		{
			isDead = DamageEnemyToPlayer(enemyUnit.GetDamage());

			if (isDead) playerAnim.SetTrigger("Death");
			else playerAnim.SetTrigger("TakeHit");

			dialogueText.text = "The enemy attack is successful!";
		}
		else
		{
			playerAnim.SetTrigger("MissHit");
			playerUnit.PlayMissHitClip();
			dialogueText.text = "The enemy attack missed!";
		}

		yield return new WaitForSeconds(0.1f);	//	Para que si pulsas encima del boton de atacar no haga todo el turno, sino que tenga una muy peque?a pausa
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

		if (isDead)
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

	public void OnAttackButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerAttack());
	}

	void CreateSpellsButtons()
    {
        foreach (Spell spell in playerUnit.spells)
        {
			GameObject button = Instantiate(spellButton, spellBox.transform);
			button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = spell.spellName + "/" + spell.manaCost + "MP";

			button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(PlayerSpell(spell)));
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

	bool DamagePlayerToEnemy(int dmg)
    {
		bool isDead = enemyUnit.TakeDamage(dmg);
		enemyHUD.UpdateHP(enemyUnit.currentHP);
		return isDead; // Devuelve si el enemigo esta muerto
	}

	bool DamageEnemyToPlayer(int dmg)
    {
		bool isDead = playerUnit.TakeDamage(dmg);
		playerHUD.UpdateHP(playerUnit.currentHP);
		return isDead;  // Devuelve si el player esta muerto
	}

	void ReducePlayerMP(int mp)
    {
		playerUnit.ReduceMP(mp);
		playerHUD.UpdateMP(playerUnit.currentMP);
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
			StartCoroutine(EnemyTurn());
		}
	}

    #endregion

}
