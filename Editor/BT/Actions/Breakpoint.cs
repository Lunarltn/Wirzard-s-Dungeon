using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEngine;

public class Breakpoint : TheKiwiCoder.ActionNode
{
    protected override void OnStart()
    {
        Debug.Log("Trigging Breakpoint");
        Debug.Break();
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        return State.Success;
    }
}
