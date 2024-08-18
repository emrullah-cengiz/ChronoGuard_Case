using Cysharp.Threading.Tasks;

public class GameStartState : GameStateBase
{
    public GameStartState(GameStateManager.StateParams stateParams) : base(stateParams)
    {
    }
    
    public override async void OnEnter()
    {
        base.OnEnter();

        await UniTask.WaitForSeconds(2);
        
        Events.GameStates.OnGameStarted?.Invoke();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}