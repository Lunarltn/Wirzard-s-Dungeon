using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStat
{
    public string Name { get; protected set; }
    public int HP { get; protected set; }
    public int Damage { get; protected set; }
    public int Defense { get; protected set; }
    public float MoveSpeed { get; protected set; }
    public float AttackSpeed { get; protected set; }
    public int CurrentHP { get; protected set; }

    public virtual bool DecreaseHP(int value)
    {
        var hp = CurrentHP - value;
        if (hp <= 0)
        {
            CurrentHP = 0;
            return false;
        }
        CurrentHP = hp;
        return true;
    }
    public virtual bool IncreaseHP(int value)
    {
        if (CurrentHP == HP)
            return false;
        CurrentHP += value;
        if (CurrentHP > HP)
            CurrentHP = HP;
        return true;
    }

    public virtual Damage GetFinalDamage(Damage damage)
    {
        damage.Value = damage.Value - Defense;
        if (damage.IsCritical)
        {
            damage.Value *= 2;
        }

        return damage;
    }
}

public class NPCStat : BaseStat
{
    public NPCStat(int num)
    {
        LoadStat(Managers.Data.NPCDic[num]);

        CurrentHP = HP;
    }

    void LoadStat(NPCData statData)
    {
        Name = statData.name;
        HP = statData.hp;
        Damage = statData.damage;
        Defense = statData.defense;
        MoveSpeed = statData.moveSpeed;
        AttackSpeed = statData.attackSpeed;
    }
}

public class MonsterStat : BaseStat
{
    public float AttackDistance { get; protected set; }
    public string DropItem { get; protected set; }

    public MonsterStat(int num)
    {
        LoadStat(Managers.Data.MonsterDic[num]);

        CurrentHP = HP;
    }

    void LoadStat(MonsterData statData)
    {
        Name = statData.name;
        HP = statData.hp;
        Damage = statData.damage;
        Defense = statData.defense;
        MoveSpeed = statData.moveSpeed;
        AttackSpeed = statData.attackSpeed;
        AttackDistance = statData.attackDistance;
        DropItem = statData.dropItem;
    }
}

public class BossStat : MonsterStat
{
    const float SHIELD_AMOUNT = 0.2f;
    const float SHIELD_DAMAGE_REDUCTION = 0.5f;
    public int Shield { get; protected set; }
    public int CurrentShield { get; protected set; }
    public Action<int, int> UpdateHP;
    public Action<int, int> UpdateShield;

    public BossStat(int num) : base(num)
    {
        LoadStat(Managers.Data.MonsterDic[num]);

        CurrentHP = HP;
        Shield = Mathf.FloorToInt(CurrentHP * SHIELD_AMOUNT);
        CurrentShield = Shield;
        RechargeShield();
    }

    void LoadStat(MonsterData statData)
    {
        Name = statData.name;
        HP = statData.hp;
        Damage = statData.damage;
        Defense = statData.defense;
        MoveSpeed = statData.moveSpeed;
        AttackSpeed = statData.attackSpeed;
        AttackDistance = statData.attackDistance;
        DropItem = statData.dropItem;
    }

    public override bool DecreaseHP(int value)
    {
        if (base.DecreaseHP(value) == false)
        {
            UpdateHP?.Invoke(HP, CurrentHP);
            return false;
        }

        UpdateHP?.Invoke(HP, CurrentHP);
        return true;
    }

    public bool DecreaseShield(int value)
    {
        var shield = CurrentShield - value;
        if (shield <= 0)
        {
            CurrentShield = 0;
            UpdateShield?.Invoke(Shield, CurrentShield);
            return false;
        }
        CurrentShield = shield;
        UpdateShield?.Invoke(Shield, CurrentShield);
        return true;
    }

    public void RechargeShield()
    {
        CurrentShield = Shield;
        UpdateShield?.Invoke(Shield, CurrentShield);
    }

