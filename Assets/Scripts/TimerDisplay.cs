using UnityEngine;
using TMPro;

/// <summary>
/// Listens to the timer tick event and displays time in MM:SS format.
/// No logic — purely presentation.
/// </summary>
public class TimerDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The text component to display the timer (supports UI or 3D Text).")]
    [SerializeField] private TMP_Text timerText;

    void OnEnable()
    {
        GameEvents.OnTimerTick += UpdateDisplay;
    }

    void OnDisable()
    {
        GameEvents.OnTimerTick -= UpdateDisplay;
    }

    private void UpdateDisplay(string clockTime)
    {
        timerText.text = clockTime;
    }
}
