using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoSporttingEnemy : TheKiwiCoder.ActionNode
{
    MonsterController controller;
    public float duration = 2;
    float startTime;
    float weight;
    protected override void OnStart()
    {
        if (blackboard.isTracking || context.agent.enabled == false)
            return;
        if (controller == null)
            controller = context.transform.GetComponent<MonsterController>();

        startTime = Time.time;
        context.agent.ResetPath();
        context.animator.SetBool(SENSE_HASH, true);
        context.animator.SetFloat(MOVE_HASH, 0);
        Managers.InfoUI.ShowHighlight(context.transform, 1);
        if (context.detection.EnemyInDetectionRange != null)
        {
            var dist = Vector3.Distance(context.detection.EnemyInDetectionRange.transform.position
               , context.transform.position);
            weight = dist / context.detection.DetectionRange;
        }
    }

    protected override void OnStop()
    {
        context.animator.SetBool(SENSE_HASH, false);
    }

    protected override State OnUpdate()
    {
        if (context.detection.EnemyInDetectionRange == null
            || (blackboard.moveToTarget != null && blackboard.isTracking)
            || context.agent.enabled == false)
        {
            return State.Failure;
        }

        if (Time.time - startTime > duration * weight ||
            (controller != null && controller.IsHitting) ||
            context.controller.AttackTarget != null)
        {
            context.animator.SetBool(BATTLE_HASH, true);
            blackboard.isTracking = true;

            return State.Success;
        }

        var dir = context.detection.EnemyInDetectionRange.transform.position
            - context.transform.position;
        context.controller.Rotation(dir.normalized);

        return State.Running;
    }
}