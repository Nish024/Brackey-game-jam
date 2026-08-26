using UnityEngine;

/// <summary>
/// Counts down from a configurable duration each day.
/// Fires OnTimerTick every frame and OnTimerExpired once at zero.
/// Owns nothing but the number — no rendering, no game-over logic.
/// </summary>
public class RoundTimer : MonoBehaviour
{
    [Header("Clock Settings")]
    [Tooltip("Start hour of the day (e.g. 9 for 9:00 AM).")]
    [SerializeField] private int startHour = 9;
    [Tooltip("Start minute of the day.")]
    [SerializeField] private int startMinute = 0;
    
    [Tooltip("End hour of the day (e.g. 17 for 5:00 PM).")]
    [SerializeField] private int endHour = 17;
    [Tooltip("End minute of the day.")]
    [SerializeField] private int endMinute = 0;

    [Tooltip("Duration of the round in real-time seconds (e.g. 360 = 6 minutes).")]
    [SerializeField] private float roundDuration = 360f;

    private float timeRemaining;
    private bool isRunning;
    private bool hasExpired;

    /// <summary>Current real-time seconds left in the day.</summary>
    public float TimeRemaining => timeRemaining;

    /// <summary>Whether the timer is actively counting down.</summary>
    public bool IsRunning => isRunning;

    /// <summary>Whether the timer has already hit zero this round.</summary>
    public bool HasExpired => hasExpired;

    void OnEnable()  => GameEvents.OnShopOpened += StartTimer;
    void OnDisable() => GameEvents.OnShopOpened -= StartTimer;

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            TimerComplete();
            return;
        }

        GameEvents.OnTimerTick?.Invoke(GetFormattedTime());
    }

    private void TimerComplete()
    {
        hasExpired = true;
        isRunning = false;
        timeRemaining = 0f;
        
        GameEvents.OnTimerTick?.Invoke(GetFormattedTime());
        GameEvents.OnTimerExpired?.Invoke();

        // Force end the day immediately!
        Debug.Log("[RoundTimer] Timer hit zero! Force-ending the day.");
        GameEvents.OnDayEnd?.Invoke();
    }

    /// <summary>Start or restart the timer with the configured duration.</summary>
    public void StartTimer()
    {
        timeRemaining = roundDuration;
        isRunning = true;
        hasExpired = false;

        GameEvents.OnTimerTick?.Invoke(GetFormattedTime());
    }

    /// <summary>Pause the timer without resetting it.</summary>
    public void PauseTimer()
    {
        isRunning = false;
    }

    /// <summary>Resume the timer from where it was paused.</summary>
    public void ResumeTimer()
    {
        if (!hasExpired)
            isRunning = true;
    }

    /// <summary>Reset countdown to full duration without starting — waits for next OnShopOpened.</summary>
    public void ResetTimer()
    {
        isRunning  = false;
        hasExpired = false;
        timeRemaining = roundDuration;
    }

    /// <summary>Maps progress to a 12-hour clock format (e.g. 9:00, 9:05, 5:00) in 5-minute steps.</summary>
    public string GetFormattedTime()
    {
        float elapsed = roundDuration - timeRemaining;
        float progress = Mathf.Clamp01(elapsed / roundDuration);

        int startTotalMinutes = startHour * 60 + startMinute;
        int endTotalMinutes = endHour * 60 + endMinute;

        float currentTotalMinutes = startTotalMinutes + (endTotalMinutes - startTotalMinutes) * progress;

        // Round to nearest 5 minutes
        int roundedMinutes = Mathf.RoundToInt(currentTotalMinutes / 5f) * 5;

        int hour = (roundedMinutes / 60) % 24;
        int minute = roundedMinutes % 60;

        // Format to 12-hour format (e.g. 17 -> 5)
        int displayHour = hour % 12;
        if (displayHour == 0) displayHour = 12;

        return $"{displayHour}:{minute:00}";
    }
}
