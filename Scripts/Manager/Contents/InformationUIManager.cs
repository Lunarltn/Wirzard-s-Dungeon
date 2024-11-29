using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class InformationUIManager
{
    UI_DamageEffect _damageEffect;
    UI_Alarm _alarm;
    UI_GameOver _gameOver;
    UI_GameClear _gameClear;
    UI_Highlight _highlight;

    public void Init()
    {
        _damageEffect = Managers.UI.ShowSceneUI<UI_DamageEffect>();
        _alarm = Managers.UI.ShowSceneUI<UI_Alarm>();
        _highlight = Managers.UI.ShowSceneUI<UI_Highlight>();
        _gameClear = Managers.UI.ShowSceneUI<UI_GameClear>();
        _gameOver = Managers.UI.ShowSceneUI<UI_GameOver>();
        Managers.UI.ShowSceneUI<UI_PlayerStat>();
        Managers.UI.ShowSceneUI<UI_HotKeySlotBar>();
        Managers.Inventory.UI_InventoryAssistent = Managers.UI.ShowSceneUI<UI_InventoryAssistent>();
    }

    public void ShowDamageEffect(Vector3 position, Damage damage)
    {
        _damageEffect?.ShowDamageText(position, damage);
    }

    public void ShowAlarm(string text)
    {
        _alarm?.ShowAlarm(text);
    }

    public void ShowItemAlarm(string text)
    {
        _alarm?.ShowItemAlarm(text);
    }

    public void ShowInterectionMessege(string name, string contents, Action action)
    {
        if (contents == string.Empty) return;
        _alarm?.ShowInterection(name, contents);
        if (Managers.Input.IsInterect)
        {
            action?.Invoke();
        }
    }
    public void ShowGameOverWindow()
    {
        _gameOver?.FadeIn();
    }

    public void ShowGameClearWindow()
    {
        _gameClear?.FadeIn();
    }

    public void HideInterectionMessege()
    {
        _alarm?.HideInterection();
    }

    public void ShowHighlight(Transform target, float high)
    {
        _highlight?.ShowHighlight(target, high);
    }

}
