using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckDetectionEnemyInRoom : TheKiwiCoder.ActionNode
{
    BossController _bossController;
    BossDetection _bossDetection;
    protected override void OnStart()
    {
        if (_bossController == null)
            _bossController = context.transform.GetComponent<BossController>();
        if (_bossDetection == null)
            _bossDetection = context.transform.GetComponent<BossDetection>();
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (_bossController == null || _bossDetection == null || _bossDetection.BossRoom == null)
            return State.Failure;

        if (_bossDetection.DetectPlayerInBossRoom())
        {
            if (blackboard.isTracking == false)
            {
                _bossController.Hostile();
                blackboard.isTracking = true;
                blackboard.moveToTarget = context.detection.EnemyInDetectionRange.transform;
            }
            return State.Success;
        }
        else
        {
            if (blackboard.isTracking == true)
            {
                _bossController.ResetHostility();
                blackboard.isTracking = false;
                blackboard.moveToTarget = null;
            }
            return State.Failure;
        }
    }
}
