using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner
{
    private LevelProgressData _progressData;
    private LevelData _levelData;
    private bool _continueLevel;

    private readonly EnemySettings _enemySettings = ServiceLocator.Resolve<EnemySettings>();
    private readonly PlayerSystem _playerSystem = ServiceLocator.Resolve<PlayerSystem>();
    private readonly Enemy.Pool _enemyPool = ServiceLocator.Resolve<Enemy.Pool>();

    private CancellationTokenSource _cts = new();

    public void Initialize(LevelProgressData progressData, LevelData levelData, bool continueLevel)
    {
        _cts = new();
        
        _progressData = progressData;
        _levelData = levelData;
        _continueLevel = continueLevel;

        WavesSpawnLoop().Forget();
    }

    public void Reset()
    {
        _cts.Cancel();
        _enemyPool.DespawnAll();
    }

    private async UniTaskVoid WavesSpawnLoop()
    {
        // var firstWaveIndex = _continueLevel ? Mathf.Clamp(_progressData.SpawnedWavesNumber - 1, 0, int.MaxValue) : 0;

        var elapsedSeconds = 0;

        foreach (var waveData in _levelData.Waves)
        {
            await UniTask.WaitForSeconds(waveData.SpawnTimeAfterLevelStart - elapsedSeconds, cancellationToken: _cts.Token);
            
            if(_cts.IsCancellationRequested)
                return;

            elapsedSeconds = waveData.SpawnTimeAfterLevelStart;

            var randomPointOnCircle = Random.insideUnitCircle * waveData.SpawnDistanceFromPlayer;
            var wavePosition = new Vector3(randomPointOnCircle.x, 0, randomPointOnCircle.y) + _playerSystem.Position;

            EnemiesSpawnLoop(waveData, wavePosition).Forget();

            Events.Enemies.OnWaveSpawned?.Invoke();
            
// #if UNITY_EDITOR
//
//             GizmosManager.AddDrawAction(() =>
//             {
//                 Gizmos.color = Color.yellow;
//                 Gizmos.DrawWireSphere(wavePosition, 2);
//             });
// #endif
        }
    }

    private async UniTaskVoid EnemiesSpawnLoop(WaveData waveData, Vector3 wavePosition)
    {
        for (var i = 0; i < waveData.EnemyNumber; i++)
        {
            _enemyPool.Spawn(waveData.EnemiesType,
                new Enemy.SpawnData(wavePosition + Random.insideUnitSphere * 2, _playerSystem.Position));

            await UniTask.WaitForSeconds(_enemySettings.InWaveSpawnDelayInSeconds, cancellationToken: _cts.Token);
            
            if(_cts.IsCancellationRequested)
                return;
        }
    }
}