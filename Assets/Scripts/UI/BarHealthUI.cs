using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarHealthUI : MonoBehaviour
{
    public GameObject healthUIPrefab;
    public Transform barPoint;
    Image healthSlider;
    Transform UIbar;
    Transform cam;
    CharacterStats currentStats;
    public bool alwaysVisible;
    public float visiableTime;
    private float timeLeft;
    private bool updateControl;
    private void Awake()
    {
        currentStats = GetComponent<CharacterStats>();
        currentStats.UpdateBarHealthOnAttack += UpdateBarHealth;
        
    }
    private void Update()
    {
        if (alwaysVisible && !updateControl)//受伤触发更新血条后，无需在Update里更新，减少开销
        {
            if (healthSlider != null && currentStats != null)
            {
                float silderPercent = (float)currentStats.CurrentHealth / currentStats.MaxHealth;
                healthSlider.fillAmount = silderPercent;
            }
        }
    }
    private void OnEnable()
    {
        cam = Camera.main.transform;
        foreach(Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)//只有一个世界类型的渲染UI
            {
                UIbar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIbar.GetChild(0).GetComponent<Image>();
                UIbar.gameObject.SetActive(alwaysVisible);
            }
           
        }
        
        
    }
    private void UpdateBarHealth(int currentHealth, int maxHealth)
    {
        updateControl = true;//更新过
        if(UIbar != null)
            UIbar.gameObject.SetActive(true);//受到攻击一定可见
        timeLeft = visiableTime;//显示时间刷新
        float silderPercent = (float)currentHealth / maxHealth;
        healthSlider.fillAmount = silderPercent;
        if (currentHealth <= 0)
            Destroy(UIbar.gameObject);
    }
    private void LateUpdate()
    {
        if(UIbar != null)
        {
            UIbar.position = barPoint.position;
            UIbar.forward = -cam.forward;
            if(timeLeft <= 0 && !alwaysVisible)
            {
                UIbar.gameObject.SetActive(false);//关闭血条显示
            }
            else
            {
                timeLeft -= Time.deltaTime;
            }
        }
    }
}
