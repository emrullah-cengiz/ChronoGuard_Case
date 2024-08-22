using System;
using System.Collections.Generic;
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
    private Transform[] _spawnPoints;

    public void Initialize(Transform[] spawnPoints)
    {
        _spawnPoints = spawnPoints;
    }

    public void Reinitialize(LevelProgressData progressData, LevelData levelData, bool continueLevel)
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

            if (_cts.IsCancellationRequested)
                return;

            elapsedSeconds = waveData.SpawnTimeAfterLevelStart;

            EnemiesSpawnLoop(waveData, _spawnPoints[Random.Range(0, _spawnPoints.Length)].position).Forget();

            Events.Enemies.OnWaveSpawned?.Invoke();
        }
    }

    private async UniTaskVoid EnemiesSpawnLoop(WaveData waveData, Vector3 wavePosition)
    {
        for (var i = 0; i < waveData.EnemyNumber; i++)
        {
            _enemyPool.Spawn(waveData.EnemiesType,
                new Enemy.SpawnData(wavePosition + Random.insideUnitSphere * _enemySettings.SpawnWaveRadius, _playerSystem.Position));

            await UniTask.WaitForSeconds(_enemySettings.InWaveSpawnDelayInSeconds, cancellationToken: _cts.Token);

            if (_cts.IsCancellationRequested)
                return;
        }
    }
}