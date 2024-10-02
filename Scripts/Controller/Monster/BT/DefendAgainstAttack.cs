using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class DefendAgainstAttack : TheKiwiCoder.ActionNode
{
    DefensiveMonsterController controller;
    public float DefendTime = 4;
    float startTime;

    protected override void OnStart()
    {
        if (controller == null)
            controller = context.transform.GetComponent<DefensiveMonsterController>();
        startTime = Time.time;
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (Time.time - startTime > DefendTime)
        {
            context.animator.SetBool(DEFEND_HASH, false);
            controller.IsDefense = false;

            return State.Success;
        }

        context.animator.SetBool(DEFEND_HASH, true);
        context.agent.ResetPath();

        return State.Running;
    }
}
