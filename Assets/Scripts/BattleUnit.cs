using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
/// <summary>
/// Base script for all the units, it has all the parameters used in the battle system
/// 
/// To create a new Debuff please remember to add the effect as a new method in the Buffs region,
/// add it to the debuff turns array and do the proper changes in BattleUnit Script and BattleSystem
/// </summary>
public class BattleUnit : MonoBehaviour
{
    #region Variables

    //	In the future a ton of these will be added in a scriptable object

	//	Multipliers for the debuffs
	[Header("Debuffs Zone")]
	public float blindMissChange = 0.5f;
	public int defNullifiedDamage = 2;

	[Header("Basic Parameters")]
	public bool itsEnemyUnit;

	public string unitName;
	public int currentHP, maxHP;
	public int currentMP, maxMP;
	public int baseEvasion, baseAccuracy;

	//	Modificadores de las estadisticas
	int modEvasion, modAccuracy;

	//	Weapons and spells equiped in the unit
    [Header("Battle thingies")]
	public Weapons[] weapons;
	public Spell[] spells;

	//	Used to pair the Hud with the unit. In the future the tag will be deprecated for a better way
	[Header("HUD Tag")]
	public string HUDtag;
	public BattleHUD HUD;

	[Header("Animation")]
	public Animator anim;
	//	AnimationOverride is used to replace the customs clips for the animations clips in the weapons/spells
	public AnimatorOverrideController animOverride;
	public bool animationAttack, animationEnded;

    [Header("Audio Clips")]
	public AudioClip attackDefaultClip;
	public AudioClip spellAttackDefaultClip;
	public AudioClip takeHitClip;
	public AudioClip missHitClip;
	public AudioClip deathClip;
	AudioSource audioSource;

	[Header("Action Probability If NPC")]
	public int attackProbability;
	public int spellProbability;

    //	Turn array for the buffs. In the future this will be changed for a better way
    int[] debuffTurns = new int[]
    {
		0,	//	blinded index 0
		0	//	Defense index 1
    };

	//	Buffs checker
	bool blinded;   //	Applied in GetAccuracy()
	bool defended;	//	Applied in TakeDamage()

    Action<int> CheckBuffs;	//	All Active buffs will be added to this event

    #endregion

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
		anim = GetComponent<Animator>();
		animOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
		anim.runtimeAnimatorController = animOverride;

		HUD = GameObject.FindGameObjectWithTag(HUDtag).GetComponent<BattleHUD>();
		HUD.SetHUD(this, itsEnemyUnit);
	}

    public void ReduceBuffTurn()
    {
		//	It will reduce the turn duration by one
        for (int i = 0; i < debuffTurns.Length; i++)
        {
			if (debuffTurns[i] > 0)
			debuffTurns[i]--;
        }
		//	It will call again all the active buff to reaply the effect
		//	or if the turns are 0, delete the buff from the event
		if (CheckBuffs != null) CheckBuffs(0);
    }

	public bool TakeDamage(int dmg)
    {
        if (defended) currentHP -= dmg/defNullifiedDamage;
        else currentHP -= dmg;

		if (currentHP <= 0)
        {
			PlayDeathClip();
			currentHP = 0;
			return true;
        }
        else 
		{
			PlayTakeHitClip();
			return false;			
		}
	}

	public void ReduceMP(int cost)
    {
		currentMP -= cost;
    }

	public void Heal(int amount)
	{
		currentHP += amount;
		if (currentHP > maxHP)
			currentHP = maxHP;
	}

    #region Animation
	//	Methods called in the Animation

	//	Called in the moment a attack hit the enemy
    public void AnimationAttack()
    {
		animationAttack = true;
    }

	//	Called in the moment a animation thats not a loop has finished
	public void AnimationEnd()
    {
		animationEnded = true;
    }
    #endregion

    #region Get Parameters

	//	The lower the Accuracy the better
    public int GetAccuracy()
	{ 
		float returnAccuracy;

		returnAccuracy = Random.Range(0 , 100) - baseAccuracy - modAccuracy;

		if (blinded) returnAccuracy = returnAccuracy * (blindMissChange);

		return (int)returnAccuracy;
    }

	//	The lower the Evasion the better
	public int GetEvasion()
	{
		float returnEvasion;

		returnEvasion = baseEvasion + modEvasion;

		return (int)returnEvasion;

	}

	public int GetDamage(int slot = 0)
    {
		int weaponBaseDamage = weapons[slot].GetDamage();

		float returnDamage;

		returnDamage = weaponBaseDamage;

        PlayAttackClip(weapons[slot].customSound);
		return (int)returnDamage;
    }
    #endregion

    #region Buffs

    public void Blinded(int turns = 0)
    {
		if (turns > 0) debuffTurns[0] = turns;
		if (debuffTurns[0] > 0)
        {
			blinded = true;
			CheckBuffs += Blinded;
			HUD.buffsIcons[0].SetActive(true);
			//	Update the text in the icon
            HUD.buffsIcons[0].GetComponentInChildren<TextMeshProUGUI>().text = debuffTurns[0].ToString();
        }
        else 
        {
			blinded=false;
			CheckBuffs -= Blinded;
			HUD.buffsIcons[0].SetActive(false);
		}
	}

	public void InDefense(int turns = 0)
	{
		if (turns > 0) debuffTurns[1] = turns;
		if (debuffTurns[1] > 0)
		{
			defended = true;
			CheckBuffs += InDefense;
			HUD.buffsIcons[1].SetActive(true);
			HUD.buffsIcons[1].GetComponentInChildren<TextMeshProUGUI>().text = debuffTurns[1].ToString();

		}
		else
		{
			defended = false;
			CheckBuffs -= InDefense;
			HUD.buffsIcons[1].SetActive(false);
		}
	}

	#endregion

    #region Play Sounds
	void PlayAttackClip(AudioClip customClip = null)
    {
		if(customClip != null)
        {
			audioSource.clip = customClip;
			audioSource.Play();
        }
        else
        {
			audioSource.clip = attackDefaultClip;
			audioSource.Play();
        }
	}
	public void PlaySpellAttackClip(AudioClip customClip = null)
    {
		if (customClip != null)
		{
			audioSource.clip = customClip;
			audioSource.Play();
		}
		else
		{
			audioSource.clip = spellAttackDefaultClip;
			audioSource.Play();
		}
	}
	void PlayTakeHitClip()
    {
		audioSource.clip = takeHitClip;
		audioSource.Play();
	}
	public void PlayMissHitClip()
    {
		audioSource.clip = missHitClip;
		audioSource.Play();
	}
	void PlayDeathClip()
    {
		audioSource.clip = deathClip;
		audioSource.Play();
	}
    #endregion
}
