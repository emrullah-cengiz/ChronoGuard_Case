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

    private CancellationTokenSource _shootCancellationTokenSource;
    private CancellationTokenSource _targetSelectionCancellationTokenSource;

    #region Target Selection

    private Collider _currentTarget;
    private float _closestDistanceSqr = 100;
    private float _overlapCheckRate = 0.3f;

    #endregion

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
        _shootCancellationTokenSource = new CancellationTokenSource();
        //_nearEnemies.Clear();
        _currentTarget = null;
        _isActive = true;
        _attackRange = _playerProperties.AttackRange;

        // SelectTarget().Forget();
        ShootTarget().Forget();
    }

    public void StopActions()
    {
        _shootCancellationTokenSource.Cancel();
        _isActive = false;
    }

    private void Update()
    {
        if (!_isActive) return;

        LookTarget();
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

                await UniTask.WaitForSeconds(_playerProperties.AttackRateInSeconds, cancellationToken: _shootCancellationTokenSource.Token);
            }
            else
                await UniTask.Yield();
        }
    }

    #region Closest enmey calculations

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(GlobalVariables.Tags.ENEMY)) return;
        
        CheckAndUpdateClosestEnemy(other);
        
        _targetSelectionCancellationTokenSource?.Cancel();
        _targetSelectionCancellationTokenSource = new CancellationTokenSource();

        TargetSelectionWithOverlapTimer().Forget();
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
            if (results[i] is null) break;

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

    private void SelectTarget(Collider target)
    {
        //var target = GetClosestEnemy();

        if (target != _currentTarget)
        {
            Events.Player.OnLockedTarget?.Invoke(target is not null);
            _animator.SetAim(target is not null);

            Debug.Log("Target selected");
        }

        _currentTarget = target;
    }

    private async UniTask TargetSelectionWithOverlapTimer()
    {
        while (_isActive)
        {
            await UniTask.WaitForSeconds(_overlapCheckRate, cancellationToken: _targetSelectionCancellationTokenSource.Token);
            RecalculateClosestEnemy();
        }
    }

    #endregion

    private void OnDestroy() => _shootCancellationTokenSource?.Cancel();
}