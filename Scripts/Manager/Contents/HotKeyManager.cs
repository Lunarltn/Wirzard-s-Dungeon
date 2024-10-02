using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class HotKeySlot
{
    protected int _index;
    protected float _currentCooldownTimer;
    public bool IsCooldown => _currentCooldownTimer > 0;
    protected CancellationTokenSource _cancleTokenSource;

    public abstract void RunCooldown(float time = 0);
    public void CancleToken() { _cancleTokenSource?.Cancel(); }
    public void DisposeToken() { _cancleTokenSource?.Dispose(); }
}

public class SkillHotKeySlot : HotKeySlot
{
    SkillData _skillData;
    public SkillData SkillData => _skillData;
    public Action<int, float, float> UpdateCooldownTime;
    //UI갱신
    public Action<int, SkillData> UpdateSkillInfo;
    public bool IsNull => _skillData.num < 0;

    public SkillHotKeySlot(int index = -1)
    {
        _index = index;
        _skillData = new SkillData();
    }

    public void SetSkill(SkillData skillData)
    {
        _skillData = skillData;
        UpdateSkillInfo?.Invoke(_index, _skillData);
    }

    public void DeleteSkill()
    {
        _skillData = new SkillData();
        UpdateSkillInfo?.Invoke(_index, _skillData);
    }

    public override void RunCooldown(float cancleCooldownTime = 0)
    {
        UpdateCooldownTime?.Invoke(_index, _skillData.cooldownTime, cancleCooldownTime);
        if (cancleCooldownTime == 0)
            PlayCooldown(_skillData.cooldownTime).Forget();
        else
            PlayCooldown(cancleCooldownTime).Forget();
    }

    async UniTask PlayCooldown(float time)
    {
        _cancleTokenSource = new CancellationTokenSource();
        _currentCooldownTimer = time;
        while (_currentCooldownTimer > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: _cancleTokenSource.Token);
            _currentCooldownTimer -= 0.1f;
        }
    }
}

public class UseItemHotKeySlot : HotKeySlot
{
    Item _item;
    public Action<int, float> UpdateCooldownTime;
    public Action<int, Item> UpdateUseItemInfo;

    public UseItemHotKeySlot(int idx)
    {
        _index = idx;
    }

    public void SetUseItem(Item item)
    {
        _item = item;
        UpdateUseItemInfo?.Invoke(_index, _item);
    }

    public Item GetUseItem() => _item;

    public void DeleteUseItem()
    {
        _item = null;
        UpdateUseItemInfo?.Invoke(_index, _item);
    }

    public bool Equals(Item item)
    {
        if (_item == null || item == null) return false;

        return _item.Equals(item);
    }

    public void UseItem()
    {
        if (Managers.Inventory.UseItem(_item))
        {
            RunCooldown(2);
            UpdateUseItemInfo?.Invoke(_index, _item);
        }
    }

    public override void RunCooldown(float time = 0)
    {
        UpdateCooldownTime?.Invoke(_index, time);
        PlayCooldown(time).Forget();
    }

    async UniTask PlayCooldown(float time)
    {
        _cancleTokenSource = new CancellationTokenSource();
        _currentCooldownTimer = time;
        while (_currentCooldownTimer > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: _cancleTokenSource.Token);
            _currentCooldownTimer -= 0.1f;
        }
    }

}

public class HotKeyManager
{
    #region AimingLine
    const string COOLTIME_MESSAGE = "쿨타임이 끝난 후 다시 시도해주세요.";
    public List<Sprite> AimingLineSprites { get; private set; }
    public Action<int> UpdateAimingLineSprite;
    public Action<float> UpdateAimingLineAlpha;
    int _currentAimingLineIndex;
    float _currentAimingLineAlpha = 1;

    public int CurrentAimingLineIndex
    {
        get { return _currentAimingLineIndex; }
        set
        {
            if (_currentAimingLineIndex == value) return;
            _currentAimingLineIndex = value;
            UpdateAimingLineSprite?.Invoke(value);
        }
    }

    public float CurrentAimingLineAlpha
    {
        get { return _currentAimingLineAlpha; }
        set
        {
            if (_currentAimingLineAlpha == value) return;
            _currentAimingLineAlpha = value;
            UpdateAimingLineAlpha?.Invoke(value);
        }
    }
    #endregion
    SkillHotKeySlot _dashSkillSlot;
    SkillHotKeySlot[] _skillSlot;
    UseItemHotKeySlot[] _useItemSlot;
    int _currentSkillIndex;
    public Action<int, int> UpdateCurrentSkill;
    public Action UpdateAllUseItemInfo;
    public SkillHotKeySlot DashSkillSlot => _dashSkillSlot;

