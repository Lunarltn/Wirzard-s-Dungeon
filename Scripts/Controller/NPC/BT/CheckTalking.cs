using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTalking : TheKiwiCoder.ActionNode
{
    NPCController controller;

    protected override void OnStart()
    {
        if (controller == null)
            controller = context.transform.GetComponent<NPCController>();
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (controller != null && controller.IsTalking)
            return State.Success;
        else
            return State.Failure;
    }
}
