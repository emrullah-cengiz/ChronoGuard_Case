using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : TransformObject, IInitializablePoolable<Enemy.SpawnData>, IDamagable
{
    public EnemyData Data;

    [SerializeField] private CharacterAnimatorController _animator;
    [SerializeField] private Health _health;
    [SerializeField] private RagdollController _ragdoll;

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Rigidbody _rb;
    
    private EnemyBehaviourTree _behaviourTree;

    private bool _initialized;
    public bool IsAlive { get; private set; }

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

        transform.position = spawnData.Position;
        transform.LookAt(spawnData.LookAtPosition);
            
        _health.Initialize(Data.MaxHealth);
        _behaviourTree.ReInitialize();
        _ragdoll.SetRagdollState(false);

        IsAlive = true;
    }

    private void Update()
    {
        if (_initialized && IsAlive)
            _behaviourTree.Update();
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        _health.TakeDamage(damage);

        // HitImpulse(hitDirection);

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

    // private void HitImpulse(Vector3 hitDirection)
    // {
    //     _rb.AddExplosionForce(,
    //         _hips.transform.position - hitDirection * .3f, .2f, 1, mode: ForceMode.Impulse);
    //
    // }

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