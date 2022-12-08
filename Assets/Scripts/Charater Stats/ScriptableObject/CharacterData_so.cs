using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data", menuName = "Character Stats/Data")]
public class CharacterData_so : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;
    public int currentHealth, baseDefence, currentDefence;
    [Header("Kill")]
    public int killPoint;
    [Header("Level")]
    public int currentLevel;
    public int maxLevel, baseExp, currentExp;
    public float levelBuff;
    public float levelMultiplier//等级数值膨胀倍率
    {
        get { return 1 + (currentLevel - 1) * levelBuff; }
    }
    public void UpdateExp(int point)
    {
        currentExp += point;
        if (currentExp >= baseExp)
        {
            currentExp %= baseExp;
            LevelUp();
        }
            
    }

    private void LevelUp()
    {
        //所有升级提升的属性
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);
        baseExp += (int)(baseExp * levelMultiplier);

        maxHealth = (int)(maxHealth * levelMultiplier);
        currentHealth = maxHealth;//升级满血
        Debug.Log("LEVEL UP:" + currentLevel + "Max Health:" + maxHealth);
    }
}
