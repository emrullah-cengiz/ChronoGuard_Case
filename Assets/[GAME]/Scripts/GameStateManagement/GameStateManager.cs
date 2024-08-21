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
        _stateMachine.AddState(GameState.LevelEnd, new LevelEndState());

        _stateMachine.SetStartState(GameState.GameStart);
        _stateMachine.Init();
    }

    public void ChangeState(GameState state, params object[] @params)
    {
        _stateMachine.ChangeState(state, @params);
    }

    private void Update()
    {
        // _stateMachine.Update();
    }
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
    protected readonly GameStateManager _gameStateManager = ServiceLocator.Resolve<GameStateManager>();
    protected readonly LevelSystem _levelSystem = ServiceLocator.Resolve<LevelSystem>();
    protected readonly SaveSystem _saveSystem = ServiceLocator.Resolve<SaveSystem>();
    protected readonly Enemy.Pool _enemyPool = ServiceLocator.Resolve<Enemy.Pool>();
    protected readonly EnemySettings _enemySettings = ServiceLocator.Resolve<EnemySettings>();

    // protected readonly GameStateManager.StateParams StateParams;
}