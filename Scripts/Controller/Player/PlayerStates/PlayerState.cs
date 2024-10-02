using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState : State
{
    protected readonly int MOVE_HASH = Animator.StringToHash("Move");
    protected readonly int POSX_HASH = Animator.StringToHash("PosX");
    protected readonly int POSY_HASH = Animator.StringToHash("PosY");
    protected readonly int ATTACK_HASH = Animator.StringToHash("Attack");
    protected readonly int ATTACK_TYPE_HASH = Animator.StringToHash("AttackType");
    protected readonly int BATTLE_HASH = Animator.StringToHash("Battle");
    protected readonly int CROUCH_HASH = Animator.StringToHash("Crouch");
    protected readonly int DODGE_HASH = Animator.StringToHash("Dodge");
    protected readonly int JUMP_HASH = Animator.StringToHash("Jump");
    protected readonly int FALL_HASH = Animator.StringToHash("Fall");
    protected readonly int LAND_HASH = Animator.StringToHash("Land");
    protected readonly int DIE_HASH = Animator.StringToHash("Die");
    protected readonly int KICK_HASH = Animator.StringToHash("Kick");

    protected const string DODGE = "Dodge";
    protected const string JUMPING = "Jumping";
    protected const string FALLING = "Falling";
    protected const string LANDING = "Landing";
    protected const string MOVE = "Move Blend";
    protected const string BATTLE = "Battle Blend";
    protected const string DIE_RECOVERY = "DieRecovery";

    protected PlayerController player;

    protected PlayerState(PlayerController player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }
    public bool IsPlayAnim(string name, int layer)
    {
        return player.Animator.GetCurrentAnimatorStateInfo(layer).IsName(name);
    }
    public float CurrentAnimTime(int layer)
    {
        return player.Animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
    }
}