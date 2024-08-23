using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

public class PlayerSystem : TransformObject, IDamagable
{
    public PlayerProperties Properties;

    [SerializeField] private NavMeshAgent _agent;

    [SerializeField] private CharacterAnimatorController _animator;

    // [SerializeField] private PlayerStateController _playerStateController;
    [SerializeField] private MovementController _movementController;
    [SerializeField] private PlayerAttackHandler _attackHandler;
    [SerializeField] private Weapon _weapon;
    [SerializeField] private Health _health;

    [SerializeField] private Rigidbody _rb;

    private Pool<ParticleType> _particlePool;
    private PlayerSettings _playerSettings;

    private int Health => _health.CurrentHealth;
    public Vector3 Velocity => _agent.velocity;
    public float MaxAgentSpeed => _agent.speed;

    public bool IsAlive { get; private set; }
    
    private void Awake()
    {
        _playerSettings = ServiceLocator.Resolve<PlayerSettings>();
    }

    private void OnEnable()
    {
        Events.GameStates.OnGameStarted += Initialize;
        Events.GameStates.OnLevelStarted += Reinitialize;
        Events.GameStates.OnLevelEnd += Deactivate;
    }

    private void OnDisable()
    {
        Events.GameStates.OnGameStarted -= Initialize;
        Events.GameStates.OnLevelStarted -= Reinitialize;
        Events.GameStates.OnLevelEnd -= Deactivate;
    }

    private void Initialize()
    {
        Debug.Log("Initializing Player..");
        
        _particlePool = ServiceLocator.Resolve<Pool<ParticleType>>();

        // _playerStateController.Initialize();

        _animator.Initialize(Properties.Speed);
        _attackHandler.Initialize();
    }

    private void Reinitialize()
    {
        // _playerStateController.Initialize();
        Debug.Log("Reinitializing Player..");

        IsAlive = true;
        
        _animator.Initialize(Properties.Speed);
        _animator.SetDead(false);

        _movementController.Activate(true);

        _health.Activate(true);

        _agent.enabled = true;

        _health.Initialize(Properties.MaxHealth);
        _attackHandler.Reinitialize();
    }

    private void Deactivate(bool success)
    {
        _movementController.Activate(false);
        _attackHandler.StopActions();
    }

    public void TakeDamage(int damage, Vector3 hitDirection = default)
    {
        _health.TakeDamage(damage);

        var obj = _particlePool.Spawn(ParticleType.Blood_Player, despawnDelay: .4f);
        obj.transform.position = transform.position + Vector3.up * 1.1f;
        obj.transform.rotation = Quaternion.LookRotation(hitDirection, Vector3.up);
        obj.transform.localScale = Vector3.one * Random.Range(0.6f, 1);

        //HitImpulse(hitDirection).Forget();

        Events.Player.OnDamageTake(damage, Health);
    }

    private async UniTaskVoid HitImpulse(Vector3 hitDirection)
    {
        _agent.enabled = false;
        _rb.isKinematic = false;

        _rb.AddExplosionForce(_playerSettings.HitImpulseForce, transform.position - hitDirection * .3f,
            .1f, 1, mode: ForceMode.Impulse);

        await UniTask.WaitForSeconds(_playerSettings.HitImpulseDuration);

        _agent.enabled = true;
        _rb.isKinematic = true;
    }

    public void OnDead()
    {
        IsAlive = false;
        
        _animator.SetDead(true);

        _health.Activate(false);

        _agent.enabled = false;

        Events.Player.OnPlayerDead();
    }

    public void SetPlayerProperties(PlayerProperties properties) => Properties = properties;
}