using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Character, IInitializablePoolable<Enemy.SpawnData>, IDamagable
{
    public EnemyData Data;

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private CharacterAnimatorController _animator;
    [SerializeField] private Health _health;

    private EnemyBehaviourTree _behaviourTree;

    private bool _initialized;

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

    public void OnSpawned(SpawnData spawnData)
    {
        transform.position = spawnData.Position;
        transform.LookAt(spawnData.LookAtPosition);
    }

    private void Update()
    {
        if (_initialized)
            _behaviourTree.Update();
    }

    public void SetRotation(Quaternion rot) => transform.rotation = rot;
    
    public void TakeDamage(int damage) => _health.TakeDamage(damage);

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