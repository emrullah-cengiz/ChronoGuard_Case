using System.Collections;
using UnityEngine;

public enum BulletType
{
    Default
}

public class Bullet : MonoBehaviour, IInitializablePoolable<ShootData>
{
    public BulletType Type;
    
    private ShootData _data;
    
    public void OnCreated()
    {
    }
    
    public void OnSpawned(ShootData data)
    {
        _data = data;

        transform.position = _data.StartPosition;
    }

    private void Update()
    {
        transform.Translate(_data.Direction * (_data.Speed * Time.deltaTime));
    }

    public class Pool : Pool<Bullet, BulletType>
    {
        public Pool(PoolSettings poolSettings) : base(poolSettings)
        {
        }
    }

}
