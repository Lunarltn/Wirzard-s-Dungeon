using System;
using System.Diagnostics;

public sealed class ActionNode : INode
{
    Func<INode.NodeState> _onUpdate = null;

    public ActionNode(Func<INode.NodeState> onUpdate)
    {
        _onUpdate = onUpdate;
    }

    public INode.NodeState Evaluate() => _onUpdate?.Invoke() ?? INode.NodeState.Failure;
}
