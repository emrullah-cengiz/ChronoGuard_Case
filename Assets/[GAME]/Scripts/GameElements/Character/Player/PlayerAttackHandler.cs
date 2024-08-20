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
    private PlayerSystem _playerSystem;
    private PlayerProperties _playerProperties;

    private HashSet<Enemy> _nearEnemies;
    private Enemy _currentTarget;

    private CancellationTokenSource _cancellationTokenSource;

    public void Initialize()
    {
        _playerProperties = ServiceLocator.Resolve<PlayerProperties>();
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();

        _nearEnemies = new();
        _currentTarget = null;
    }

    public void Reinitialize()
    {
        _nearEnemies = new();
        _currentTarget = null;
        _cancellationTokenSource = new CancellationTokenSource();
        // _canAttack = true;

        SelectTarget().Forget();
        ShootTarget().Forget();
    }

    public void StopActions()
    {
        _cancellationTokenSource.Cancel();
    }

    private void Update()
    {
        if (_currentTarget == null)
            return;

        var dir = (_currentTarget.Position - _playerSystem.Position).normalized;
        var targetRotation = Quaternion.LookRotation(dir);

        _playerSystem.Rotation = targetRotation;

        _playerSystem.transform.Rotate(Vector3.up * _playerSystem._aimRotationOffset, Space.World);
    }

    private async UniTaskVoid ShootTarget()
    {
        while (true)
        {
            if (_currentTarget != null)
            {
                _weapon.Shoot(_weapon.Forward, _playerProperties.Damage, _playerProperties.BulletSpeed);

                await UniTask.Delay((int)(_playerProperties.AttackRateInSeconds * 1000), cancellationToken: _cancellationTokenSource.Token);
                
                if (_cancellationTokenSource.IsCancellationRequested)
                    break;
            }
            else
                await UniTask.Yield();
        }
    }

    private async UniTaskVoid SelectTarget()
    {
        while (true)
        {
            await UniTask.WaitUntil(() => _currentTarget == null || !_currentTarget.IsAlive, cancellationToken: _cancellationTokenSource.Token);

            if (_cancellationTokenSource.IsCancellationRequested)
                break;
            
            _currentTarget = GetNearestEnemy();

            if (_currentTarget == null)
                await UniTask.Yield();
        }
    }

    private Enemy GetNearestEnemy()
    {
        Enemy nearestEnemy = null;
        float nearestDistanceSqr = float.MaxValue;
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
}