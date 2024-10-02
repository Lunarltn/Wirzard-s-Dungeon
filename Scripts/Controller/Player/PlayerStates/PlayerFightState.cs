using UnityEngine;

public class FightState : PlayerState
{
    float _fallTimer;
    public FightState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.Animator.SetBool(BATTLE_HASH, true);
        player.Animator.SetFloat(POSX_HASH, 0);
        player.Animator.SetFloat(POSY_HASH, 0);
        player.RunSpeed = 1;
    }

    public override void LogicUpdate()
    {
        //run
        if (Managers.Input.IsRun)
            stateMachine.ChangeState(player.WalkState);
        //zoom
        if (Managers.Input.MouseRightClick)
            Managers.Camera.ZoomInCamera();
        else
            Managers.Camera.ZoomOutCamera();
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
        if (StateTimer < 0.2f)
            return;
        //attack
        if (Managers.Input.MouseLeftClick)
        {
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
    }

    public override void Exit()
    {
        Managers.Camera.ZoomOutCamera();
        player.Animator.SetBool(BATTLE_HASH, false);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.FightMove(Managers.Input.MoveDirection);
    }

    public override void AnimationUpdate()
    {
        float x = Mathf.Clamp(Managers.Input.MoveDirection.x * 2, -1, 1);
        float z = Mathf.Clamp(Managers.Input.MoveDirection.z * 2, -1, 1);
        player.Animator.SetFloat(POSX_HASH, x);
        player.Animator.SetFloat(POSY_HASH, z);
    }
}