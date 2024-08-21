using System;
using System.Collections;
using UnityEngine;

public enum BulletType
{
    Default
}

public class Bullet : MonoBehaviour, IInitializablePoolable<ShootData>
{
    [SerializeField] private ParticleSystem _hitParticle;
    
    public BulletType Type;
    
    private ShootData _data;
    
    public int Damage => _data.Damage;
    // public float Speed => _data.Speed;
    
    public void OnCreated()
    {
    }

    public void OnSpawned(ShootData data)
    {
        _data = data;

        transform.position = _data.StartPosition;
    }

    public void OnDeSpawned()
    {
    }

    private void Update()
    {
        transform.Translate(_data.Direction * (_data.Speed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GlobalVariables.Tags.ENEMY) && other.TryGetComponent(out Enemy enemy))
        {
            enemy.TakeDamage(Damage, transform.forward);
            
            OnHit();
            
            Events.Weapon.OnBulletHit?.Invoke(this);
        }
    }

    private void OnHit()
    {
        _hitParticle.Play();
    }

    public class Pool : Pool<Bullet, BulletType>
    {
        public Pool(PoolSettings poolSettings) : base(poolSettings)
        {
        }
    }

}
