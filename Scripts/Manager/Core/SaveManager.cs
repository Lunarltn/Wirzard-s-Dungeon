using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[Serializable]
public class GameData
{
    public string Name;
}
public class SaveManger
{
    GameData _gameData = new GameData();
    public GameData SaveData { get { return _gameData; } set { _gameData = value; } }
    public string _path = /*Application.persistentDataPath+*/"/SaveData.json";

    public void Init()
    {

    }

    public void SaveGame()
    {
        string jsonStr = JsonUtility.ToJson(Managers.Save.SaveData);
        File.WriteAllText(_path, jsonStr);
        Debug.Log($"Save Game Completed : {_path}");
    }

    public bool LoadGame()
    {
        if (File.Exists(_path) == false)
            return false;

        string fileStr = File.ReadAllText(_path);
        GameData data = JsonUtility.FromJson<GameData>(fileStr);
        if (data != null)
        {
            Managers.Save.SaveData = data;
        }

        Debug.Log($"Save Game Loaded : {_path}");
        return true;
    }
}