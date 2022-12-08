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
    //广播
    public void NotifyObserver()
    {
        foreach(var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }
    //找到场景初始位置
    public Transform GetEntrance()
    {
        foreach(var item in FindObjectsOfType<TransitionDestination>())
        {//找到Tag为ENTER的传送点坐标并返回
            
            if(item.destinationTag == TransitionDestination.DestinationTag.ENTER)
            {
                return item.transform;
            }
            
        }
        return null;//没有找到则返回空
    }
}
