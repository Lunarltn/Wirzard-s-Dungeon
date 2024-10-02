using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_OneOffPopup : UI_Base
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.UI.SetCanvas(gameObject, true);
        return true;
    }

    public virtual void CloseOneOffPopupUI()
    {
        Managers.UI.CloseOneOffPopupUI();
    }
}
