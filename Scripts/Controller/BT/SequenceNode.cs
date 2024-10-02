using System.Collections.Generic;

public sealed class SequenceNode : INode
{
    List<INode> _childs;

    public SequenceNode(List<INode> childs)
    {
        _childs = childs;
    }

    public INode.NodeState Evaluate()
    {
        if (_childs == null || _childs.Count == 0)
            return INode.NodeState.Failure;

        foreach (INode child in _childs)
        {
            switch (child.Evaluate())
            {
                case INode.NodeState.Running:
                    return INode.NodeState.Running;
                case INode.NodeState.Success:
                    continue;
                case INode.NodeState.Failure:
                    return INode.NodeState.Failure;
            }
        }

        return INode.NodeState.Success;
    }
}