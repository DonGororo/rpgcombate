using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Battle/Spell")]
public class Spell : ScriptableObject
{
    new public string name;

    public enum SpellType { Damage, Health, Debuff }
    public SpellType spellType;
    public int manaCost;

    public int healthValue;
    public int minDamage, maxDamage;

    public bool blind;

    public int debuffProbability;
    public int debuffDuration;

    public int GetDamage()
    {
        return Random.Range(minDamage, maxDamage);
    }
}
