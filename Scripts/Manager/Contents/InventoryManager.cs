using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Item
{
    int num;
    int count;
    Define.MainCategory mainCategory;
    public Define.MainCategory MainCategory => mainCategory;
    public int Num => num;
    public int Count
    {
        get => count;
        set
        {
            count = value;
            Managers.Inventory.UpdateInventorySlot?.Invoke();
            if (mainCategory == Define.MainCategory.Use)
                Managers.HotKey.UpdateAllUseItemInfo?.Invoke();
        }
    }
    public Item(Define.MainCategory mainCategory, int num, int count)
    {
        this.mainCategory = mainCategory;
        this.num = num;
        this.count = count;
    }
    public Item(int num, int count)
    {
        this.num = num;
        this.count = count;
        mainCategory = (Define.MainCategory)(num / 1000);
    }
    public string GetName()
    {
        if (num == 0) return "°ñµå";

        switch (mainCategory)
        {
            case Define.MainCategory.Equip:
                return Managers.Data.EquipItemDic[num].name;
            case Define.MainCategory.Use:
                return Managers.Data.UseItemDic[num].name;
            case Define.MainCategory.Etc:
                return Managers.Data.EtcItemDic[num].name;
        }
        return string.Empty;
    }
    public bool Equals(Item item)
    {
        return num == item.num;
    }
}

public class InventoryManager
{
    Dictionary<Define.MainCategory, Item[]> _inventory;
    Dictionary<Define.EquipCategory, Item> _equipment;
    int _gold;
    public UI_InventoryAssistent UI_InventoryAssistent;
    public Dictionary<Define.MainCategory, Dictionary<int, Sprite>> ItemSprite { get; private set; }
    public Dictionary<Define.EquipCategory, Sprite> EquipmentBaseSprite { get; private set; }
    public Action UpdateInventorySlot;
    public Action UpdateGoldAmount;
    public Action UpdateEquipment;

    public void Init()
    {
        _inventory = new Dictionary<Define.MainCategory, Item[]>();
        _equipment = new Dictionary<Define.EquipCategory, Item>();

        for (int i = 1; i <= 3; i++)
        {
            var idx = (Define.MainCategory)i;
            _inventory.Add(idx, new Item[15]);
            for (int j = 0; j < _inventory[idx].Length; j++)
                _inventory[idx][j] = null;
        }

        for (int i = 1; i < Enum.GetValues(typeof(Define.EquipCategory)).Length; i++)
        {
            _equipment.Add((Define.EquipCategory)i, null);
        }

        //BindSprite
        BindItemSprite();
        BindEquipmentSprite();
    }

    #region BindSprite
    void BindItemSprite()
    {
        ItemSprite = new Dictionary<Define.MainCategory, Dictionary<int, Sprite>>();
        for (int i = 0; i <= 3; i++)
        {
            ItemSprite.Add((Define.MainCategory)i, new Dictionary<int, Sprite>());
            switch ((Define.MainCategory)i)
            {
                case Define.MainCategory.None:
                    {
                        var sprite = Managers.Resource.Load<Sprite>("Sprite/Icon/Menu/UseItemSlot");
                        ItemSprite[(Define.MainCategory)i].Add(0, sprite);
                    }
                    break;
                case Define.MainCategory.Equip:
                    foreach (var num in Managers.Data.EquipItemDic.Keys)
                    {
                        var path = Managers.Data.EquipItemDic[num].icon;
                        var sprite = Managers.Resource.Load<Sprite>($"Sprite/Icon/Item/{path}");
                        ItemSprite[(Define.MainCategory)i].Add(num, sprite);
                    }
                    break;
                case Define.MainCategory.Use:
                    foreach (var num in Managers.Data.UseItemDic.Keys)
                    {
                        var path = Managers.Data.UseItemDic[num].icon;
                        var sprite = Managers.Resource.Load<Sprite>($"Sprite/Icon/Item/{path}");
                        ItemSprite[(Define.MainCategory)i].Add(num, sprite);
                    }
                    break;
                case Define.MainCategory.Etc:
                    foreach (var num in Managers.Data.EtcItemDic.Keys)
                    {
                        var path = Managers.Data.EtcItemDic[num].icon;
                        var sprite = Managers.Resource.Load<Sprite>($"Sprite/Icon/Item/{path}");
                        ItemSprite[(Define.MainCategory)i].Add(num, sprite);
                    }
                    break;
            }
        }
    }

