using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialActionState : PlayerState
{
    public SpecialActionState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void LogicUpdate()
    {
        if (player.SpecialAction != null)
            player.SpecialAction.Invoke();
    }
    public override void AnimationUpdate()
    {
        if (player.SpecialActionAnimation != null)
            player.SpecialActionAnimation.Invoke();
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
    public override void Exit()
    {
        base.Exit();
    }
}