    public void Init()
    {
        _dashSkillSlot = new SkillHotKeySlot();
        _dashSkillSlot.SetSkill(Managers.Data.SkillDic[(int)Define.SkillName.Dash]);

        _skillSlot = new SkillHotKeySlot[4];
        for (int i = 0; i < _skillSlot.Length; i++)
            _skillSlot[i] = new SkillHotKeySlot(i);

        _useItemSlot = new UseItemHotKeySlot[2];
        for (int i = 0; i < _useItemSlot.Length; i++)
            _useItemSlot[i] = new UseItemHotKeySlot(i);

        AimingLineSprites = new List<Sprite>();
        for (int i = 0; ; i++)
        {
            var sprite = Managers.Resource.Load<Sprite>($"Sprite/CrossLine/CrossLine{i}");
            if (sprite == null)
                break;
            AimingLineSprites.Add(sprite);
        }
    }

    #region Skill
    public void BindSkillAction(Action<int, float, float> coolDown, Action<int, SkillData> skillInfo)
    {
        for (int i = 0; i < _skillSlot.Length; i++)
        {
            _skillSlot[i].UpdateCooldownTime += coolDown;
            _skillSlot[i].UpdateSkillInfo += skillInfo;
        }
    }

    public SkillHotKeySlot CurrentSkillSlot
    {
        get
        {
            return _skillSlot[_currentSkillIndex];
        }
        set
        {
            _skillSlot[_currentSkillIndex] = value;
        }
    }

    public bool IsNullCurrentSkillSlot() { return IsNullSkillSlot(_currentSkillIndex); }
    public bool IsNullSkillSlot(int idx) => _skillSlot[idx].IsNull;

    public void ChangeCurrentSkill(int idx)
    {
        if (idx >= _skillSlot.Length || _skillSlot[idx] == null)
            return;
        if (idx != _currentSkillIndex)
        {
            UpdateCurrentSkill?.Invoke(_currentSkillIndex, idx);
            _currentSkillIndex = idx;
        }
    }

    public void SetSkill(int idx, SkillData skillData)
    {
        if (idx < _skillSlot.Length)
        {
            if (_skillSlot[idx].IsCooldown)
            {
                Managers.InfoUI.ShowAlarm(COOLTIME_MESSAGE);
                return;
            }

            int beforeSkillIndex = -1;
            SkillData swapSkillData = new SkillData();
            for (int i = 0; i < _skillSlot.Length; i++)
            {
                if (_skillSlot[i].SkillData.num == skillData.num)
                {
                    if (_skillSlot[i].IsCooldown)
                    {
                        Managers.InfoUI.ShowAlarm(COOLTIME_MESSAGE);
                        return;
                    }
                    beforeSkillIndex = i;
                }

                if (_skillSlot[i].SkillData.num > 0 && i == idx)
                    swapSkillData = _skillSlot[i].SkillData;
            }
            if (swapSkillData.num > 0 && beforeSkillIndex != -1)
                _skillSlot[beforeSkillIndex].SetSkill(swapSkillData);
            if (beforeSkillIndex != -1)
                _skillSlot[beforeSkillIndex].DeleteSkill();
            _skillSlot[idx].SetSkill(skillData);
        }
    }

    public bool DeleteSkill(Define.SkillName skillName)
    {
        for (int i = 0; i < _skillSlot.Length; i++)
        {
            if (_skillSlot[i].SkillData.num == (int)skillName)
            {
                if (_skillSlot[i].IsCooldown)
                {
                    Managers.InfoUI.ShowAlarm(COOLTIME_MESSAGE);
                    return false;
                }
                _skillSlot[i].DeleteSkill();
                break;
            }
        }
        return true;
    }
    #endregion

    #region UseItem
    public void BindItemAction(Action<int, float> coolDown, Action<int, Item> itemInfo)
    {
        for (int i = 0; i < _useItemSlot.Length; i++)
        {
            _useItemSlot[i].UpdateCooldownTime += coolDown;
            _useItemSlot[i].UpdateUseItemInfo += itemInfo;
        }
    }

    public void SetUseItemSlot(int idx, Item item)
    {
        if (item.MainCategory != Define.MainCategory.Use)
            return;

        for (int i = 0; i < _useItemSlot.Length; i++)
            if (_useItemSlot[i].Equals(item) && i != idx)
                _useItemSlot[i].DeleteUseItem();

        _useItemSlot[idx].SetUseItem(item);
    }

    public Item GetUseItemSlot(int idx) => _useItemSlot[idx].GetUseItem();

    public void UseItem(int idx)
    {
        if (_useItemSlot[idx].IsCooldown)
            return;
        _useItemSlot[idx].UseItem();
    }
    #endregion

    public void Clear()
    {
        _dashSkillSlot.CancleToken();
        _dashSkillSlot.DisposeToken();
        for (int i = 0; i < _skillSlot.Length; i++)
        {
            _skillSlot[i].CancleToken();
            _skillSlot[i].DisposeToken();
            if (i < _useItemSlot.Length)
            {
                _useItemSlot[i].CancleToken();
                _useItemSlot[i].DisposeToken();
            }
        }
    }
}
