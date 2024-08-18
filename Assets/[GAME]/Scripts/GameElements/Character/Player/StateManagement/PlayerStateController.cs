using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using GAME.Utilities.StateMachine;
using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    [SerializeField] private StateParams _stateParams;

    private StateMachine<PlayerState> _stateMachine;
    private bool _isInitialized;

    public void Initialize()
    {
        InitializeStateMachine();
        _isInitialized = true;
    }

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine<PlayerState>();

        _stateMachine.AddState(PlayerState.Start, new PlayerStartState(_stateParams));

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

public enum PlayerState
{
    Start
}


public abstract class PlayerStateBase : StateBase<GameState>
{
    protected readonly PlayerStateController.StateParams StateParams;

    protected PlayerStateBase(PlayerStateController.StateParams stateParams)
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

public class PlayerStartState : PlayerStateBase
{
    public PlayerStartState(PlayerStateController.StateParams stateParams) : base(stateParams)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();

        Shoot();
    }

    private async void Shoot()
    {
        while (true)
        {
            StateParams.Weapon.Shoot();

            await UniTask.Delay(500);
        }
    }
}