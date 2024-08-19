using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace BehaviourTree
{
    /// <summary>
    /// Success when any node returns Success.
    /// Running when any node returns Running.
    /// </summary>
    public class Parallel : CompositeNode
    {
        public Parallel(List<Node> nodes) : base(nodes)
        {
        }

        public override NodeState Evaluate()
        {
            bool isAnyRunning = false, isAnySuccess = false;

            foreach (var node in _childNodes)
            {
                switch (node.Evaluate())
                {
                    case NodeState.RUNNING:
                        isAnyRunning = true;
                        continue;
                    case NodeState.SUCCESS:
                        isAnySuccess = true;
                        continue;
                }
            }

            return isAnyRunning ? NodeState.RUNNING : isAnySuccess ? NodeState.SUCCESS : NodeState.FAILURE;
        }
    }
}