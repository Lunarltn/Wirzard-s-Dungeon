using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWait : TheKiwiCoder.ActionNode
{
    public float minDuration = 0;
    public float maxDuration = 1;
    float duration;
    float startTime;

    protected override void OnStart()
    {
        startTime = Time.time;
        duration = Random.Range(minDuration, maxDuration);
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (Time.time - startTime > duration)
        {
            return State.Success;
        }
        return State.Running;
    }
}