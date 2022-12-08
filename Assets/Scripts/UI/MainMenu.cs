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

        newBtn.onClick.AddListener(PlayTimeline);//新游戏
        continueBtn.onClick.AddListener(ContinueGame);
        quitBtn.onClick.AddListener(QuitGame);//事件回调
        DestroyGameUI();//主菜单不显示游戏区UI
        director = FindObjectOfType<PlayableDirector>();
        director.stopped += NewGame;//NewGame加入帧回调需要设置对应的参数格式
    }

    void PlayTimeline()
    {
        director.Play();
    }
    void NewGame(PlayableDirector obj)
    {
        Debug.Log("NewGame");
        PlayerPrefs.DeleteAll();
        //转换场景
        TransitionSceneControl.Instance.TransitionToFirstLevel();
    }
    void ContinueGame()
    {
        //转换场景，读取进度
        TransitionSceneControl.Instance.TransitionToLoadGame();
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("退出游戏");
    }

    //主菜单不显示血条
    void DestroyGameUI()
    {
        var healthUIs = FindObjectsOfType<PlayerHealthUI>();
        foreach(var item in healthUIs)
        {
            Destroy(item.gameObject);
        }
    }
}

