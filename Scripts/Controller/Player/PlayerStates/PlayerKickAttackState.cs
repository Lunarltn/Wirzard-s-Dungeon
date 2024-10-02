using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKickAttackState : PlayerState
{
    const string KICK = "Kick";
    public PlayerKickAttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        player.Animator.SetTrigger(KICK_HASH);
    }
    public override void LogicUpdate()
    {
        if (IsPlayAnim(KICK, 2) && CurrentAnimTime(2) >= 0.99f)
            stateMachine.ChangeState(player.FightState);
    }
}
