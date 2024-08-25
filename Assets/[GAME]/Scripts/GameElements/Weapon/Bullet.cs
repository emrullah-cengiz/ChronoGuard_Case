using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BulletType
{
    Default,
    Shotgun
}

public class Bullet : MonoBehaviour, IInitializablePoolable<ShootData>
{
    [SerializeField] private ParticleSystem _hitParticle;
    [SerializeField] private TrailRenderer[] _trails;
    [SerializeField] private ParticleSystem[] _particles;

    private Bullet.Pool _bulletPool;
    private Pool<ParticleType> _particlePool;

    private CancellationTokenSource _autoDespawnCancellationTokenSource;

    public BulletType Type;

    private ShootData _data;

    private RaycastHit[] _hitResults;
    private Enemy _enemyToHit;
    private float _hitTime = 0;

    public void OnCreated()
    {
        _trails = GetComponentsInChildren<TrailRenderer>();
        _particles = GetComponentsInChildren<ParticleSystem>();

        _bulletPool = ServiceLocator.Resolve<Bullet.Pool>();
        _particlePool = ServiceLocator.Resolve<Pool<ParticleType>>();

        _hitResults = new RaycastHit[1];
        
        ClearEffects();
    }

    public void OnSpawned(ShootData data)
    {
        _data = data;
        
        _enemyToHit = null;
        _hitResults[0] = default;
        _hitTime = 0;

        transform.position = _data.StartPosition;
        transform.forward = _data.Direction; //.LookAt(transform.position + _data.Direction);

        CheckForHit(data);

        PlayEffects();
    }
    
    public void OnDespawned() => ClearEffects();

    private void CheckForHit(ShootData data)
    {
        Physics.RaycastNonAlloc(transform.position, data.Direction, _hitResults, data.Speed, 1 << GlobalVariables.Layers.ENEMY);

        var hitResult = _hitResults[0];
        if (hitResult.transform != null)
        {
            _enemyToHit = hitResult.transform.GetComponent<Enemy>();
            _hitTime = Time.time + hitResult.distance / data.Speed;
        }
    }

    private void Update()
    {
        transform.position += _data.Direction * (_data.Speed * Time.deltaTime);

        if (_enemyToHit is not null && Time.time >= _hitTime)
        {
            _enemyToHit.TakeDamage(_data.Damage, transform.forward, _data.Speed);

            SpawnParticle();

            _autoDespawnCancellationTokenSource.Cancel();
            _bulletPool.Despawn(this, Type);
        }
    }

    #region Particles

    private void SpawnParticle()
    {
        var obj = _particlePool.Spawn(ParticleType.Blood_Enemy, despawnDelay: .4f);
        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;
        obj.transform.localScale = Vector3.one * Random.Range(0.6f, 1);
    }

    private void PlayEffects()
    {
        foreach (var particle in _particles)
            particle.Play();
    }

    private void ClearEffects()
    {
        foreach (var trail in _trails)
            trail.Clear();
    }

    #endregion

    public void SetCancellationTokenSource(CancellationTokenSource cts) => _autoDespawnCancellationTokenSource = cts;

    public class Pool : Pool<Bullet, BulletType>
    {
        public Pool(PoolSettings poolSettings) : base(poolSettings)
        {
        }
    }
}