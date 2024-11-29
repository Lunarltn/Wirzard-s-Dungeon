using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSidewaysToTarget : TheKiwiCoder.ActionNode
{
    public float stepDistance = 4.0f;
    public float stoppingDistance = 0.1f;
    public bool updateRotation = true;
    public float acceleration = 8.0f;
    public float tolerance = 1.0f;
    public float hostilityTime = 5;

    Vector3 _targetPosition;
    bool _isArrived;
    float _currentMoveTimer;
    float _hostilityTimer;

    protected override void OnStart()
    {
        if (blackboard.moveToTarget == null)
            return;

        var sideDir = (blackboard.moveToTarget.position - context.transform.position).normalized;
        if (Random.Range(0, 2) == 0)
            sideDir = new Vector3(-sideDir.z, 0, sideDir.x);
        else
            sideDir = new Vector3(sideDir.z, 0, -sideDir.x);

        var ray = new Ray(context.transform.position + Vector3.up * (tolerance + 1), sideDir);
        if (Physics.SphereCast(ray, tolerance, stepDistance, Managers.Layer.GroundLayerMask))
        {
            ray.direction = -sideDir;
            if (Physics.SphereCast(ray, tolerance, stepDistance, Managers.Layer.GroundLayerMask))
                sideDir = Vector3.zero;
            else
                sideDir *= -1;
        }
        _targetPosition = context.transform.position + sideDir * stepDistance;
        context.agent.destination = _targetPosition;
        context.agent.stoppingDistance = stoppingDistance;
        context.agent.speed = context.controller.baseStat.MoveSpeed;
        context.agent.updateRotation = updateRotation;
        context.agent.acceleration = acceleration;
        _isArrived = false;
        _currentMoveTimer = context.animator.GetFloat(MOVE_HASH);

        if (hostilityTime != 0)
        {
            context.detection.ChangeDetectionRangeOption(10, 360);
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
        if (hostilityTime != 0 && context.detection.DetectEnemyInDetectionRange() == false && context.controller.AttackTarget == null)
        {
            _hostilityTimer += Time.deltaTime;
        }
        var dist = Vector3.Distance(context.transform.position, _targetPosition);
        if (_isArrived == false && dist < tolerance)
        {
            _isArrived = true;
        }

        if (context.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid
        || blackboard.moveToTarget == null || (hostilityTime != 0 && _hostilityTimer > hostilityTime))
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
        if (context == null)
            return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(context.transform.position, tolerance);
        Gizmos.DrawLine(context.transform.position, context.agent.destination);
        Gizmos.DrawWireSphere(context.agent.destination, tolerance);
    }
}
