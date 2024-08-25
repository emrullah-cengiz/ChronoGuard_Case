using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : TransformObject, IInitializablePoolable<Enemy.SpawnData>, IDamagable
{
    public EnemyData Data;

    [SerializeField] public EnemyAnimatorController Animator;
    [SerializeField] private Health _health;
    [SerializeField] private RagdollController _ragdoll;

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Collider _collider;

    private EnemySettings _enemySettings;
    private Pool<ParticleType> _particlePool;

    private EnemyBehaviourTree _behaviourTree;

    private CancellationTokenSource _hitCancellationTokenSource;

    private bool _initialized;
    public bool IsAlive { get; private set; }
    public int Health => _health.CurrentHealth;

    private const int IMPULSE_TIME_FACTOR = 30;

    public void OnCreated()
    {
        _enemySettings = ServiceLocator.Resolve<EnemySettings>();
        _particlePool = ServiceLocator.Resolve<Pool<ParticleType>>();

        _agent.updateUpAxis = false;
        _agent.updateRotation = false;
        
        _behaviourTree = new(new EnemyBehaviourTree.TreeParams()
        {
            Enemy = this,
            Agent = _agent,
            Animator = Animator
        });

        _initialized = true;
        _health.Initialize(Data.MaxHealth);

        _hitCancellationTokenSource = new();
    }

    //Reinitialize
    public void OnSpawned(SpawnData spawnData)
    {
        transform.position = spawnData.Position;
        transform.LookAt(spawnData.LookAtPosition);

        _collider.enabled = true;
        _agent.enabled = true;
        _agent.speed = _enemySettings.BaseSpeed * Data.SpeedMultiplier;

        Animator.Initialize(_enemySettings.BaseSpeed);
        Animator.SetAttackSpeedByAttackRate(Data.AttackType, Data.AttackRateInSeconds);

        _health.Initialize(Data.MaxHealth);
        _behaviourTree.ReInitialize(spawnData.DifficultyMultiplier);

        IsAlive = true;
    }

    public void OnDespawned()
    {
        _behaviourTree.Stop();

        _ragdoll.SetRagdollState(false);
    }

    private void Update()
    {
        if (_initialized && IsAlive)
            _behaviourTree.Update();
    }

    public void TakeDamage(int damage, Vector3 hitDirection, float? hitSpeed = null)
    {
        _hitCancellationTokenSource.Cancel();
        _hitCancellationTokenSource = new CancellationTokenSource();
        
        _health.TakeDamage(damage);

        HitImpulse(hitDirection, hitSpeed!.Value).Forget();
    }

    //Calling from _health's UnityEvent
    public void OnDead()
    {
        IsAlive = false;
        _collider.enabled = false;

        // _ragdoll.SetRagdollState(true);

        Animator.TriggerDie();

        _health.Activate(false);

        _agent.enabled = false;

        var particle = _particlePool.Spawn(ParticleType.Blood_Explosion, 1);
        particle.position = transform.position + Vector3.up * 1.4f;

        _hitCancellationTokenSource.Cancel();

        Events.Enemies.OnEnemyDead(this);
    }

    private async UniTaskVoid HitImpulse(Vector3 hitDirection, float hitSpeed)
    {
        _agent.enabled = false;
        // _rb.AddExplosionForce(_enemySettings.HitImpulseForce, transform.position - hitDirection * .3f,
        //     .3f, 1, mode: ForceMode.Impulse);

        // if (_hitCancellationTokenSource.IsCancellationRequested)
        //     return;

        var impulseDistance = _enemySettings.BaseImpulseDistance / Data.Mass;
        await transform.DOMove(Position + hitDirection * impulseDistance, impulseDistance / (hitSpeed / IMPULSE_TIME_FACTOR))
                       .SetEase(Ease.OutQuart)
                       .AwaitForComplete(TweenCancelBehaviour.CompleteAndCancelAwait, _hitCancellationTokenSource.Token);

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
        public float DifficultyMultiplier;
        public Vector3 Position;
        public Vector3 LookAtPosition;

        public SpawnData(Vector3 position, Vector3 lookAtPosition, float difficultyMultiplier)
        {
            DifficultyMultiplier = difficultyMultiplier;
            Position = position;
            LookAtPosition = lookAtPosition;
        }
    }
}