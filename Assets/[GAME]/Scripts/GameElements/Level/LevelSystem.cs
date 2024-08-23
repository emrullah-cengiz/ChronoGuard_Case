using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class LevelSystem : SerializedMonoBehaviour
{
    [SerializeField] private Transform[] _spawnPoints;

    private SaveSystem _saveSystem;
    private EnemySpawner _enemySpawner;
    private LevelSettings _levelSettings;
    private PlayerSystem _playerSystem;
    private int _countdownDuration;

    private CancellationTokenSource _cts;

    private void Awake()
    {
        _saveSystem = ServiceLocator.Resolve<SaveSystem>();
        _enemySpawner = ServiceLocator.Resolve<EnemySpawner>();
        _levelSettings = ServiceLocator.Resolve<LevelSettings>();
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();

        _countdownDuration = _levelSettings.LevelCountdownDurationInSeconds;
        _enemySpawner.Initialize(_spawnPoints);
    }

    public async UniTask StartLevel(bool continueLevel)
    {
        _cts = new CancellationTokenSource();

        var levelData = await GetLevelData(_saveSystem.Data.CurrentLevelProgress.LevelNumber);

        _playerSystem.SetPlayerProperties(levelData.PlayerProperties);

        _enemySpawner.Reinitialize(progressData: _saveSystem.Data.CurrentLevelProgress, levelData, continueLevel);

        StartCountdown().Forget();
    }

    public void ClearLevel()
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
        Events.Level.OnLevelCountdownTick(_countdownDuration);

        for (var i = _countdownDuration - 1; i >= 0; i--)
        {
            await UniTask.WaitForSeconds(1, cancellationToken: _cts.Token);

            if (_cts.IsCancellationRequested)
                return;

            Events.Level.OnLevelCountdownTick(i + 1);
        }

        Events.Level.OnLevelCountdownEnd();
    }


    //
    // private void OnDrawGizmos()
    // {
    //     if (_spawnPoints.Length == 0)
    //         return;
    //
    //     Gizmos.color = Color.yellow;
    //     foreach (var spawnPoint in _spawnPoints)
    //         Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
    // }
}