using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadmeButtonControl : MonoBehaviour
{
    private void Awake()
    {
        //var foldBtn = transform.GetChild(1).GetComponent<Button>();
       
        //foldBtn.onClick.AddListener(OnBtnFold);
    }
    
    void OnBtnFold()
    {
        var readmePanel = transform.GetChild(0).GetComponent<Image>().gameObject;
        if (readmePanel.activeSelf)
        {
            readmePanel.SetActive(false);
        }
        else
        {
            readmePanel.SetActive(true); ;
        }
    }
}
