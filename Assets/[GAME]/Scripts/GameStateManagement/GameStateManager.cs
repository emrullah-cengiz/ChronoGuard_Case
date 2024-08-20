using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GAME.Utilities.StateMachine;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GameStateManager : MonoBehaviour
{
    private StateMachine<GameState> _stateMachine;

    public void Initialize()
    {
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine<GameState>();

        _stateMachine.AddState(GameState.GameStart, new GameStartState());
        _stateMachine.AddState(GameState.LevelLoading, new LevelLoadingState());
        _stateMachine.AddState(GameState.LevelPlaying, new LevelPlayingState());

        _stateMachine.SetStartState(GameState.GameStart);
        _stateMachine.Init();
    }

    public void ChangeState(GameState state)
    {
        _stateMachine.ChangeState(state);
    }

    private void Update()
    {
        // _stateMachine.Update();
    }
    //
    // [Serializable]
    // public class StateParams
    // {
    // }
}

public enum GameState
{
    GameStart,
    LevelLoading,
    LevelPlaying,
    LevelEnd,
}


public abstract class GameStateBase : StateBase<GameState>
{
    protected readonly GameStateManager _gameStateManager;
    protected readonly LevelSystem _levelSystem;
    protected readonly Enemy.Pool _enemyPool;
    protected readonly EnemySettings _enemySettings;

    // protected readonly GameStateManager.StateParams StateParams;

    protected GameStateBase()
    {
        _gameStateManager = ServiceLocator.Resolve<GameStateManager>();
        _levelSystem = ServiceLocator.Resolve<LevelSystem>();
        _enemyPool = ServiceLocator.Resolve<Enemy.Pool>();
        _enemySettings = ServiceLocator.Resolve<EnemySettings>();
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