    void BindEquipmentSprite()
    {
        EquipmentBaseSprite = new Dictionary<Define.EquipCategory, Sprite>();
        for (int i = 1; i < Enum.GetValues(typeof(Define.EquipCategory)).Length; i++)
        {
            var path = Enum.GetName(typeof(Define.EquipCategory), (Define.EquipCategory)i);
            var sprite = Managers.Resource.Load<Sprite>($"Sprite/Icon/Menu/{path}");
            EquipmentBaseSprite.Add((Define.EquipCategory)i, sprite);
        }
    }
    #endregion

    public Item GetItem(Define.MainCategory category, int idx)
    {
        if (_inventory[category].Length > idx)
            return _inventory[category][idx];

        return null;
    }

    void SetItem(Define.MainCategory category, int idx, Item item)
    {
        if (_inventory[category].Length <= idx)
            return;
        _inventory[category][idx] = item;
        UpdateInventorySlot?.Invoke();
    }

    public bool AddItem(Item item)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(item.GetName());
        stringBuilder.Append(" ");
        stringBuilder.Append(item.Count);
        stringBuilder.Append("°³ È¹µæ!");
        Managers.InfoUI.ShowItemAlarm(stringBuilder.ToString());
        Managers.Quest.CountQuestRequest(Define.QuestCategory.Collect, item.Num, item.Count);
        if (item.Count > 0)
        {
            if (item.MainCategory == Define.MainCategory.Equip)
            {
                int count = item.Count;
                for (int i = 0; i < _inventory[item.MainCategory].Length; i++)
                {
                    if (count < 1)
                        break;
                    if (_inventory[item.MainCategory][i] == null)
                    {
                        SetItem(item.MainCategory, i, new Item(item.Num, 1));
                        count--;
                    }
                }
                return true;
            }
            else
            {
                int firstNullIndex = -1;
                for (int i = 0; i < _inventory[item.MainCategory].Length; i++)
                {
                    if (firstNullIndex < 0 && _inventory[item.MainCategory][i] == null)
                        firstNullIndex = i;
                    if (_inventory[item.MainCategory][i] != null && _inventory[item.MainCategory][i].Num == item.Num)
                    {
                        _inventory[item.MainCategory][i].Count += item.Count;
                        UpdateInventorySlot?.Invoke();
                        return true;
                    }
                }
                if (firstNullIndex >= 0)
                {
                    SetItem(item.MainCategory, firstNullIndex, item);
                    return true;
                }
            }
        }
        return false;
    }

    public void DropItem(Define.MainCategory category, int idx, int count = 1)
    {
        if (_inventory[category].Length > idx && _inventory[category][idx].Count > 0)
        {
            var item = _inventory[category][idx];
            if (item.Count - count > 0)
                item.Count -= count;
            else
                _inventory[category][idx] = null;
            UpdateInventorySlot?.Invoke();
        }
    }

    public int SearchItemIndex(Define.MainCategory category, int num)
    {
        for (int i = 0; i < _inventory[category].Length; i++)
            if (_inventory[category][i] != null && _inventory[category][i].Num == num)
                return i;

        return -1;
    }

    public int SearchItemCount(Define.MainCategory category, int num)
    {
        int count = 0;
        for (int i = 0; i < _inventory[category].Length; i++)
            if (_inventory[category][i].Num == num)
                count = _inventory[category][i].Count;

        return count;
    }

    public bool UseItem(Item item)
    {
        if (item == null || (item != null && item.Count <= 0)) return false;
        if (AvailableUseItem(item.Num))
        {
            item.Count--;
            return true;
        }
        return false;
    }

    public bool AvailableUseItem(int num)
    {
        if (Managers.Data.UseItemDic.ContainsKey(num) == false)
            return false;
        float[] stat = new float[5];
        stat[0] = Managers.Data.UseItemDic[num].hp;
        stat[1] = Managers.Data.UseItemDic[num].mp;
        stat[2] = Managers.Data.UseItemDic[num].damage;
        stat[3] = Managers.Data.UseItemDic[num].defense;
        stat[4] = Managers.Data.UseItemDic[num].speed;
        for (int i = 0; i < stat.Length; i++)
        {
            if (stat[i] == 0)
                continue;
            switch (i)
            {
                case 0:
                    {
                        int increment = Mathf.RoundToInt(Managers.PlayerInfo.Controller.Stat.HP * stat[0]);
                        return Managers.PlayerInfo.Controller.ReceiveRecoveryHP(increment);
                    }
                case 1:
                    {
                        int increment = Mathf.RoundToInt(Managers.PlayerInfo.Controller.Stat.MP * stat[1]);
                        return Managers.PlayerInfo.Controller.ReceiveRecoveryMP(increment);
                    }
            }
        }

        return false;
    }

    public void MoveItemAtEquipment(int idx, Define.EquipCategory equipCategory)
    {
        if (_inventory[Define.MainCategory.Equip].Length > idx)
        {
            var item = GetItem(Define.MainCategory.Equip, idx);
            if (_equipment[equipCategory] != null)
            {
                var equip = GetEquipment(equipCategory);
                SetEquipment(equipCategory, item);
                SetItem(Define.MainCategory.Equip, idx, equip);
            }
            else
            {
                SetEquipment(equipCategory, item);
                DropItem(Define.MainCategory.Equip, idx);
            }
        }
    }

    public void MoveItemAtInventory(Define.MainCategory category, int idx1, int idx2)
    {
        if (_inventory[category].Length > idx1 && _inventory[category].Length > idx2)
        {
            var item = GetItem(category, idx1);
            SetItem(category, idx1, GetItem(category, idx2));
            SetItem(category, idx2, item);
        }
    }

    public Item GetEquipment(Define.EquipCategory category) => _equipment[category];

    public void SetEquipment(Define.EquipCategory category, Item item)
    {
        Managers.PlayerInfo.Controller.Stat.UpdateStat(_equipment[category] != null ? _equipment[category].Num : 0, item != null ? item.Num : 0);
        _equipment[category] = item;
        if (category == Define.EquipCategory.Weapon)
            Managers.PlayerInfo.Controller.ChangeWeapon(item);
        UpdateEquipment?.Invoke();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(amount);
        stringBuilder.Append("°ñµå È¹µæ!");
        Managers.InfoUI.ShowItemAlarm(stringBuilder.ToString());

        _gold += amount;
        UpdateGoldAmount?.Invoke();
    }

    public bool UseGold(int amount)
    {
        if (_gold < amount || amount < 0)
        {
            Managers.InfoUI.ShowAlarm("°ñµå ºÎÁ·");
            return false;
        }

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(amount);
        stringBuilder.Append("°ñµå »ç¿ë");
        Managers.InfoUI.ShowItemAlarm(stringBuilder.ToString());

        _gold -= amount;
        UpdateGoldAmount?.Invoke();
        return true;
    }

    public int GetGold() { return _gold; }

    //Monster
    public void DropItem(string dropItem)
    {
        if (dropItem == string.Empty) return;
        int[] itemArray = Util.SplitItemString(dropItem);
        for (int i = 0; i < itemArray.Length / 2; i++)
        {
            if (itemArray[(i * 2)] == 0)
                Managers.Inventory.AddGold(itemArray[i * 2 + 1]);
            else
            {
                Item item = new Item(itemArray[i * 2], itemArray[i * 2 + 1]);
                Managers.Inventory.AddItem(item);
            }
        }
    }

}
