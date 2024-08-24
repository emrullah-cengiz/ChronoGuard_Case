using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPanel : SerializedMonoBehaviour
{
    [SerializeField] private UIPanelType _panelType;
    
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _defeatedNumberText;
    [SerializeField] private TMP_Text _totalNumberText;
    [SerializeField] private TMP_Text _timeLeftText;
    [SerializeField] private TMP_Text _healthText;
    
    [SerializeField] private Button _actionButton;
    
    private PlayerSystem _playerSystem;

    private SaveData _data;

    private void OnEnable() => _actionButton.onClick.AddListener(ActionButtonClicked);
    private void OnDisable() => _actionButton.onClick.RemoveListener(ActionButtonClicked);

    private bool _isInfoForPreviousLevel;
    
    public void Initialize(SaveData data)
    {
        _playerSystem = ServiceLocator.Resolve<PlayerSystem>();

        _data = data;

        _isInfoForPreviousLevel = _panelType switch
        {
            UIPanelType.StartWithNextLevel => false,
            UIPanelType.FirstStart => false,
            UIPanelType.Won => true,
            UIPanelType.Dead => true,
            UIPanelType.Continue => false,//*
            UIPanelType.RestartGame => true,
            _ => true
        };

        UpdateView();
    }

    private void UpdateView()
    {
        var progressData = _isInfoForPreviousLevel ? _data.PreviousLevelProgress : _data.CurrentLevelProgress;
        
        // _levelText.text = $"Level {_data.CurrentLevelProgress.LevelNumber - (_isInfoForPreviousLevel ? 1 : 0)}";
        _levelText.text = $"Level {progressData.LevelNumber}";
        _defeatedNumberText.text = progressData.DefeatedEnemiesNumber.ToString();
        _totalNumberText.text = _data.TotalDefeatedEnemiesNumber.ToString();
        _healthText.text = $"{progressData.CurrentHealth} / {_playerSystem.Properties.MaxHealth}";
        // _timeLeftText.text = _data.PreviousLevelProgress.TimeRemaining.GetTimeFormatInSeconds();
    }
 
    private void ActionButtonClicked() => 
        Events.UI.OnPanelActionButtonClick(_panelType);
}