using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_Skill : UI_Popup
{
    class MenuGroup
    {
        public Image ImageColumn;
        public Image ImageIcon;
        public TextMeshProUGUI TextType;
        public TextMeshProUGUI TextName;
        public TextMeshProUGUI TextLevel;
        public Image ImageUp;
        public Image ImageDown;
        public Image ImageScreen;
    }

    enum GameObjects
    {
        _MenuLayoutGroup
    }

    enum Images
    {
        Image_Panel,
        Image_WindowTab,
        Image_Close,
        Image_ScrollBar,
        Image_Slider,
        Image_DragIconMask,
        Image_DragIcon
    }

    enum Texts
    {
        Text_Point,
        Text_Cost,
        Text_SkillInfo
    }

    enum UIGroupComponents
    {
        Image_Icon,
        Text_Type,
        Text_Name,
        Text_Level,
        Image_Up,
        Image_Down,
        Image_KeySetting,
        Image_Screen
    }

    Dictionary<Define.SkillName, MenuGroup> _menus;
    RectTransform _menuLGRT;
    float _menusSize;
    float _menusRange;
    float _scrollRange;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        //BindEnum
        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        BindTMP(typeof(Texts));

        BindMenu();
        BindDelegate();
        InitMenuVariable();
        _menuLGRT = GetObject((int)GameObjects._MenuLayoutGroup).GetComponent<RectTransform>();
        InitScrollBar();
        GetImage((int)Images.Image_DragIconMask).gameObject.SetActive(false);

        //BindWindow
        BindEvent(GetImage((int)Images.Image_WindowTab).gameObject, DownMoveWindow, Define.UI_Event.Down);
        BindEvent(GetImage((int)Images.Image_WindowTab).gameObject, DragMoveWindow, Define.UI_Event.Drag);
        BindEvent(GetImage((int)Images.Image_Close).gameObject, ClickCloseWindow, Define.UI_Event.Click);
        //BindScroll
        BindEvent(GetImage((int)Images.Image_Slider).gameObject, DownSlider, Define.UI_Event.Down);
        BindEvent(GetImage((int)Images.Image_Slider).gameObject, DragSlider, Define.UI_Event.Drag);
        //Init
        UpdateSkillPoint();
        return true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GetImage((int)Images.Image_DragIconMask).gameObject.SetActive(false);
    }

    void Update()
    {
        if (IsIncludeMouse() && Input.mouseScrollDelta.y != 0)
            MoveSlider(Input.mouseScrollDelta.y * (-_scrollRange * 0.1f));
    }

    #region Init
    void BindMenu()
    {
        int skillCount = Managers.Data.SkillDic.Count - 1;
        var menuCopy = GetObject((int)GameObjects._MenuLayoutGroup).transform.GetChild(0).gameObject;

        for (int i = 0; i < skillCount - 1; i++)
        {
            Instantiate(menuCopy, GetObject((int)GameObjects._MenuLayoutGroup).transform);
        }
        _menus = new Dictionary<Define.SkillName, MenuGroup>();
        for (int i = 0; i < skillCount; i++)
        {
            Define.SkillName skillName = (Define.SkillName)(i + 1);
            _menus[skillName] = new MenuGroup();
            var menu = GetObject((int)GameObjects._MenuLayoutGroup).transform.GetChild(i).gameObject;

            _menus[skillName].ImageColumn = menu.GetComponent<Image>();
            BindEvent(_menus[skillName].ImageColumn.gameObject, ClickColumn, Define.UI_Event.Click);

            _menus[skillName].ImageIcon = Util.FindChild<Image>(menu, GetName(UIGroupComponents.Image_Icon), true);
            var path = Managers.Data.SkillDic[(int)skillName].icon;
            var sprite = Managers.Resource.Load<Sprite>($"Sprite/Icon/Skill/{path}");
            _menus[skillName].ImageIcon.sprite = sprite;
            BindEvent(_menus[skillName].ImageIcon.gameObject, DownIcon, Define.UI_Event.Down);
            BindEvent(_menus[skillName].ImageIcon.gameObject, DragIcon, Define.UI_Event.Drag);
            BindEvent(_menus[skillName].ImageIcon.gameObject, UpIcon, Define.UI_Event.Up);
            BindEvent(_menus[skillName].ImageIcon.gameObject, ClickIcon, Define.UI_Event.Click);

            _menus[skillName].TextType = Util.FindChild<TextMeshProUGUI>(menu, GetName(UIGroupComponents.Text_Type), true);
            _menus[skillName].TextType.text = GetSkillTypeName((Define.SkillType)Managers.Data.SkillDic[(int)skillName].type);

            _menus[skillName].TextName = Util.FindChild<TextMeshProUGUI>(menu, GetName(UIGroupComponents.Text_Name), true);
            _menus[skillName].TextName.text = Managers.Data.SkillDic[(int)skillName].name;

            _menus[skillName].TextLevel = Util.FindChild<TextMeshProUGUI>(menu, GetName(UIGroupComponents.Text_Level), true);
            _menus[skillName].TextLevel.text = Managers.PlayerInfo.Skill.Level[(Define.SkillName)skillName].ToString();

            _menus[skillName].ImageScreen = Util.FindChild<Image>(menu, GetName(UIGroupComponents.Image_Screen), true);

            _menus[skillName].ImageDown = Util.FindChild<Image>(menu, GetName(UIGroupComponents.Image_Down), true);
            BindEvent(_menus[skillName].ImageDown.gameObject, ClickDownSkillLevel, Define.UI_Event.Click);

            _menus[skillName].ImageUp = Util.FindChild<Image>(menu, GetName(UIGroupComponents.Image_Up), true);
            BindEvent(_menus[skillName].ImageUp.gameObject, ClickUpSkillLevel, Define.UI_Event.Click);
        }
    }

    void BindDelegate()
    {
        Managers.PlayerInfo.Skill.UpdateSkillPoint += UpdateSkillPoint;
        Managers.PlayerInfo.Skill.UpdateSkillLevel += UpdateSkillLevel;
    }

    void InitMenuVariable()
    {
        var spcing = GetObject((int)GameObjects._MenuLayoutGroup).GetComponent<GridLayoutGroup>().spacing.y;
        var singleSize = GetObject((int)GameObjects._MenuLayoutGroup).GetComponent<GridLayoutGroup>().cellSize.y;
        _menusSize = ((singleSize + spcing) * _menus.Count) - spcing;
        _menusRange = _menusSize - GetObject((int)GameObjects._MenuLayoutGroup).GetComponent<RectTransform>().sizeDelta.y;
    }

    void InitScrollBar()
    {
        var scrollSize = GetImage((int)Images.Image_ScrollBar).rectTransform.sizeDelta.y;
        var sliderWidth = GetImage((int)Images.Image_Slider).rectTransform.sizeDelta.x;
        GetImage((int)Images.Image_Slider).rectTransform.sizeDelta = new Vector2(sliderWidth, scrollSize * (scrollSize / _menusSize));

        _scrollRange = GetImage((int)Images.Image_Slider).rectTransform.sizeDelta.y - GetImage((int)Images.Image_ScrollBar).rectTransform.sizeDelta.y;
    }

    string GetName(UIGroupComponents uINames) => Enum.GetName(typeof(UIGroupComponents), uINames);
    string GetSkillTypeName(Define.SkillType skillType)
    {
        switch (skillType)
        {
            case Define.SkillType.None:
                return null;
            case Define.SkillType.SingleFront:
                return "사출형";
            case Define.SkillType.SingleCasting:
                return "지점형";
            case Define.SkillType.ContinuousFront:
                return "방출형";
            case Define.SkillType.ContinuousCasting:
                return "주문형";
        }
        return null;
    }
    #endregion

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
        ClosePopupUI<UI_Skill>(KeyCode.K);
    }
    #endregion

    #region Scroll
    float _sliderVeticalPosition;
    void DownSlider(PointerEventData evt)
    {
        _sliderVeticalPosition = evt.position.y;
    }

    void DragSlider(PointerEventData evt)
    {
        var offset = evt.position.y - _sliderVeticalPosition;
        _sliderVeticalPosition = evt.position.y;
        MoveSlider(offset);
    }

    bool IsIncludeMouse()
    {
        var panelPos = GetImage((int)Images.Image_Panel).rectTransform.position;
        var panelSize = GetImage((int)Images.Image_Panel).rectTransform.sizeDelta;
        var mousePos = Input.mousePosition;
        return (mousePos.x > panelPos.x - (panelSize.x * 0.5f) && mousePos.x < panelPos.x + (panelSize.x * 0.5f) &&
            mousePos.y > panelPos.y - (panelSize.y * 0.5f) && mousePos.y < panelPos.y + (panelSize.y * 0.5f));
    }

    void MoveSlider(float value)
    {
        var posY = GetImage((int)Images.Image_Slider).rectTransform.anchoredPosition.y;
        if (posY + value < 0 && posY + value > _scrollRange)
            GetImage((int)Images.Image_Slider).rectTransform.anchoredPosition += new Vector2(0, value);
        else if (posY + value >= 0)
            GetImage((int)Images.Image_Slider).rectTransform.anchoredPosition = new Vector2(0, 0);
        else if (posY + value <= _scrollRange)
            GetImage((int)Images.Image_Slider).rectTransform.anchoredPosition = new Vector2(0, _scrollRange);

        _menuLGRT.anchoredPosition = new Vector2(0, _menusRange * (posY / _scrollRange));
    }
    #endregion

    void UpdateSkillPoint()
    {
        GetTMP((int)Texts.Text_Point).text = Managers.PlayerInfo.Skill.CurrentSkillPoint.ToString() + "/" + Managers.PlayerInfo.Skill.TotalSkillPoint.ToString();
    }
    const string MAX = "<color=red>Max</color>";
    void UpdateSkillLevel(Define.SkillName skillName, int skillLevel)
    {
        if (skillLevel < 10)
            _menus[skillName].TextLevel.text = skillLevel.ToString();
        else if (skillLevel == 10)
            _menus[skillName].TextLevel.text = MAX;
        if (skillLevel > 0 && _menus[skillName].ImageScreen.gameObject.activeSelf)
            _menus[skillName].ImageScreen.gameObject.SetActive(false);
        else if (skillLevel == 0)
            _menus[skillName].ImageScreen.gameObject.SetActive(true);
    }

    void UpdateSkillInfo(Define.SkillName skillName)
    {
        if (Managers.Data.SkillDic[(int)skillName].type == (int)Define.SkillType.ContinuousFront)
            GetTMP((int)Texts.Text_Cost).text = Managers.Data.SkillDic[(int)skillName].cost.ToString("0/s");
        else
            GetTMP((int)Texts.Text_Cost).text = Managers.Data.SkillDic[(int)skillName].cost.ToString("00");
        int damage = Managers.PlayerInfo.Skill.GetSkillDamage(skillName);
        float duration = Managers.Data.SkillDic[(int)skillName].duration;
        var result = Managers.Data.SkillDic[(int)skillName].comment
            .Replace("{damage}", damage.ToString("<color=red>0</color>"))
            .Replace("{duration}", duration.ToString("<color=green>0</color>"));
        GetTMP((int)Texts.Text_SkillInfo).text = result;
    }

    void ClickUpSkillLevel(PointerEventData evt)
    {
        foreach (var key in _menus.Keys)
        {
            if (ReferenceEquals(evt.pointerEnter, _menus[key].ImageUp.gameObject))
            {
                Managers.PlayerInfo.Skill.UpSkillLevel(key);
                UpdateSkillInfo(key);
                break;
            }
        }
    }

    void ClickDownSkillLevel(PointerEventData evt)
    {
        foreach (var key in _menus.Keys)
        {
            if (ReferenceEquals(evt.pointerEnter, _menus[key].ImageDown.gameObject))
            {
                Managers.PlayerInfo.Skill.DownSkillLevel(key);
                UpdateSkillInfo(key);
                break;
            }
        }
    }

    Vector2 _mousePosition;
    Define.SkillName _dragSkill;
    void DownIcon(PointerEventData evt)
    {
        foreach (var key in _menus.Keys)
        {
            if (ReferenceEquals(evt.pointerEnter, _menus[key].ImageIcon.gameObject))
            {
                if (Managers.PlayerInfo.Skill.Level[key] > 0)
                {
                    _dragSkill = key;
                    GetImage((int)Images.Image_DragIconMask).gameObject.SetActive(true);
                    var path = Managers.Data.SkillDic[(int)key].icon;
                    var sprite = Managers.Resource.Load<Sprite>($"Sprite/Icon/Skill/{path}");
                    GetImage((int)Images.Image_DragIcon).sprite = sprite;
                    GetImage((int)Images.Image_DragIconMask).rectTransform.position = evt.position;
                    _mousePosition = evt.position;
                }
                break;
            }
        }
    }

    void DragIcon(PointerEventData evt)
    {
        if (GetImage((int)Images.Image_DragIconMask).gameObject.activeSelf)
        {
            var offset = evt.position - _mousePosition;
            _mousePosition = evt.position;
            GetImage((int)Images.Image_DragIconMask).rectTransform.anchoredPosition += offset;
        }
    }

    void UpIcon(PointerEventData evt)
    {
        if (GetImage((int)Images.Image_DragIconMask).gameObject.activeSelf)
        {
            GetImage((int)Images.Image_DragIconMask).gameObject.SetActive(false);
            var target = evt.pointerEnter;
            if (target == null)
                return;
            var hotKeySlot = target.GetComponent<Slot_HotKey>();
            if (hotKeySlot != null && hotKeySlot.Type == Slot_HotKey.SlotType.Skill)
            {
                Managers.HotKey.SetSkill(hotKeySlot.Index, Managers.Data.SkillDic[(int)_dragSkill]);
            }
        }
    }

    void ClickIcon(PointerEventData evt)
    {
        foreach (var key in _menus.Keys)
        {
            if (ReferenceEquals(evt.pointerEnter, _menus[key].ImageIcon.gameObject))
            {
                UpdateSkillInfo(key);
                break;
            }
        }
    }

    void ClickColumn(PointerEventData evt)
    {
        foreach (var key in _menus.Keys)
        {
            if (ReferenceEquals(evt.pointerEnter, _menus[key].ImageColumn.gameObject))
            {
                UpdateSkillInfo(key);
                break;
            }
        }
    }
}
