using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NPCData
{
    public int num;
    public string name;
    public string talk;
    public string completeQuest;
    public int hp;
    public int damage;
    public int defense;
    public float moveSpeed;
    public float attackSpeed;
}

[Serializable]
public class NPCDataLoader : ILoader<int, NPCData>
{
    public List<NPCData> nPCData = new List<NPCData>();

    public Dictionary<int, NPCData> MakeDict()
    {
        Dictionary<int, NPCData> dic = new Dictionary<int, NPCData>();
        foreach (NPCData data in nPCData)
            dic.Add(data.num, data);
        return dic;
    }
}