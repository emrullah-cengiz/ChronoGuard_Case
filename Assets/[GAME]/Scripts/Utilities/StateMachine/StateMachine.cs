using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAME.Utilities.StateMachine
{
    public interface IState
    {
        void OnEnter(object[] @params = null);
        void OnExit();
        void OnUpdate();
    }

    public abstract class StateBase<TStateEnum> : IState
    {
        public virtual void OnEnter(object[] @params)
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnUpdate()
        {
        }
    }

    public class StateMachine<TStateEnum> where TStateEnum : Enum
    {
        private TStateEnum _startState;
        private IState _currentState;
        private Dictionary<TStateEnum, IState> _states = new();

        public void AddState(TStateEnum stateType, IState state)
        {
            _states.TryAdd(stateType, state);
        }

        public void ChangeState(TStateEnum newState, params object[] @params)
        {
            _currentState?.OnExit();

            if (_states.TryGetValue(newState, out var state))
            {
                _currentState = state;
                _currentState.OnEnter(@params);
            }
            else
                Debug.LogWarning($"State {newState} not found!");
        }

        public void Init()
        {
            ChangeState(_startState);
        }

        public void Update()
        {
            _currentState?.OnUpdate();
        }

        public void SetStartState(TStateEnum state) => _startState = state;
    }
}