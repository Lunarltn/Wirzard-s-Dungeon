using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RootMotion.FinalIK.RagdollUtility;

public class WaitNode : INode
{
    float _time;
    float _timer;

    public WaitNode(float time)
    {
        _time = time;
    }

    public INode.NodeState Evaluate()
    {
        if (_timer < _time)
        {
            _timer += Time.deltaTime;
            return INode.NodeState.Running;
        }
        else
        {
            _timer = 0;
            return INode.NodeState.Success;
        }
    }
}
