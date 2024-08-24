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

    private PlayerSystem _playerSystem;
    private PlayerSettings _playerSettings;

    #endregion

    private CancellationTokenSource _CancellationTokenSource;
    // private CancellationTokenSource _targetSelectionCancellationTokenSource;

    #region Target Selection

    private Enemy _currentTarget;
    private float _closestDistanceSqr = 100;
    private float _overlapCheckRate = 0.5f;

    #endregion

    private Quaternion _targetRotation;

    private bool _isActive;
    private float _aimingProgressRatio;
    private float _attackRange;

    public void Initialize()
    {
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
        _playerSettings = ServiceLocator.Resolve<PlayerSettings>();

        _currentTarget = null;
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
            if (_currentTarget && _currentTarget.IsAlive)
            {
                if (_aimingProgressRatio <= _playerSettings.AimingProgressShootingThreshold)
                {
                    await UniTask.Yield();
                    continue;
                }

                _weapon.Shoot(_playerSystem.Properties.Damage, _playerSystem.Properties.BulletSpeed);
                // Debug.Log("SHOOT!");

                await UniTask.WaitForSeconds(_playerSystem.Properties.AttackRateInSeconds, cancellationToken: _CancellationTokenSource.Token);
            }
            else
                await UniTask.Yield();
        }
    }

    #region Closest enmey calculations

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (!other.CompareTag(GlobalVariables.Tags.ENEMY)) return;
    //     
    //     CheckAndUpdateClosestEnemy(other);
    //     
    //     _targetSelectionCancellationTokenSource?.Cancel();
    //     _targetSelectionCancellationTokenSource = new CancellationTokenSource();
    //
    //     TargetSelectionWithOverlapTimer().Forget();
    // }

    private async UniTask TargetSelectionWithOverlapTimer()
    {
        while (_isActive)
        {
            await UniTask.WaitForSeconds(_overlapCheckRate, cancellationToken: _CancellationTokenSource.Token);
            RecalculateMostSuitableTarget();
        }
    }

    private void RecalculateMostSuitableTarget()
    {
        var results = new Collider[7];

        _closestDistanceSqr = float.MaxValue;

        if (Physics.OverlapSphereNonAlloc(transform.position, _attackRange, results, 1 << GlobalVariables.Layers.ENEMY) <= 0)
        {
            SelectTarget(null);
            return;
        }

        var playerPos = transform.position;

        Enemy closestEnemy = null;
        Enemy lowestHealthEnemy = null;
        float closestDistSqr = float.MaxValue;
        float lowestHealth = float.MaxValue;

        foreach (var coll in results)
        {
            if (coll == null) continue;

            var enemy = coll.GetComponent<Enemy>();

            var distSqr = (playerPos - enemy.transform.position).sqrMagnitude;

            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestEnemy = enemy;
            }

            if (enemy.Health < lowestHealth)
            {
                lowestHealth = enemy.Health;
                lowestHealthEnemy = enemy;
            }
        }

        if (closestEnemy == lowestHealthEnemy)
            SelectTarget(closestEnemy);
        else
            //Select target by current health or distance
            SelectTarget(closestDistSqr / (playerPos - lowestHealthEnemy!.transform.position).sqrMagnitude <= .4f
                ? closestEnemy
                : lowestHealthEnemy);
    }

    private void CheckAndUpdateClosestEnemy(Collider enemy)
    {
        // var distanceSqr = (transform.position - enemy.transform.position).sqrMagnitude;
        //
        // if (distanceSqr > _closestDistanceSqr) return;
        //
        // _closestDistanceSqr = distanceSqr;
        // SelectTarget(enemy);
    }

    private void SelectTarget(Enemy target)
    {
        if (target != _currentTarget)
        {
            Events.Player.OnLockedTarget(target is not null);
            _animator.SetAim(target is not null);

            Debug.Log("Target selected");
        }

        _currentTarget = target?.GetComponent<Enemy>();
    }

    #endregion

    private void OnDestroy() => _CancellationTokenSource?.Cancel();
}