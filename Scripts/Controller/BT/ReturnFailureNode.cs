using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnFailureNode : INode
{
    INode _child;

    public ReturnFailureNode(INode child)
    {
        _child = child;
    }

    public INode.NodeState Evaluate()
    {
        _child?.Evaluate();
        return INode.NodeState.Failure;
    }
}
