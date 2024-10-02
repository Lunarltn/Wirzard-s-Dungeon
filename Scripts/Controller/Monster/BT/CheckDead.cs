using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckDead : TheKiwiCoder.ActionNode
{

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (context.controller.IsDead)
            return State.Success;
        else
            return State.Failure;
    }
}
