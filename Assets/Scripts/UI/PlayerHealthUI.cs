using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : Singleton<PlayerHealthUI>
{
    Text levelText;
    Image healthSlider, expSlider;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        levelText = transform.GetChild(2).GetComponent<Text>();//等级
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();//血条
        expSlider = transform.GetChild(1).GetChild(0).GetComponent<Image>();//经验条
    }
    private void Update()
    {
        UpdateExp();
        UpdateHealth();
        levelText.text = "Lv."+GameManager.Instance.playerStats.characterData.currentLevel.ToString();
    }
    void UpdateHealth()
    {
        CharacterStats playerStats = GameManager.Instance.playerStats;
        float sliderPercent = (float)playerStats.CurrentHealth / playerStats.MaxHealth;//血条百分比
        healthSlider.fillAmount = sliderPercent;
    }
    void UpdateExp()
    {
        CharacterStats playerStats = GameManager.Instance.playerStats;
        float sliderPercent = (float)playerStats.characterData.currentExp / playerStats.characterData.baseExp;//经验条
        expSlider.fillAmount = sliderPercent;
    }

}
