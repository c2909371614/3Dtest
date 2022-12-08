using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attack/Attack Data" )]
public class AttackData_so : ScriptableObject
{
    public float attackRange, skillRange, coolDown;
    public int minDamage, maxDamage;
    public float criticalMultiplier, criticalChance;
}
