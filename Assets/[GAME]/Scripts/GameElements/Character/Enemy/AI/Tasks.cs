using BehaviourTree;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class MoveToPlayerTask : EnemyBehaviourTree.NodeBase
{
    private readonly EnemySettings _enemySettings;
    private readonly float _executeRate;
    private float _startTime;

    public MoveToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard) : base(@params, blackBoard)
    {
        _enemySettings = ServiceLocator.Resolve<EnemySettings>();
        _executeRate = _enemySettings.SetDestinationRate;
    }

    //calculate by executeRate 
    public override NodeState Evaluate()
    {
        if (Time.time - _startTime < _executeRate)
            return NodeState.SUCCESS;
        
        _startTime = Time.time;

        var dest = _playerSystem.Position - (-_blackBoard.CurrentDirection * (Params.Enemy.Data.AttackRange - .3f));

        // Debug.Log((dest - _playerSystem.Position).magnitude);
        
        // if ((dest - Params.Enemy.Position).magnitude < .5f)
        //     return NodeState.SUCCESS;

        // GizmosManager.ClearGizmos();
        // GizmosManager.AddDrawAction(() =>
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawSphere(dest, .03f);
        // });
        //
        Params.Agent.SetDestination(dest);

        return NodeState.SUCCESS;
    }
}

public class CheckForAttack : EnemyBehaviourTree.NodeBase
{
    private readonly float _attackRangeSqr;

    public CheckForAttack(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard) : base(@params, blackBoard)
    {
        _attackRangeSqr = Params.Enemy.Data.AttackRange * Params.Enemy.Data.AttackRange;
    }

    public override NodeState Evaluate()
    {
        if (_blackBoard.IsAttackCooldownEnd && _blackBoard.CurrentPlayerDistance.sqrMagnitude < _attackRangeSqr)
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
        Params.Animator.TriggerAttack();

        Debug.Log("Attack to player! " + Params.Enemy.name);

        StartAttackCoolDown().Forget();

        return NodeState.SUCCESS;
    }

    private async UniTaskVoid StartAttackCoolDown()
    {
        _blackBoard.IsAttackCooldownEnd = false;
        await UniTask.Delay((int)(Params.Enemy.Data.AttackRateInSeconds * 1000));
        _blackBoard.IsAttackCooldownEnd = true;
    }
}

public class LookToPlayerTask : EnemyBehaviourTree.NodeBase
{
    public LookToPlayerTask(EnemyBehaviourTree.TreeParams @params, EnemyBehaviourTree.BlackBoard blackBoard) : base(@params, blackBoard)
    {
    }

    public override NodeState Evaluate()
    {
        var targetRotation = Quaternion.LookRotation(_blackBoard.CurrentDirection);

        Params.Enemy.SetRotation(Quaternion.Lerp(Params.Enemy.Rotation, targetRotation, Time.deltaTime * .3f));
        
        return NodeState.SUCCESS;
    }
}