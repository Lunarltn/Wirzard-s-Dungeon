using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;
using static Define;

public class UI_GameClear : UI_Scene
{
    CancellationTokenSource _cancleTokenSource;

    enum CanvasGroups
    {
        _TextGroup
    }

    enum Images
    {
        Image_Background
    }

    enum Texts
    {
        Text_Exit
    }

    public Action RespawnAction;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        GetComponent<Canvas>().sortingOrder = 101;

        Bind<CanvasGroup>(typeof(CanvasGroups));
        BindImage(typeof(Images));
        BindTMP(typeof(Texts));

        BindEvent(GetTMP((int)Texts.Text_Exit).gameObject, ClickExit, Define.UI_Event.Click);

        gameObject.SetActive(false);
        _cancleTokenSource = new CancellationTokenSource();

        return true;
    }


    public void FadeIn()
    {
        AnimationFadeIn().Forget();
    }

    async UniTask AnimationFadeIn()
    {
        await DOTween.Sequence()
            .OnStart(() =>
            {
                Managers.Input.UnlockMouse();
                gameObject.SetActive(true);
                GetImage((int)Images.Image_Background).gameObject.SetActive(true);
                Get<CanvasGroup>((int)CanvasGroups._TextGroup).alpha = 0;
                var color = GetImage((int)Images.Image_Background).color;
                color.a = 0;
                GetImage((int)Images.Image_Background).color = color;
            })
            .Append(GetImage((int)Images.Image_Background).DOFade(0.8f, 0.5f))
            .Append(Get<CanvasGroup>((int)CanvasGroups._TextGroup).DOFade(1, 0.8f))
            .WithCancellation(_cancleTokenSource.Token);
    }

    void ClickExit(PointerEventData evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void CancleToken() { _cancleTokenSource?.Cancel(); }
    public void DisposeToken() { _cancleTokenSource?.Dispose(); }

    void OnDestroy()
    {
        CancleToken();
        DisposeToken();
    }
}
