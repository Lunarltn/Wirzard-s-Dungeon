using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public PlayerData PlayerData { get; private set; }
    public Dictionary<int, MonsterData> MonsterDic { get; private set; }
    public Dictionary<int, SkillData> SkillDic { get; private set; }

    public Dictionary<int, EquipItemData> EquipItemDic { get; private set; }
    public Dictionary<int, UseItemData> UseItemDic { get; private set; }
    public Dictionary<int, EtcItemData> EtcItemDic { get; private set; }

    public Dictionary<int, NPCData> NPCDic { get; private set; }
    public Dictionary<int, QuestData> QuestDic { get; private set; }

    public void Init()
    {
        PlayerData = LoadJsonSingle<PlayerData>("PlayerData");
        MonsterDic = LoadJson<MonsterDataLoader, int, MonsterData>("MonsterData").MakeDict();
        SkillDic = LoadJson<SkillDataLoader, int, SkillData>("SkillData").MakeDict();

        EquipItemDic = LoadJson<EquipItemDataLoader, int, EquipItemData>("EquipItemData").MakeDict();
        UseItemDic = LoadJson<UseItemDataLoader, int, UseItemData>("UseItemData").MakeDict();
        EtcItemDic = LoadJson<EtcItemDataLoader, int, EtcItemData>("EtcItemData").MakeDict();

        NPCDic = LoadJson<NPCDataLoader, int, NPCData>("NPCData").MakeDict();
        QuestDic = LoadJson<QuestDataLoader, int, QuestData>("QuestData").MakeDict();

    }

    T LoadJson<T, Key, Value>(string path) where T : ILoader<Key, Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Datas/{path}");
        return JsonUtility.FromJson<T>(textAsset.text);
    }

    T LoadJsonSingle<T>(string path)
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Datas/{path}");
        return JsonUtility.FromJson<T>(textAsset.text);
    }
}
