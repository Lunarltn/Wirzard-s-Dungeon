using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LoadingBar : UI_Scene
{
    enum Images
    {
        Image_BackGround,
        Image_Bar
    }

    public float fillAmount => GetImage((int)Images.Image_Bar).fillAmount;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));
        GetImage((int)Images.Image_Bar).fillAmount = 0;

        return true;
    }

    public void FillAmount(float amount)
    {
        GetImage((int)Images.Image_Bar).fillAmount = amount;
    }
}
