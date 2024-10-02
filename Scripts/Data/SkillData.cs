using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillData
{
    public SkillData()
    {
        num = -1;
    }
    public int num;
    public string name;
    public int cost;
    public float speed;
    public float duration;
    public float damage;
    public float increase;
    public float cooldownTime;
    public int type;
    public string prefeb;
    public string icon;
    public string comment;
}

[Serializable]
public class SkillDataLoader : ILoader<int, SkillData>
{
    public List<SkillData> skillData = new List<SkillData>();

    public Dictionary<int, SkillData> MakeDict()
    {
        Dictionary<int, SkillData> dic = new Dictionary<int, SkillData>();
        foreach (SkillData data in skillData)
            dic.Add(data.num, data);
        return dic;
    }
}
