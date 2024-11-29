using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UI_FadeEffect : UI_Scene
{
    enum Images
    {
        Image_BackGround
    }
    public enum Options
    {
        FadeIn,
        FadeOut,
    }

    [SerializeField]
    AnimationCurve _curve;
    public bool IsStop;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        IsStop = false;
        return true;
    }

    public async UniTask FadeEffect(Options Option, float fadeTime)
    {
        float currentFadeTime = 0;
        float percent = 0;
        var image = GetImage((int)Images.Image_BackGround);

        while (percent < 1)
        {
            currentFadeTime += Time.deltaTime;
            percent = currentFadeTime / fadeTime;
            Color tempColor = Color.black;
            if (Option == Options.FadeIn)
                tempColor.a = Mathf.Lerp(1, 0, _curve.Evaluate(percent));
            else
                tempColor.a = Mathf.InverseLerp(0, 1, _curve.Evaluate(percent));

            image.color = tempColor;
            await UniTask.Yield();
        }
        if (Option == Options.FadeIn)
            Managers.Resource.Destroy(gameObject);
        IsStop = true;
    }
}
