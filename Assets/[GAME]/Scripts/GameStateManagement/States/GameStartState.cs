using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameStartState : GameStateBase
{
    private bool _continueLevel;

    public override void OnEnter(params object[] @params)
    {
        Events.UI.OnPanelActionButtonClick += OnPanelActionButtonClick;

        _continueLevel = _saveSystem.Data.CurrentLevelProgress.IsCurrentLevelStarted;
        
        Events.GameStates.OnGameStarted?.Invoke();
    }

    public override void OnExit() => 
        Events.UI.OnPanelActionButtonClick -= OnPanelActionButtonClick;

    private void OnPanelActionButtonClick(UIPanelType panelType) => 
        _gameStateManager.ChangeState(GameState.LevelLoading, _continueLevel);
}