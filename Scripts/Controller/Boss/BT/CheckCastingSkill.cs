using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCastingSkill : TheKiwiCoder.ActionNode
{

    protected override void OnStart()
    {

    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (blackboard.isCastingSkill)
            return State.Success;
        else
            return State.Failure;
    }
}
