using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UI_BossStat : UI_Scene
{
    enum Images
    {
        Image_HPBackGround,
        Image_ShieldBackGround,
        Image_HP,
        Image_Shield
    }

    enum Texts
    {
        Text_Name
    }

    Color32 _grayColor = new Color32(137, 137, 137, 255);
    TweenerCore<Color, Color, ColorOptions> _changeColorTweener;

    CancellationTokenSource _hPCancelTokenSource;
    CancellationTokenSource _shieldCancelTokenSource;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        BindTMP(typeof(Texts));

        return true;
    }

    public void SetUI(BossStat bossStat)
    {
        bossStat.UpdateHP += UpdateHPStat;
        bossStat.UpdateShield += UpdateShieldStat;
    }

    public void SetBossName(string name)
    {
        GetTMP((int)Texts.Text_Name).text = name;
    }

    public void UpdateHPStat(int maxHP, int currentHP)
    {
        CancleToken(_hPCancelTokenSource);
        _hPCancelTokenSource = new CancellationTokenSource();
        float per = (float)currentHP / (float)maxHP;
        var background = Images.Image_HPBackGround;
        AnimationBar(Images.Image_HP, background, per, _hPCancelTokenSource).Forget();
    }

    public void UpdateShieldStat(int maxShield, int currentShield)
    {
        CancleToken(_shieldCancelTokenSource);
        _shieldCancelTokenSource = new CancellationTokenSource();
        float per = (float)currentShield / (float)maxShield;
        var background = Images.Image_ShieldBackGround;
        AnimationBar(Images.Image_Shield, background, per, _shieldCancelTokenSource).Forget();
    }

    async UniTask AnimationBar(Images images, Images backgroundImages, float percent, CancellationTokenSource tokenSource)
    {
        Image image = GetImage((int)images);
        Image backGroundImage = GetImage((int)backgroundImages);

        float targetImageSize = percent * backGroundImage.rectTransform.sizeDelta.x;
        float currentImageSize = image.rectTransform.sizeDelta.x;
        if (currentImageSize - targetImageSize == 0)
            return;

        float speed = Mathf.Abs(currentImageSize - targetImageSize) * 2;
        int sign = currentImageSize > targetImageSize ? -1 : 1;
        int increase = currentImageSize > targetImageSize ? 1 : 0;
        int subtrahend = currentImageSize < targetImageSize ? 1 : 0;

        if (_changeColorTweener != null) _changeColorTweener.Kill();
        image.color = Color.white;

        while ((currentImageSize - targetImageSize) * increase > (currentImageSize - targetImageSize) * subtrahend)
        {
            currentImageSize += speed * sign * Time.deltaTime;
            image.rectTransform.sizeDelta = new Vector2(currentImageSize, backGroundImage.rectTransform.sizeDelta.y);
            await UniTask.NextFrame(cancellationToken: tokenSource.Token);
        }

        _changeColorTweener = image.DOColor(_grayColor, 1f);
        await _changeColorTweener;
    }

    void CancleToken(CancellationTokenSource tokenSource) { tokenSource?.Cancel(); }
    void DisposeToken(CancellationTokenSource tokenSource) { tokenSource?.Dispose(); }

    void OnDestroy()
    {
        CancleToken(_hPCancelTokenSource);
        CancleToken(_shieldCancelTokenSource);
        DisposeToken(_hPCancelTokenSource);
        DisposeToken(_shieldCancelTokenSource);
    }
}
