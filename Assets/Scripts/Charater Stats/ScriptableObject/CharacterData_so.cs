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
    public float levelMultiplier//�ȼ���ֵ���ͱ���
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
        //������������������
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);
        baseExp += (int)(baseExp * levelMultiplier);

        maxHealth = (int)(maxHealth * levelMultiplier);
        currentHealth = maxHealth;//������Ѫ
        Debug.Log("LEVEL UP:" + currentLevel + "Max Health:" + maxHealth);
    }
}
