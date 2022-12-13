using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;

public class SoundManager : Singleton<SoundManager>
{
    //---绑定lua相关配置---
    public TextAsset luaScript;
    //注入lua的变量表
    public Injection[] injections;
    //配置lua环境
    internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!
    internal static float lastGCTime = 0;
    internal const float GCInterval = 1;//1 second 
    //lua接口
    private Action luaStart;
    private Action luaUpdate;
    private Action luaOnDestroy;
    //lua script 运行
    private LuaTable scriptEnv;

    // Start is called before the first frame update
    public AudioSource audioSource;
    public AudioClip hitSound, hurtSound, levelUpSound;

    protected override void Awake()
    {
        base.Awake();
        SetLuaAwake();
    }
    private void Start()
    {
        SetLuaStart();
    }
    private void Update()
    {
        SetLuaUpdate();
    }
    protected override void OnDestroy()
    {
        SetLuaOnDestroy();
        base.OnDestroy();
    }
    //lua初始化
    void SetLuaAwake()
    {
        scriptEnv = luaEnv.NewTable();

        // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
        LuaTable meta = luaEnv.NewTable();
        meta.Set("__index", luaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        scriptEnv.Set("self", this);
        if(injections.GetLength(0) > 0)
        {
            foreach (var injection in injections)
            {
                scriptEnv.Set(injection.name, injection.value);//注入的变量
            }
        }
        
        luaEnv.DoString(luaScript.text, "LuaTestScript", scriptEnv);//绑定lua

        Action luaAwake = scriptEnv.Get<Action>("awake");
        scriptEnv.Get("start", out luaStart);
        scriptEnv.Get("update", out luaUpdate);
        scriptEnv.Get("ondestroy", out luaOnDestroy);

        if (luaAwake != null)
        {
            luaAwake();
        }
    }
    //lua Start
    void SetLuaStart()
    {
        if (luaStart != null)
        {
            luaStart();
        }
    }
    //lua Update
    void SetLuaUpdate()
    {
        if (luaUpdate != null)
        {
            luaUpdate();
        }
        if (Time.time - lastGCTime > GCInterval)
        {
            luaEnv.Tick();
            lastGCTime = Time.time;
        }
    }

    void SetLuaOnDestroy()
    {
        if (luaOnDestroy != null)
        {
            luaOnDestroy();
        }
        luaOnDestroy = null;
        luaUpdate = null;
        luaStart = null;
        scriptEnv.Dispose();
        injections = null;
    }
    public void HitAudio()
    {
        audioSource.clip = hitSound;
        audioSource.Play();
    }

    public void HurtAudio()
    {
        audioSource.clip = hurtSound;
        audioSource.Play();
    }
    public void LevelUpSoundAudio()
    {
        audioSource.clip = levelUpSound;
        audioSource.Play();
    }

}
