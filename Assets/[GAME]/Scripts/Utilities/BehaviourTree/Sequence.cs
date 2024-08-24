using System.Collections.Generic;

namespace BehaviourTree
{
    public class Sequence : CompositeNode
    {
        public Sequence(List<Node> nodes) : base(nodes)
        {
        }

        public override NodeState Evaluate()
        {
            foreach (var node in _childNodes)
            {
                var nodeState = node.Evaluate();

                switch (nodeState)
                {
                    case NodeState.SUCCESS:
                        continue;
                    case NodeState.FAILURE:
                    case NodeState.RUNNING:
                        return nodeState;
                }
            }

            return NodeState.SUCCESS;
        }
    }
}