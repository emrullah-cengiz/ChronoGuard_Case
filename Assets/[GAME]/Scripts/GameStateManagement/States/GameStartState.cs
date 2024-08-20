using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameStartState : GameStateBase
{
    public override async void OnEnter()
    {
        Debug.Log("GameStartState invoked");

        Events.GameStates.OnGameStarted?.Invoke();

        //open start panel
        
        //listen OnStartLevelRequested 
        
        //temp
        await UniTask.WaitForSeconds(1);
        
        _gameStateManager.ChangeState(GameState.LevelLoading);
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}