using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
/// <summary>
/// Primary script that deals with all the logic
/// 
/// To create a new Debuff please remember to add the debuff into the switch in the SpellTarget method and do the proper changes in BattleUnit Script and Spell
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

	//This is the scripts inside the Prefab that controll all the parameters
	BattleUnit playerUnit, enemyUnit;

	[SerializeField] TextMeshProUGUI dialogueText;  //	Dialogue added in previous versions, with the new UI is not being used.
	[SerializeField] GameObject ButtonPanel;

	[Header("Spells Zone")]
	[SerializeField] GameObject spellButtonPf;
	[SerializeField] GameObject spellBoxPanel;

	[Header("Miscelanea")]
	[SerializeField] int defenseDuration; // How many turns the player defense action is active TODO change to a fixed value
	[SerializeField] Button actionButtonSpell;	//	The button prefab used to show the spells

	[SerializeField] float animTransition = 0.26f;	//	Can be eliminated, only mantained to add a little delay before checking if the animations are complete
	#endregion

	private void Start()
	{
		state = BattleState.START;
		StartCoroutine(SetupBattle());
	}

	private void Update()
	{
		// If you press SPACE all the game will work in x2 time
		if (Input.GetKey(KeyCode.Space)) Time.timeScale = 2;
		else Time.timeScale = 1;
	}

	IEnumerator SetupBattle()
	{	
		//	Load the player and enemies in the corresponding place, in the future we plan to load multiple Player controller units and multiple enemies
		GameObject playerGO = Instantiate(playerPrefab, playerPosition);
		playerUnit = playerGO.GetComponent<BattleUnit>();

		GameObject enemyGO = Instantiate(enemyPrefab, enemyPosition);
		enemyUnit = enemyGO.GetComponent<BattleUnit>();

		//	Dialogue used in the previous versions, with the new UI is not being used.
		dialogueText.text = "A wild " + enemyUnit.unitName + " approaches...";

		// Espera hasta que se pulse el raton para continuar con el combate, "() =>" permite crear una funcion ya que introduciendolo sin esto no detecta el booleano
		//yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

		//	This method is disabling all the player buttons
		StartCoroutine(EnableActionButtons(false));
		yield return new WaitUntil(() => Input.GetMouseButtonDown(0));


		//	Alwais start with the player turn, in the future this will change to use a parameter in the units loaded to calculate the turns
		state = BattleState.PLAYERTURN;
		PlayerTurn();
	}

	#region Battle Logic
	//	Basic attack action, works for both player and enemies.
	//	Attaker is the one doing the attack, target is the one reciving the damage and weaponSlot is the weapon in the array you want to use
	IEnumerator AttackTarget(BattleUnit attaker, BattleUnit target, int weaponSlot = 0)
	{
		bool isDead = false;	//	Var used to check if the target is dead
		DestroySpellsInSpellBox();	//	If the Spell Box is left open, this will close it
		StartCoroutine(EnableActionButtons(false));

		//	If the weapon has a AnimationClip, the Animator will changue the AttackCustom clip
		//	in the unit for the one in the weapon and play it
		if (attaker.weapons[weaponSlot].customAnimation != null)
		{
			//	Is telling to the AnimatorOverride to replace the clip AttackCustom in the unit with the clip added in the weapon
			attaker.animOverride["AttackCustom"] = attaker.weapons[weaponSlot].customAnimation;
			//	Now the Animator will take the info from the AnimatorOverride and apply it at runtime,
			//	replacing the AttackCustomClip with the clip in the weapon
			attaker.anim.runtimeAnimatorController = attaker.animOverride;
			attaker.anim.SetTrigger("AttackCustom");
		}
		else attaker.anim.SetTrigger("AttackDefault");

		//	The attack animation have an event that tell when the attack hits, it will wait until a bool in the unit is set to true.
		//	Useful if the attack has an windup to synchronize the attack with the hit
		yield return new WaitUntil(() => attaker.animationAttack);
		attaker.animationAttack = false;

		//	Check to see if the attack is evaded
		if (attaker.GetAccuracy() <= target.GetEvasion())
		{
			//	At the time of the attack, the target will return if is dead or not
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

		//	Wait until both animations (Attack and receiving a hit) are complete
		yield return new WaitUntil(() => attaker.animationEnded);
		attaker.animationEnded = false;
		yield return new WaitUntil(() => target.animationEnded);
		target.animationEnded = false;

		TurnSelector(isDead);
	}

	//	Basic Spell action, works for both player and enemies. Lots of thing are shared with the AttackTarget method
	//	Spells will always hit
	//	Spell is the spell selected, Attaker is the one doing the attack and target is the one reciving the damage
	IEnumerator SpellTarget(Spell spell, BattleUnit attaker, BattleUnit target)
	{
		DestroySpellsInSpellBox();
		bool isDead = false;

		if (attaker.currentMP >= spell.manaCost)
		{
			StartCoroutine(EnableActionButtons(false));
			ReduceMP(spell.manaCost, attaker);

			// Same logic as see in the AttackTarget method
			attaker.PlaySpellAttackClip(spell.customSound);
			if (spell.customAnimation != null)
			{
				attaker.animOverride["SpellAttackCustom"] = spell.customAnimation;
				attaker.anim.runtimeAnimatorController = attaker.animOverride;
				attaker.anim.SetTrigger("SpellAttackCustom");
			}
			else attaker.anim.SetTrigger("SpellAttackDefault");

			dialogueText.text = attaker.unitName + " used " + spell.spellName;

			yield return new WaitUntil(() => attaker.animationAttack);
			attaker.animationAttack = false;

			//	Depending of the spell type it will do diferents things
			//	If you crate a new debuff renember to add it to the swich with the corresponding method call in the attacker/target
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

			yield return new WaitForSeconds(animTransition);
			if (spell.spellType == Spell.SpellType.Damage)
            {
				yield return new WaitUntil(() => attaker.animationEnded);
				attaker.animationEnded = false;
				yield return new WaitUntil(() => target.animationEnded);
				target.animationEnded = false;
			}
            else
            {
				yield return new WaitUntil(() => attaker.animationEnded);
				attaker.animationEnded = false;
			}

			TurnSelector(isDead);
		}
		// If is the enemy turn and it doesn't have enough mana for the spell selected, it will reroll the attack
		else if (state == BattleState.ENEMYTURN) 
			EnemyIA();
		else dialogueText.text = "You dont have enough mana!";
	}

	void TurnSelector(bool IsDead)  
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

	//	Basic IA where you can changue te probability to use a spell or a attack.
	//	In the future you can add a script that will overdrive this IA for more advanced battles
	void EnemyIA()
	{
		int[] chance = { enemyUnit.attackProbability, enemyUnit.spellProbability };
		int counter = 0;
		for (int i = 0; i < chance.Length; i++)
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
	}

    IEnumerator PlayerDefending()
	{
		playerUnit.InDefense(defenseDuration);

		dialogueText.text = "You are on guard!";

		playerUnit.anim.SetTrigger("Defend");

		state = BattleState.ENEMYTURN;
		yield return new WaitForSeconds(animTransition);
		yield return new WaitForSeconds(ReturnCurrentClipDuration(playerUnit));
		//yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

		EnemyTurn();
	}

    #endregion

    #region UI Elements


	//	Called when you press "Attack" in the player actions
	public void OnAttackButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(AttackTarget(playerUnit, enemyUnit));
	}

	//	Called when you press "Defend" in the player actions, starting the PlayerDefending coroutine
	public void OnDefendButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerDefending());
	}

	//	Called when you press "Spell" in the player actions, it will open or close the Spell panel
	public void OnSpellButton()
	{
		dialogueText.text = null;

		if (!spellBoxPanel.activeInHierarchy)
		{
			CreateSpellsButtons(playerUnit);
			spellBoxPanel.SetActive(true);
			// Used to maintain the spell button highlighted when you click in other part of the game... If anyone knows a better way I am all ears
			actionButtonSpell.animator.SetBool("SelectedBool", true);
		}
		else
		{
			DestroySpellsInSpellBox();
		};
	}

	//	Called by OnSpellButton method. It will open a panel with all the spells of the specified target
	//	Its created and destroyed everytime for convenience, if there is any change in the mana of the target it will be updated
	void CreateSpellsButtons(BattleUnit target)
	{
		foreach (Spell spell in target.spells)
		{
			GameObject button = Instantiate(spellButtonPf, spellBoxPanel.transform);

			TextMeshProUGUI _spellName = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI _manaCost = button.transform.GetChild(1).GetComponent<TextMeshProUGUI>();


			_spellName.text = spell.spellName;
			_manaCost.text = spell.manaCost + "MP";

			//	If you dont have enought mana the spell will be dissabled
			if (spell.manaCost > target.currentMP)
			{
				button.GetComponent<Button>().interactable = false;
				_manaCost.color = new Color32(201, 152, 151, 255);

			}

			button.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(SpellTarget(spell, target, enemyUnit)));
			spellBoxPanel.SetActive(true);
		}
	}

    void DestroySpellsInSpellBox()
	{
		spellBoxPanel.SetActive(false);
		actionButtonSpell.animator.SetBool("SelectedBool", false);
		actionButtonSpell.animator.SetTrigger("Normal");
		foreach (Transform child in spellBoxPanel.transform)
		{
			GameObject.Destroy(child.gameObject);
		}
	}

	//	Called when you need to enable/disable the player buttons
	IEnumerator EnableActionButtons(bool _bool)
	{
		if (_bool) yield return new WaitForSeconds(0.1f);

		Button[] actionButtons = ButtonPanel.GetComponentsInChildren<Button>();

		for (int i = 0; i < actionButtons.Length; i++)
		{
			actionButtons[i].interactable = _bool;
		}
	}

    #endregion

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

	//	Old way to check if the attack animations has ended, deprecated by using AnimationEvents
	float ReturnCurrentClipDuration(BattleUnit target)
	{
		float duration = target.anim.GetCurrentAnimatorStateInfo(0).length;
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
