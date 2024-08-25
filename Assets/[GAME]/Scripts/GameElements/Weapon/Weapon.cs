using UnityEngine;

public class Weapon : TransformObject
{
    [SerializeField] private Transform _barrelPoint;
    [SerializeField] private ParticleSystem _muzzle;

    [SerializeField] public Transform Barrel => _barrelPoint;

    private Bullet.Pool _bulletPool;
    private WeaponSettings _weaponSettings;

    private void Awake()
    {
        _bulletPool = ServiceLocator.Resolve<Bullet.Pool>();
        _weaponSettings = ServiceLocator.Resolve<WeaponSettings>();
    }

    private void OnEnable() => Events.GameStates.OnLevelEnd += OnReset;
    private void OnDisable() => Events.GameStates.OnLevelEnd -= OnReset;

    private void OnReset(bool obj) => _bulletPool.DespawnAll();

    public void Shoot(int damage, float speed, int bulletNumber = 1)
    {
        if (bulletNumber == 1)
            SpawnBullet(BulletType.Default, damage, speed, _barrelPoint.forward);
        else
        {
            var angleStepPerBullet = _weaponSettings.ShotgunAngleOffsetPerBullet;
            var totalAngle = (bulletNumber - 1) * angleStepPerBullet; 
            var startAngle = -totalAngle / 2f;

            var rotationOffset = Quaternion.Euler(Vector3.up * -90);

            for (var i = 0; i < bulletNumber; i++)
            {
                var currentAngle = startAngle + i * angleStepPerBullet;
                var direction = rotationOffset * new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), 0, 
                                                                    Mathf.Sin(currentAngle * Mathf.Deg2Rad));
                
                SpawnBullet(BulletType.Shotgun, damage, speed, _barrelPoint.TransformDirection(direction));
            }
        }

        _muzzle.Play();
    }

    private void SpawnBullet(BulletType type, int damage, float speed, Vector3 direction)
    {
        var shootData = new ShootData()
        {
            Direction = direction,
            StartPosition = _barrelPoint.position,
            Damage = damage,
            Speed = speed
        };

        var bullet = _bulletPool.Spawn(type, shootData, despawnDelay: .5f, out var cancellationTokenSource);
        bullet.SetCancellationTokenSource(cancellationTokenSource);
    }
}