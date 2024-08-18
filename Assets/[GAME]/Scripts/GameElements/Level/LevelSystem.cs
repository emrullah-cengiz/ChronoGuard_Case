using UnityEngine;

public class LevelSystem
{
    private readonly EnemySpawner _enemySpawner = ServiceLocator.Resolve<EnemySpawner>();

    public void StartLevel()
    {
        WaveData _waveData = new(); //load from save data

        _enemySpawner.SpawnWave(_waveData);
    }
}