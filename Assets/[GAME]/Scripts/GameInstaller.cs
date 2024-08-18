using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [SerializeField] private GameSettings _gameSettings;

    [SerializeField] private GameStateManager _gameStateManager;

    private void Awake()
    {
        BindServices();

        Initialize();
    }

    private void BindServices()
    {
        //Settings
        ServiceLocator.Register(_gameSettings.BulletPoolSettings);
        ServiceLocator.Register(_gameSettings.WeaponSettings);
        ServiceLocator.Register(_gameSettings.SaveSettings);

        //Pools
        ServiceLocator.Register(new Bullet.Pool(_gameSettings.BulletPoolSettings));

        //Systems
        ServiceLocator.Register(_gameStateManager);
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