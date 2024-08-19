using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace BehaviourTree
{
    public abstract class Node
    {
        public abstract NodeState Evaluate();
    }

    public abstract class CompositeNode : Node
    {
        protected List<Node> _childNodes { get; private set; }

        protected CompositeNode(List<Node> nodes) => _childNodes = nodes;
    }


    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }
}