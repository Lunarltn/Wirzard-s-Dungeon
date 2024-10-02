using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Inventory : UI_Popup
{
    public enum GameObjects
    {
        _CategoryTapLayoutGroup,
        _ItemLayoutGroup,
    }
    public enum Images
    {
        Image_Panel,
        Image_WindowTab,
        Image_Close,
    }
    public enum Texts
    {
        Text_Gold
    }

    Define.MainCategory _currentCategory;
    Define.MainCategory currentCategory
    {
        set
        {
            if (_currentCategory != value)
            {
                _currentCategory = value;
                UpdateItemSlot();
            }
        }
    }
    //Equip,Use,Etc
    Image[] _categoryTaps = new Image[3];
    Image[] _itemSlots = new Image[15];
    Slot_InventoryItem[] _item = new Slot_InventoryItem[15];

    readonly Color32 _gray = new Color32(150, 150, 150, 255);

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        //BindEnum
        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        BindTMP(typeof(Texts));
        //BindWindow
        BindEvent(GetImage((int)Images.Image_WindowTab).gameObject, DownMoveWindow, Define.UI_Event.Down);
        BindEvent(GetImage((int)Images.Image_WindowTab).gameObject, DragMoveWindow, Define.UI_Event.Drag);
        BindEvent(GetImage((int)Images.Image_Close).gameObject, ClickCloseWindow, Define.UI_Event.Click);
        //BindCategoryTap
        for (int i = 0; i < _categoryTaps.Length; i++)
        {
            var child = GetObject((int)GameObjects._CategoryTapLayoutGroup).transform.GetChild(i);
            _categoryTaps[i] = child.GetComponent<Image>();
            if (i == 0)
                BindEvent(_categoryTaps[i].gameObject, ClickEquipCategoryTap, Define.UI_Event.Click);
            else if (i == 1)
                BindEvent(_categoryTaps[i].gameObject, ClickUseCategoryTap, Define.UI_Event.Click);
            else if (i == 2)
                BindEvent(_categoryTaps[i].gameObject, ClickEtcCategoryTap, Define.UI_Event.Click);
        }
        //BindItemSlot
        for (int i = 0; i < _itemSlots.Length; i++)
        {
            var itemSlot = GetObject((int)GameObjects._ItemLayoutGroup).transform.GetChild(i);
            _itemSlots[i] = itemSlot.GetComponent<Image>();
            var item = itemSlot.GetChild(0);
            _item[i] = item.gameObject.AddComponent<Slot_InventoryItem>();
            _item[i].Init(i);

            BindEvent(_item[i].gameObject, DragItem, Define.UI_Event.Drag);
            BindEvent(_item[i].gameObject, DownItem, Define.UI_Event.Down);
            BindEvent(_item[i].gameObject, UpItem, Define.UI_Event.Up);
            BindEvent(_item[i].gameObject, EnterItem, Define.UI_Event.Enter);
            BindEvent(_item[i].gameObject, ExitItem, Define.UI_Event.Exit);
            BindEvent(_item[i].gameObject, EndDragItem, Define.UI_Event.EndDrag);
        }
        //Init
        _currentCategory = Define.MainCategory.Equip;
        UpdateCategoryTapColor(_currentCategory);
        UpdateItemSlot();
        UpdateGoldAmount();
        Managers.Inventory.UpdateInventorySlot = UpdateItemSlot;
        Managers.Inventory.UpdateGoldAmount = UpdateGoldAmount;

        return true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateItemSlot();
    }

    #region Window
    Vector2 _windowTabPosition;
    public void DownMoveWindow(PointerEventData evt)
    {
        Managers.UI.SortPopup(gameObject);
        _windowTabPosition = evt.position;
    }

    public void DragMoveWindow(PointerEventData evt)
    {
        var offset = evt.position - _windowTabPosition;
        _windowTabPosition = evt.position;
        GetImage((int)Images.Image_Panel).rectTransform.anchoredPosition += offset;
    }

    public void ClickCloseWindow(PointerEventData evt)
    {
        ClosePopupUI<UI_Inventory>(KeyCode.I);
    }
    #endregion

    #region CategoryTap
    public void ClickEquipCategoryTap(PointerEventData evt)
    {
        currentCategory = Define.MainCategory.Equip;
        UpdateCategoryTapColor(_currentCategory);
    }

    public void ClickUseCategoryTap(PointerEventData evt)
    {
        currentCategory = Define.MainCategory.Use;
        UpdateCategoryTapColor(_currentCategory);
    }

    public void ClickEtcCategoryTap(PointerEventData evt)
    {
        currentCategory = Define.MainCategory.Etc;
        UpdateCategoryTapColor(_currentCategory);
    }

    void UpdateCategoryTapColor(Define.MainCategory category)
    {

        for (int i = 0; i < 3; i++)
        {
            if (i == (int)category - 1) _categoryTaps[i].color = Color.white;
            else _categoryTaps[i].color = _gray;
        }
    }
    #endregion

    #region ItemSlot
    public void UpdateItemSlot()
    {
        for (int i = 0; i < _itemSlots.Length; i++)
        {
            var item = Managers.Inventory.GetItem(_currentCategory, i);
            _item[i].category = _currentCategory;
            if (item != null && item.Count > 0)
            {
                UpdateItemImage(i, item.Num);
                UpdateItemCount(i, item.Count);
                _item[i].IsNull = false;
            }
            else if (item == null || (item != null && item.Count <= 0))
            {
                _item[i].IsNull = true;
            }
        }
    }

    /*public void UpdateItemSlot(Define.MainCategory category, int idx)
    {
        if (_currentCategory != category)
            return;
        var item = Managers.Inventory.GetItem(category, idx);
        if (item != null)
        {
            UpdateItemImage(idx, item.Num);
            UpdateItemCount(idx, item.Count);
            _item[idx].IsNull = false;
        }
        else
        {
            _item[idx].IsNull = true;
        }
        _item[idx].SetImageColor();
    }*/

    void UpdateItemCount(int idx, int count)
    {
        if (_currentCategory == Define.MainCategory.Equip)
            _item[idx].Count.gameObject.SetActive(false);
        else
        {
            _item[idx].Count.gameObject.SetActive(true);
            _item[idx].Count.text = count.ToString();
        }
    }

    void UpdateItemImage(int idx, int num)
    {
        switch (_currentCategory)
        {
            case Define.MainCategory.Equip:
                _item[idx].Image.sprite = Managers.Inventory.ItemSprite[Define.MainCategory.Equip][num];
                break;
            case Define.MainCategory.Use:
                _item[idx].Image.sprite = Managers.Inventory.ItemSprite[Define.MainCategory.Use][num];
                break;
            case Define.MainCategory.Etc:
                _item[idx].Image.sprite = Managers.Inventory.ItemSprite[Define.MainCategory.Etc][num];
                break;
        }
    }

    Vector2 _mousePosition;
    Slot_InventoryItem _selectItem;

    public void DownItem(PointerEventData evt)
    {
        var taget = evt.pointerEnter;

        if (taget == null)
            return;

        _selectItem = taget.GetComponent<Slot_InventoryItem>();

        if (_selectItem == null || _selectItem.IsNull)
            return;

        _mousePosition = evt.position;

        Managers.Inventory.UI_InventoryAssistent.SetDragSprite(_selectItem.Image.sprite);
        Managers.Inventory.UI_InventoryAssistent.RectPosition = _mousePosition;
    }

    public void DragItem(PointerEventData evt)
    {
        if (_selectItem == null || _selectItem.IsNull)
            return;

        var offset = evt.position - _mousePosition;
        _mousePosition = evt.position;
        Managers.Inventory.UI_InventoryAssistent.RectPosition += (Vector3)offset;
        Managers.Inventory.UI_InventoryAssistent.CloseItemInfo();
    }

    public void UpItem(PointerEventData evt)
    {
        Managers.Inventory.UI_InventoryAssistent.DisabledImage();

        var taget = evt.pointerEnter;
        int idx = _selectItem != null ? _selectItem.Index : -1;

        if (taget == null || idx == -1)
            return;
        //DragEndInventory
        _selectItem = taget.GetComponent<Slot_InventoryItem>();
        if (_selectItem != null)
        {
            if (idx != _selectItem.Index)
            {
                Managers.Inventory.MoveItemAtInventory(_currentCategory, idx, _selectItem.Index);
            }
        }
        //DragEndEquipment
        else if (_currentCategory == Define.MainCategory.Equip)
        {
            var equipmentItem = taget.GetComponent<Slot_EquipmentItem>();
            if (equipmentItem != null)
            {
                var currentItem = Managers.Inventory.GetItem(Define.MainCategory.Equip, idx);
                if ((int)equipmentItem.Category == Managers.Data.EquipItemDic[currentItem.Num].equipCategory)
                {
                    Managers.Inventory.MoveItemAtEquipment(idx, equipmentItem.Category);
                }
            }
        }
        //DragEndHotKey
        else if (_currentCategory == Define.MainCategory.Use)
        {
            var hotKeySlot = taget.GetComponent<Slot_HotKey>();
            if (hotKeySlot != null && hotKeySlot.Type == Slot_HotKey.SlotType.UseItem)
            {
                var currentItem = Managers.Inventory.GetItem(Define.MainCategory.Use, idx);
                Managers.HotKey.SetUseItemSlot(hotKeySlot.Index, currentItem);
            }
        }
    }

    public void EndDragItem(PointerEventData evt)
    {
        var taget = evt.pointerEnter;
        if (taget != null)
        {
            if (OpenItemInfo(taget.GetComponent<Slot_InventoryItem>()) == false)
                OpenItemInfo(taget.GetComponent<Slot_EquipmentItem>());
        }
    }

    bool OpenItemInfo(Slot_InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.IsNull)
            return false;
        Managers.Inventory.UI_InventoryAssistent.OpenItemInfo(_currentCategory, Managers.Inventory.GetItem(_currentCategory, inventoryItem.Index).Num);
        return true;
    }

    bool OpenItemInfo(Slot_EquipmentItem equipmentItem)
    {
        if (equipmentItem == null || equipmentItem.IsNull)
            return false;
        Managers.Inventory.UI_InventoryAssistent.OpenItemInfo(Define.MainCategory.Equip, Managers.Inventory.GetEquipment(equipmentItem.Category).Num);
        return true;
    }

    public void EnterItem(PointerEventData evt)
    {
        var taget = evt.pointerEnter;
        if (taget != null)
        {
            if (!evt.dragging)
            {
                var component = taget.GetComponent<Slot_InventoryItem>();
                if (component == null || component.IsNull)
                    return;
                Managers.Inventory.UI_InventoryAssistent.OpenItemInfo(_currentCategory, Managers.Inventory.GetItem(_currentCategory, component.Index).Num);
            }
        }
    }

    public void ExitItem(PointerEventData evt)
    {
        Managers.Inventory.UI_InventoryAssistent.CloseItemInfo();
    }
    #endregion

    public void UpdateGoldAmount()
    {
        GetTMP((int)Texts.Text_Gold).text = Managers.Inventory.GetGold().ToString("N0");
    }
}


