using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    protected override void Awake()
    {
        base.Awake();
        //DontDestroyOnLoad(this);
    }

}
