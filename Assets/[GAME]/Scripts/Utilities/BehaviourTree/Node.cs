using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BehaviourTree
{
    public abstract class Node
    {
        protected CancellationTokenSource _cancellationTokenSource;

        protected Node(CancellationTokenSource cts = null)
        {
            _cancellationTokenSource = cts;
        }
        
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