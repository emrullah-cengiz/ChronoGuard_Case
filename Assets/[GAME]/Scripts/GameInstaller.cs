using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [SerializeField] private GameSettings _gameSettings;

    [SerializeField] private GameStateManager _gameStateManager;
    [SerializeField] private PlayerSystem _playerSystem;

    private void Awake()
    {
        BindServices();

        Initialize();
    }

    private void BindServices()
    {
        //Settings
        ServiceLocator.Register(_gameSettings.SaveSettings);
        ServiceLocator.Register(_gameSettings.PlayerSettings);
        ServiceLocator.Register(_gameSettings.WeaponSettings);
        ServiceLocator.Register(_gameSettings.EnemySettings);

        //Pools
        ServiceLocator.Register(new Bullet.Pool(_gameSettings.WeaponSettings.PoolSettings));
        ServiceLocator.Register(new Enemy.Pool(_gameSettings.EnemySettings.PoolSettings));

        //Systems
        ServiceLocator.Register(new SaveSystem());
        ServiceLocator.Register(_gameStateManager);
        ServiceLocator.Register(_playerSystem);
        ServiceLocator.Register(new EnemySpawner());
        ServiceLocator.Register(new LevelSystem());
    }

    private void Initialize()
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