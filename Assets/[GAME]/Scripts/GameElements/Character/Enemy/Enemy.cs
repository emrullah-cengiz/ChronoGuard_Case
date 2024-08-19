using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Character, IInitializablePoolable<Enemy.SpawnData>
{
    public EnemyData Data;

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private CharacterAnimatorController _animator;
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