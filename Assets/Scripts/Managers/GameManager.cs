 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;
    private CinemachineFreeLook followCamera; 
    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    public void RegisterPlayer(CharacterStats player)
    {
        playerStats = player;
        followCamera = FindObjectOfType<CinemachineFreeLook>();
        if(followCamera != null)
        {
            followCamera.Follow = playerStats.transform.GetChild(2);
            followCamera.LookAt = playerStats.transform.GetChild(2);
        }
    }

    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }
    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }
    //�㲥
    public void NotifyObserver()
    {
        foreach(var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }
    //�ҵ�������ʼλ��
    public Transform GetEntrance()
    {
        foreach(var item in FindObjectsOfType<TransitionDestination>())
        {//�ҵ�TagΪENTER�Ĵ��͵����겢����
            
            if(item.destinationTag == TransitionDestination.DestinationTag.ENTER)
            {
                return item.transform;
            }
            
        }
        return null;//û���ҵ��򷵻ؿ�
    }
}