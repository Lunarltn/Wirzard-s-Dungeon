using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : WalkState
{
    float _landWeight;
    float _jumpTimer;
    bool _isLanding;
    public JumpState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _landWeight = 0;
        _jumpTimer = 0;
        _isLanding = false;
        player.Animator.SetLayerWeight(1, 1);
        player.Animator.SetBool(BATTLE_HASH, false);
        player.Animator.SetBool(JUMP_HASH, true);
        player.Animator.SetBool(FALL_HASH, false);
        player.Animator.SetFloat(LAND_HASH, _landWeight);
        player.Jump();
    }

    public override void Exit()
    {
        _isLanding = true;
        player.Animator.SetFloat(LAND_HASH, 0);
    }

    public override void LogicUpdate()
    {
        if (_isLanding)
            _landWeight -= Time.deltaTime * 2;

        player.Animator.SetFloat(LAND_HASH, _landWeight);

        _jumpTimer += Time.deltaTime;
        if (IsPlayAnim(JUMPING, 1) && _jumpTimer > 1.2f && !_isLanding)
            player.Animator.SetBool(FALL_HASH, true);

        if (_jumpTimer > 0.2f && player.Detection.IsOnGround && !_isLanding)
        {
            player.Animator.SetBool(JUMP_HASH, false);
            _isLanding = true;
            _landWeight = 1;
        }

        if (_isLanding && _landWeight < 0.3f)
            stateMachine.ChangeState(player.WalkState);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

    }
}