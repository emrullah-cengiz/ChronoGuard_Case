using UnityEngine;

namespace BehaviourTree
{
    public class Delay : Node
    {
        private readonly float _delayTime;
        private float _startTime;
        private bool _isRunning;
        
        public Delay(float delayTime)
        {
            _delayTime = delayTime;
        }

        public override NodeState Evaluate()
        {
            if (!_isRunning)
            {
                _startTime = Time.time;
                _isRunning = true;
            }

            if (Time.time - _startTime >= _delayTime)
            {
                _isRunning = false;
                return NodeState.SUCCESS;
            }

            return NodeState.RUNNING;
        }
    }
}