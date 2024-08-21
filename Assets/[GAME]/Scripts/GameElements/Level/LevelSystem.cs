using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelSystem    
{
    private readonly SaveSystem _saveSystem = ServiceLocator.Resolve<SaveSystem>();
    private readonly EnemySpawner _enemySpawner = ServiceLocator.Resolve<EnemySpawner>();
    private readonly LevelSettings _levelSettings = ServiceLocator.Resolve<LevelSettings>();
    private readonly int _countdownDuration;

    private CancellationTokenSource _cts;

    public LevelSystem()
    {
        _countdownDuration = _levelSettings.LevelCountdownDurationInSeconds;
    }

    public async void StartLevel(bool continueLevel)
    {
        _cts = new CancellationTokenSource();

        var levelData = await GetLevelData(_saveSystem.Data.CurrentLevelProgress.LevelNumber);

        _enemySpawner.Initialize(progressData: _saveSystem.Data.CurrentLevelProgress, levelData, continueLevel);

        StartCountdown().Forget();
    }

    public void Reset()
    {
        _cts.Cancel();
        _enemySpawner.Reset();
    }

    private async UniTask<LevelData> GetLevelData(int levelNumber)
    {
        return await Extensions.LoadResource<LevelData>(
            $"{GlobalVariables.LEVEL_DATA_RESOURCE_PATH}/{GlobalVariables.LEVEL_DATA_PREFIX}{levelNumber}");
    }
    
    private async UniTaskVoid StartCountdown()
    {
        for (var i = _countdownDuration - 1; i >= 0; i--)
        {
            await UniTask.WaitForSeconds(1, cancellationToken: _cts.Token);
            
            if(_cts.IsCancellationRequested)
                return;
            
            Events.Level.OnLevelCountdownTick?.Invoke(i + 1);
        }
        
        Events.Level.OnLevelCountdownEnd?.Invoke();
    }

}