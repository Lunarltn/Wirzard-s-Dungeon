using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Logo;
        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }


    public override void Clear()
    {
    }
}
