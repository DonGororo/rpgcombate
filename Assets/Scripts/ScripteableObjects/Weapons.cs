using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Scripteable object to create new Weapons
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Battle/Weapon")]
public class Weapons : ScriptableObject
{
    new public string name = "Weapon";
    public int minDamage, maxDamage;

    //  The spell can have a custom animation or/and sound effect that will overdrive the default ones
    //  Can check the BattleSystem to look how it works
    [Header("Custom (Left empty for character default)")]
    public AnimationClip customAnimation;
    public AudioClip customSound;
    //public int accuracityModifier;

    public int GetDamage()
    {
        return Random.Range(minDamage, maxDamage);
    }
}
