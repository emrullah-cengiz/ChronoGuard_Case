using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelSystem    
{
    private readonly SaveSystem _saveSystem = ServiceLocator.Resolve<SaveSystem>();
    private readonly EnemySpawner _enemySpawner = ServiceLocator.Resolve<EnemySpawner>();

    public async void StartLevel()
    {
        var levelData = await GetLevelData(_saveSystem.Data.CurrentLevel.LevelNumber);

        _enemySpawner.Initialize(_saveSystem.Data.CurrentLevel, levelData);

    }

    private async UniTask<LevelData> GetLevelData(int levelNumber)
    {
        var operation = Resources.LoadAsync<LevelData>(
            $"{GlobalVariables.LEVEL_DATA_RESOURCE_PATH}/{GlobalVariables.LEVEL_DATA_PREFIX}{levelNumber}");

        await operation;

        return (LevelData)operation.asset;
    }
}