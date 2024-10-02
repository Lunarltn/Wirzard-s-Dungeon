using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[Serializable]
public class EquipItemData
{
    public int num;
    public string name;
    public int equipCategory;
    public float hp;
    public float mp;
    public float damage;
    public float defense;
    public float speed;
    public string comment;
    public string icon;
}

[Serializable]
public class EquipItemDataLoader : ILoader<int, EquipItemData>
{
    public List<EquipItemData> equipItemData = new List<EquipItemData>();

    public Dictionary<int, EquipItemData> MakeDict()
    {
        Dictionary<int, EquipItemData> dic = new Dictionary<int, EquipItemData>();
        foreach (EquipItemData data in equipItemData)
            dic.Add(data.num, data);
        return dic;
    }
}
