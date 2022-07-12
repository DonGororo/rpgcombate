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

		GameObject enemyGO = Instantiate(enemyPrefab, enemyPosition);
		enemyUnit = enemyGO.GetComponent<BattleUnit>();

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
		//state = BattleState.BUSY;
		bool isDead = false;

		if (playerUnit.GetAccuracy() <= enemyUnit.GetEvasion())
        {
			isDead = DamagePlayerToEnemy(playerUnit.GetDamage());
			dialogueText.text = "The attack is successful!";
        }
        else
        {
			dialogueText.text = "The attack missed!";
		}

		state = BattleState.ENEMYTURN;
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

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

	IEnumerator PlayerSpell(Spell spell)
    {
		spellBox.SetActive(false);
		bool isDead = false;
        switch (spell.spellType)
        {
            case Spell.SpellType.Damage:
				if (playerUnit.currentMP > spell.manaCost)
				{
					ReducePlayerMP(spell.manaCost);

					isDead = DamagePlayerToEnemy(spell.GetPower());

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

                    dialogueText.text = "You used " + spell.spellName;

					state = BattleState.ENEMYTURN;
					yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

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
				else dialogueText.text = "You dont have enough mana!";
				break;

            case Spell.SpellType.Health:
				if (playerUnit.currentMP > spell.manaCost)
				{
					ReducePlayerMP(spell.manaCost);

					playerUnit.Heal(spell.GetPower());
					playerHUD.UpdateHP(playerUnit.currentHP);

					dialogueText.text = "You used " + spell.spellName;

					state = BattleState.ENEMYTURN;
					yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
					StartCoroutine(EnemyTurn());
				}
				else dialogueText.text = "You dont have enough mana!";
				break;

            case Spell.SpellType.Debuff:
				if (playerUnit.currentMP > spell.manaCost)
				{
					ReducePlayerMP(spell.manaCost);

					switch (spell.spellDebuff)
					{
						case Spell.SpellDebuff.Blind:
							enemyUnit.Blinded(spell.debuffDuration);
							break;
						default:
							break;
					}

					dialogueText.text = "You used " + spell.spellName;

					state = BattleState.ENEMYTURN;
					yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

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
				else dialogueText.text = "You dont have enough mana!"; 
				break;

        }
    }

	IEnumerator EnemyTurn()
	{
		enemyUnit.ReduceBuffTurn();
		bool isDead = false;
		dialogueText.text = enemyUnit.unitName + " attacks!";
		print("Dialogo de ataque");

		if (enemyUnit.GetAccuracy() <= playerUnit.GetEvasion())
		{
			isDead = DamageEnemyToPlayer(enemyUnit.GetDamage());
			dialogueText.text = "The enemy attack is successful!";
		}
		else
		{
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

    #endregion

}
