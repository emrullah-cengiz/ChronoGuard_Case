using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GAME.Utilities.StateMachine;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] private StateParams _stateParams;

    private StateMachine<GameState> _stateMachine;

    public void Initialize()
    {
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine<GameState>();
        
        _stateMachine.AddState(GameState.GameStart, new GameStartState(_stateParams));
        
        _stateMachine.Init();
    }

    private void Update()
    {
        _stateMachine.Update();
    }
    
    [Serializable]
    public class StateParams
    {
    }
}

public enum GameState
{
    GameStart,
    LevelLoading,
    LevelStart,
    LevelPlaying,
    LevelEnd,
}


public abstract class GameStateBase : StateBase<GameState>
{
    protected readonly GameStateManager.StateParams StateParams;

    public GameStateBase(GameStateManager.StateParams stateParams)
    {
        StateParams = stateParams;
    }
    
    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}
