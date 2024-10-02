using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToEnemy : TheKiwiCoder.ActionNode
{
    float currentMoveTime;

    protected override void OnStart()
    {
        if (context.detection.EnemyInDetectionRange == null)
            return;
        var dest = context.detection.EnemyInDetectionRange.transform.position;
        context.agent.SetDestination(dest);
        context.animator.SetBool(BATTLE_HASH, true);
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