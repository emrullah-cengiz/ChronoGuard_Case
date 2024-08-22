using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerSystem : TransformObject, IDamagable
{
    private PlayerProperties Properties;

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private CharacterAnimatorController _animator;
    // [SerializeField] private PlayerStateController _playerStateController;
    [SerializeField] private PlayerAttackHandler _attackHandler;
    [SerializeField] private Weapon _weapon;
    [SerializeField] private Health _health;

    private int Health => _health.CurrentHealth;
    public Vector3 Velocity => _agent.velocity;
    
    private void OnEnable()
    {
        Debug.Log("PlayerSystem");

        Events.GameStates.OnGameStarted += Initialize;
        Events.GameStates.OnLevelStarted += Reinitialize;
        Events.GameStates.OnLevelEnd += StopActions;
    }

    private void OnDisable()
    {
        Events.GameStates.OnGameStarted -= Initialize;
        Events.GameStates.OnLevelStarted -= Reinitialize;
        Events.GameStates.OnLevelEnd -= StopActions;
    }

    private void Initialize()
    {
        Debug.Log("Initializing Player..");
        Properties = ServiceLocator.Resolve<PlayerProperties>();

        // _playerStateController.Initialize();
        
        _attackHandler.Initialize();
    }

    private void Reinitialize()
    {
        // _playerStateController.Initialize();
        Debug.Log("ReInitializing Player..");

        _animator.Initialize(Properties.Speed);
        _animator.SetDead(false);
        
        _health.Activate(true);
        
        _agent.enabled = true;
        
        _health.Initialize(Properties.MaxHealth);
        _attackHandler.Reinitialize();
    }

    private void StopActions(bool success)
    {
        _attackHandler.StopActions();
    }

    public void TakeDamage(int damage, Vector3 hitDirection = default)
    {
        _health.TakeDamage(damage);

        Events.Player.OnDamageTake?.Invoke(damage, Health);
    }

    public void OnDead()
    {
        _animator.SetDead(true);
        
        _health.Activate(false);
        
        _agent.enabled = false;
        
        Events.Player.OnPlayerDead?.Invoke();
    }

    public void LookAt(Vector3 position) => transform.LookAt(position);
}