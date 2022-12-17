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
    public SceneFader sceneFaderPrefab;//��������prefabs
    bool fadeFinished;
    protected override void Awake()
    {
        Debug.Log("Trans Awake");
        base.Awake();
        DontDestroyOnLoad(this);
    }

   void Start()
    {
        GameManager.Instance.AddObserver(this);//ע��㲥����
        fadeFinished = true;
    }

    //���͵㴫���߼�
    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                //Transition(SceneManager.GetActiveScene().name, transitionPoint.desitinationTag);
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.desitinationTag));//Э��
                //StartCoroutine();
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.desitinationTag));
                break;
        }
            
    }
    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        //TODO:��������
        SaveManager.Instance.SavePlayerData();//��������ʱ�Զ���������
        //����Э��
        
        if (sceneName != SceneManager.GetActiveScene().name)//��ǰ����ĳ����봫�ͳ�����ͬ
        {
           
            yield return SceneManager.LoadSceneAsync(sceneName);
            Transform destTransform = GetDestination(destinationTag).transform;
            //�ڿ糡������ʱ���Ŀ�곡���Ƿ���player,����ɾ��
            var players = FindObjectsOfType<PlayerControl>();//�ҵ����д���PlayerControl��object
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].CompareTag("Player"))
                {
                    Destroy(players[i].gameObject);
                }
            }
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            SaveManager.Instance.LoadPlayerData();//�쳡�����ʹ�json��������
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
        var entrances = FindObjectsOfType<TransitionDestination>();//�ҵ����еĴ��ͳ���
        for(int i = 0; i < entrances.Length; i++)
        {
            if (entrances[i].destinationTag == destinationTag)//�ҵ���Ӧ��ŵĴ��ͳ���
                return entrances[i];
        }
        return null;
    }

    //�������˵�
    public void TransitionToMain()
    {
        StartCoroutine(LoadMain());
    }

    //������һ�α���Ľ��ȳ���
    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }

    //�����ʼ����
    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("Game"));
    }

    //���ض�Ӧ����
    IEnumerator LoadLevel(string scene)
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);//��������
        if(scene != "")
        {
            yield return StartCoroutine(fade.FadeOut(2f));//����
            yield return SceneManager.LoadSceneAsync(scene);//���س���
            var Entrance = GameManager.Instance.GetEntrance();
            //�ڿ糡������ʱ���Ŀ�곡���Ƿ���player,����ɾ��
            var players = FindObjectsOfType<PlayerControl>();//�ҵ����д���PlayerControl��object
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].CompareTag("Player"))
                {
                    Destroy(players[i].gameObject);
                }
            }
            yield return player = Instantiate(playerPrefab, Entrance.position, Entrance.rotation);//ָ�������������
            
            SaveManager.Instance.SavePlayerData();//�����������
            yield return StartCoroutine(fade.FadeIn(2f));//����  
            yield break;
        }
    }

    //�������˵�
    IEnumerator LoadMain()
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);//��������
        yield return StartCoroutine(fade.FadeOut(2f));//����
        yield return SceneManager.LoadSceneAsync("Main");
        yield return StartCoroutine(fade.FadeIn(2f));//����  

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
