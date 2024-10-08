using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAttackHandler : MonoBehaviour
{
    #region References

    [SerializeField] private Weapon _weapon;
    [SerializeField] private CharacterAnimatorController _animator;
    [SerializeField] private NavMeshAgent _agent;

    [SerializeField] private Transform _targetIndicator;

    private PlayerSystem _playerSystem;
    private PlayerSettings _playerSettings;

    #endregion

    private CancellationTokenSource _CancellationTokenSource;
    // private CancellationTokenSource _targetSelectionCancellationTokenSource;

    #region Target Selection

    private Enemy _currentTarget;
    private readonly float _overlapCheckRate = 0.5f;

    #endregion

    private Quaternion _targetRotation;

    private bool _isActive;
    private float _aimingProgressRatio;
    private float _attackRange;

    public void Initialize()
    {
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
        _playerSettings = ServiceLocator.Resolve<PlayerSettings>();
    }

    public void Reinitialize()
    {
        _CancellationTokenSource = new CancellationTokenSource();
        _currentTarget = null;
        _isActive = true;
        _attackRange = _playerSystem.Properties.AttackRange;

        TargetSelectionWithOverlapTimer().Forget();
        ShootTarget().Forget();
    }

    public void StopActions()
    {
        _CancellationTokenSource.Cancel();
        _isActive = false;
    }

    private void Update()
    {
        if (!_isActive) return;

        AdjustTargetIndicator();

        LookTarget();
    }

    private void AdjustTargetIndicator()
    {
        if (_currentTarget is not null)
            _targetIndicator.transform.position = _currentTarget.transform.position;
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
            if (_currentTarget && _currentTarget.IsAlive)
            {
                if (_aimingProgressRatio <= _playerSettings.AimingProgressShootingThreshold)
                {
                    await UniTask.Yield();
                    continue;
                }

                _weapon.Shoot(_playerSystem.Properties.Damage, _playerSystem.Properties.BulletSpeed, _playerSystem.Properties.BulletNumberPerShot);
                // Debug.Log("SHOOT!");

                await UniTask.WaitForSeconds(_playerSystem.Properties.AttackRateInSeconds, cancellationToken: _CancellationTokenSource.Token);
            }
            else
                await UniTask.Yield();
        }
    }

    #region Target selection calculations

    private async UniTask TargetSelectionWithOverlapTimer()
    {
        while (_isActive)
        {
            await UniTask.WaitForSeconds(_overlapCheckRate, cancellationToken: _CancellationTokenSource.Token);
            RecalculateMostSuitableTarget();
        }
    }

    private Collider[] _overlapResults;
    private Enemy _tempClosestEnemy = null;
    private Enemy _tempLowestHealthEnemy = null;
    private void RecalculateMostSuitableTarget()
    {
        _overlapResults = new Collider[5];
        var playerPos = transform.position;

        if (Physics.OverlapSphereNonAlloc(playerPos, _attackRange, _overlapResults, 1 << GlobalVariables.Layers.ENEMY) <= 0)
        {
            SelectTarget(null);
            return;
        }

        var attackRangeSqr = _attackRange * _attackRange;

        var closestDistSqr = float.MaxValue;
        var lowestHealth = float.MaxValue;

        for (var i = 0; i < _overlapResults.Length; i++)
        {
            var coll = _overlapResults[i];
            if (coll == null) continue;

            var distSqr = (playerPos - coll.transform.position).sqrMagnitude;

            //patch for the Overlap bug :(
            if (attackRangeSqr < distSqr) continue;

            var enemy = coll.GetComponent<Enemy>();

            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                _tempClosestEnemy = enemy;
            }

            if (enemy.Health < lowestHealth)
            {
                lowestHealth = enemy.Health;
                _tempLowestHealthEnemy = enemy;
            }
        }

        if (_tempClosestEnemy == _tempLowestHealthEnemy)
            SelectTarget(_tempClosestEnemy);
        else
            //Select target by current health or distance
            SelectTarget(closestDistSqr / (playerPos - _tempLowestHealthEnemy!.transform.position).sqrMagnitude <= .6f
                ? _tempClosestEnemy
                : _tempLowestHealthEnemy);
    }

    private void SelectTarget(Enemy target)
    {
        if (target != _currentTarget)
        {
            var isNull = target is null;
            
            Events.Player.OnLockedTarget(!isNull);
            _animator.SetAim(!isNull);
            _targetIndicator.gameObject.SetActive(!isNull);
        }

        _currentTarget = target;
    }

    #endregion

    private void OnDestroy() => _CancellationTokenSource?.Cancel();
}