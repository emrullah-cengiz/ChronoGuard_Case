public class LevelEndState : GameStateBase
{
    public override void OnEnter(params object[] @params)
    {
        Events.UI.OnPanelActionButtonClick += StartLevel;
        
        var success = (bool)@params[0];
        
        _levelSystem.Reset();
        
        Events.GameStates.OnLevelEnd?.Invoke(success);
    }

    public override void OnExit()
    {
        Events.UI.OnPanelActionButtonClick -= StartLevel;
    }

    private void StartLevel(UIPanelType panelType)
    {
        _gameStateManager.ChangeState(GameState.LevelLoading);
    }

}