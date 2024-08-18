public class LevelLoadingState : GameStateBase
{
    public override void OnEnter()
    {
        _levelSystem.StartLevel();
        
        
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}