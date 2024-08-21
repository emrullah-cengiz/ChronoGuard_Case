using Cysharp.Threading.Tasks;

public class LevelPlayingState : GameStateBase
{
    public override void OnEnter(params object[] @params)
    {
        Events.Level.OnLevelCountdownEnd += OnCountdownEnd;
        Events.Player.OnPlayerDead += OnPlayerDead;
        
        Events.Enemies.OnEnemyDead += OnEnemyDead;
    }

    public override void OnExit()
    {
        Events.Level.OnLevelCountdownEnd -= OnCountdownEnd;
        Events.Player.OnPlayerDead -= OnPlayerDead;
        
        Events.Enemies.OnEnemyDead -= OnEnemyDead;
    }

    private void OnCountdownEnd() => LevelEnd(true);
    private void OnPlayerDead() => LevelEnd(false);
    private void LevelEnd(bool s) => _gameStateManager.ChangeState(GameState.LevelEnd, s);

    private async void OnEnemyDead(Enemy enemy)
    {
        await UniTask.Delay((int)(_enemySettings.DespawnDelayAfterDead * 1000));
        _enemyPool.Despawn(enemy, enemy.Data.Type);        
    }
}