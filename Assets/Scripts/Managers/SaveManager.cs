using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SaveManager : Singleton<SaveManager>
{
    string sceneName = "";
    public string SceneName { get { return PlayerPrefs.GetString(sceneName); } }

   protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TransitionSceneControl.Instance.TransitionToMain();//�ص����˵�
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SavePlayerData();//�������ݵ�json�ļ���
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayerData();//��json�ļ�����������
        }
    }

    public void SavePlayerData()
    {
        Debug.Log("SavePlayerData");
        Save(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.name);
    }
    
    public void LoadPlayerData()
    {
        Debug.Log("LoadPlayerData");
        Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.name);
    }

     public void Save(Object data, string key)
    {
        var jsonData  = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);//���浱ǰ��������
        PlayerPrefs.Save();
    }

    public void Load(Object data, string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        }
    }
}
