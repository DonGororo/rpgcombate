using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Battle/Spell")]
public class Spell : ScriptableObject
{
    public string spellName;

    public enum SpellType { Damage, Health, Debuff }
    public SpellType spellType;
    public int manaCost;

    public int minPowe, maxPower;

    public enum SpellDebuff { NONE, Blind }
    public SpellDebuff spellDebuff;

    public int debuffProbability;
    public int debuffDuration;

    public int GetPower()
    {
        return Random.Range(minPowe, maxPower);
    }
}
