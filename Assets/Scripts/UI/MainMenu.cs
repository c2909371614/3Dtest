using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{
    Button newBtn;
    Button continueBtn;
    Button quitBtn;
    PlayableDirector director;

    private void Awake()
    {
        //newBtn = transform.GetChild(1).GetComponent<Button>();
        //continueBtn = transform.GetChild(2).GetComponent<Button>();
        //quitBtn = transform.GetChild(3).GetComponent<Button>();
        newBtn = GameObject.Find("NewGame").GetComponent<Button>();
        continueBtn = GameObject.Find("ContinueGame").GetComponent<Button>();
        quitBtn = GameObject.Find("QuitGame").GetComponent<Button>();

        newBtn.onClick.AddListener(PlayTimeline);//����Ϸ
        continueBtn.onClick.AddListener(ContinueGame);
        quitBtn.onClick.AddListener(QuitGame);//�¼��ص�
        DestroyGameUI();//���˵�����ʾ��Ϸ��UI
        director = FindObjectOfType<PlayableDirector>();
        director.stopped += NewGame;//NewGame����֡�ص���Ҫ���ö�Ӧ�Ĳ�����ʽ
    }

    void PlayTimeline()
    {
        director.Play();
    }
    void NewGame(PlayableDirector obj)
    {
        Debug.Log("NewGame");
        PlayerPrefs.DeleteAll();
        //ת������
        TransitionSceneControl.Instance.TransitionToFirstLevel();
    }
    void ContinueGame()
    {
        //ת����������ȡ����
        TransitionSceneControl.Instance.TransitionToLoadGame();
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("�˳���Ϸ");
    }

    //���˵�����ʾѪ��
    void DestroyGameUI()
    {
        var healthUIs = FindObjectsOfType<PlayerHealthUI>();
        foreach(var item in healthUIs)
        {
            Destroy(item.gameObject);
        }
    }
}

