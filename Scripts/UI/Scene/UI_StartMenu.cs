using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static UI_Inventory;
using static Unity.Burst.Intrinsics.X86.Avx;

public class UI_StartMenu : UI_Scene
{
    enum Texts
    {
        Text_Start,
        Text_Load,
        Text_Exit
    }
    [SerializeField]
    LobbyPlayerController playerController;
    [SerializeField]
    Transform playerDestination;
    CancellationTokenSource[] _optionCancelSources;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        _optionCancelSources = new CancellationTokenSource[3];
        BindTMP(typeof(Texts));

        BindEvent(GetTMP((int)Texts.Text_Start).gameObject, EnterStartButton, Define.UI_Event.Enter);
        BindEvent(GetTMP((int)Texts.Text_Load).gameObject, EnterLoadButton, Define.UI_Event.Enter);
        BindEvent(GetTMP((int)Texts.Text_Exit).gameObject, EnterExitButton, Define.UI_Event.Enter);

        BindEvent(GetTMP((int)Texts.Text_Start).gameObject, ExitStartButton, Define.UI_Event.Exit);
        BindEvent(GetTMP((int)Texts.Text_Load).gameObject, ExitLoadButton, Define.UI_Event.Exit);
        BindEvent(GetTMP((int)Texts.Text_Exit).gameObject, ExitExitButton, Define.UI_Event.Exit);

        BindEvent(GetTMP((int)Texts.Text_Start).gameObject, ClickStartButton, Define.UI_Event.Click);
        BindEvent(GetTMP((int)Texts.Text_Load).gameObject, ClickLoadButton, Define.UI_Event.Click);
        BindEvent(GetTMP((int)Texts.Text_Exit).gameObject, ClickExitButton, Define.UI_Event.Click);

        _cancelTokenSource = new CancellationTokenSource();
        return true;
    }

    void EnterStartButton(PointerEventData evt)
    {
        AnimationFontSize(GetTMP((int)Texts.Text_Start), 60, 0.4f, 0).Forget();
    }

    void EnterLoadButton(PointerEventData evt)
    {
        AnimationFontSize(GetTMP((int)Texts.Text_Load), 60, 0.4f, 1).Forget();
    }

    void EnterExitButton(PointerEventData evt)
    {
        AnimationFontSize(GetTMP((int)Texts.Text_Exit), 60, 0.4f, 2).Forget();
    }

    void ExitStartButton(PointerEventData evt)
    {
        AnimationFontSize(GetTMP((int)Texts.Text_Start), 50, 0.2f, 0).Forget();
    }

    void ExitLoadButton(PointerEventData evt)
    {
        AnimationFontSize(GetTMP((int)Texts.Text_Load), 50, 0.2f, 1).Forget();
    }

    void ExitExitButton(PointerEventData evt)
    {
        AnimationFontSize(GetTMP((int)Texts.Text_Exit), 50, 0.2f, 2).Forget();
    }

    void ClickStartButton(PointerEventData evt)
    {
        AnimationPlayerOpenDoor().Forget();
        Managers.Scene.LoadScene(Define.Scene.Game);
    }
    void ClickLoadButton(PointerEventData evt)
    {
        //¹Ì±¸Çö
    }

    void ClickExitButton(PointerEventData evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }

    async UniTask AnimationFontSize(TextMeshProUGUI tmp, float targetFontSize, float speed, int idx)
    {
        _optionCancelSources[idx]?.Cancel();
        _optionCancelSources[idx] = new CancellationTokenSource();

        float time = 0;
        float size = targetFontSize - tmp.fontSize;
        float currentSize = tmp.fontSize;

        while (time < speed)
        {
            time += Time.deltaTime;
            tmp.fontSize = currentSize + (size * (time / speed));
            await UniTask.WaitForFixedUpdate(cancellationToken: _optionCancelSources[idx].Token);
        }
    }
    CancellationTokenSource _cancelTokenSource;

    async UniTask AnimationPlayerOpenDoor()
    {
        while (!playerController.MoveToDestination(playerDestination.position, 0.2f))
            await UniTask.WaitForFixedUpdate(_cancelTokenSource.Token);

        playerController.AnimationAttack();
    }

    private void OnDestroy()
    {
        _cancelTokenSource?.Cancel();
    }
}
