using UnityEngine;

public class Weapon : TransformObject
{
    [SerializeField] private Transform _barrelPoint;
    [SerializeField] private ParticleSystem _muzzle;

    [SerializeField] public Transform Barrel => _barrelPoint;

    private Bullet.Pool _bulletPool;

    private void Awake() => _bulletPool = ServiceLocator.Resolve<Bullet.Pool>();

    private void OnEnable() => Events.GameStates.OnLevelEnd += OnReset;
    private void OnDisable() => Events.GameStates.OnLevelEnd -= OnReset;

    private void OnReset(bool obj) => _bulletPool.DespawnAll();

    public void Shoot(int damage, float speed)
    {
        var shootData = new ShootData()
        {
            Direction = _barrelPoint.forward,
            StartPosition = _barrelPoint.position,
            Damage = damage,
            Speed = speed
        };

        var bullet = _bulletPool.Spawn(BulletType.Default, shootData, despawnDelay: .5f, out var cancellationTokenSource);
        bullet.SetCancellationTokenSource(cancellationTokenSource);

        _muzzle.Play();
    }
}