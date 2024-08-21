using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class Tree
    {
        protected CancellationTokenSource _taskCancellationTokenSource = new();

        private Node _root = null;

        protected void Initialize() => _root = SetupTree();

        public virtual void Update()
        {
            _root?.Evaluate();
        }

        public void Stop() => _taskCancellationTokenSource.Cancel();

        protected abstract Node SetupTree();
    }
}