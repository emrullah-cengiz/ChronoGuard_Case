using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GAME.Utilities.StateMachine
{
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
    }

    public abstract class StateBase<TStateEnum> : IState
    {
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate() { }
    }

    public class StateMachine<TStateEnum> where TStateEnum : Enum
    {
        private IState _currentState;
        private Dictionary<TStateEnum, IState> _states = new();

        public void AddState(TStateEnum stateType, IState state)
        {
            _states.TryAdd(stateType, state);
        }

        public void ChangeState(TStateEnum newState)
        {
            _currentState?.OnExit();

            if (_states.TryGetValue(newState, out var state))
            {
                _currentState = state;
                _currentState.OnEnter();
            }
            else
                Debug.LogWarning($"State {newState} not found!");
        }

        public void Init()
        {
            ChangeState(_states.First().Key);
        }

        public void Update()
        {
            _currentState?.OnUpdate();
        }
    }
    
}