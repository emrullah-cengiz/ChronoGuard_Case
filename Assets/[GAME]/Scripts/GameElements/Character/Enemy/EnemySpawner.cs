using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
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

        var elapsedSeconds = 0f;

        foreach (var waveData in _levelData.Waves)
        {
            await UniTask.WaitForSeconds(waveData.SpawnTimeAfterLevelStart - elapsedSeconds, cancellationToken: _cts.Token);

            elapsedSeconds = waveData.SpawnTimeAfterLevelStart;

            Vector3 pos, playerPos = _playerSystem.Position;
            if (Random.value < _levelData.DifficultyMultiplier && _playerSystem.Velocity.magnitude > 0.3f) // && Random.value > 0.5f)
                pos = playerPos + _playerSystem.Velocity * 6;
            else
                pos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

            var distance = pos - playerPos;
            EnemiesSpawnLoop(waveData, distance).Forget();

            Events.Enemies.OnWaveSpawned();
        }
    }

    private async UniTaskVoid EnemiesSpawnLoop(WaveData waveData, Vector3 distance)
    {
        for (var i = 0; i < waveData.EnemyNumber; i++)
        {
            var playerPos = _playerSystem.Position;

            // NavMesh.is

            var pos = playerPos + distance + Random.insideUnitSphere * _enemySettings.SpawnWaveRadius;
            NavMesh.SamplePosition(pos, out var hit, 32, NavMesh.AllAreas);

            if (hit.hit)
                pos = playerPos + distance + Random.insideUnitSphere * _enemySettings.SpawnWaveRadius;
            else
            {
                var pos1 = pos;
                GizmosManager.AddDrawAction(() =>
                {
                    Gizmos.DrawCube(pos1, Vector3.one * .05f);
                });
                pos = Vector3.zero;
            }
            _enemyPool.Spawn(waveData.EnemiesType,
                             new Enemy.SpawnData(pos, playerPos,
                                                 _levelData.DifficultyMultiplier));

            await UniTask.WaitForSeconds(_enemySettings.InWaveSpawnDelayInSeconds, cancellationToken: _cts.Token);

            // if (_cts.IsCancellationRequested)
            //     return;
        }
    }
}