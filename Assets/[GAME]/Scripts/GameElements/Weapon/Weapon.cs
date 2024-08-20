using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Weapon : TransformObject
{
    [SerializeField] private Transform _barrelPoint;
    [SerializeField] private ParticleSystem _muzzle;

    // [SerializeField] public ShootData shootData;

    private Bullet.Pool _bulletPool;

    private void Awake()
    {
        _bulletPool = ServiceLocator.Resolve<Bullet.Pool>();
    }

    private void OnEnable() => Events.Weapon.OnBulletHit += OnBulletHitEnemy;
    private void OnDisable() => Events.Weapon.OnBulletHit -= OnBulletHitEnemy;

    private async void OnBulletHitEnemy(Bullet bullet)
    {
        await UniTask.Delay(100);
        _bulletPool.Despawn(bullet, bullet.Type);
    }

    public void Shoot(Vector3 direction, int damage, float speed)
    {
        var shootData = new ShootData()
        {
            Direction = direction,
            StartPosition = _barrelPoint.position,
            Damage =  damage,
            Speed = speed
        }; 

        _bulletPool.Spawn(BulletType.Default, shootData);
        
        _muzzle.Play();
    }
}
