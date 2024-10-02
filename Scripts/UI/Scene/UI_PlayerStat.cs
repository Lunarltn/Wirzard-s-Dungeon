using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStat : UI_Scene
{
    enum Images
    {
        Image_HPBackGround,
        Image_HP,
        Image_HPAfterImage,
        Image_MPBackGround,
        Image_MP,
        Image_MPAfterImage,
    }

    CancellationTokenSource _hPCancelTokenSource;
    CancellationTokenSource _mPCancelTokenSource;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));

        var _playerStat = Managers.PlayerInfo.Controller.Stat;

        _playerStat.UpdateHPStat += UpdateHPStat;
        _playerStat.UpdateMPStat += UpdateMPStat;
        _playerStat.UpdateCurrentHPStat += UpdateCurrentHPStat;
        _playerStat.UpdateCurrentMPStat += UpdateCurrentMPStat;

        return true;
    }

    public void UpdateCurrentHPStat(int maxHP, int currentHP)
    {
        CancleToken(_hPCancelTokenSource);
        _hPCancelTokenSource = new CancellationTokenSource();
        float per = (float)currentHP / (float)maxHP;
        var background = Images.Image_HPBackGround;
        AnimationBar(Images.Image_HP, background, per, _hPCancelTokenSource).Forget();
        AnimationBar(Images.Image_HPAfterImage, background, per, _hPCancelTokenSource, 1f).Forget();
    }

    public void UpdateCurrentMPStat(int maxMP, int currentMP)
    {
        CancleToken(_mPCancelTokenSource);
        _mPCancelTokenSource = new CancellationTokenSource();
        float per = (float)currentMP / (float)maxMP;
        var background = Images.Image_MPBackGround;
        AnimationBar(Images.Image_MP, background, per, _mPCancelTokenSource).Forget();
        AnimationBar(Images.Image_MPAfterImage, background, per, _mPCancelTokenSource, 1f).Forget();
    }

    public void UpdateHPStat(int maxHP, int currentHP)
    {
        CancleToken(_hPCancelTokenSource);
        _hPCancelTokenSource = new CancellationTokenSource();
        float currentImageSize = GetImage((int)Images.Image_HPBackGround).rectTransform.sizeDelta.x;
        float per = (float)currentHP / (float)maxHP;
        currentImageSize *= per;
        GetImage((int)Images.Image_HP).rectTransform.sizeDelta
            = new Vector2(currentImageSize, GetImage((int)Images.Image_HPBackGround).rectTransform.sizeDelta.y);
        var background = Images.Image_HPBackGround;
        AnimationBar(Images.Image_HPAfterImage, background, per, _hPCancelTokenSource, 1f).Forget();
    }

    public void UpdateMPStat(int maxMP, int currentMP)
    {
        CancleToken(_mPCancelTokenSource);
        _mPCancelTokenSource = new CancellationTokenSource();
        float currentImageSize = GetImage((int)Images.Image_MPBackGround).rectTransform.sizeDelta.x;
        float per = (float)currentMP / (float)maxMP;
        currentImageSize *= per;
        GetImage((int)Images.Image_MP).rectTransform.sizeDelta
            = new Vector2(currentImageSize, GetImage((int)Images.Image_MPBackGround).rectTransform.sizeDelta.y);
        var background = Images.Image_MPBackGround;
        AnimationBar(Images.Image_MPAfterImage, background, per, _mPCancelTokenSource, 1f).Forget();
    }

    async UniTask AnimationBar(Images images, Images backgroundImages, float percent, CancellationTokenSource tokenSource, float delay = 0)
    {
        Image image = GetImage((int)images);
        Image backGroundImage = GetImage((int)backgroundImages);

        float targetImageSize = percent * backGroundImage.rectTransform.sizeDelta.x;
        float currentImageSize = image.rectTransform.sizeDelta.x;
        if (currentImageSize - targetImageSize == 0)
            return;
        if (currentImageSize > targetImageSize)
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: tokenSource.Token);

        float speed = Mathf.Abs(currentImageSize - targetImageSize);
        int sign = currentImageSize > targetImageSize ? -1 : 1;
        int increase = currentImageSize > targetImageSize ? 1 : 0;
        int subtrahend = currentImageSize < targetImageSize ? 1 : 0;

        while ((currentImageSize - targetImageSize) * increase > (currentImageSize - targetImageSize) * subtrahend)
        {
            currentImageSize += speed * sign * Time.deltaTime;
            image.rectTransform.sizeDelta = new Vector2(currentImageSize, backGroundImage.rectTransform.sizeDelta.y);
            await UniTask.NextFrame(cancellationToken: tokenSource.Token);
        }
    }

    void CancleToken(CancellationTokenSource tokenSource) { tokenSource?.Cancel(); }
    void DisposeToken(CancellationTokenSource tokenSource) { tokenSource?.Dispose(); }

    void OnDestroy()
    {
        CancleToken(_hPCancelTokenSource);
        CancleToken(_mPCancelTokenSource);
        DisposeToken(_hPCancelTokenSource);
        DisposeToken(_mPCancelTokenSource);
    }
}
