using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationPatrol : TheKiwiCoder.ActionNode
{
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (context.controller.PatrolDestinations == null)
        {
            if (Vector3.Distance(context.transform.position, context.controller.StartPosition) > 1)
            {
                blackboard.moveToPosition = context.controller.StartPosition;
                return State.Success;
            }
            if (context.controller.Rotation(context.controller.StartDirection))
                return State.Failure;
            else
                return State.Running;
        }

        blackboard.moveToPosition = context.controller.PatrolDestinations.GetPositionNearestDestination(context.transform.position);
        return State.Success;
    }

}
