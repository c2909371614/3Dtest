using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarHealthManager : Singleton<BarHealthManager>
{
    protected override void Awake()
    {
        base.Awake();
        //DontDestroyOnLoad(this);
    }
}