    public override Damage GetFinalDamage(Damage damage)
    {
        damage.Value = damage.Value - Defense;
        if (damage.IsCritical)
        {
            damage.Value *= 2;
        }
        if (CurrentShield > 0)
        {
            damage.Value = Mathf.RoundToInt(damage.Value * SHIELD_DAMAGE_REDUCTION);
        }
        return damage;
    }
}


public class PlayerStat : BaseStat
{
    public int MP { get; protected set; }
    public int CurrentMP { get; protected set; }
    public Action<int, int> UpdateHPStat;
    public Action<int, int> UpdateMPStat;
    public Action<int, int> UpdateCurrentHPStat;
    public Action<int, int> UpdateCurrentMPStat;
    public Action UpdateEquipmentStat;
    public Action<float[]> PlayEquipmentStatEffect;

    public PlayerStat()
    {
        LoadStat(Managers.Data.PlayerData);

        CurrentHP = HP;
        CurrentMP = MP;
    }

    void LoadStat(PlayerData playerData)
    {
        Name = playerData.name;
        HP = playerData.hp;
        MP = playerData.mp;
        Damage = playerData.damage;
        Defense = playerData.defense;
        MoveSpeed = playerData.moveSpeed;
        AttackSpeed = playerData.attackSpeed;
    }

    public override bool DecreaseHP(int value)
    {
        if (base.DecreaseHP(value) == false)
        {
            UpdateCurrentHPStat?.Invoke(HP, CurrentHP);
            return false;
        }

        UpdateCurrentHPStat?.Invoke(HP, CurrentHP);
        return true;
    }

    public override bool IncreaseHP(int value)
    {
        if (base.IncreaseHP(value) == false)
        {
            Managers.InfoUI.ShowAlarm("더 이상 회복할 수 없습니다.");
            return false;
        }
        UpdateCurrentHPStat?.Invoke(HP, CurrentHP);
        return true;
    }

    public bool DecreaseMP(int value)
    {
        var mp = CurrentMP - value;
        if (mp < 0)
        {
            Managers.InfoUI.ShowAlarm("마나가 부족합니다.");
            return false;
        }
        CurrentMP = mp;
        UpdateCurrentMPStat?.Invoke(MP, CurrentMP);
        return true;
    }

    public bool IncreaseMP(int value)
    {
        if (CurrentMP == MP)
        {
            Managers.InfoUI.ShowAlarm("더 이상 회복할 수 없습니다.");
            return false;
        }
        CurrentMP += value;
        if (CurrentMP > MP)
            CurrentMP = MP;
        UpdateCurrentMPStat?.Invoke(MP, CurrentMP);
        return true;
    }

    public void UpdateStat(int beforeNum, int afterNum)
    {
        float[] stat = new float[5];
        if (beforeNum != 0)
        {
            stat[0] -= Managers.Data.EquipItemDic[beforeNum].hp;
            stat[1] -= Managers.Data.EquipItemDic[beforeNum].mp;
            stat[2] -= Managers.Data.EquipItemDic[beforeNum].damage;
            stat[3] -= Managers.Data.EquipItemDic[beforeNum].defense;
            stat[4] -= Managers.Data.EquipItemDic[beforeNum].speed;
        }
        if (afterNum != 0)
        {
            stat[0] += Managers.Data.EquipItemDic[afterNum].hp;
            stat[1] += Managers.Data.EquipItemDic[afterNum].mp;
            stat[2] += Managers.Data.EquipItemDic[afterNum].damage;
            stat[3] += Managers.Data.EquipItemDic[afterNum].defense;
            stat[4] += Managers.Data.EquipItemDic[afterNum].speed;
        }
        HP += (int)stat[0];
        MP += (int)stat[1];
        Damage += (int)stat[2];
        Defense += (int)stat[3];
        MoveSpeed += stat[4];
        if (CurrentHP > HP) { CurrentHP = HP; }
        UpdateHPStat?.Invoke(HP, CurrentHP);
        UpdateMPStat?.Invoke(MP, CurrentMP);
        UpdateEquipmentStat?.Invoke();
        PlayEquipmentStatEffect?.Invoke(stat);
    }
}