using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemThrowingAttackEnemy : TheKiwiCoder.ActionNode
{
    float _attackSpeed;
    float _startTime;
    bool _isPlayingAnimation;
    GolemController _golemController;

    protected override void OnStart()
    {
        blackboard.isCastingSkill = true;
        if (_golemController == null)
            _golemController = context.transform.GetComponent<GolemController>();
        _attackSpeed = context.controller.baseStat.AttackSpeed * 1.2f;
        _isPlayingAnimation = false;
        _golemController.ThrowAttack();
        _startTime = Time.time;
    }

    protected override void OnStop()
    {
        blackboard.isCastingSkill = false;
    }

    protected override State OnUpdate()
    {
        if (_golemController == null)
            return State.Failure;

        var dir = (blackboard.moveToTarget.position - context.transform.position).normalized;
        context.controller.Rotation(dir);

        if (_golemController.IsPlayingAttackTagAnimation() && _isPlayingAnimation == false)
            _isPlayingAnimation = true;

        if (_isPlayingAnimation == false)
            _startTime = Time.time;

        if (_golemController.IsPlayingAttackTagAnimation() == false && Time.time - _startTime > _attackSpeed)
            return State.Success;
        else
            return State.Running;
    }
}
