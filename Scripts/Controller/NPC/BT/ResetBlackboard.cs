using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBlackboard : TheKiwiCoder.ActionNode
{
    protected override void OnStart()
    {
        blackboard.isTracking = false;
        blackboard.isCastingSkill = false;
        blackboard.moveToTarget = null;
        blackboard.moveToPosition = Vector3.zero;

        if (context.agent.enabled)
            context.agent.ResetPath();
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        return State.Success;
    }
}
