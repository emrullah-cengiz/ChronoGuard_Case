using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackHandler : MonoBehaviour
{
    [SerializeField] private Weapon _weapon;
    [SerializeField] private CharacterAnimatorController _animator;
    private PlayerSystem _playerSystem;
    private PlayerProperties _playerProperties;
    private PlayerSettings _playerSettings;

    private HashSet<Enemy> _nearEnemies;
    private Enemy _currentTarget;

    private CancellationTokenSource _cancellationTokenSource;

    private bool _isActive;

    public void Initialize()
    {
        _playerProperties = ServiceLocator.Resolve<PlayerProperties>();
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
        _playerSettings = ServiceLocator.Resolve<PlayerSettings>();

        _nearEnemies = new();
        _currentTarget = null;
    }

    public void Reinitialize()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _nearEnemies = new();
        _currentTarget = null;
        _isActive = true;

        SelectTarget().Forget();
        ShootTarget().Forget();
    }

    public void StopActions()
    {
        _cancellationTokenSource.Cancel();
        _isActive = false;
    }

    private void Update()
    {
        LookTarget();
    }

    private void LookTarget()
    {
        if (_currentTarget == null)
            return;

        var dir = (_currentTarget.Position - _playerSystem.Position).normalized;
        var targetRotation = Quaternion.LookRotation(dir);

        _playerSystem.Rotation = targetRotation;
        _playerSystem.transform.Rotate(Vector3.up * _playerSettings.AimRotationOffset, Space.World);
    }

    private async UniTaskVoid ShootTarget()
    {
        while (_isActive)
        {
            if (_currentTarget != null)
            {
                _animator.SetAim(_currentTarget != null);
                
                _weapon.Shoot(_weapon.Forward, _playerProperties.Damage, _playerProperties.BulletSpeed);

                await UniTask.WaitForSeconds(_playerProperties.AttackRateInSeconds, cancellationToken: _cancellationTokenSource.Token);
            }
            else
            {
                _animator.SetAim(_currentTarget != null);
                
                await UniTask.Yield();
            }
        }
    }

    private async UniTaskVoid SelectTarget()
    {
        while (_isActive)
        {
            await UniTask.WaitUntil(() => _currentTarget == null || !_currentTarget.IsAlive, cancellationToken: _cancellationTokenSource.Token);

            _currentTarget = GetNearestEnemy();

            if (_currentTarget == null)
                await UniTask.Yield();
        }
    }

    private Enemy GetNearestEnemy()
    {
        Enemy nearestEnemy = null;
        float nearestDistanceSqr = _playerProperties.AttackRange * _playerProperties.AttackRange;
        var currentPosition = transform.position;

        foreach (var enemy in _nearEnemies)
        {
            if (!enemy.IsAlive) continue;

            var distanceSqr = (enemy.transform.position - currentPosition).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GlobalVariables.Tags.ENEMY) &&
            other.TryGetComponent(out Enemy enemy))
            _nearEnemies.Add(enemy);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(GlobalVariables.Tags.ENEMY) &&
            other.TryGetComponent(out Enemy enemy))
        {
            _nearEnemies.Remove(enemy);
            if (enemy == _currentTarget)
                _currentTarget = null;
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
    }
}