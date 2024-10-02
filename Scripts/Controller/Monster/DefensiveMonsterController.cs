using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensiveMonsterController : MonsterController
{
    int _defenseCount = 0;
    public bool IsDefense;

    protected override void Init()
    {
        ID = 3;
        base.Init();
    }

    public override Damage TakeDamage(Damage damage)
    {
        Damage finalDamage = base.TakeDamage(damage);
        if (IsDefense)
            finalDamage.Value = Mathf.CeilToInt(finalDamage.Value * 0.5f);

        return finalDamage;
    }

    protected override void GetHit()
    {
        base.GetHit();

        if (IsDefense == false && _defenseCount++ >= 2)
        {
            _defenseCount = 0;
            IsDefense = true;
        }
    }
}
