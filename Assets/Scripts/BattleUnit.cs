using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
/// <summary>
/// Este script contiene todos los datos de la unidad, incluido estadisticas base y modificadores
/// Tambien realizara los calculos de los ataques
/// </summary>
public class BattleUnit : MonoBehaviour
{
    #region Variables

    //En verdad todo esto se podria hacer un ScripteableObject para que mantenga los valores de vida y eso... pero bue
    [Header("Basic Parameters")]
	public string unitName;
	public int currentHP, maxHP;
	public int currentMP, maxMP;
	public int baseEvasion, baseAccuracy;

	//	Modificadores de las estadisticas
	int modEvasion, modAccuracy;

    [Header("Battle thingies")]
	public Weapons[] weapons;
	public Spell[] spells;

    [Header("Audio Clips")]
	public AudioClip attackDefaultClip;
	public AudioClip spellAttackDefaultClip;
	public AudioClip takeHitClip;
	public AudioClip missHitClip;
	public AudioClip deathClip;
	AudioSource audioSource;

    //	Array de turnos para cada debuffo
    int[] debuffTurns = new int[]
    {
		0,	//	blinded index 0
		0	//	Defense index 1
    };

	//	Debuffos aplicados
	bool blinded;

    Action<int> CheckBuffs;	//	Es un evento pero mas corto... yokse hace magias

    #endregion

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
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
		currentHP -= dmg;

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

    #region Get Parameters

    public int GetAccuracy()
	{ 
		float returnAccuracy;

		returnAccuracy = Random.Range(0 , 100) - baseAccuracy - modAccuracy;

		if (blinded) returnAccuracy = returnAccuracy * 1.25f;

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
        }
        else 
        {
			blinded=false;
			CheckBuffs -= Blinded;
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
