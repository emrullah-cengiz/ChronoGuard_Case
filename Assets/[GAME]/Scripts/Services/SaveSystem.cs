using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class SaveSystem
{
    private readonly DataSaveService<SaveData> _saveService;
    private readonly LevelSettings _levelSettings;
    private readonly PlayerSystem _playerSystem;

    public SaveData Data => _saveService.Data;

    public SaveSystem()
    {
        var settings = ServiceLocator.Resolve<SaveSettings>();
        _levelSettings = ServiceLocator.Resolve<LevelSettings>();
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();

        _saveService = new DataSaveService<SaveData>(
            autoSave: false, settings.SaveIntervalInSeconds,
            Path.Combine(Application.persistentDataPath, GlobalVariables.SAVE_DATA_FILE_NAME));
        
        _saveService.Initialize(out var isDataFound);
        
        if(!isDataFound)
            ResetLevelProgress();

        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        Events.OnApplicationPause += _saveService.Save;
        Events.OnApplicationQuit += _saveService.Save;

        Events.GameStates.OnLevelStarted += OnLevelStarted;
        Events.GameStates.OnLevelEnd += OnLevelEnd;

        Events.Enemies.OnEnemyDead += OnEnemyDead;

        Events.Player.OnDamageTake += OnDamageTake;

        // Events.Level.OnLevelCountdownTick += OnLevelTimerTick;
        // Events.Enemies.OnWaveSpawned += OnWaveSpawned;
    }

    #region Data set operations

    private void OnLevelStarted()
    {
        Data.CurrentLevelProgress.IsCurrentLevelStarted = true;

        _saveService.Save();
        _saveService.StartAutoSave();
    }

    private void OnLevelEnd(bool success)
    {
        if (success)
        {
            Data.CurrentLevelProgress.LevelNumber++;
            Data.CurrentLevelProgress.LevelNumber = Data.CurrentLevelProgress.LevelNumber > _levelSettings.MaxLevel ? 1 : Data.CurrentLevelProgress.LevelNumber;
        }
        
        Data.PreviousLevelProgress = (LevelProgressData)Data.CurrentLevelProgress.Clone();

        ResetLevelProgress();

        _saveService.Save();
        _saveService.StopAutoSave();
    }

    private void OnEnemyDead(Enemy obj)
    {
        Data.CurrentLevelProgress.DefeatedEnemiesNumber++;
        Data.TotalDefeatedEnemiesNumber++;
    }

    // private void OnLevelTimerTick(int s) => Data.CurrentLevelProgress.TimeRemaining = s;
    // private void OnWaveSpawned() => Data.CurrentLevelProgress.SpawnedWavesNumber++;
    private void OnDamageTake(int damage, int health) => Data.CurrentLevelProgress.CurrentHealth = health;

    private void ResetLevelProgress()
    {
        Data.CurrentLevelProgress.IsCurrentLevelStarted = false;
        Data.CurrentLevelProgress.DefeatedEnemiesNumber = 0;
        Data.CurrentLevelProgress.CurrentHealth = _playerSystem.Properties.MaxHealth;
        // Data.CurrentLevelProgress.SpawnedWavesNumber = 0;
        // Data.CurrentLevelProgress.TimeRemaining = _levelSettings.LevelCountdownDurationInSeconds;
    }

    #endregion
}

public class SaveData
{
    public LevelProgressData CurrentLevelProgress = new();
    public LevelProgressData PreviousLevelProgress = new();
    public int TotalDefeatedEnemiesNumber;
}

[Serializable]
public class LevelProgressData : ICloneable
{
    public int LevelNumber = 1;
    public bool IsCurrentLevelStarted;
    public int DefeatedEnemiesNumber;
    // public int TimeRemaining;
    // public int SpawnedWavesNumber;

    public int CurrentHealth;
    public object Clone()
    {
        return this.MemberwiseClone();
    }
}