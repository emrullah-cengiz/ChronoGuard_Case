using System.Threading;
using BehaviourTree;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class CheckForDestinationUpdateRate : EnemyBehaviourTree.NodeBase
{
    public CheckForDestinationUpdateRate(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard, CancellationTokenSource cts)
        : base(@params, blackBoard, cts)
    {
    }

    public override NodeState Evaluate()
    {
        if (Time.time - _blackBoard.LastDestinationUpdateTime < _blackBoard.DestinationUpdateRate
            || !Params.Agent.enabled || !_blackBoard.IsPlayerAlive)
            return NodeState.FAILURE;

        return NodeState.SUCCESS;
    }
}

public class MoveToPlayerTask : EnemyBehaviourTree.NodeBase
{
    private const float GUN_LENGTH = 1.1f;

    private Vector3 _lastDestination;
    private float _lastDistanceSqr;

    public MoveToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard, CancellationTokenSource cts)
        : base(@params, blackBoard, cts)
    {
    }

    public override NodeState Evaluate()
    {
        var dest = _playerSystem.Position;

        //Is on front of the player?
        if (Vector3.Dot(_playerSystem.Forward, _blackBoard.CurrentPlayerDistance.normalized) < 0)
            dest -= _blackBoard.PlayerDirection * GUN_LENGTH;
        else if (_blackBoard._FollowPlayersVelocity && _blackBoard.CurrentPlayerDistance.magnitude < 3)
            dest += _playerSystem.Velocity * 2;

        //destination is not changed enough
        if (dest == _lastDestination && !(Mathf.Abs(_blackBoard.CurrentPlayerDistanceSqr - _lastDistanceSqr) > 0.1f))
            return NodeState.SUCCESS;

        //hold last position
        _lastDestination = dest;
        _lastDistanceSqr = _blackBoard.CurrentPlayerDistanceSqr;

        RecalculateDestinationUpdateRate();

        Params.Agent.SetDestination(dest);

        _blackBoard.LastDestinationUpdateTime = Time.time;

        return NodeState.SUCCESS;
    }

    private void RecalculateDestinationUpdateRate()
    {
        _blackBoard.DestinationUpdateRate = Mathf.Clamp(_blackBoard._MinDestinationUpdateRate +
                                                        (_blackBoard._MaxDestinationUpdateRate - _blackBoard._MinDestinationUpdateRate)
                                                        * (_blackBoard.CurrentPlayerDistanceSqr / _blackBoard._MaxDistanceForMaxUpdateRateSqr),
            _blackBoard._MinDestinationUpdateRate, _blackBoard._MaxDestinationUpdateRate);
    }
}

public class CheckForAttack : EnemyBehaviourTree.NodeBase
{
    public CheckForAttack(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard, CancellationTokenSource cts) : base(@params,
        blackBoard, cts)
    {
    }

    public override NodeState Evaluate()
    {
        if (_blackBoard.IsPlayerAlive && _blackBoard.IsAttackCooldownEnd &&
            _blackBoard.CurrentPlayerDistance.sqrMagnitude <= _blackBoard._AttackRangeSqr)
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

    private async UniTaskVoid TryHitPlayer()
    {
        Params.Animator.TriggerAttack(Params.Enemy.Data.AttackType);

        await UniTask.WaitForSeconds(_enemySettings.HitTimePerAnimation[Params.Enemy.Data.AttackType] /
                                     Params.Animator.GetAttackClipSpeed(Params.Enemy.Data.AttackType), cancellationToken: _cancellationTokenSource.Token);

        var angle = Quaternion.Angle(Params.Enemy.Rotation, quaternion.LookRotation(_blackBoard.PlayerDirection, Vector3.up));

        if (Mathf.Abs(angle) <= _enemySettings.PlayerAttackAngleThreshold)
            _playerSystem.TakeDamage(Params.Enemy.Data.Damage, Params.Enemy.Forward);

        // var res = new RaycastHit[1];
        // if (Physics.RaycastNonAlloc(Params.Enemy.Position, Params.Enemy.Forward, res, Params.Enemy.Data.AttackRange) > 0
        //     && res[0].collider.CompareTag(GlobalVariables.Tags.PLAYER))
        //     _playerSystem.TakeDamage(Params.Enemy.Data.Damage, Params.Enemy.Forward);
    }

    private async UniTaskVoid StartAttackCoolDown()
    {
        _blackBoard.IsAttackCooldownEnd = false;

        await UniTask.WaitForSeconds(Params.Enemy.Data.AttackRateInSeconds, cancellationToken: _cancellationTokenSource.Token);

        _blackBoard.IsAttackCooldownEnd = true;
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
        var targetRotation = Quaternion.LookRotation(_playerSystem.Position - Params.Enemy.Position);

        Params.Enemy.Rotation = Quaternion.Lerp(Params.Enemy.Rotation, targetRotation, Time.deltaTime * _enemySettings.RotationSpeed);

        return NodeState.SUCCESS;
    }
}