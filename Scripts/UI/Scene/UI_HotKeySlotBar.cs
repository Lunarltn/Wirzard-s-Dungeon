using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HotKeySlotBar : UI_Scene
{
    class SlotGroup
    {
        public Image ImageIcon;
        public Image ImageCooldown;
        public Image ImageBorder;
        public Image ImageKey;
        public TextMeshProUGUI TextCooldown;
        public TextMeshProUGUI TextKey;
        public TextMeshProUGUI TextNum;
    }

    enum Images
    {
        Image_CrossLine
    }


    enum GameObjects
    {
        _SkillLayoutGroup,
        _UseItemLayoutGroup,
        _MenuLayoutGroup
    }

    enum UIGroupComponents
    {
        Image_Icon,
        Image_Cooldown,
        Image_Border,
        Image_Key,
        Text_Cooldown,
        Text_Key,
        Text_Num
    }

    SlotGroup[] _skillSloats;
    SlotGroup[] _useItemSlots;
    SlotGroup _dashSlot;

    readonly Color32[] _colorGray = new Color32[2]
    { new Color32(100, 100, 100, 255), new Color32(150, 150, 150, 255) };
    CancellationTokenSource _cancleTokenSource;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _cancleTokenSource = new CancellationTokenSource();

        BindImage(typeof(Images));
        BindObject(typeof(GameObjects));

        BindSkillSlot();
        BindUseItemSlot();
        BindRightDashSlot();
        BindDeligate();

        Managers.HotKey.UpdateAllUseItemInfo = UpdateAllUseItemInfo;
        return true;
    }

    #region Init
    void BindSkillSlot()
    {
        int skillSlotCount = 4;
        _skillSloats = new SlotGroup[skillSlotCount];
        for (int i = 0; i < skillSlotCount; i++)
        {
            _skillSloats[i] = new SlotGroup();

            var slot = GetObject((int)GameObjects._SkillLayoutGroup).transform.GetChild(i).gameObject;

            _skillSloats[i].ImageIcon = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Icon), true);
            _skillSloats[i].ImageIcon.gameObject.AddComponent<Slot_HotKey>().Init(Slot_HotKey.SlotType.Skill, i);

            _skillSloats[i].ImageCooldown = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Cooldown), true);
            _skillSloats[i].ImageCooldown.fillAmount = 0;

            _skillSloats[i].TextCooldown = Util.FindChild<TextMeshProUGUI>(slot, GetName(UIGroupComponents.Text_Cooldown), true);
            _skillSloats[i].TextCooldown.gameObject.SetActive(false);

            _skillSloats[i].ImageBorder = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Border), true);
            _skillSloats[i].ImageBorder.color = _colorGray[0];

            _skillSloats[i].ImageKey = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Key), true);
            _skillSloats[i].TextKey = Util.FindChild<TextMeshProUGUI>(slot, GetName(UIGroupComponents.Text_Key), true);

            _skillSloats[i].TextNum = Util.FindChild<TextMeshProUGUI>(slot, GetName(UIGroupComponents.Text_Num), true);
            _skillSloats[i].TextNum.gameObject.SetActive(false);
        }
        _skillSloats[0].ImageBorder.color = Color.white;
    }

    void BindUseItemSlot()
    {
        int itemSlotCount = 2;
        _useItemSlots = new SlotGroup[itemSlotCount];
        for (int i = 0; i < itemSlotCount; i++)
        {
            _useItemSlots[i] = new SlotGroup();

            var slot = GetObject((int)GameObjects._UseItemLayoutGroup).transform.GetChild(i).gameObject;

            _useItemSlots[i].ImageIcon = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Icon), true);
            _useItemSlots[i].ImageIcon.gameObject.AddComponent<Slot_HotKey>().Init(Slot_HotKey.SlotType.UseItem, i);

            _useItemSlots[i].ImageCooldown = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Cooldown), true);
            _useItemSlots[i].ImageCooldown.fillAmount = 0;

            _useItemSlots[i].TextCooldown = Util.FindChild<TextMeshProUGUI>(slot, GetName(UIGroupComponents.Text_Cooldown), true);
            _useItemSlots[i].TextCooldown.gameObject.SetActive(false);

            _useItemSlots[i].ImageBorder = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Border), true);
            _useItemSlots[i].ImageBorder.color = _colorGray[0];

            _useItemSlots[i].ImageKey = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Key), true);
            _useItemSlots[i].TextKey = Util.FindChild<TextMeshProUGUI>(slot, GetName(UIGroupComponents.Text_Key), true);

            _useItemSlots[i].TextNum = Util.FindChild<TextMeshProUGUI>(slot, GetName(UIGroupComponents.Text_Num), true);
            _useItemSlots[i].TextNum.gameObject.SetActive(false);

        }
    }

    void BindRightDashSlot()
    {
        _dashSlot = new SlotGroup();

        var slot = GetObject((int)GameObjects._UseItemLayoutGroup).transform.GetChild(2).gameObject;

        _dashSlot.ImageCooldown = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Cooldown), true);
        _dashSlot.ImageCooldown.fillAmount = 0;

        _dashSlot.TextCooldown = Util.FindChild<TextMeshProUGUI>(slot, GetName(UIGroupComponents.Text_Cooldown), true);
        _dashSlot.TextCooldown.gameObject.SetActive(false);

        _dashSlot.ImageBorder = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Border), true);
        _dashSlot.ImageBorder.color = Color.white;

        _dashSlot.ImageKey = Util.FindChild<Image>(slot, GetName(UIGroupComponents.Image_Key), true);
        _dashSlot.TextKey = Util.FindChild<TextMeshProUGUI>(slot, GetName(UIGroupComponents.Text_Key), true);
    }

    void BindDeligate()
    {
        Managers.HotKey.UpdateCurrentSkill += UpdateCurrentSkill;

        Managers.HotKey.BindSkillAction(UpdateSkillCooldown, UpdateSkillInfo);

        Managers.HotKey.DashSkillSlot.UpdateCooldownTime += UpdateDashSkillCooldown;

        Managers.HotKey.BindItemAction(UpdateItemCooldown, UpdateUseItemInfo);
        Managers.HotKey.UpdateAimingLineSprite += UpdateAimingLineSprite;
        Managers.HotKey.UpdateAimingLineAlpha += UpdateAimingLineAlpha;
    }

    string GetName(UIGroupComponents uINames) => Enum.GetName(typeof(UIGroupComponents), uINames);
    #endregion

    #region Event
    void UpdateCurrentSkill(int beforeSkill, int afterSkill)
    {
        _skillSloats[beforeSkill].ImageBorder.color = _colorGray[0];
        _skillSloats[afterSkill].ImageBorder.color = Color.white;
    }

    void UpdateSkillInfo(int idx, SkillData skillData)
    {
        if (skillData.num < 0)
        {
            _skillSloats[idx].TextNum.gameObject.SetActive(false);
            _skillSloats[idx].ImageIcon.sprite = Managers.Resource.Load<Sprite>("Sprite/Icon/Menu/SkillSlot");
            ;
        }
        else
        {
            _skillSloats[idx].TextNum.gameObject.SetActive(true);
            if (skillData.type == (int)Define.SkillType.ContinuousFront)
                _skillSloats[idx].TextNum.text = skillData.cost.ToString("0/s");
            else
                _skillSloats[idx].TextNum.text = skillData.cost.ToString("00");

            var path = Managers.Data.SkillDic[skillData.num].icon;
            var sprite = Managers.Resource.Load<Sprite>($"Sprite/Icon/Skill/{path}");
            _skillSloats[idx].ImageIcon.sprite = sprite;
        }
    }

    void UpdateAllUseItemInfo()
    {
        for (int i = 0; i < 2; i++)
        {
            var item = Managers.HotKey.GetUseItemSlot(i);
            if (item == null)
            {
                _useItemSlots[i].ImageIcon.sprite = Managers.Inventory.ItemSprite[Define.MainCategory.None][0];
                _useItemSlots[i].TextNum.gameObject.SetActive(false);
            }
            else
            {
                _useItemSlots[i].ImageIcon.sprite = Managers.Inventory.ItemSprite[Define.MainCategory.Use][item.Num];
                _useItemSlots[i].TextNum.gameObject.SetActive(true);
                _useItemSlots[i].TextNum.text = item.Count.ToString("00");
            }
        }
    }

    void UpdateUseItemInfo(int idx, Item item)
    {
        if (item == null || (item != null && item.Count <= 0))
        {
            _useItemSlots[idx].ImageIcon.sprite = Managers.Inventory.ItemSprite[Define.MainCategory.None][0];
            _useItemSlots[idx].TextNum.gameObject.SetActive(false);
        }
        else
        {
            _useItemSlots[idx].ImageIcon.sprite = Managers.Inventory.ItemSprite[Define.MainCategory.Use][item.Num];
            _useItemSlots[idx].TextNum.gameObject.SetActive(true);
            _useItemSlots[idx].TextNum.text = item.Count.ToString("00");
        }

    }

    void UpdateSkillCooldown(int idx, float cooldownTime, float cancleCooldownTime = 0)
    {
        float time = (cancleCooldownTime != 0 ? cancleCooldownTime : cooldownTime);
        UpdateCooldownImage(_skillSloats[idx], time).Forget();
    }

    void UpdateItemCooldown(int idx, float cooldownTime)
    {
        UpdateCooldownImage(_useItemSlots[idx], cooldownTime).Forget();
    }

    void UpdateDashSkillCooldown(int idx, float cooldownTime, float cancleCooldownTime = 0)
    {
        UpdateCooldownImage(_dashSlot, cooldownTime).Forget();
    }

    async UniTask UpdateCooldownImage(SlotGroup hotKeySlot, float cancleCooldownTime = 0)
    {
        float currentCooldownTime = cancleCooldownTime;
        hotKeySlot.TextCooldown.gameObject.SetActive(true);
        hotKeySlot.TextCooldown.text = (Mathf.RoundToInt(cancleCooldownTime)).ToString("00");
        while (currentCooldownTime > 0)
        {
            await UniTask.DelayFrame(1, cancellationToken: _cancleTokenSource.Token);
            currentCooldownTime -= Time.deltaTime;
            hotKeySlot.ImageCooldown.fillAmount = currentCooldownTime / cancleCooldownTime;
            hotKeySlot.TextCooldown.text = (Mathf.RoundToInt(currentCooldownTime)).ToString("00");
        }
        hotKeySlot.TextCooldown.gameObject.SetActive(false);
    }

    void UpdateAimingLineSprite(int index)
    {
        GetImage((int)Images.Image_CrossLine).sprite = Managers.HotKey.AimingLineSprites[index];
    }
    void UpdateAimingLineAlpha(float value)
    {
        Color color = GetImage((int)Images.Image_CrossLine).color;
        color.a = value;
        GetImage((int)Images.Image_CrossLine).color = color;
    }
    #endregion

    void CancleToken() { _cancleTokenSource.Cancel(); }
    void DisposeToken() { _cancleTokenSource.Dispose(); }

    void OnDestroy()
    {
        CancleToken();
        DisposeToken();
    }
}
