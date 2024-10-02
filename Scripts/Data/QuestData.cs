using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestData
{
    public int num;
    public string name;
    public string description;
    public string mission;
    public int npc;
    public int category;
    public string request;
    public string rewards;
    public int preQuest;
}

[Serializable]
public class QuestDataLoader : ILoader<int, QuestData>
{
    public List<QuestData> questData = new List<QuestData>();

    public Dictionary<int, QuestData> MakeDict()
    {
        Dictionary<int, QuestData> dic = new Dictionary<int, QuestData>();
        foreach (QuestData data in questData)
            dic.Add(data.num, data);
        return dic;
    }
}