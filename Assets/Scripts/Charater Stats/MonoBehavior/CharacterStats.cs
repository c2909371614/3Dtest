using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public event Action<int, int> UpdateBarHealthOnAttack;
    public CharacterData_so characterData;
    public CharacterData_so templateData;
    public AttackData_so attackData;
    [HideInInspector]
    public bool isCritical;
    protected void Awake()
    {
        if(templateData != null)
        {
            characterData = Instantiate(templateData);
        }
    }
    #region Read from Data_so
    public int MaxHealth
    {
        get    { if (characterData != null) return characterData.maxHealth; else return 0; }
        set    { characterData.maxHealth = value; }
    }
    public int CurrentHealth
    {
        get { if (characterData != null) return characterData.currentHealth; else return 0; }
        set { characterData.currentHealth = value; }
    }
    public int BaseDefence
    {
        get { if (characterData != null) return characterData.baseDefence; else return 0; }
        set { characterData.baseDefence = value; }
    }
    public int CurrentDefence
    {
        get { if (characterData != null) return characterData.currentDefence; else return 0; }
        set { characterData.currentDefence = value; }
    }
    #endregion
    #region Character Combat
    public void TakeDamage(CharacterStats attacker, CharacterStats defener)
    {
        //---受到伤害---
        int damage = Mathf.Max( attacker.CurrentDamage() - defener.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        if (attacker.isCritical)
        {
            if(!defener.CompareTag("Player"))//玩家不僵直 
                defener.GetComponent<Animator>().SetTrigger("hit");
        }
        //Update Enemy UI
        UpdateBarHealthOnAttack?.Invoke(CurrentHealth, MaxHealth);
        //攻击者经验update
        if (CurrentHealth <= 0)
            attacker.characterData.UpdateExp(characterData.killPoint);
    }
    public void TakeDamage(int damage, CharacterStats defener)//重载
    {
        int currentDamage = Mathf.Max(damage - defener.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);
        //Update UI
        UpdateBarHealthOnAttack?.Invoke(CurrentHealth, MaxHealth);
        //攻击者经验update
        if (CurrentHealth <= 0)
            GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint);

    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);
        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
            Debug.Log("暴击" + coreDamage);
        }
            
        return (int)coreDamage;
    }
    #endregion
}
