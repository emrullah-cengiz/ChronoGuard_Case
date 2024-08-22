using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerAttackHandler : MonoBehaviour
{
    #region References

    // [SerializeField] private SphereCollider _collider;
    [SerializeField] private Weapon _weapon;
    [SerializeField] private CharacterAnimatorController _animator;
    [SerializeField] private NavMeshAgent _agent;

    private PlayerSystem _playerSystem;
    private PlayerProperties _playerProperties;
    private PlayerSettings _playerSettings;

    #endregion

    private CancellationTokenSource _cancellationTokenSource;

    #region Target Selection

    // private Enemy _currentTarget;

    private Collider _currentTarget;
    private float _closestDistanceSqr = 100;
    private float _overlapCheckRate = 0.3f;
    private float _lastOverlapCheckTime;

    #endregion

    // private HashSet<Enemy> _nearEnemies;
    private Quaternion _targetRotation;

    private bool _isActive;
    private float _aimingProgressRatio;
    private float _attackRange;

    public void Initialize()
    {
        _playerProperties = ServiceLocator.Resolve<PlayerProperties>();
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
        _playerSettings = ServiceLocator.Resolve<PlayerSettings>();

        // _collider.radius = _playerProperties.AttackRange;

        // _nearEnemies = new();
        _currentTarget = null;
    }

    public void Reinitialize()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        //_nearEnemies.Clear();
        _currentTarget = null;
        _isActive = true;
        _attackRange = _playerProperties.AttackRange;

        // SelectTarget().Forget();
        ShootTarget().Forget();
    }

    public void StopActions()
    {
        _cancellationTokenSource.Cancel();
        _isActive = false;
    }

    private void Update()
    {
        if (!_isActive) return;

        //SelectTarget();

        LookTarget();
    }

    private void SelectTarget(Collider target)
    {
        //var target = GetClosestEnemy();

        if (target != _currentTarget)
        {
            Events.Player.OnLockedTarget?.Invoke(target is not null);
            _animator.SetAim(_currentTarget is not null);
            
            Debug.Log("Target selected");
        }
        
        _currentTarget = target;
    }

    private void LookTarget()
    {
        if (!_currentTarget)
            return;

        var dir = (_currentTarget.transform.position - _playerSystem.Position).normalized;
        _targetRotation = Quaternion.LookRotation(dir);

        //it's from inside of transform.Rotate
        var offset = Quaternion.Euler(Vector3.up * _playerSettings.AimOffset);
        _targetRotation *= _playerSystem.Rotation * offset * Quaternion.Inverse(_playerSystem.Rotation);

        var targetAngle = Quaternion.Angle(_targetRotation, _playerSystem.Rotation);

        _aimingProgressRatio = 1 - (_playerSettings.LookNewTargetSpeed * targetAngle * .1f * Time.deltaTime);

        // Debug.Log("_aimingProgressRatio: " + _aimingProgressRatio);

        _playerSystem.Rotation = Quaternion.Slerp(_playerSystem.Rotation, _targetRotation, 1 - _aimingProgressRatio);
    }

    private async UniTaskVoid ShootTarget()
    {
        while (_isActive)
        {
            if (_currentTarget != null)
            {
                if (_aimingProgressRatio <= _playerSettings.AimingProgressShootingThreshold)
                {
                    await UniTask.Yield();
                    continue;
                }

                _weapon.Shoot(_playerProperties.Damage, _playerProperties.BulletSpeed);
                // Debug.Log("SHOOT!");

                await UniTask.WaitForSeconds(_playerProperties.AttackRateInSeconds, cancellationToken: _cancellationTokenSource.Token);
            }
            else
                await UniTask.Yield();
        }
    }

    private Enemy GetClosestEnemy()
    {
        var playerPos = transform.position;
        var radius = _playerProperties.AttackRange;
        var results = new Collider[7];

        if (Physics.OverlapSphereNonAlloc(playerPos, radius, results, 1 << GlobalVariables.Layers.ENEMY) > 0)
        {
            var _closestDistanceSqr = radius * radius;
            Collider _closestEnemy = default;

            for (var i = 0; i < results.Length; i++)
            {
                var c = results[i];
                if (c is null)
                    break;

                var distanceSqr = (c.transform.position - playerPos).sqrMagnitude;
                if (distanceSqr < _closestDistanceSqr)
                {
                    _closestDistanceSqr = distanceSqr;
                    _closestEnemy = c;
                }
            }

            return _closestEnemy?.GetComponent<Enemy>();
        }

        return null;

        // Enemy nearestEnemy = null;
        // float nearestDistanceSqr = _playerProperties.AttackRange * _playerProperties.AttackRange;
        // var currentPosition = transform.position;
        //
        // foreach (var enemy in _nearEnemies)
        // {
        //     if (!enemy.IsAlive) continue;
        //
        //     var distanceSqr = (enemy.transform.position - currentPosition).sqrMagnitude;
        //     if (distanceSqr < nearestDistanceSqr)
        //     {
        //         nearestDistanceSqr = distanceSqr;
        //         nearestEnemy = enemy;
        //     }
        // }
        //
        // return nearestEnemy;
    }
    
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag(GlobalVariables.Tags.ENEMY) &&
    //         other.TryGetComponent(out Enemy enemy))
    //         _nearEnemies.Add(enemy);
    // }
    //
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag(GlobalVariables.Tags.ENEMY) &&
    //         other.TryGetComponent(out Enemy enemy))
    //     {
    //         _nearEnemies.Remove(enemy);
    //         if (enemy == _currentTarget)
    //             _currentTarget = null;
    //     }
    // }

    #region Closest enmey calculations

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GlobalVariables.Tags.ENEMY))
            CheckAndUpdateClosestEnemy(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if ((Time.time - _lastOverlapCheckTime < _overlapCheckRate) ||
            !other.CompareTag(GlobalVariables.Tags.ENEMY)) return;

        RecalculateClosestEnemy();
        _lastOverlapCheckTime = Time.time;
    }

    private void RecalculateClosestEnemy()
    {
        var results = new Collider[7];

        SelectTarget(null);
        _closestDistanceSqr = float.MaxValue;

        if (Physics.OverlapSphereNonAlloc(transform.position, _attackRange, results, 1 << GlobalVariables.Layers.ENEMY) <= 0)
            return;

        for (var i = 0; i < results.Length; i++)
        {
            if(results[i] is null) break;
            
            var enemy = results[i];
            CheckAndUpdateClosestEnemy(enemy);
        }
    }

    private void CheckAndUpdateClosestEnemy(Collider enemy)
    {
        var distanceSqr = (transform.position - enemy.transform.position).sqrMagnitude;

        if (distanceSqr > _closestDistanceSqr) return;

        _closestDistanceSqr = distanceSqr;
        SelectTarget(enemy);
    }

    #endregion

    private void OnDestroy() => _cancellationTokenSource?.Cancel();
}