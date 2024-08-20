using Cysharp.Threading.Tasks;

public class LevelPlayingState : GameStateBase
{
    public override void OnEnter()
    {
        Events.Enemies.OnEnemyDead += OnEnemyDead;
    }

    public override void OnExit()
    {
        Events.Enemies.OnEnemyDead -= OnEnemyDead;
    }
    
    private async void OnEnemyDead(Enemy enemy)
    {
        await UniTask.Delay((int)(_enemySettings.DespawnDelayAfterDead * 1000));
        _enemyPool.Despawn(enemy, enemy.Data.Type);        
    }
}