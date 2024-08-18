using UnityEngine;

public class Enemy : MonoBehaviour, IInitializablePoolable<Enemy.SpawnData>
{
    [SerializeField] private EnemyData Data;

    public void OnSpawned(SpawnData spawnData)
    {
        transform.position = spawnData.Position;
        transform.LookAt(spawnData.LookAtPosition);
    }

    public class Pool : Pool<Enemy, EnemyType>
    {
        public Pool(PoolSettings poolSettings) : base(poolSettings)
        {
        }
    }

    public struct SpawnData : IPoolableInitializationData
    {
        public Vector3 Position;
        public Vector3 LookAtPosition;
        public SpawnData(Vector3 position, Vector3 lookAtPosition)
        {
            Position = position;
            LookAtPosition = lookAtPosition;
        }
    }
}