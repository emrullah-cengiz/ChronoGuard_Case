using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Transform _barrelPoint;

    [SerializeField] public ShootData shootData;

    private Bullet.Pool _bulletPool;

    private void Awake()
    {
        _bulletPool = ServiceLocator.Resolve<Bullet.Pool>();
    }

    public void Shoot()
    {
        shootData.StartPosition = _barrelPoint.position;
        shootData.Direction = _barrelPoint.forward;

        _bulletPool.Spawn(BulletType.Default, shootData);
    }
}
