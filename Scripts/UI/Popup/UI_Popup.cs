using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Popup : UI_Base
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.UI.SetCanvas(gameObject, false);
        return true;
    }

    protected virtual void OnEnable()
    {
        Managers.UI.SortPopup(gameObject);
    }

    public virtual void ClosePopupUI<T>(KeyCode keyCode)
    {
        Managers.UI.ClosePopupUI<T>();
        Managers.Input.ClosePopupKeyCode(keyCode);
    }
}
