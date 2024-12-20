using System.Collections.Generic;
using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [SerializeField] private GameSettings _gameSettings;
    // [SerializeField] private PlayerProperties _playerProperties;

    [SerializeField] private GameStateManager _gameStateManager;
    [SerializeField] private LevelSystem _levelSystem;
    [SerializeField] private PlayerSystem _playerSystem;

    private bool _initialized;

    private void Awake()
    {
        RegisterServices();
    }

    private void Start()
    {
        InitializeGame();
    }

    private void RegisterServices()
    {
        //Settings
        ServiceLocator.Register(_gameSettings.SaveSettings);
        ServiceLocator.Register(_gameSettings.PlayerSettings);
        ServiceLocator.Register(_gameSettings.LevelSettings);
        ServiceLocator.Register(_gameSettings.WeaponSettings);
        ServiceLocator.Register(_gameSettings.EnemySettings);
        ServiceLocator.Register(_gameSettings.VFXSettings);
        
        // ServiceLocator.Register(_playerProperties);
        
        //Pools
        var pools = RegisterPools();

        //Systems
        ServiceLocator.Register(_playerSystem);
        ServiceLocator.Register(new SaveSystem());
        ServiceLocator.Register(new EnemySpawner());
        ServiceLocator.Register(_levelSystem);
        ServiceLocator.Register(_gameStateManager);
        
        InitializePools(pools);
    }

    private List<IPool> RegisterPools()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var pools = new List<IPool>();

        RegisterAndAddToList(new Bullet.Pool(_gameSettings.WeaponSettings.PoolSettings));
        RegisterAndAddToList(new Enemy.Pool(_gameSettings.EnemySettings.PoolSettings));
        RegisterAndAddToList(new Pool<ParticleType>(_gameSettings.VFXSettings.PoolSettings));

        return pools;

        void RegisterAndAddToList<T>(T pool) where T : class, IPool
        {
            ServiceLocator.Register<T>(pool);
            pools.Add(pool);
        }
    }
    
    private void InitializePools(List<IPool> pools)
    {
        foreach (var pool in pools) 
            pool.Initialize();
    }
    
    private void InitializeGame()
    {
        _gameStateManager.Initialize();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            Events.OnApplicationPause();
    }

    private void OnApplicationQuit()
    {
        Events.OnApplicationQuit();
    }
}