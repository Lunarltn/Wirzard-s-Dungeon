using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_QuestWindow : UI_Scene
{
    enum Texts
    {
        Text_Name,
        Text_Contents,
        Text_Reward,
        Text_Answer1,
        Text_Answer2,
        Text_NPCName
    }

    enum Images
    {
        Image_Background
    }

    enum CanvasGroups
    {
        _TextGroup
    }

    Sequence _questWindowSequence;
    RectTransform _questWindowRectTransform;
    float _questWindowMaxHigh = 700;
    float _questWindowMinHigh = 0;
    float _questWindowWight;
    Action _actionAccept;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindTMP((typeof(Texts)));
        BindImage((typeof(Images)));
        Bind<CanvasGroup>(typeof(CanvasGroups));

        BindEvent(GetTMP((int)Texts.Text_Answer1).gameObject, ClikcAccept, Define.UI_Event.Click);
        BindEvent(GetTMP((int)Texts.Text_Answer2).gameObject, ClikcClose, Define.UI_Event.Click);

        _questWindowRectTransform = GetImage((int)Images.Image_Background).GetComponent<RectTransform>();
        _questWindowWight = _questWindowRectTransform.sizeDelta.x;
        GetImage((int)Images.Image_Background).gameObject.SetActive(false);
        Get<CanvasGroup>(0);

        return true;
    }

    public void ShowQuestWindow(int questID)
    {
        var questData = Managers.Data.QuestDic[questID];
        GetTMP((int)Texts.Text_Name).text = questData.name;
        GetTMP((int)Texts.Text_Contents).text = questData.description;
        GetTMP((int)Texts.Text_NPCName).text = Managers.Data.NPCDic[questData.npc].name;

        var rewards = Util.SplitItemString(questData.rewards);
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < rewards.Length / 2; i++)
        {
            Item item = new Item(rewards[i * 2], rewards[i * 2 + 1]);
            stringBuilder.Append(item.GetName() + " " + item.Count + "°³");
            if (i < (rewards.Length / 2) - 1)
                stringBuilder.Append(", ");
        }
        GetTMP((int)Texts.Text_Reward).text = "º¸»ó : " + stringBuilder;
        SetAnswer(questID);
        AnimationShowQuestWindow();
    }

    void SetAnswer(int questID)
    {
        if (Managers.Data.QuestDic[questID].category == 0)
        {
            GetTMP((int)Texts.Text_Answer1).text = "";
            GetTMP((int)Texts.Text_Answer2).text = "´Ý±â";
            _questID = 0;
        }
        else
        {
            GetTMP((int)Texts.Text_Answer1).text = "¼ö¶ô";
            GetTMP((int)Texts.Text_Answer2).text = "´Ý±â";
            _questID = questID;
        }
    }

    void HideQuestWindow()
    {
        AnimationHideQuestWindow();
    }

    void AnimationShowQuestWindow()
    {
        _questWindowSequence?.Kill();
        _questWindowSequence = DOTween.Sequence()
            .OnStart(() =>
            {
                GetImage((int)Images.Image_Background).gameObject.SetActive(true);
                _questWindowRectTransform.sizeDelta = new Vector2(_questWindowWight, _questWindowMinHigh);
                Get<CanvasGroup>(0).alpha = 0;

            })
            .Append(_questWindowRectTransform.DOSizeDelta(new Vector2(_questWindowWight, _questWindowMaxHigh), 0.3f))
            .Insert(0.05f, DOTween.To(() => Get<CanvasGroup>(0).alpha, x => Get<CanvasGroup>(0).alpha = x, 1, 0.1f));
    }

    void AnimationHideQuestWindow()
    {
        _questWindowSequence?.Kill();
        _questWindowSequence = DOTween.Sequence()
            .Append(_questWindowRectTransform.DOSizeDelta(new Vector2(_questWindowWight, _questWindowMinHigh), 0.3f))
            .Insert(0.05f, DOTween.To(() => Get<CanvasGroup>(0).alpha, x => Get<CanvasGroup>(0).alpha = x, 0, 0.1f))
            .OnKill(() =>
            {
                GetImage((int)Images.Image_Background).gameObject.SetActive(false);
            });
    }
    int _questID;
    void ClikcAccept(PointerEventData evt)
    {
        Managers.Quest.AcceptQuest(_questID);
        HideQuestWindow();
    }

    void ClikcClose(PointerEventData evt)
    {
        Managers.Quest.CloseQuestWindow();
        HideQuestWindow();
    }
}

