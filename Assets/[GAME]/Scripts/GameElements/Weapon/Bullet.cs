using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BulletType
{
    Default
}

public class Bullet : MonoBehaviour, IInitializablePoolable<ShootData>
{
    [SerializeField] private ParticleSystem _hitParticle;
    [SerializeField] private TrailRenderer[] _trails;
    [SerializeField] private ParticleSystem[] _particles;

    private Bullet.Pool _bulletPool;
    private Pool<ParticleType> _particlePool;

    public CancellationTokenSource CancellationTokenSource { get; private set; }

    public BulletType Type;

    private ShootData _data;

    private void Start()
    {
        _trails = GetComponentsInChildren<TrailRenderer>();
        _particles = GetComponentsInChildren<ParticleSystem>();

        _bulletPool = ServiceLocator.Resolve<Bullet.Pool>();
        _particlePool = ServiceLocator.Resolve<Pool<ParticleType>>();
    }

    public void OnCreated()
    {
        ClearEffects();
    }

    public void OnSpawned(ShootData data)
    {
        _data = data;

        transform.position = _data.StartPosition;
        transform.LookAt(transform.position + _data.Direction);

        PlayEffects();
    }

    public void OnDespawned()
    {
        ClearEffects();
    }

    private void Update()
    {
        transform.Translate(_data.Direction * (_data.Speed * Time.deltaTime), Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GlobalVariables.Tags.ENEMY) && other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(_data.Damage, transform.forward);

            OnHit();

            var obj = _particlePool.Spawn(ParticleType.Blood, despawnDelay: .4f);
            obj.transform.position = transform.position;
            obj.transform.rotation = transform.rotation;
            obj.transform.localScale = Vector3.one * Random.Range(0.6f, 1);

            CancellationTokenSource.Cancel();
            _bulletPool.Despawn(this, Type);
        }
    }

    private void OnHit()
    {
        // _hitParticle.gameObject.SetActive(true);
        // _hitParticle.Play();
    }

    private void PlayEffects()
    {
        foreach (var particle in _particles)
            // if (particle != _hitParticle)
            particle.Play();
    }

    private void ClearEffects()
    {
        // _hitParticle.gameObject.SetActive(false);
        foreach (var trail in _trails)
            trail.Clear();
        // foreach (var particle in _particles)
        //     particle.Stop();
    }

    public void SetCancellationTokenSource(CancellationTokenSource cts) => CancellationTokenSource = cts;

    public class Pool : Pool<Bullet, BulletType>
    {
        public Pool(PoolSettings poolSettings) : base(poolSettings)
        {
        }
    }
}