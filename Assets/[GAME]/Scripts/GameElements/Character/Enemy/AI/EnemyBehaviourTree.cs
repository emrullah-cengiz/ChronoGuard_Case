using System;
using System.Collections.Generic;
using BehaviourTree;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Tree = BehaviourTree.Tree;

public class EnemyBehaviourTree : Tree
{
    private readonly TreeParams _params;

    private readonly PlayerSystem _playerSystem;
    
    public EnemyBehaviourTree(TreeParams @params)
    {
        _params = @params;
        
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();

        Initialize();
    }

    protected override Node SetupTree()
    {
        var blackBoard = new BlackBoard();

        blackBoard.CurrentPlayerDistance = _playerSystem.Position - _params.Enemy.Position;
        blackBoard.CurrentDirection = blackBoard.CurrentPlayerDistance.normalized;

        return new Selector(new()
        {
            new Sequence(new()
            {
                new LookToPlayerTask(_params, blackBoard),
                new CheckForAttack(_params, blackBoard),
                new AttackToPlayerTask(_params, blackBoard),
            }),
            new MoveToPlayerTask(_params, blackBoard)
        });
    }

    public abstract class NodeBase : Node
    {
        protected readonly TreeParams Params;
        protected readonly PlayerSystem _playerSystem;

        // protected readonly EnemySettings _enemySettings;
        protected readonly BlackBoard _blackBoard;

        protected NodeBase(TreeParams @params, BlackBoard blackBoard) : base()
        {
            Params = @params;
            _blackBoard = blackBoard;
            _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
        }
    }

    public class TreeParams
    {
        public Enemy Enemy;
        public NavMeshAgent Agent;
        public CharacterAnimatorController Animator;
        
    }

    public class BlackBoard
    {
        public bool IsAttackCooldownEnd = true;
        public Vector3 CurrentDirection;
        public Vector3 CurrentPlayerDistance;
    }
}