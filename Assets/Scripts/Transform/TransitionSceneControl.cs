using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class TransitionSceneControl : Singleton<TransitionSceneControl>, IEndGameObserver
{
    GameObject player;
    public GameObject playerPrefab;
    NavMeshAgent playerAgent;
    public SceneFader sceneFaderPrefab;//渐出渐入prefabs
    bool fadeFinished;
    protected override void Awake()
    {
        Debug.Log("Trans Awake");
        base.Awake();
        DontDestroyOnLoad(this);
    }

   void Start()
    {
        GameManager.Instance.AddObserver(this);//注册广播接收
        fadeFinished = true;
    }

    //传送点传送逻辑
    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                //Transition(SceneManager.GetActiveScene().name, transitionPoint.desitinationTag);
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.desitinationTag));//协程
                //StartCoroutine();
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.desitinationTag));
                break;
        }
            
    }
    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        //TODO:保存数据
        SaveManager.Instance.SavePlayerData();//触发传送时自动保存数据
        //传送协程
        
        if (sceneName != SceneManager.GetActiveScene().name)//当前激活的场景与传送场景不同
        {
           
            yield return SceneManager.LoadSceneAsync(sceneName);
            Transform destTransform = GetDestination(destinationTag).transform;
            //在跨场景传送时检查目标场景是否有player,有则删除
            var players = FindObjectsOfType<PlayerControl>();//找到所有带有PlayerControl的object
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].CompareTag("Player"))
                {
                    Destroy(players[i].gameObject);
                }
            }
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            SaveManager.Instance.LoadPlayerData();//异场景传送从json载入数据
            yield break;
        }
        else
        {
            Transform destTransform = GetDestination(destinationTag).transform;
            player = GameManager.Instance.playerStats.gameObject;

            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            player.transform.SetPositionAndRotation(destTransform.position, destTransform.rotation);
            playerAgent.enabled = true;
            yield return null;
        }
    }

    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag)
    {
        var entrances = FindObjectsOfType<TransitionDestination>();//找到所有的传送出口
        for(int i = 0; i < entrances.Length; i++)
        {
            if (entrances[i].destinationTag == destinationTag)//找到对应编号的传送出口
                return entrances[i];
        }
        return null;
    }

    //载入主菜单
    public void TransitionToMain()
    {
        StartCoroutine(LoadMain());
    }

    //载入上一次保存的进度场景
    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }

    //载入初始场景
    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("Game"));
    }

    //加载对应场景
    IEnumerator LoadLevel(string scene)
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);//渐出渐入
        if(scene != "")
        {
            yield return StartCoroutine(fade.FadeOut(2f));//渐出
            yield return SceneManager.LoadSceneAsync(scene);//加载场景
            var Entrance = GameManager.Instance.GetEntrance();
            //在跨场景传送时检查目标场景是否有player,有则删除
            var players = FindObjectsOfType<PlayerControl>();//找到所有带有PlayerControl的object
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].CompareTag("Player"))
                {
                    Destroy(players[i].gameObject);
                }
            }
            yield return player = Instantiate(playerPrefab, Entrance.position, Entrance.rotation);//指定坐标生成玩家
            
            SaveManager.Instance.SavePlayerData();//保存玩家数据
            yield return StartCoroutine(fade.FadeIn(2f));//渐入  
            yield break;
        }
    }

    //加载主菜单
    IEnumerator LoadMain()
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);//渐出渐入
        yield return StartCoroutine(fade.FadeOut(2f));//渐出
        yield return SceneManager.LoadSceneAsync("Main");
        yield return StartCoroutine(fade.FadeIn(2f));//渐入  

    }

    public void EndNotify()
    {
        if (fadeFinished)
        {
            fadeFinished = false;
            StartCoroutine(LoadMain());
        }
         
    }
}
