using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner
{
    private LevelProgressData _progressData;
    private LevelData _levelData;

    private readonly EnemySettings _enemySettings = ServiceLocator.Resolve<EnemySettings>();
    private readonly PlayerSystem _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
    private readonly Enemy.Pool _enemyPool = ServiceLocator.Resolve<Enemy.Pool>();

    public void Initialize(LevelProgressData progressData, LevelData levelData)
    {
        _progressData = progressData;
        _levelData = levelData;

        WavesSpawnLoop().Forget();
    }

    private async UniTaskVoid WavesSpawnLoop()
    {
        var firstWaveIndex = _progressData.IsCurrentLevelStarted ? _progressData.SpawnedWavesNumber - 1 : 0;

        var elapsedSeconds = 0;

        for (var i = firstWaveIndex; i < _levelData.Waves.Count; i++)
        {
            var waveData = _levelData.Waves[i];

            await UniTask.Delay((waveData.SpawnTimeAfterLevelStart - elapsedSeconds) * 1000);

            elapsedSeconds = waveData.SpawnTimeAfterLevelStart;

            var wavePosition = waveData.SpawnDistanceFromPlayer * _playerSystem.Forward
                               + _playerSystem.Position;

#if UNITY_EDITOR

            GizmosManager.AddDrawAction(() =>
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(wavePosition, 2);
            });
#endif
            EnemiesSpawnLoop(waveData, wavePosition).Forget();
        }
    }

    private async UniTaskVoid EnemiesSpawnLoop(WaveData waveData, Vector3 wavePosition)
    {
        for (var i = 0; i < waveData.EnemyNumber; i++)
        {
            _enemyPool.Spawn(waveData.EnemiesType, 
                new Enemy.SpawnData(wavePosition + Random.insideUnitSphere * 2, _playerSystem.Position.SetYZero()));

            await UniTask.Delay(TimeSpan.FromSeconds(_enemySettings.InWaveSpawnDelayInSeconds));
        }
    }
}