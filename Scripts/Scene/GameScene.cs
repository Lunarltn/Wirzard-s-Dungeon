using System;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Game;
        Managers.Input.PlayMove();
        Managers.Input.LockMouse();
    }

    private void Update()
    {

    }

    public override void Clear()
    {
    }
}
