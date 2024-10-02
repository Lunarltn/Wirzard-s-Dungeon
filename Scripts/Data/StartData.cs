using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StartData
{
    public string id;
    public int maxHp;
    public int defence;
    public int damage;
    public int attackRange;
    public float attackSpeed;
    public int moveSpeed;
    public int skillCoolTime;
}

[Serializable]
public class StartDataLoader : ILoader<string, StartData>
{
    public List<StartData> playerStatData = new List<StartData>();

    public Dictionary<string, StartData> MakeDict()
    {
        Dictionary<string, StartData> dic = new Dictionary<string, StartData>();

        foreach (StartData stat in playerStatData)
            dic.Add(stat.id, stat);
        return dic;
    }
}