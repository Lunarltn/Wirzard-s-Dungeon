using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class MoveToTarget : TheKiwiCoder.ActionNode
{
    public float stoppingDistance = 0.1f;
    public bool updateRotation = true;
    public float acceleration = 8.0f;
    public float tolerance = 1.0f;
    public float hostilityTime = 5;
    public float detectionRange = 10;

    bool _isArrived;
    float _currentMoveTimer;
    float _hostilityTimer;

    protected override void OnStart()
    {
        if (blackboard.moveToTarget == null || context.agent.enabled == false)
            return;

        context.agent.destination = blackboard.moveToTarget.position;
        context.agent.stoppingDistance = stoppingDistance;
        context.agent.speed = context.controller.baseStat.MoveSpeed;
        context.agent.updateRotation = updateRotation;
        context.agent.acceleration = acceleration;
        _isArrived = false;
        _currentMoveTimer = context.animator.GetFloat(MOVE_HASH);

        if (hostilityTime != 0)
        {
            context.detection.ChangeDetectionRangeOption(detectionRange, 360);
            _hostilityTimer = 0;
        }
    }

    protected override void OnStop()
    {
        context.animator.SetFloat(MOVE_HASH, 0);
        if (context.agent.enabled)
            context.agent.ResetPath();
    }

    protected override State OnUpdate()
    {
        if (context.agent.enabled == false)
            return State.Failure;

        if (hostilityTime != 0 && context.controller.AttackTarget == null
            && (context.detection.DetectEnemyInDetectionRange() == false
               || context.detection.EnemyInDetectionRange.transform != blackboard.moveToTarget))
        {
            _hostilityTimer += Time.deltaTime;
        }
        if (context.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid
        || blackboard.moveToTarget == null || (hostilityTime != 0 && _hostilityTimer > hostilityTime))
        {
            return State.Failure;
        }

        var dist = Vector3.Distance(context.transform.position, blackboard.moveToTarget.position);
        if (_isArrived == false && dist < tolerance)
        {
            _isArrived = true;
        }

        if (_isArrived)
        {
            _currentMoveTimer = Mathf.Max(0, _currentMoveTimer - Time.deltaTime * 2);
            if (_currentMoveTimer <= 0)
                return State.Success;
        }
        else
        {
            if (blackboard.moveToTarget != null)
            {
                context.agent.destination = blackboard.moveToTarget.position;
            }
            _currentMoveTimer = Mathf.Min(1, _currentMoveTimer + Time.deltaTime);
        }

        context.animator.SetFloat(MOVE_HASH, _currentMoveTimer);

        return State.Running;
    }
}
