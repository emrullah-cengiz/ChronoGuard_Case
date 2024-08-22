using System;
using System.Collections.Generic;
using System.Threading;
using BehaviourTree;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Tree = BehaviourTree.Tree;

public class EnemyBehaviourTree : Tree
{
    private readonly TreeParams _params;
    private readonly PlayerSystem _playerSystem;

    private BlackBoard _blackBoard;

    public EnemyBehaviourTree(TreeParams @params)
    {
        _params = @params;
        
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();

        Initialize();
    }

    protected override Node SetupTree()
    {
        _blackBoard = new BlackBoard();

        return new Selector(new()
        {
            new Sequence(new()
            {
                new LookToPlayerTask(_params, _blackBoard, _taskCancellationTokenSource),
                new CheckForAttack(_params, _blackBoard, _taskCancellationTokenSource),
                new AttackToPlayerTask(_params, _blackBoard, _taskCancellationTokenSource),
            }),
            new MoveToPlayerTask(_params, _blackBoard, _taskCancellationTokenSource)
        });
    }

    public void ReInitialize()
    {
        _blackBoard.IsAttackCooldownEnd = true;
        _blackBoard.AttackRangeSqr = _params.Enemy.Data.AttackRange * _params.Enemy.Data.AttackRange;
        _taskCancellationTokenSource = new CancellationTokenSource();
    }
    
    public override void Update()
    {
        PrepareBlackboard();
        
        base.Update();
    }

    private void PrepareBlackboard()
    {
        _blackBoard.CurrentPlayerDistance = _playerSystem.Position - _params.Enemy.Position;
        _blackBoard.PlayerDirection = _blackBoard.CurrentPlayerDistance.normalized;
    }

    public abstract class NodeBase : Node
    {
        protected readonly TreeParams Params;
        protected readonly PlayerSystem _playerSystem;

        protected readonly EnemySettings _enemySettings;
        protected readonly BlackBoard _blackBoard;

        protected NodeBase(TreeParams @params, BlackBoard blackBoard, CancellationTokenSource cts) : base(cts)
        {
            Params = @params;
            _blackBoard = blackBoard;
            _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
            _enemySettings = ServiceLocator.Resolve<EnemySettings>();
            
        }
    }

    public class TreeParams
    {
        public Enemy Enemy;
        public NavMeshAgent Agent;
        public EnemyAnimatorController Animator;
        
    }

    public class BlackBoard
    {
        public bool IsAttackCooldownEnd = true;
        public Vector3 PlayerDirection;
        public Vector3 CurrentPlayerDistance;
        public float AttackRangeSqr;
    }
}