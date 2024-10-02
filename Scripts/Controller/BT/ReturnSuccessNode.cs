using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnSuccessNode : INode
{
    INode _child;

    public ReturnSuccessNode(INode child)
    {
        _child = child;
    }

    public INode.NodeState Evaluate()
    {
        _child?.Evaluate();
        return INode.NodeState.Success;
    }
}
