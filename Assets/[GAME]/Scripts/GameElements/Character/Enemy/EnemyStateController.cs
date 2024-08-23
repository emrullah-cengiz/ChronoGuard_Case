using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using GAME.Utilities.StateMachine;
using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    [SerializeField] private StateParams _stateParams;

    private StateMachine<EnemyState> _stateMachine;
    private bool _isInitialized;

    public void Initialize()
    {
        InitializeStateMachine();
        _isInitialized = true;
    }

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine<EnemyState>();

        _stateMachine.AddState(EnemyState.Start, new EnemyStartState(_stateParams));

        _stateMachine.Init();
    }

    private void Update()
    {
        if (_isInitialized)
            _stateMachine?.Update();
    }

    [Serializable]
    public class StateParams
    {
        public Weapon Weapon;
    }
}

public enum EnemyState
{
    Start
}


public abstract class EnemyStateBase : StateBase<GameState>
{
    protected readonly EnemyStateController.StateParams StateParams;

    protected EnemyStateBase(EnemyStateController.StateParams stateParams)
    {
        StateParams = stateParams;
    }
}

public class EnemyStartState : EnemyStateBase
{
    public EnemyStartState(EnemyStateController.StateParams stateParams) : base(stateParams)
    {
    }
}