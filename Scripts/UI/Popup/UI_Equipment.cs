using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Equipment : UI_Popup
{
    enum GameObjects
    {
        _EquipmentLayoutGroup,
        _StatInfoLayoutGroup
    }
    enum Images
    {
        Image_Panel,
        Image_WindowTab,
        Image_Close
    }

    const string T_V = "Text_Value";
    const string T_E = "Text_Effect";

    TextMeshProUGUI[] _statValue;
    TextMeshProUGUI[] _statEffect;
    Dictionary<Define.EquipCategory, Slot_EquipmentItem> _equipItem;
    CancellationTokenSource[] _cancleTokenSources;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        //BindEnum
        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        //BindMoveWindow
        BindEvent(GetImage((int)Images.Image_WindowTab).gameObject, DownMoveWindow, Define.UI_Event.Down);
        BindEvent(GetImage((int)Images.Image_WindowTab).gameObject, DragMoveWindow, Define.UI_Event.Drag);
        BindEvent(GetImage((int)Images.Image_Close).gameObject, ClickCloseWindow, Define.UI_Event.Click);
        //BindStat
        int statCount = GetObject((int)GameObjects._StatInfoLayoutGroup).transform.childCount;
        _statValue = new TextMeshProUGUI[statCount];
        _statEffect = new TextMeshProUGUI[statCount];
        for (int i = 0; i < statCount; i++)
        {
            _statValue[i] = Util.FindChild<TextMeshProUGUI>(GetObject((int)GameObjects._StatInfoLayoutGroup).transform.GetChild(i).gameObject, T_V, true);
            _statEffect[i] = Util.FindChild<TextMeshProUGUI>(GetObject((int)GameObjects._StatInfoLayoutGroup).transform.GetChild(i).gameObject, T_E, true);
            _statEffect[i].gameObject.SetActive(false);
        }
        _cancleTokenSources = new CancellationTokenSource[statCount];
        //BindEquip
        _equipItem = new Dictionary<Define.EquipCategory, Slot_EquipmentItem>();
        for (int i = 1; i <= GetObject((int)GameObjects._EquipmentLayoutGroup).transform.childCount; i++)
        {
            var item = GetObject((int)GameObjects._EquipmentLayoutGroup).transform.GetChild(i - 1).GetChild(0);
            _equipItem.Add((Define.EquipCategory)i, item.gameObject.AddComponent<Slot_EquipmentItem>());
            _equipItem[(Define.EquipCategory)i].Init((Define.EquipCategory)i);

            BindEvent(_equipItem[(Define.EquipCategory)i].gameObject, UpEquipment, Define.UI_Event.Up);
            BindEvent(_equipItem[(Define.EquipCategory)i].gameObject, DragEquipment, Define.UI_Event.Drag);
            BindEvent(_equipItem[(Define.EquipCategory)i].gameObject, DownEquipment, Define.UI_Event.Down);
            BindEvent(_equipItem[(Define.EquipCategory)i].gameObject, EnterEquipment, Define.UI_Event.Enter);
            BindEvent(_equipItem[(Define.EquipCategory)i].gameObject, EndDragEquipment, Define.UI_Event.EndDrag);
            BindEvent(_equipItem[(Define.EquipCategory)i].gameObject, ExitEquipment, Define.UI_Event.Exit);
        }
        //Init
        UpdateStatValue();
        UpdateEquipmentSlot();
        Managers.PlayerInfo.Controller.Stat.UpdateEquipmentStat += UpdateStatValue;
        Managers.PlayerInfo.Controller.Stat.PlayEquipmentStatEffect += PlayStatEffect;
        Managers.Inventory.UpdateEquipment += UpdateEquipmentSlot;

        return true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateStatValue();
        UpdateEquipmentSlot();
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
        ClosePopupUI<UI_Equipment>(KeyCode.E);
    }
    #endregion

    #region Stat
    void UpdateStatValue()
    {
        _statValue[0].text = Managers.PlayerInfo.Controller.Stat.HP.ToString();
        _statValue[1].text = Managers.PlayerInfo.Controller.Stat.MP.ToString();
        _statValue[2].text = Managers.PlayerInfo.Controller.Stat.Damage.ToString();
        _statValue[3].text = Managers.PlayerInfo.Controller.Stat.Defense.ToString();
        _statValue[4].text = Managers.PlayerInfo.Controller.Stat.MoveSpeed.ToString();
    }

    void PlayStatEffect(float[] stat)
    {
        for (int i = 0; i < stat.Length; i++)
        {
            if (stat[i] != 0)
            {
                if (stat[i] > 0)
                    _statEffect[i].color = Color.green;
                else if (stat[i] < 0)
                    _statEffect[i].color = Color.red;
                _statEffect[i].text = stat[i].ToString("+#;-#;0");
                _statEffect[i].gameObject.SetActive(true);
                TextEffect(_statValue[i].transform.position, i).Forget();
            }
        }
    }

    async UniTask TextEffect(Vector3 position, int idx)
    {
        _cancleTokenSources[idx] = new CancellationTokenSource();
        await DOTween.Sequence()
        .OnStart(() =>
        {
            _statEffect[idx].transform.position = position;
            _statEffect[idx].alpha = 1;
        })
        .Append(_statEffect[idx].transform.DOMove(position + (Vector3.up * 40), 1f))
        .Join(_statEffect[idx].DOFade(0, 1f))
        .WithCancellation(_cancleTokenSources[idx].Token);
        _statEffect[idx].gameObject.SetActive(false);
    }

    #endregion

    #region Equipment
    void UpdateEquipmentSlot()
    {
        for (int i = 1; i < Enum.GetValues(typeof(Define.EquipCategory)).Length; i++)
        {
            if (Managers.Inventory.GetEquipment((Define.EquipCategory)i) == null)
            {
                _equipItem[(Define.EquipCategory)i].Image.sprite = Managers.Inventory.EquipmentBaseSprite[(Define.EquipCategory)i];
                _equipItem[(Define.EquipCategory)i].IsNull = true;
            }
            else
            {
                int idx = Managers.Inventory.GetEquipment((Define.EquipCategory)i).Num;
                _equipItem[(Define.EquipCategory)i].Image.sprite = Managers.Inventory.ItemSprite[Define.MainCategory.Equip][idx];
                _equipItem[(Define.EquipCategory)i].IsNull = false;

            }
            _equipItem[(Define.EquipCategory)i].SetImageColor();
        }
    }

    Vector2 _mousePosition;
    Slot_EquipmentItem _selectEquip;

    public void DownEquipment(PointerEventData evt)
    {
        var taget = evt.pointerEnter;

        if (taget == null)
            return;

        _selectEquip = taget.GetComponent<Slot_EquipmentItem>();

        if (_selectEquip == null || _selectEquip.IsNull)
            return;

        _mousePosition = evt.position;

        Managers.Inventory.UI_InventoryAssistent.SetDragSprite(_selectEquip.Image.sprite);
        Managers.Inventory.UI_InventoryAssistent.RectPosition = _mousePosition;
    }

    public void DragEquipment(PointerEventData evt)
    {
        if (_selectEquip == null || _selectEquip.IsNull)
            return;

        var offset = evt.position - _mousePosition;
        _mousePosition = evt.position;
        Managers.Inventory.UI_InventoryAssistent.RectPosition += (Vector3)offset;
        Managers.Inventory.UI_InventoryAssistent.CloseItemInfo();
    }

    public void UpEquipment(PointerEventData evt)
    {
        Managers.Inventory.UI_InventoryAssistent.DisabledImage();

        var taget = evt.pointerEnter;
        Define.EquipCategory category = _selectEquip != null ? _selectEquip.Category : Define.EquipCategory.None;

        if (taget == null || category == Define.EquipCategory.None)
            return;
        var _selectItem = taget.GetComponent<Slot_InventoryItem>();
        if (_selectItem != null && _selectItem.category == Define.MainCategory.Equip)
        {
            Managers.Inventory.MoveItemAtEquipment(_selectItem.Index, _selectEquip.Category);
        }
    }

    public void EndDragEquipment(PointerEventData evt)
    {
        var taget = evt.pointerEnter;
        if (taget != null)
        {
            if (OpenItemInfo(taget.GetComponent<Slot_EquipmentItem>()) == false)
                OpenItemInfo(taget.GetComponent<Slot_InventoryItem>());
        }
    }

    bool OpenItemInfo(Slot_InventoryItem inventoryItem)
    {
        if (inventoryItem == null || inventoryItem.IsNull)
            return false;
        Managers.Inventory.UI_InventoryAssistent.OpenItemInfo(inventoryItem.category, Managers.Inventory.GetItem(Define.MainCategory.Equip, inventoryItem.Index).Num);
        return true;
    }

    bool OpenItemInfo(Slot_EquipmentItem equipmentItem)
    {
        if (equipmentItem == null || equipmentItem.IsNull)
            return false;
        Managers.Inventory.UI_InventoryAssistent.OpenItemInfo(Define.MainCategory.Equip, Managers.Inventory.GetEquipment(equipmentItem.Category).Num);
        return true;
    }

    public void EnterEquipment(PointerEventData evt)
    {
        var taget = evt.pointerEnter;
        if (taget != null)
        {
            if (!evt.dragging)
            {
                var component = taget.GetComponent<Slot_EquipmentItem>();
                if (component == null || component.IsNull)
                    return;
                Managers.Inventory.UI_InventoryAssistent.OpenItemInfo(Define.MainCategory.Equip, Managers.Inventory.GetEquipment(component.Category).Num);
            }
        }
    }

    public void ExitEquipment(PointerEventData evt)
    {
        Managers.Inventory.UI_InventoryAssistent.CloseItemInfo();
    }
    #endregion

    public void CancleToken(int idx) { _cancleTokenSources[idx]?.Cancel(); }
    public void DisposeToken(int idx) { _cancleTokenSources[idx]?.Dispose(); }

    void OnDestroy()
    {
        for (int i = 0; i < _cancleTokenSources.Length; i++)
        {
            if (_cancleTokenSources[i] == null)
                continue;
            CancleToken(i);
            DisposeToken(i);
        }
    }
}
