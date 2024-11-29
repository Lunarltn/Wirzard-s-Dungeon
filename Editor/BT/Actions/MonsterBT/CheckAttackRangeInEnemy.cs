using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckAttackRangeInEnemy : TheKiwiCoder.ActionNode
{
    public float attackRange = 0.7f;
    public float attackDistance = 1;

    protected override void OnStart()
    {
        context.detection.ChangeAttackRange(attackRange, attackDistance);
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (context.detection.DetectEnemyInAttackRange() && blackboard.isTracking)
            return State.Success;
        else
            return State.Failure;
    }


    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (context == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(context.transform.position + context.transform.forward * attackDistance, attackRange);

    }
}
