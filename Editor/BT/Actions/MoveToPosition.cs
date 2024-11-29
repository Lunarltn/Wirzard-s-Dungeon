using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class MoveToPosition : TheKiwiCoder.ActionNode
{
    public float stoppingDistance = 0.1f;
    public bool updateRotation = true;
    public float acceleration = 8.0f;
    public float tolerance = 1.0f;
    bool _isArrived;
    float _currentMoveTimer;

    protected override void OnStart()
    {
        if (context.agent.enabled == false || context.gameObject.activeSelf == false)
            return;

        context.agent.stoppingDistance = stoppingDistance;
        context.agent.speed = context.controller.baseStat.MoveSpeed;
        context.agent.destination = blackboard.moveToPosition;
        context.agent.updateRotation = updateRotation;
        context.agent.acceleration = acceleration;
        _isArrived = false;
        _currentMoveTimer = context.animator.GetFloat(MOVE_HASH);
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

        if (context.agent.pathPending)
        {
            return State.Running;
        }

        if (_isArrived == false && context.agent.remainingDistance < tolerance)
        {
            _isArrived = true;
        }

        if (context.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            return State.Failure;
        }

        if (_isArrived)
        {
            _currentMoveTimer = Mathf.Max(0, _currentMoveTimer - Time.deltaTime * 2);
            if (_currentMoveTimer <= 0)
                return State.Success;
        }
        else
        {
            _currentMoveTimer = Mathf.Min(1, _currentMoveTimer + Time.deltaTime);
        }
        context.animator.SetFloat(MOVE_HASH, _currentMoveTimer);

        return State.Running;
    }

    public override void OnDrawGizmos()
    {
        if (context == null || blackboard == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(blackboard.moveToPosition, 1);
        Gizmos.DrawLine(context.transform.position, blackboard.moveToPosition);
    }
}
