using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RootMotion.FinalIK.RagdollUtility;

public class RepeaterNode : INode
{
    INode _child;
    int _count;
    bool _repeatForever;
    bool _endOnFailure;

    public RepeaterNode(int count, bool repeatForever, bool endOnFailure, INode child)
    {
        _child = child;
        _count = count;
        _repeatForever = repeatForever;
        _endOnFailure = endOnFailure;
    }

    public INode.NodeState Evaluate()
    {
        if (_count > 0 || _repeatForever)
        {
            var result = _child.Evaluate();
            if (result == INode.NodeState.Failure && _endOnFailure == true)
            {
                _count = 0;
                return INode.NodeState.Failure;
            }
            else if (result == INode.NodeState.Success)
            {
                _count--;
                return INode.NodeState.Success;
            }
        }
        return INode.NodeState.Failure;
    }
}
