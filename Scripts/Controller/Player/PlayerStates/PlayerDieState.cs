using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState : PlayerState
{
    public DieState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.Move(Vector3.zero);
        /*if (IsPlayAnim(MOVE, 1) == false)
            player.Animator.Play(MOVE, 1);*/
    }

    public override void LogicUpdate()
    {
        if (player.IsDead == false
            && IsPlayAnim(DIE_RECOVERY, 2) && CurrentAnimTime(2) > 0.99f)
        {
            stateMachine.ChangeState(player.WalkState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}