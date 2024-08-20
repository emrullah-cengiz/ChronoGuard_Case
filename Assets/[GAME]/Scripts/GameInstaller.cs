using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [SerializeField] private GameSettings _gameSettings;
    [SerializeField] private PlayerProperties _playerProperties;

    [SerializeField] private GameStateManager _gameStateManager;
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
        ServiceLocator.Register(_gameSettings);
        ServiceLocator.Register(_gameSettings.SaveSettings);
        ServiceLocator.Register(_gameSettings.PlayerSettings);
        ServiceLocator.Register(_gameSettings.WeaponSettings);
        ServiceLocator.Register(_gameSettings.EnemySettings);
        
        ServiceLocator.Register(_playerProperties);
        
        //Pools
        var pools = RegisterPools();

        //Systems
        ServiceLocator.Register(new SaveSystem());
        ServiceLocator.Register(_gameStateManager);
        ServiceLocator.Register(_playerSystem);
        ServiceLocator.Register(new EnemySpawner());
        ServiceLocator.Register(new LevelSystem());
        
        InitializePools(pools);
    }

    private List<IPool> RegisterPools()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var pools = new List<IPool>();

        RegisterAndAddToList(new Bullet.Pool(_gameSettings.WeaponSettings.PoolSettings));
        RegisterAndAddToList(new Enemy.Pool(_gameSettings.EnemySettings.PoolSettings));

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
            Events.OnApplicationPause?.Invoke();
    }

    private void OnApplicationQuit()
    {
        Events.OnApplicationQuit?.Invoke();
    }
}