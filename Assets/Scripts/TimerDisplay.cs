using UnityEngine;
using TMPro;

/// <summary>
/// Listens to the timer tick event and displays time in MM:SS format.
/// No logic — purely presentation.
/// </summary>
public class TimerDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    void OnEnable()
    {
        GameEvents.OnTimerTick += UpdateDisplay;
    }

    void OnDisable()
    {
        GameEvents.OnTimerTick -= UpdateDisplay;
    }

    private void UpdateDisplay(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
