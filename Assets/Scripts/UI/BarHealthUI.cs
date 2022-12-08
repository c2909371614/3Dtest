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
        if (alwaysVisible && !updateControl)//���˴�������Ѫ����������Update����£����ٿ���
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
            if(canvas.renderMode == RenderMode.WorldSpace)//ֻ��һ���������͵���ȾUI
            {
                UIbar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIbar.GetChild(0).GetComponent<Image>();
                UIbar.gameObject.SetActive(alwaysVisible);
            }
           
        }
        
        
    }
    private void UpdateBarHealth(int currentHealth, int maxHealth)
    {
        updateControl = true;//���¹�
        if(UIbar != null)
            UIbar.gameObject.SetActive(true);//�ܵ�����һ���ɼ�
        timeLeft = visiableTime;//��ʾʱ��ˢ��
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
                UIbar.gameObject.SetActive(false);//�ر�Ѫ����ʾ
            }
            else
            {
                timeLeft -= Time.deltaTime;
            }
        }
    }
}
