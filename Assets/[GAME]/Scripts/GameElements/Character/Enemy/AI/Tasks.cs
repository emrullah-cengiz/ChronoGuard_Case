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

    public MoveToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard) : base(@params, blackBoard)
    {
        _executeRate = _enemySettings.SetDestinationRate;
    }

    //calculate by executeRate 
    public override NodeState Evaluate()
    {
        if (Time.time - _startTime < _executeRate)
            return NodeState.SUCCESS;

        _startTime = Time.time;

        var dest = _playerSystem.Position - (_blackBoard.PlayerDirection * (Params.Enemy.Data.AttackRange));

        // Debug.Log((dest - _playerSystem.Position).magnitude);

        // if ((dest - Params.Enemy.Position).magnitude < .5f)
        //     return NodeState.SUCCESS;

        GizmosManager.ClearGizmos();
        GizmosManager.AddDrawAction(() =>
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(dest + Vector3.up, .05f);
        });
        
        Params.Agent.SetDestination(dest);

        return NodeState.SUCCESS;
    }
}

public class CheckForAttack : EnemyBehaviourTree.NodeBase
{
    private const float DISTANCE_BUFFER = 15f;
    
    public CheckForAttack(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard) : base(@params, blackBoard)
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
    public AttackToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard) : base(@params, blackBoard)
    {
    }

    public override NodeState Evaluate()
    {
        StartAttackCoolDown().Forget();

        Params.Animator.TriggerAttack();

        TryHitPlayer().Forget();

        return NodeState.SUCCESS;
    }

    private async UniTaskVoid StartAttackCoolDown()
    {
        _blackBoard.IsAttackCooldownEnd = false;

        await UniTask.Delay((int)(Params.Enemy.Data.AttackRateInSeconds * 1000));

        _blackBoard.IsAttackCooldownEnd = true;
    }

    private async UniTaskVoid TryHitPlayer()
    {
        await UniTask.Delay((int)(_enemySettings.HitTimePerAnimation[Params.Enemy.Data.AttackType] * 1000));

        var res = new RaycastHit[1];
        if (Physics.RaycastNonAlloc(Params.Enemy.Position, Params.Enemy.Forward, res, Params.Enemy.Data.AttackRange,
                LayerMask.GetMask(GlobalVariables.Layers.PLAYER)) > 0)
            _playerSystem.TakeDamage(Params.Enemy.Data.Damage);
    }
}

public class LookToPlayerTask : EnemyBehaviourTree.NodeBase
{
    public LookToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard) : base(@params, blackBoard)
    {
    }

    public override NodeState Evaluate()
    {
        var targetRotation = Quaternion.LookRotation(_blackBoard.PlayerDirection);

        Params.Enemy.Rotation = Quaternion.Lerp(Params.Enemy.Rotation, targetRotation, Time.deltaTime * 5f);

        return NodeState.SUCCESS;
    }
}