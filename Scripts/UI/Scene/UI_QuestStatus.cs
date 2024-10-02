using Cysharp.Threading.Tasks;
using DG.Tweening;
using RootMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuestStatus : UI_Scene
{
    enum Images
    {
        Image_Background
    }
    enum Texts
    {
        Text_Caption,
        Text_Contents
    }
    enum CanvasGroups
    {
        CanvasGroup_Texts
    }

    Vector2 _backgroundImageSize;
    Color _backgroundImageColor;
    readonly float[] BackgroundImageHeight = new float[3] { 105, 135, 165 };
    const float BackgroundImageWidth = 450;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindTMP(typeof(Texts));
        Bind<CanvasGroup>(typeof(CanvasGroups));

        _backgroundImageSize = GetImage((int)Images.Image_Background).rectTransform.sizeDelta;
        _backgroundImageColor = GetImage((int)Images.Image_Background).color;
        GetImage((int)Images.Image_Background).gameObject.SetActive(false);
        Get<CanvasGroup>((int)CanvasGroups.CanvasGroup_Texts).alpha = 0;

        return true;
    }

    string GetContents(Quest quest)
    {
        string contents = string.Empty;

        if (quest.RequestCount == 0)
        {
            contents = Managers.Data.QuestDic[quest.ID].mission;
            GetImage((int)Images.Image_Background).rectTransform.sizeDelta =
                new Vector2(BackgroundImageWidth, BackgroundImageHeight[0]);
        }
        else
        {
            GetImage((int)Images.Image_Background).rectTransform.sizeDelta =
                new Vector2(BackgroundImageWidth, BackgroundImageHeight[quest.RequestCount - 1]);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < quest.RequestCount; i++)
            {
                if (quest.GetProgressRequirements(i) >= quest.GetRequirements(i))
                    stringBuilder.Append("<#FFFF00>");
                stringBuilder.Append(quest.GetRequestName(i));
                stringBuilder.Append(" ");
                stringBuilder.Append(quest.GetProgressRequirements(i));
                stringBuilder.Append("/");
                stringBuilder.Append(quest.GetRequirements(i));
                if (quest.GetProgressRequirements(i) >= quest.GetRequirements(i))
                    stringBuilder.Append("</color>");
                if (i != quest.RequestCount - 1)
                    stringBuilder.AppendLine();
            }
            contents = stringBuilder.ToString();
        }
        return contents;
    }

    public void UpdateQuestRequest(Quest quest)
    {
        string contents = GetContents(quest);
        GetTMP((int)Texts.Text_Contents).text = contents;
    }

    public void UpdateQuestStatus(Quest quest)
    {
        string caption = Managers.Data.QuestDic[quest.ID].name;
        string contents = GetContents(quest);

        GetTMP((int)Texts.Text_Caption).text = caption;
        GetTMP((int)Texts.Text_Contents).text = contents;

        AnimationUpdateQuestState(caption, contents).Forget();
    }

    public void ShowQuestStatus(Quest quest)
    {
        string caption = Managers.Data.QuestDic[quest.ID].name;
        string contents = GetContents(quest);

        GetTMP((int)Texts.Text_Caption).text = caption;
        GetTMP((int)Texts.Text_Contents).text = contents;

        AnimationShowQuestState().Forget();
    }

    public void HideQuestStatus()
    {
        AnimationHideQuestState().Forget();
    }

    async UniTask AnimationUpdateQuestState(string caption, string contents)
    {
        await DOTween.Sequence()
            .Append(DOTween.To(() => Get<CanvasGroup>((int)CanvasGroups.CanvasGroup_Texts).alpha, alpha => Get<CanvasGroup>((int)CanvasGroups.CanvasGroup_Texts).alpha = alpha, 0, 0.5f))
            .AppendCallback(() =>
            {
                GetTMP((int)Texts.Text_Caption).text = caption;
                GetTMP((int)Texts.Text_Contents).text = contents;
            })
            .Append(DOTween.To(() => Get<CanvasGroup>((int)CanvasGroups.CanvasGroup_Texts).alpha, alpha => Get<CanvasGroup>((int)CanvasGroups.CanvasGroup_Texts).alpha = alpha, 1, 0.5f));
    }


    async UniTask AnimationShowQuestState()
    {
        await DOTween.Sequence()
            .OnStart(() =>
            {
                GetImage((int)Images.Image_Background).gameObject.SetActive(true);
                Get<CanvasGroup>((int)CanvasGroups.CanvasGroup_Texts).alpha = 0;
                Color color = _backgroundImageColor;
                color.a = 0;
                GetImage((int)Images.Image_Background).color = color;
                GetImage((int)Images.Image_Background).rectTransform.sizeDelta = new Vector2(_backgroundImageSize.x, 0);
            })
            .Append(GetImage((int)Images.Image_Background).rectTransform.DOSizeDelta(_backgroundImageSize, 0.5f))
            .Join(GetImage((int)Images.Image_Background).DOColor(_backgroundImageColor, 0.5f))
            .Insert(0.2f, DOTween.To(() => 0, alpha => Get<CanvasGroup>((int)CanvasGroups.CanvasGroup_Texts).alpha = alpha, 1, 0.7f));
    }

    async UniTask AnimationHideQuestState()
    {
        Color color = _backgroundImageColor;
        color.a = 0;
        await DOTween.Sequence()
            .Append(DOTween.To(() => 1, alpha => Get<CanvasGroup>((int)CanvasGroups.CanvasGroup_Texts).alpha = alpha, 0, 0.3f))
            .Join(GetImage((int)Images.Image_Background).rectTransform.DOSizeDelta(new Vector2(_backgroundImageSize.x, 0), 0.7f))
            .Join(GetImage((int)Images.Image_Background).DOColor(color, 0.5f))
            .OnComplete(() =>
            {
                GetImage((int)Images.Image_Background).gameObject.SetActive(false);
            });
    }
}
