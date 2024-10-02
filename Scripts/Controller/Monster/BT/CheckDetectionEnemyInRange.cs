using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckDetectionEnemyInRange : TheKiwiCoder.ActionNode
{
    public enum Type
    {
        Monster,
        NPC
    }
    public Type type = Type.Monster;
    public float detectionRange = 10;
    public float viewAngle = 90;
    public float delay = 1;
    float startTime;

    protected override void OnStart()
    {
        if (blackboard.moveToTarget == null)
            context.detection.ChangeDetectionRangeOption(detectionRange, viewAngle);
        startTime = Time.time;
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (context.controller.AttackTarget != null)
        {
            if (context.controller.AttackTarget.GetComponent<BaseController>().IsDead == false)
                blackboard.moveToTarget = context.controller.AttackTarget;
            else
                blackboard.moveToTarget = null;
        }
        else if (context.detection.DetectEnemyInDetectionRange())
        {
            if (Time.time - startTime > delay)
                blackboard.moveToTarget = context.detection.EnemyInDetectionRange.transform;
            else
                return State.Running;
        }
        else
        {
            if (type == Type.Monster)
                context.animator.SetBool(BATTLE_HASH, false);
            startTime = Time.time;
            blackboard.isTracking = false;
            blackboard.moveToTarget = null;
        }

        if (blackboard.moveToTarget != null)
        {
            if (type == Type.Monster)
                context.animator.SetBool(BATTLE_HASH, true);

            blackboard.isTracking = true;

            return State.Success;
        }

        return State.Failure;
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (context == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(context.transform.position, context.detection.DetectionRange);
        float fowardAngle = context.transform.eulerAngles.y;
        Vector3 rightDir = Util.AngleToVector3(fowardAngle + context.detection.DetectionViewAngle * 0.5f);
        Vector3 leftDir = Util.AngleToVector3(fowardAngle - context.detection.DetectionViewAngle * 0.5f);
        Gizmos.DrawRay(context.transform.position, rightDir * context.detection.DetectionRange);
        Gizmos.DrawRay(context.transform.position, leftDir * context.detection.DetectionRange);
    }
}