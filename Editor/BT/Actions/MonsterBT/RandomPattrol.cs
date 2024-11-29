using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TheKiwiCoder;
using UnityEngine;

public class RandomPattrol : TheKiwiCoder.ActionNode
{
    public Vector2 min;
    public Vector2 max;
    float currentMoveTime;

    protected override void OnStart()
    {
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        Vector3 dest = new Vector3(context.controller.StartPosition.x + x, 0, context.controller.StartPosition.z + y);
        context.agent.SetDestination(dest);
        context.animator.SetBool(BATTLE_HASH, false);
        currentMoveTime = context.animator.GetFloat(MOVE_HASH);
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (context.agent.remainingDistance < 0.1f)
            return State.Success;

        context.animator.SetFloat(MOVE_HASH, currentMoveTime);
        currentMoveTime = Mathf.Min(1, currentMoveTime + Time.deltaTime);
        return State.Running;
    }
}
