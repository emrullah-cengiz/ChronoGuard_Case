using Cysharp.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class Tree
    {
        private Node _root = null;

        protected void Initialize() => _root = SetupTree();
        public void Update() => _root?.Evaluate();

        protected abstract Node SetupTree();
    }
}