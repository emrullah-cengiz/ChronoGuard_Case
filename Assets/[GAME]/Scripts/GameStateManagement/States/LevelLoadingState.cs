public class LevelLoadingState : GameStateBase
{
    public override void OnEnter(params object[] @params)
    {
        var continueLevel = @params.Length > 0 && (bool)@params[0];
        
        _levelSystem.StartLevel(continueLevel);
        
        Events.GameStates.OnLevelStarted?.Invoke();
        
        _gameStateManager.ChangeState(GameState.LevelPlaying);
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}