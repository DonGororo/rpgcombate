using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Scripteable object to create new spells
/// 
/// To create a new Debuff please remember to add the name here in the SpellDebuff enum and do the proper changes in BattleUnit Script and BattleSystem
/// </summary>
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
    public enum SpellDebuff { NONE, Blind, Defend }

    //public int debuffProbability;
    public int debuffDuration;

    //  The spell can have a custom animation or/and sound effect that will overdrive the default ones
    //  Can check the BattleSystem to look how it works
    [Header("Custom (Left empty for character default)")]
    public AnimationClip customAnimation;
    public AudioClip customSound;

    // This method is called when you whant to get the power of the spell
    public int GetPower()
    {
        return Random.Range(minPowe, maxPower);
    }
}
