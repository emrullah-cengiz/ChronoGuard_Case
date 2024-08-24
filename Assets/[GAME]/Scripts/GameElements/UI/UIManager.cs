using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class UIManager : SerializedMonoBehaviour
{
    [SerializeField] private GameHud _gameHud;

    private LevelSettings _levelSettings;
    private SaveSystem _saveSystem;

    private UIPanel _currentPanel;

    private void Awake()
    {
        _saveSystem = ServiceLocator.Resolve<SaveSystem>();
        _levelSettings = ServiceLocator.Resolve<LevelSettings>();
    }

    private void OnEnable()
    {
        Events.GameStates.OnGameStarted += OnGameStarted;
        Events.GameStates.OnLevelEnd += OnLevelEnd;
        Events.UI.OnPanelActionButtonClick += ClosePanel;
    }

    private void OnDisable()
    {
        Events.GameStates.OnGameStarted -= OnGameStarted;
        Events.GameStates.OnLevelEnd -= OnLevelEnd;
        Events.UI.OnPanelActionButtonClick -= ClosePanel;
    }

    private void OnGameStarted()
    {
        var data = _saveSystem.Data;

        if (!data.CurrentLevelProgress.IsCurrentLevelStarted)
            OpenPanel(data.CurrentLevelProgress.LevelNumber == 1 ? UIPanelType.FirstStart : UIPanelType.StartWithNextLevel);
        else
            OpenPanel(UIPanelType.Continue);
    }

    private async void OnLevelEnd(bool success)
    {
        var data = _saveSystem.Data;

        if (!success)
            await UniTask.WaitForSeconds(_levelSettings.PopupDelayAfterDie);

        OpenPanel(success ? (data.CurrentLevelProgress.LevelNumber == 1 ? UIPanelType.RestartGame : UIPanelType.Won) : UIPanelType.Dead);
    }

    private async void OpenPanel(UIPanelType type)
    {
        //prefab
        _currentPanel = await LoadPanel(type);

        _gameHud.SetActive(false);

        //object
        _currentPanel = Instantiate(_currentPanel, transform);

        _currentPanel.Initialize(_saveSystem.Data);
    }

    //Trigger by action button's UnityEvent
    private void ClosePanel(UIPanelType panelType)
    {
        Destroy(_currentPanel.gameObject);
        _gameHud.SetActive(true);
    }

    private static async UniTask<UIPanel> LoadPanel(UIPanelType type)
    {
        return await Extensions.LoadResource<UIPanel>(
            $"{GlobalVariables.UI_PANELS_RESOURCE_PATH}/{GlobalVariables.UI_PANELS_PREFIX + type}");
    }
}

public enum UIPanelType
{
    FirstStart,
    StartWithNextLevel,
    Won,
    Dead,
    Continue,
    RestartGame
}