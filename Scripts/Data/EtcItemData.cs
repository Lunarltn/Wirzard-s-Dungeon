using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class EtcItemData
{
    public int num;
    public string name;
    public string comment;
    public string icon;
}

[Serializable]
public class EtcItemDataLoader : ILoader<int, EtcItemData>
{
    public List<EtcItemData> etcItemData = new List<EtcItemData>();

    public Dictionary<int, EtcItemData> MakeDict()
    {
        Dictionary<int, EtcItemData> dic = new Dictionary<int, EtcItemData>();
        foreach (EtcItemData data in etcItemData)
            dic.Add(data.num, data);
        return dic;
    }
}
