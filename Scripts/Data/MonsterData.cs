using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[Serializable]
public class MonsterData
{
    public int num;
    public string name;
    public int hp;
    public int damage;
    public int defense;
    public float moveSpeed;
    public float attackSpeed;
    public float attackDistance;
    public string dropItem;

}

[Serializable]
public class MonsterDataLoader : ILoader<int, MonsterData>
{
    public List<MonsterData> monsterData = new List<MonsterData>();

    public Dictionary<int, MonsterData> MakeDict()
    {
        Dictionary<int, MonsterData> dic = new Dictionary<int, MonsterData>();
        foreach (MonsterData data in monsterData)
            dic.Add(data.num, data);
        return dic;
    }
}
