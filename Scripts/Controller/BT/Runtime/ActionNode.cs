using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TheKiwiCoder
{
    public abstract class ActionNode : Node
    {
        protected readonly int DIE_HASH = Animator.StringToHash("Die");
        protected readonly int MOVE_HASH = Animator.StringToHash("Move");
        protected readonly int ATTACK_HASH = Animator.StringToHash("Attack");
        protected readonly int HIT_HASH = Animator.StringToHash("Hit");
        protected readonly int BATTLE_HASH = Animator.StringToHash("Battle");
        protected readonly int SENSE_HASH = Animator.StringToHash("Sense");
        protected readonly int DEFEND_HASH = Animator.StringToHash("Defend");

        protected string ATTACK = "Attack";

        protected bool IsPlayAnim(Animator animator, string name, int layer)
        {
            return animator.GetCurrentAnimatorStateInfo(layer).IsName(name);
        }

        protected float CurrentAnimTime(Animator animator, int layer)
        {
            return animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
        }

    }
}