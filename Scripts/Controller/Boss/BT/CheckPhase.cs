using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPhase : TheKiwiCoder.ActionNode
{
    public float phase;
    BossController _bossController;

    protected override void OnStart()
    {
        if (_bossController == null)
            _bossController = context.transform.GetComponent<BossController>();
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (_bossController == null)
            return State.Failure;

        if (_bossController.Phase == phase)
            return State.Success;
        else
            return State.Failure;
    }
}
