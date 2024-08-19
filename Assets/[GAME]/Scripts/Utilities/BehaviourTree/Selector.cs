using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace BehaviourTree
{
    public class Selector : CompositeNode
    {
        public Selector(List<Node> nodes) : base(nodes)
        {
        }

        public override NodeState Evaluate()
        {
            foreach (var node in _childNodes)
            {
                var nodeState = node.Evaluate();
                switch (nodeState)
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                    case NodeState.RUNNING:
                        return nodeState;
                }
            }

            return NodeState.FAILURE;
        }
    }
}