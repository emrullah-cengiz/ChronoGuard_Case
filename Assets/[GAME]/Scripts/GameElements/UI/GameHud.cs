using TMPro;
using UnityEngine;

public class GameHud : MonoBehaviour
{
    [SerializeField] private TMP_Text _countdownText;

    private void OnEnable() => Events.Level.OnLevelCountdownTick += UpdateCountdown;
    private void OnDisable() => Events.Level.OnLevelCountdownTick -= UpdateCountdown;

    private void UpdateCountdown(int s)
    {
        _countdownText.text = s.GetTimeFormatInSeconds();
    }

    public void SetActive(bool s)=> gameObject.SetActive(s);
    public void CompleteLevelCheatBtn() => Events.Level.OnLevelCountdownEnd();
}
