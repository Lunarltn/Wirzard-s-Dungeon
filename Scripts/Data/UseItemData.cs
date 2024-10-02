using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[Serializable]
public class UseItemData
{
    public int num;
    public string name;
    public float hp;
    public float mp;
    public float damage;
    public float defense;
    public float speed;
    public string comment;
    public string icon;
}

[Serializable]
public class UseItemDataLoader : ILoader<int, UseItemData>
{
    public List<UseItemData> useItemData = new List<UseItemData>();

    public Dictionary<int, UseItemData> MakeDict()
    {
        Dictionary<int, UseItemData> dic = new Dictionary<int, UseItemData>();
        foreach (UseItemData data in useItemData)
            dic.Add(data.num, data);
        return dic;
    }
}
