using System.Linq;
using System.Threading;
using BehaviourTree;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveToPlayerTask : EnemyBehaviourTree.NodeBase
{
    private const float GUN_LENGTH = 1;
    private readonly float _minUpdateRate;
    private readonly float _maxUpdateRate;
    private readonly float _maxDistanceForMaxUpdateRateSqr;
    private float _executeRate;
    private float _startTime;
    private Vector3 _lastPlayerPosition;
    private float _lastDistanceSqr;

    public MoveToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard, CancellationTokenSource cts)
        : base(@params, blackBoard, cts)
    {
        _minUpdateRate = _enemySettings.AgentSetDestinationMinRate;
        _maxUpdateRate = _enemySettings.AgentSetDestinationMaxRate;
        _maxDistanceForMaxUpdateRateSqr = _enemySettings.AgentMaxDistanceForMaxUpdateRate * _enemySettings.AgentMaxDistanceForMaxUpdateRate;
        _executeRate = _minUpdateRate;
    }

    public override NodeState Evaluate()
    {
        if (Time.time - _startTime < _executeRate || !Params.Agent.enabled || !_blackBoard.IsPlayerAlive)
            return NodeState.SUCCESS;

        _startTime = Time.time;

        var currentPlayerPosition = _playerSystem.Position;
        var distanceSqr = (Params.Agent.transform.position - currentPlayerPosition).sqrMagnitude;

        if (currentPlayerPosition != _lastPlayerPosition || Mathf.Abs(distanceSqr - _lastDistanceSqr) > 0.1f)
        {
            _lastPlayerPosition = currentPlayerPosition;
            _lastDistanceSqr = distanceSqr;

            _executeRate = _minUpdateRate + (_maxUpdateRate - _minUpdateRate) * (distanceSqr / _maxDistanceForMaxUpdateRateSqr);
            _executeRate = Mathf.Clamp(_executeRate, _minUpdateRate, _maxUpdateRate);
        }
        else
            return NodeState.SUCCESS;

        //distance calculation by player velocity
        var velocityFactor = Mathf.Clamp(_playerSystem.Velocity.magnitude / _playerSystem.MaxAgentSpeed, 0, 1);
        // var distanceFactor = Mathf.Clamp(_blackBoard.CurrentPlayerDistance.magnitude / Params.Enemy.Data.AttackRange, 0, 1);

        // var extraDistance = distanceFactor * GUN_LENGTH;

        var dest = currentPlayerPosition - _blackBoard.PlayerDirection * (Params.Enemy.Data.AttackRange * (1 - velocityFactor) 
                                                                          + velocityFactor * GUN_LENGTH);

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
        if (_blackBoard.IsPlayerAlive && _blackBoard.IsAttackCooldownEnd &&
            _blackBoard.CurrentPlayerDistance.sqrMagnitude - DISTANCE_BUFFER < _blackBoard.AttackRangeSqr)
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

        if (Mathf.Abs(angle) < _enemySettings.HitToPlayerAngleThreshold)
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

    // private async UniTaskVoid TryHitPlayer()
    // {
    //     Params.Animator.TriggerAttack(Params.Enemy.Data.AttackType);
    //
    //     await UniTask.WaitForSeconds(_enemySettings.HitTimePerAnimation[Params.Enemy.Data.AttackType] /
    //                                  Params.Enemy.Data.AttackRateInSeconds, cancellationToken: _cancellationTokenSource.Token);
    //
    //     if (!HitIfPlayerOnForward(Params.Enemy.Position + Vector3.left * 0.3f, Params.Enemy.Forward))
    //         HitIfPlayerOnForward(Params.Enemy.Position + Vector3.right * 0.3f, Params.Enemy.Forward);
    // }
    //
    // private bool HitIfPlayerOnForward(Vector3 pos, Vector3 dir)
    // {
    //     var res = new RaycastHit[1];
    //     if (Physics.RaycastNonAlloc(pos, dir, res, Params.Enemy.Data.AttackRange, 1 << GlobalVariables.Layers.ENEMY) == 0)
    //         return false;
    //
    //     _playerSystem.TakeDamage(Params.Enemy.Data.Damage, Params.Enemy.Forward);
    //     
    //     Debug.DrawRay(pos, dir * Params.Enemy.Data.AttackRange, Color.green, 1);
    //     return true;
    // }
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