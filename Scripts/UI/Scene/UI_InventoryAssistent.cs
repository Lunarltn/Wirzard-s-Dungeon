using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;

public class UI_InventoryAssistent : UI_Scene
{
    enum GameObjects
    {
        _HPGroup,
        _MPGroup,
        _DamageGroup,
        _DefenseGroup,
        _SpeedGroup
    }
    enum Images
    {
        Image_ItemIcon,
        Image_ItemInfo,
        Image_Icon
    }
    enum Texts
    {
        Text_Name,
        Text_Category,
        Text_Comment
    }

    public Vector3 RectPosition
    {
        set
        {
            GetImage((int)Images.Image_ItemIcon).rectTransform.position = value;
        }
        get
        {
            return GetImage((int)Images.Image_ItemIcon).rectTransform.position;
        }
    }

    const string T_V = "Text_Value";
    CancellationTokenSource _cancleTokenSource;
    TextMeshProUGUI[] _statValue;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindTMP(typeof(Texts));
        BindObject(typeof(GameObjects));

        GetImage((int)Images.Image_ItemIcon).gameObject.SetActive(false);
        GetImage((int)Images.Image_ItemInfo).gameObject.SetActive(false);
        _statValue = new TextMeshProUGUI[Enum.GetValues(typeof(GameObjects)).Length];
        for (int i = 0; i < Enum.GetValues(typeof(GameObjects)).Length; i++)
        {
            _statValue[i] = Util.FindChild<TextMeshProUGUI>(GetObject(i), T_V, true);
            GetObject(i).SetActive(false);
        }

        return true;
    }

    #region ItemIcon
    public void SetDragSprite(Sprite sprite)
    {
        GetImage((int)Images.Image_ItemIcon).gameObject.SetActive(true);
        GetImage((int)Images.Image_ItemIcon).sprite = sprite;
    }

    public void DisabledImage()
    {
        GetImage((int)Images.Image_ItemIcon).gameObject.SetActive(false);
    }
    #endregion

    #region ItemInfo
    Vector2 _mousePosition;

    public void OpenItemInfo(Define.MainCategory category, int num)
    {
        GetImage((int)Images.Image_ItemInfo).gameObject.SetActive(true);

        SetItemInfo(category, num);
        SetItemStatInfo(category, num);

        _mousePosition = (Vector2)Input.mousePosition;
        GetImage((int)Images.Image_ItemInfo).rectTransform.position = _mousePosition;
        UpdatePositionItemInfo().Forget();
    }

    void SetItemInfo(Define.MainCategory category, int num)
    {
        GetTMP((int)Texts.Text_Category).text = Enum.GetName(typeof(Define.MainCategory), category);
        GetImage((int)Images.Image_Icon).sprite = Managers.Inventory.ItemSprite[category][num];
        switch (category)
        {
            case Define.MainCategory.Equip:
                GetTMP((int)Texts.Text_Name).text = Managers.Data.EquipItemDic[num].name;
                GetTMP((int)Texts.Text_Comment).text = Managers.Data.EquipItemDic[num].comment;
                break;
            case Define.MainCategory.Use:
                GetTMP((int)Texts.Text_Name).text = Managers.Data.UseItemDic[num].name;
                GetTMP((int)Texts.Text_Comment).text = Managers.Data.UseItemDic[num].comment;
                break;
            case Define.MainCategory.Etc:
                GetTMP((int)Texts.Text_Name).text = Managers.Data.EtcItemDic[num].name;
                GetTMP((int)Texts.Text_Comment).text = Managers.Data.EtcItemDic[num].comment;
                break;
        }
    }

    void SetItemStatInfo(Define.MainCategory category, int num)
    {
        float[] stat = new float[Enum.GetValues(typeof(GameObjects)).Length];
        switch (category)
        {
            case Define.MainCategory.Equip:
                stat[0] = Managers.Data.EquipItemDic[num].hp;
                stat[1] = Managers.Data.EquipItemDic[num].mp;
                stat[2] = Managers.Data.EquipItemDic[num].damage;
                stat[3] = Managers.Data.EquipItemDic[num].defense;
                stat[4] = Managers.Data.EquipItemDic[num].speed;
                break;
            case Define.MainCategory.Use:
                stat[0] = Managers.Data.UseItemDic[num].hp;
                stat[1] = Managers.Data.UseItemDic[num].mp;
                stat[2] = Managers.Data.UseItemDic[num].damage;
                stat[3] = Managers.Data.UseItemDic[num].defense;
                stat[4] = Managers.Data.UseItemDic[num].speed;
                break;
        }

        for (int i = 0; i < Enum.GetValues(typeof(GameObjects)).Length; i++)
        {
            if (stat[i] > 0)
            {
                GetObject(i).SetActive(true);
                if (stat[i] < 1)
                    _statValue[i].text = (stat[i]).ToString("0%");
                else
                    _statValue[i].text = stat[i].ToString();
            }
            else
            {
                GetObject(i).SetActive(false);
            }
        }
    }

    async UniTask UpdatePositionItemInfo()
    {
        _cancleTokenSource = new CancellationTokenSource();
        while (true)
        {
            var offset = Input.mousePosition - (Vector3)_mousePosition;
            _mousePosition = Input.mousePosition;

            if (offset != Vector3.zero)
            {
                GetImage((int)Images.Image_ItemInfo).rectTransform.position += offset;
            }
            await UniTask.NextFrame(_cancleTokenSource.Token);
        }
    }

    public void CloseItemInfo()
    {
        if (GetImage((int)Images.Image_ItemInfo).gameObject.activeSelf == false)
            return;
        CancleToken();
        GetImage((int)Images.Image_ItemInfo).gameObject.SetActive(false);
    }
    #endregion

    void OnDestroy()
    {
        CancleToken();
        DisposeToken();
    }

    void CancleToken() { _cancleTokenSource?.Cancel(); }
    void DisposeToken() { _cancleTokenSource?.Dispose(); }
}
