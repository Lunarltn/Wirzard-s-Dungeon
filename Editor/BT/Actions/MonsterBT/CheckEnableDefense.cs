using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckEnableDefense : TheKiwiCoder.ActionNode
{
    DefensiveMonsterController controller;

    protected override void OnStart()
    {
        if (controller == null)
            controller = context.transform.GetComponent<DefensiveMonsterController>();
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (controller != null && controller.IsDefense)
            return State.Success;
        else
            return State.Failure;
    }
}
