using System.Collections.Generic;
using System.Threading;
using BehaviourTree;
using UnityEngine;
using UnityEngine.AI;
using Tree = BehaviourTree.Tree;

public class EnemyBehaviourTree : Tree
{
    private readonly TreeParams _params;
    private readonly PlayerSystem _playerSystem;
    private readonly EnemySettings _enemySettings;

    private BlackBoard _blackBoard;

    private float _difficultyMultiplier;
    private const float MIN_DISTANCE_FOR_NAVMESH_SQR = 22 * 22;

    public EnemyBehaviourTree(TreeParams @params)
    {
        _params = @params;

        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
        _enemySettings = ServiceLocator.Resolve<EnemySettings>();

        Initialize();
    }

    protected override Node SetupTree()
    {
        _blackBoard = new BlackBoard();

        return new Parallel(new()
        {
            new SetAgentActiveStatus(_params, _blackBoard, _taskCancellationTokenSource),
            new LookToPlayerTask(_params, _blackBoard, _taskCancellationTokenSource),
            new Sequence(new List<Node>() //Movement
            {
                new CheckForDestinationUpdateRate(_params, _blackBoard, _taskCancellationTokenSource),
                new MoveToPlayerTask(_params, _blackBoard, _taskCancellationTokenSource),
            }),
            new Sequence(new() //Attack
            {
                new CheckForAttack(_params, _blackBoard, _taskCancellationTokenSource),
                new AttackToPlayerTask(_params, _blackBoard, _taskCancellationTokenSource),
            }),
        });
    }

    public void ReInitialize(float difficultyMultiplier)
    {
        _difficultyMultiplier = difficultyMultiplier;

        InitializeBlackBoard();
    }

    public override void Update()
    {
        UpdateBlackboard();

        base.Update();
    }

    #region BlackBoard

    private void InitializeBlackBoard()
    {
        _taskCancellationTokenSource = new CancellationTokenSource();

        _blackBoard._DifficultyMultiplier = _difficultyMultiplier;
        _blackBoard._FollowPlayersVelocity = Random.value < _blackBoard._DifficultyMultiplier;

        _blackBoard.IsAttackCooldownEnd = true;
        _blackBoard._AttackRangeSqr = _params.Enemy.Data.AttackRange * _params.Enemy.Data.AttackRange;

        _blackBoard._MinDestinationUpdateRate = _enemySettings.AgentSetDestinationMinRate;
        _blackBoard._MaxDestinationUpdateRate = _enemySettings.AgentSetDestinationMaxRate;
        _blackBoard._MaxDistanceForMaxUpdateRateSqr = _enemySettings.AgentMaxDistanceForMaxUpdateRate * _enemySettings.AgentMaxDistanceForMaxUpdateRate;
    }

    private void UpdateBlackboard()
    {
        _blackBoard.CurrentPlayerDistance = _playerSystem.Position - _params.Enemy.Position;
        _blackBoard.CurrentPlayerDistanceSqr = _blackBoard.CurrentPlayerDistance.sqrMagnitude;

        _blackBoard.PlayerDirection = _blackBoard.CurrentPlayerDistance.normalized;
        _blackBoard.IsPlayerAlive = _playerSystem.IsAlive;

        _blackBoard.IsOnNavMesh = _blackBoard.CurrentPlayerDistanceSqr < MIN_DISTANCE_FOR_NAVMESH_SQR;
        _blackBoard.DestinationUpdateRate = _blackBoard._MinDestinationUpdateRate;
    }

    #endregion

    public class TreeParams
    {
        public Enemy Enemy;
        public NavMeshAgent Agent;
        public EnemyAnimatorController Animator;
    }

    public class BlackBoard
    {
        #region Constants per respawn

        public float _DifficultyMultiplier;
        public bool _FollowPlayersVelocity;

        public float _AttackRangeSqr;
        public float _MinDestinationUpdateRate;
        public float _MaxDestinationUpdateRate;
        public float _MaxDistanceForMaxUpdateRateSqr;

        #endregion

        public float DestinationUpdateRate;
        public float LastDestinationUpdateTime;

        public bool IsOnNavMesh = false;
        public bool IsAttackCooldownEnd = true;

        public Vector3 PlayerDirection;
        public bool IsPlayerAlive;
        public Vector3 CurrentPlayerDistance;
        public float CurrentPlayerDistanceSqr;
    }

    public abstract class NodeBase : Node
    {
        protected readonly TreeParams Params;
        protected readonly BlackBoard _blackBoard;

        protected readonly PlayerSystem _playerSystem;
        protected readonly EnemySettings _enemySettings;

        protected NodeBase(TreeParams @params, BlackBoard blackBoard, CancellationTokenSource cts) : base(cts)
        {
            Params = @params;
            _blackBoard = blackBoard;
            _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
            _enemySettings = ServiceLocator.Resolve<EnemySettings>();
        }
    }
}