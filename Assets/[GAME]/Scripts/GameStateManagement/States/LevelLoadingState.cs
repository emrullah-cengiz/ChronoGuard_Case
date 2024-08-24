public class LevelLoadingState : GameStateBase
{
    public override async void OnEnter(params object[] @params)
    {
        var continueLevel = @params.Length > 0 && (bool)@params[0];
        
        await _levelSystem.StartLevel(continueLevel);
        
        Events.GameStates.OnLevelStarted();
        
        _gameStateManager.ChangeState(GameState.LevelPlaying);
    }


    public override void OnExit()
    {
        base.OnExit();
    }
}