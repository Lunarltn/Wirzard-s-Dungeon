using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchState : PlayerState
{
    float _layerWeight;
    bool _isCrouch;
    public CrouchState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }
    public override void Enter()
    {
        base.Enter();
        _layerWeight = 0;
        _isCrouch = true;
        player.Animator.SetLayerWeight(1, 0);
        player.Animator.SetBool(CROUCH_HASH, true);
        player.Animator.SetFloat(POSX_HASH, 0);
        player.Animator.SetFloat(POSY_HASH, 0);
        player.Crouch(true);
    }

    public override void Exit()
    {
        player.Animator.SetBool(CROUCH_HASH, false);
    }

    public override void LogicUpdate()
    {
        //stand
        if ((Managers.Input.IsCrouch || Managers.Input.IsRun) && !player.Detection.IsAboveHead && _isCrouch && StateTimer > 0.5f)
        {
            _isCrouch = false;
            player.Crouch(false);
        }
        if (!_isCrouch && _layerWeight < 1)
        {
            _layerWeight += Time.deltaTime * 2;
            player.Animator.SetLayerWeight(1, _layerWeight);
        }
        if (_layerWeight >= 1)
        {
            stateMachine.ChangeState(player.WalkState);
        }
        //jump
        if (Managers.Input.IsJump && !player.Detection.IsAboveHead && StateTimer > 0.5f)
        {
            player.Crouch(false);
            stateMachine.ChangeState(player.JumpState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.CrouchMove(Managers.Input.MoveDirection);
    }

    public override void AnimationUpdate()
    {
        float x = Mathf.Clamp(Managers.Input.MoveDirection.x * 2, -1, 1);
        float z = Mathf.Clamp(Managers.Input.MoveDirection.z * 2, -1, 1);
        player.Animator.SetFloat(POSX_HASH, x);
        player.Animator.SetFloat(POSY_HASH, z);
    }
}