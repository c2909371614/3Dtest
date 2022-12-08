using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
   public enum TransitionType//同场景传送、异场景传送
    {
        SameScene, DifferentScene
    }
    [Header("Portal info")]
    public string sceneName;
    public TransitionType transitionType;
    public TransitionDestination.DestinationTag desitinationTag;
    private bool canTrans;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canTrans)
        {
            //传送
            TransitionSceneControl.Instance.TransitionToDestination(this);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            canTrans = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            canTrans = false;
    }
}
