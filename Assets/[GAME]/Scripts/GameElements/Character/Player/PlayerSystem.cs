using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSystem : TransformObject, IDamagable
{
    // [SerializeField] private PlayerStateController _playerStateController;
    [SerializeField] private PlayerAttackHandler _attackHandler;
    [SerializeField] private Weapon _weapon;
    [SerializeField] private Health _health;
    
    [SerializeField] public float _aimRotationOffset;
    
    private PlayerProperties _playerProperties;

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
        _playerProperties = ServiceLocator.Resolve<PlayerProperties>();

        _attackHandler.Initialize();
    }

    private void Reinitialize()
    {
        // _playerStateController.Initialize();
        Debug.Log("ReInitializing Player..");

        _health.Initialize(_playerProperties.MaxHealth);
        _attackHandler.Reinitialize();
    }


    private void StopActions(bool success)
    {
        _attackHandler.StopActions();
    }

    public void TakeDamage(int damage, Vector3 hitDirection = default)
    {
        _health.TakeDamage(damage);

        Events.Player.OnDamageTake?.Invoke();
    }

    public void LookAt(Vector3 position) => transform.LookAt(position);
}