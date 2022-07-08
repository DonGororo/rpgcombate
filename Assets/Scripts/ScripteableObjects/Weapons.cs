using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Battle/Weapon")]
public class Weapons : ScriptableObject
{
    new public string name = "Weapon";
    public int minDamage, maxDamage;
    public int accuracityModifier;

    public int GetDamage()
    {
        return Random.Range(minDamage, maxDamage);
    }
}
