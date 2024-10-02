using UnityEngine;

public class DodgeState : PlayerState
{
    Vector3 _dodgeDir;
    public DodgeState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.Animator.SetLayerWeight(1, 0);
        _dodgeDir = Managers.Input.MoveDirection;
    }

    public override void Exit()
    {
        player.Animator.SetLayerWeight(1, 1);
        Managers.HotKey.DashSkillSlot.RunCooldown();
    }

    public override void LogicUpdate()
    {
        if (IsPlayAnim(DODGE, 0) && CurrentAnimTime(0) > 0.95f)
        {
            player.Animator.SetBool(DODGE_HASH, false);
            stateMachine.ChangeState(player.FightState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (player.Rotation(player.MoveToCameraDirection(_dodgeDir), true, 0.1f, 10))
        {
            player.Dodge(_dodgeDir);
            player.Animator.SetBool(DODGE_HASH, true);
        }
    }
}