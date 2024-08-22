using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : TransformObject, IInitializablePoolable<Enemy.SpawnData>, IDamagable
{
    public EnemyData Data;

    [SerializeField] private EnemyAnimatorController _animator;
    [SerializeField] private Health _health;
    [SerializeField] private RagdollController _ragdoll;

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Rigidbody _rb;

    private EnemySettings _enemySettings;

    private EnemyBehaviourTree _behaviourTree;

    private bool _initialized;
    public bool IsAlive { get; private set; }

    private void Awake()
    {
        _enemySettings = ServiceLocator.Resolve<EnemySettings>();
        
        _agent.updateUpAxis = false;
        _agent.updateRotation = false;
    }

    public void OnCreated()
    {
        _behaviourTree = new(new EnemyBehaviourTree.TreeParams()
        {
            Enemy = this,
            Agent = _agent,
            Animator = _animator
        });

        _initialized = true;
        _health.Initialize(Data.MaxHealth);
    }

    //Reinitialize
    public void OnSpawned(SpawnData spawnData)
    {
        _agent.enabled = true;
        _agent.speed = _enemySettings.BaseSpeed * _enemySettings.SpeedMultipliers[Data.SpeedMode];
        
        _animator.Initialize(_enemySettings.SpeedMultipliers.Max(x=>x.Value) * _enemySettings.BaseSpeed);
        _animator.SetAttackSpeedByAttackRate(Data.AttackType, Data.AttackRateInSeconds);

        transform.position = spawnData.Position;
        transform.LookAt(spawnData.LookAtPosition);

        _health.Initialize(Data.MaxHealth);
        _behaviourTree.ReInitialize();
        _ragdoll.SetRagdollState(false);

        IsAlive = true;
    }

    public void OnDespawned()
    {
        _behaviourTree.Stop();
    }

    private void Update()
    {
        if (_initialized && IsAlive)
            _behaviourTree.Update();
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        _health.TakeDamage(damage);

        HitImpulse(hitDirection).Forget();

        //reaction
    }

    //Calling from _health's UnityEvent
    public void OnDead()
    {
        IsAlive = false;

        _ragdoll.SetRagdollState(true);

        _health.Activate(false);

        _agent.enabled = false;

        Events.Enemies.OnEnemyDead?.Invoke(this);
    }

    private async UniTaskVoid HitImpulse(Vector3 hitDirection)
    {
        _agent.enabled = false;
        _rb.AddExplosionForce(_enemySettings.HitImpulseForce, transform.position - hitDirection * .3f,
            .3f, 1, mode: ForceMode.Impulse);
        
        await UniTask.WaitForSeconds(.1f);
        
        _agent.enabled = true;
    }


    public class Pool : Pool<Enemy, EnemyType>
    {
        public Pool(PoolSettings poolSettings) : base(poolSettings)
        {
        }
    }

    public struct SpawnData : IPoolableInitializationData
    {
        public Vector3 Position;
        public Vector3 LookAtPosition;

        public SpawnData(Vector3 position, Vector3 lookAtPosition)
        {
            Position = position;
            LookAtPosition = lookAtPosition;
        }
    }
}