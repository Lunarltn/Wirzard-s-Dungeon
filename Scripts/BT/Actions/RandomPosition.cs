using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class RandomPosition : TheKiwiCoder.ActionNode
{
    public Vector2 min = Vector2.one * -10;
    public Vector2 max = Vector2.one * 10;

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        var startPos = context.controller.StartPosition;
        blackboard.moveToPosition.x = Random.Range(startPos.x + min.x, startPos.x + max.x);
        blackboard.moveToPosition.z = Random.Range(startPos.z + min.y, startPos.z + max.y);
        return State.Success;
    }

    public override void OnDrawGizmos()
    {
        if (context == null)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(context.controller.StartPosition, new Vector3(max.x, 0, max.y) * 2);
    }
}
