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
	[Header("Debuffs Zone")]
	float blindMissChange = 0.5f;
	int defNullifiedDamage =2;


	//En verdad todo esto se podria hacer un ScripteableObject para que mantenga los valores de vida y eso... pero bue
	[Header("NO TOCAR Zone")]
	public string unitName;
	public int currentHP, maxHP;
	public int currentMP, maxMP;
	public int baseEvasion, baseAccuracy;

	//	Modificadores de las estadisticas
	int modEvasion, modAccuracy;
	
	public Action<int> CheckBuffs;

	//	Array de turnos para cada debuffo
	int[] debuffTurns = new int[]
    {
		0,	//	blinded index 0
		0	//	Defense index 1
    };

	//	Debuffos aplicados
	bool blinded;
	bool defended;

	public Weapons[] weapons;
	public Spell[] spells;

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
        if (defended)
        {
            currentHP -= dmg/defNullifiedDamage;
			Debug.Log(defNullifiedDamage);
		}
        else
        {
			currentHP -= dmg;
		}

		Debug.Log(dmg);
		Debug.Log(defNullifiedDamage);

		if (currentHP <= 0)
			return true;
		else
			return false;
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

		if (blinded) returnAccuracy = returnAccuracy * (1 + blindMissChange);

		return (int)returnAccuracy;
    }

	public int GetEvasion()
	{
		float returnEvasion;

		returnEvasion = baseEvasion + modEvasion;

		return (int)returnEvasion;

	}

	public int GetDamage()
    {
		int weaponBaseDamage = weapons[0].GetDamage();

		float returnDamage;

		returnDamage = weaponBaseDamage;

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

	public void InDefense(int turns = 0)
	{
		Debug.Log("check1");
		
		if (turns > 0) debuffTurns[1] = turns;
		if (debuffTurns[1] > 0)
		{
			Debug.Log("check2");
			defended = true;
			CheckBuffs += InDefense;
		}
		else
		{
			Debug.Log("check3");
			defended = false;
			CheckBuffs -= InDefense;
		}
	}

	#endregion
}
