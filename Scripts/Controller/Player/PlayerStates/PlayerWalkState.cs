using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WalkState : PlayerState
{
    float _animationLerpTimer;
    float _fallTimer;

    public WalkState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.RunSpeed = 1;
    }

    public override void Exit()
    {

    }

    public override void LogicUpdate()
    {
        //run
        if (Managers.Input.IsRun)
            player.RunSpeed += Time.deltaTime * 2;
        else
            player.RunSpeed -= Time.deltaTime * 2;
        //jump
        if (Managers.Input.IsJump)
            stateMachine.ChangeState(player.JumpState);
        //dodge
        if (Managers.Input.IsDodge && Managers.Input.MoveDirection != Vector3.zero
        && !Managers.HotKey.DashSkillSlot.IsCooldown)
            stateMachine.ChangeState(player.DodgeState);
        //fall
        if (!player.Detection.IsOnGround)
        {
            if (StateTimer - _fallTimer > 1f)
            {
                stateMachine.ChangeState(player.JumpState);
            }
        }
        else
        {
            _fallTimer = StateTimer;
        }
        //fight
        if (Managers.Input.MouseLeftClick)
        {
            player.RunSpeed = 1f;
            if (Managers.HotKey.CurrentSkillSlot.IsCooldown == false
            && Managers.HotKey.IsNullCurrentSkillSlot() == false
            && player.CurrentWeaponCastingTransform != null)
            {
                if (player.Stat.DecreaseMP(Managers.HotKey.CurrentSkillSlot.SkillData.cost))
                    stateMachine.ChangeState(player.AttackState);
            }
            else
            {
                stateMachine.ChangeState(player.KickAttackState);
            }
        }
        if (Managers.Input.MouseRightClick)
            Managers.Camera.ZoomInCamera();
        else
            Managers.Camera.ZoomOutCamera();
        //crouch
        if (Managers.Input.IsCrouch)
        {
            player.RunSpeed = 1f;
            stateMachine.ChangeState(player.CrouchState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.NormalMove(Managers.Input.MoveDirection);
    }

    public override void AnimationUpdate()
    {
        if (Managers.Input.MoveDirection != Vector3.zero)
            _animationLerpTimer = Mathf.Clamp(_animationLerpTimer + Time.deltaTime * 3, 0f, 1f);
        else
            _animationLerpTimer = Mathf.Clamp(_animationLerpTimer - Time.deltaTime * 3, 0f, 1f);
        player.Animator.SetFloat(MOVE_HASH, _animationLerpTimer * player.RunSpeed);
    }
}