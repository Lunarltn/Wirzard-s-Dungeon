using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Alarm : UI_Scene
{
    enum GameObjects
    {
        _Interection,
        _ItemAlarm
    }

    enum Images
    {
        Image_Alarm,
    }

    enum Texts
    {
        Text_Alarm,
        Text_InterectionKey,
        Text_InterectionContents,
        Text_InterectionName
    }
    const int ItemAlarmCount = 4;

    Sequence _alarmSequence;
    Sequence _interectionSequence;
    Color _alarmColor;
    RectTransform _interectionRectTransform;
    CanvasGroup _interectionCanvasGroup;

    Sequence[] _itemAlarmSequence = new Sequence[ItemAlarmCount];
    Image[] _itemAlarmImages = new Image[ItemAlarmCount];
    TextMeshProUGUI[] _itemAlarmTexts = new TextMeshProUGUI[ItemAlarmCount];

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindTMP(typeof(Texts));
        BindImage(typeof(Images));

        GetImage((int)Images.Image_Alarm).gameObject.SetActive(false);
        _alarmColor = GetImage((int)Images.Image_Alarm).color;
        InitItemAlarm();

        GetTMP((int)Texts.Text_InterectionKey).text = "F";
        _interectionRectTransform = GetObject((int)GameObjects._Interection).GetComponent<RectTransform>();
        _interectionCanvasGroup = GetObject((int)GameObjects._Interection).GetComponent<CanvasGroup>();
        GetObject((int)GameObjects._Interection).SetActive(false);
        return true;
    }

    void InitItemAlarm()
    {
        var itemAlarm = GetObject((int)GameObjects._ItemAlarm).transform;
        for (int i = 0; i < itemAlarm.childCount; i++)
        {
            _itemAlarmImages[i] = itemAlarm.GetChild(i).GetComponent<Image>();
            _itemAlarmTexts[i] = itemAlarm.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();
            _itemAlarmImages[i].color = Color.clear;
            _itemAlarmTexts[i].color = Color.clear;
        }
    }

    public void ShowAlarm(string text)
    {
        GetTMP((int)Texts.Text_Alarm).text = text;
        AnimationAlarm();
    }

    public void ShowItemAlarm(string text)
    {
        int index = 0;
        for (int i = 0; i < ItemAlarmCount; i++)
        {
            if (_itemAlarmImages[i].rectTransform.anchoredPosition.y == 150)
            {
                _itemAlarmImages[i].rectTransform.anchoredPosition = Vector2.zero;
                index = i;
            }
            else
                _itemAlarmImages[i].rectTransform.anchoredPosition += Vector2.up * 50;
        }
        _itemAlarmTexts[index].text = text;
        AnimationItemAlarm(index);
    }

    void AnimationAlarm()
    {
        _alarmSequence?.Kill();

        _alarmSequence = DOTween.Sequence()
            .OnStart(() =>
            {
                GetImage((int)Images.Image_Alarm).gameObject.SetActive(true);
                GetImage((int)Images.Image_Alarm).rectTransform.rotation = Quaternion.Euler(90, 0, 0);
                GetTMP((int)Texts.Text_Alarm).color = new Color(1, 1, 1, 0.2f);
                GetImage((int)Images.Image_Alarm).color = _alarmColor;
            })
            .Append(GetImage((int)Images.Image_Alarm).rectTransform.DORotate(Vector3.zero, 0.8f))
            .Join(GetTMP((int)Texts.Text_Alarm).DOColor(Color.white, 0.8f))
            .AppendInterval(1)
            .Append(GetImage((int)Images.Image_Alarm).DOColor(new Color(_alarmColor.r, _alarmColor.g, _alarmColor.b, 0), 1))
            .Join(GetTMP((int)Texts.Text_Alarm).DOColor(new Color(1, 1, 1, 0), 0.5f))
            .OnComplete(() =>
            {
                GetImage((int)Images.Image_Alarm).gameObject.SetActive(false);
            });
    }

    void AnimationItemAlarm(int index)
    {
        _itemAlarmSequence[index]?.Kill();
        _itemAlarmSequence[index] = DOTween.Sequence()
            .OnStart(() =>
            {
                _itemAlarmImages[index].color = new Color(0, 0, 0, 0.2f);
                _itemAlarmTexts[index].color = new Color(1, 1, 1, 0.2f);
            })
            .Append(_itemAlarmImages[index].DOColor(new Color(0, 0, 0, 0.8f), 1f))
            .Join(_itemAlarmTexts[index].DOColor(Color.white, 1f))
            .AppendInterval(2)
            .Append(_itemAlarmImages[index].DOColor(Color.clear, 0.5f))
            .Join(_itemAlarmTexts[index].DOColor(Color.clear, 0.5f));
    }

    public void ShowInterection(string name, string contents)
    {
        GetTMP((int)Texts.Text_InterectionName).text = name;
        GetTMP((int)Texts.Text_InterectionContents).text = contents;
        GetObject((int)GameObjects._Interection).SetActive(true);
        // AnimationShowInterection();
    }

    public void HideInterection()
    {
        GetObject((int)GameObjects._Interection).SetActive(false);
        // AnimationHideInterection();
    }
    bool _isShow;
    bool _isHide;
    void AnimationShowInterection()
    {
        if (_isShow) return;
        _interectionSequence?.Kill();
        _interectionSequence = DOTween.Sequence()
            .OnStart(() =>
            {
                _isShow = true;
                GetObject((int)GameObjects._Interection).SetActive(true);
                _interectionRectTransform.anchoredPosition = new Vector2(0, 20);
                _interectionCanvasGroup.alpha = 0.2f;
            })
            .Append(_interectionRectTransform.DOAnchorPos(new Vector2(0, 100), 0.8f))
            .Join(DOTween.To(() => 0.2f, x => _interectionCanvasGroup.alpha = x, 1, 0.8f))
            .SetAutoKill(false)
            .OnKill(() =>
            {
                _isShow = false;
            });
    }

    void AnimationHideInterection()
    {
        if (_isHide) return;
        _interectionSequence?.Kill();
        _interectionSequence = DOTween.Sequence()
            .OnStart(() =>
            {
                _isHide = true;
            })
            .Append(_interectionRectTransform.DOAnchorPos(new Vector2(0, 20), 0.4f))
            .Join(DOTween.To(() => _interectionCanvasGroup.alpha, x => _interectionCanvasGroup.alpha = x, 0, 0.3f))
            .OnKill(() =>
            {
                _isHide = false;
                GetObject((int)GameObjects._Interection).SetActive(false);
            });
    }
}
