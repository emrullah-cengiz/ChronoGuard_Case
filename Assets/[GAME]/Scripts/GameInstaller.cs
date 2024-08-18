using System;
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
        
        //Pools
        ServiceLocator.Register(new Bullet.Pool(_gameSettings.BulletPoolSettings));
    }

    private void Initialize()
    {
        _gameStateManager.Initialize();

    }
}
