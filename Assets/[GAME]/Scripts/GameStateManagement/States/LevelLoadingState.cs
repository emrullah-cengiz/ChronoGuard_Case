public class LevelLoadingState : GameStateBase
{
    public override void OnEnter()
    {
        _levelSystem.StartLevel();
        
        Events.GameStates.OnLevelStarted?.Invoke();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}