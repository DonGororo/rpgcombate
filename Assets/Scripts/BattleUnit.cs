using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
/// <summary>
/// Este script contiene todos los datos de la unidad, incluido estadisticas base y modificadores
/// Tambien realizara los calculos de los ataques
/// </summary>
public class BattleUnit : MonoBehaviour
{
    #region Variables

    //En verdad todo esto se podria hacer un ScripteableObject para que mantenga los valores de vida y eso... pero bue

	[Header("Debuffs Zone")]
	public float blindMissChange = 0.5f;
	public int defNullifiedDamage =2;

	[Header("Basic Parameters")]
	public bool itsEnemyUnit;
	public string unitName;
	public int currentHP, maxHP;
	public int currentMP, maxMP;
	public int baseEvasion, baseAccuracy;

	//	Modificadores de las estadisticas
	int modEvasion, modAccuracy;

    [Header("Battle thingies")]
	public Weapons[] weapons;
	public Spell[] spells;

	[Header("HUD Tag")]
	public string HUDtag;
	public BattleHUD HUD;

	[Header("Animation")]
	public Animator anim;
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

    //	Array de turnos para cada debuffo
    int[] debuffTurns = new int[]
    {
		0,	//	blinded index 0
		0	//	Defense index 1
    };

	//	Debuffos aplicados
	bool blinded;
	bool defended;

    Action<int> CheckBuffs;	//	Es un evento pero mas corto... yokse hace magias

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
        for (int i = 0; i < debuffTurns.Length; i++)
        {
			if (debuffTurns[i] > 0)
			debuffTurns[i]--;
        }
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
    public void AnimationAttack()
    {
		animationAttack = true;
    }

	public void AnimationEnd()
    {
		animationEnded = true;
    }
    #endregion

    #region Get Parameters

    public int GetAccuracy()
	{ 
		float returnAccuracy;

		returnAccuracy = Random.Range(0 , 100) - baseAccuracy - modAccuracy;

		if (blinded) returnAccuracy = returnAccuracy * (1 + blindMissChange);

		return (int)returnAccuracy;
    }

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
