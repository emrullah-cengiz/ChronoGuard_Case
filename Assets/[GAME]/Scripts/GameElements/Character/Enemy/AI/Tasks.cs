using System.Linq;
using System.Threading;
using BehaviourTree;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveToPlayerTask : EnemyBehaviourTree.NodeBase
{
    private readonly float _executeRate;
    private float _startTime;

    public MoveToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard, CancellationTokenSource cts) : base(@params,
        blackBoard, cts)
    {
        _executeRate = _enemySettings.AgentSetDestinationRate;
    }

    //calculate by executeRate 
    public override NodeState Evaluate()
    {
        if (!Params.Agent.enabled)
            return NodeState.SUCCESS;

        if (Time.time - _startTime < _executeRate)
            return NodeState.SUCCESS;

        _startTime = Time.time;

        var dest = _playerSystem.Position - (_blackBoard.PlayerDirection * (Params.Enemy.Data.AttackRange));

        Params.Agent.SetDestination(dest);

        return NodeState.SUCCESS;
    }
}

public class CheckForAttack : EnemyBehaviourTree.NodeBase
{
    private const float DISTANCE_BUFFER = .15f;

    public CheckForAttack(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard, CancellationTokenSource cts) : base(@params,
        blackBoard, cts)
    {
    }

    public override NodeState Evaluate()
    {
        if (_blackBoard.IsAttackCooldownEnd && _blackBoard.CurrentPlayerDistance.sqrMagnitude - DISTANCE_BUFFER < _blackBoard.AttackRangeSqr)
            return NodeState.SUCCESS;

        return NodeState.FAILURE;
    }
}

public class AttackToPlayerTask : EnemyBehaviourTree.NodeBase
{
    public AttackToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard, CancellationTokenSource cts) : base(@params,
        blackBoard, cts)
    {
    }

    public override NodeState Evaluate()
    {
        TryHitPlayer().Forget();

        StartAttackCoolDown().Forget();

        return NodeState.SUCCESS;
    }

    private async UniTaskVoid StartAttackCoolDown()
    {
        _blackBoard.IsAttackCooldownEnd = false;

        await UniTask.WaitForSeconds(Params.Enemy.Data.AttackRateInSeconds, cancellationToken: _cancellationTokenSource.Token);

        _blackBoard.IsAttackCooldownEnd = true;
    }

    private async UniTaskVoid TryHitPlayer()
    {
        Params.Animator.TriggerAttack(Params.Enemy.Data.AttackType);

        await UniTask.WaitForSeconds(_enemySettings.HitTimePerAnimation[Params.Enemy.Data.AttackType], cancellationToken: _cancellationTokenSource.Token);

        var res = new RaycastHit[1];
        if (Physics.RaycastNonAlloc(Params.Enemy.Position, Params.Enemy.Forward, res, Params.Enemy.Data.AttackRange
                // ,LayerMask.GetMask(GlobalVariables.Layers.PLAYER)
            ) > 0 && res.Any(x => x.collider.CompareTag("Player")))
        {
            _playerSystem.TakeDamage(Params.Enemy.Data.Damage);
        }
    }
}

public class LookToPlayerTask : EnemyBehaviourTree.NodeBase
{
    public LookToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard, CancellationTokenSource cts) : base(@params,
        blackBoard, cts)
    {
    }

    public override NodeState Evaluate()
    {
        var targetRotation = Quaternion.LookRotation(_blackBoard.PlayerDirection);

        Params.Enemy.Rotation = Quaternion.Lerp(Params.Enemy.Rotation, targetRotation, Time.deltaTime * 5f);

        return NodeState.SUCCESS;
    }
}