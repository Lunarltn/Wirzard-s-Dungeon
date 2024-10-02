using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class AttackEnemy : TheKiwiCoder.ActionNode
{
    float _attackSpeed;
    float _startTime;
    bool _isPlayingAnimation;

    protected override void OnStart()
    {
        _attackSpeed = context.controller.baseStat.AttackSpeed;
        _isPlayingAnimation = false;
        context.controller.Attack();
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {

        if (context.controller.IsPlayingAttackTagAnimation() && _isPlayingAnimation == false)
            _isPlayingAnimation = true;

        if (_isPlayingAnimation == false)
            _startTime = Time.time;

        if (context.controller.IsPlayingAttackTagAnimation() == false && Time.time - _startTime > _attackSpeed)
            return State.Success;
        else
            return State.Running;

    }
}
