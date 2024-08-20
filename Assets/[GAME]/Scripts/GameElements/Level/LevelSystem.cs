using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelSystem    
{
    private readonly SaveSystem _saveSystem = ServiceLocator.Resolve<SaveSystem>();
    private readonly EnemySpawner _enemySpawner = ServiceLocator.Resolve<EnemySpawner>();
    private readonly GameSettings _gameSettings = ServiceLocator.Resolve<GameSettings>();
    private readonly int _countdownDuration;

    public LevelSystem()
    {
        _countdownDuration = _gameSettings.LevelCountdownDurationInSeconds;
    }

    public async void StartLevel()
    {
        var levelData = await GetLevelData(_saveSystem.Data.CurrentLevel.LevelNumber);

        _enemySpawner.Initialize(progressData: _saveSystem.Data.CurrentLevel, levelData);

        StartCountdown().Forget();
    }

    private async UniTask<LevelData> GetLevelData(int levelNumber)
    {
        var operation = Resources.LoadAsync<LevelData>(
            $"{GlobalVariables.LEVEL_DATA_RESOURCE_PATH}/{GlobalVariables.LEVEL_DATA_PREFIX}{levelNumber}");

        await operation;

        return (LevelData)operation.asset;
    }
    
    private async UniTaskVoid StartCountdown()
    {
        for (var i = _countdownDuration - 1; i >= 0; i--)
        {
            await UniTask.Delay(1000);
            
            Events.Level.OnLevelTimerTick?.Invoke(i + 1);
        }
        
        Events.GameStates.OnLevelEnd?.Invoke(true);
    }

}