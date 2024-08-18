using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SaveSystem
{
    private readonly string _saveFilePath;
    private readonly int _saveTimerInterval;

    // ReSharper disable once MemberCanBePrivate.Global
    public SaveData Data { get; private set; }

    private bool _isSaveTimerActive;

    public SaveSystem()
    {
        var _settings = ServiceLocator.Resolve<SaveSettings>();

        _saveTimerInterval = _settings.SaveIntervalInSeconds;
        _saveFilePath = Path.Combine(Application.persistentDataPath, GlobalVariables.SAVE_DATA_FILE_NAME);

        SubscribeEvents();

        Initialize();
    }

    private void Initialize()
    {
        Data = new SaveData();

        LoadData();
    }

    private void SubscribeEvents()
    {
        Events.OnApplicationPause += Save;
        Events.OnApplicationQuit += Save;

        Events.GameStates.OnLevelStarted += OnLevelStarted;
        Events.GameStates.OnLevelEnd += OnLevelEnd;
    }

    public void SetData(SaveData saveData) => Data = saveData;

    private void LoadData()
    {
        try
        {
            var json = File.ReadAllText(_saveFilePath);
            Data = JsonUtility.FromJson<SaveData>(json);
        }
        catch (FileNotFoundException)
        {
            Data = new SaveData();
        }
    }

    private void Save()
    {
        var json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(_saveFilePath, json);
    }

    private void OnLevelStarted()
    {
        _isSaveTimerActive = true;
        StartAutoSaveLoop().Forget();
    }

    private void OnLevelEnd() => _isSaveTimerActive = false;

    private async UniTaskVoid StartAutoSaveLoop()
    {
        while (_isSaveTimerActive)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_saveTimerInterval));
            Save();
        }
    }
}

public class SaveData
{
    public LevelProgressData CurrentLevel = new();
    public int TotalDefeatedEnemiesNumber;
    public int CurrentHealth;
}

public class LevelProgressData
{
    public int LevelNumber = 1;
    public bool IsCurrentLevelStarted;
    public int TimeRemaining;
    public int SpawnedWavesNumber;
    public int DefeatedEnemiesNumber;
}