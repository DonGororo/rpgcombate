using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Battle/Spell")]
public class Spell : ScriptableObject
{
    [Header("Basic Parameters")]
    public string spellName;

    public enum SpellType { Damage, Health, Debuff }
    public SpellType spellType;
    public int manaCost;
    public int minPowe, maxPower;

    [Header("Debuff Parameters")]
    public SpellDebuff spellDebuff;
    public enum SpellDebuff { NONE, Blind }

    public int debuffProbability;
    public int debuffDuration;
    [Header("Custom (Left empty for character default)")]
    public AnimationClip customAnimation;
    public AudioClip customSound;

    public int GetPower()
    {
        return Random.Range(minPowe, maxPower);
    }
}
