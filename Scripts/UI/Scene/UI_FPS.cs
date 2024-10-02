using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_FPS : UI_Scene
{
    enum Texts
    {
        Text
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindTMP(typeof(Texts));

        return true;
    }

    void Update()
    {
        float fps = 1.0f / Time.deltaTime;
        float ms = Time.deltaTime * 1000.0f;
        string text = string.Format("{0:N1} FPS ({1:N1}ms)", fps, ms);
        GetTMP((int)Texts.Text).text = text;
    }

    /*void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
            Debug.Log(e.keyCode);
    }*/
}
