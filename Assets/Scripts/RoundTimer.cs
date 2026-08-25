using UnityEngine;

/// <summary>
/// Counts down from a configurable duration each day.
/// Fires OnTimerTick every frame and OnTimerExpired once at zero.
/// Owns nothing but the number — no rendering, no game-over logic.
/// </summary>
public class RoundTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Duration of the round in seconds. 360 = 6 minutes.")]
    [SerializeField] private float roundDuration = 360f;

    private float timeRemaining;
    private bool isRunning;
    private bool hasExpired;

    /// <summary>Current time left in seconds.</summary>
    public float TimeRemaining => timeRemaining;

    /// <summary>Whether the timer is actively counting down.</summary>
    public bool IsRunning => isRunning;

    /// <summary>Whether the timer has already hit zero this round.</summary>
    public bool HasExpired => hasExpired;

    void OnEnable()  => GameEvents.OnShopOpened += StartTimer;
    void OnDisable() => GameEvents.OnShopOpened -= StartTimer;

    // StartTimer is now called by OnShopOpened — NOT in Start()

    void Update()
    {
        if (!isRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            TimerComplete();
            return;
        }

        GameEvents.OnTimerTick?.Invoke(timeRemaining);
    }

    private void TimerComplete()
    {
        hasExpired = true;
        isRunning = false;
        timeRemaining = 0f;
        GameEvents.OnTimerTick?.Invoke(0f);
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

        GameEvents.OnTimerTick?.Invoke(timeRemaining);
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
}
