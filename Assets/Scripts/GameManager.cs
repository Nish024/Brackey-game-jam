using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton that survives scene loads.
/// Owns session-wide state: current game phase, run stats, config references.
/// Listens for game-over signals and drives scene transitions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Inspector Config ───────────────────────────
    [Header("UI References")]
    [Tooltip("Drag the GameOver game object here.")]
    [SerializeField] private GameOverPanel gameOverPanel;

    [Header("Starting Values")]
    [Tooltip("Cash the player starts with on Day 1.")]
    [SerializeField] private float startingCash = 1000f;

    [Header("Day Configs")]
    [Tooltip("Define the targets and spawn pool for each of the 3 days.")]
    public DayConfig[] dailyConfigs = new DayConfig[3];

    [Header("Penalty System")]
    [Tooltip("How many fakes can the player buy before getting fired?")]
    public int maxStrikes = 3;

    // ── Session State (persists across scenes) ─────
    [HideInInspector] public int currentDay = 1;
    [HideInInspector] public float currentCash;
    [HideInInspector] public float todaysProfitTarget;
    [HideInInspector] public int daysSurvived;
    [HideInInspector] public float peakNetWorth;
    [HideInInspector] public GameOverReason lastGameOverReason;
    [HideInInspector] public int loanUseCount; // tracks loan interest escalation across days
    [HideInInspector] public int currentStrikes;
    [HideInInspector] public bool isPaused;
    
    // Tracks models bought during the entire run so they never spawn again
    public System.Collections.Generic.HashSet<string> boughtModelNames = new System.Collections.Generic.HashSet<string>();

    // ── Public Accessors ───────────────────────────
    public float StartingCash => startingCash;
    
    public DayConfig GetCurrentDayConfig()
    {
        if (dailyConfigs == null || dailyConfigs.Length == 0) return new DayConfig();
        int index = Mathf.Clamp(currentDay - 1, 0, dailyConfigs.Length - 1);
        return dailyConfigs[index];
    }

    // ─────────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────────
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null); // Detach from parent to allow DontDestroyOnLoad without warnings
        DontDestroyOnLoad(gameObject);

        // Enforce 3 day limit even if the Inspector has older data saved
        if (dailyConfigs != null && dailyConfigs.Length > 3)
        {
            System.Array.Resize(ref dailyConfigs, 3);
        }

        // If we are starting directly in the Shop scene (for testing), initialize cash
        if (currentDay == 1 && currentCash == 0f)
        {
            currentCash = startingCash;
            todaysProfitTarget = GetCurrentDayConfig().profitTarget;
        }
    }

    void OnEnable()
    {
        GameEvents.OnGameOver += HandleGameOver;
        GameEvents.OnGameWon += HandleGameWon;
    }

    void OnDisable()
    {
        GameEvents.OnGameOver -= HandleGameOver;
        GameEvents.OnGameWon -= HandleGameWon;
    }

    // ─────────────────────────────────────────────────
    //  PUBLIC API — called by menu buttons, etc.
    // ─────────────────────────────────────────────────

    /// <summary>Start a brand-new game from Day 1.</summary>
    public void StartNewGame()
    {
        currentDay = 1;
        currentCash = startingCash;
        todaysProfitTarget = GetCurrentDayConfig().profitTarget;
        daysSurvived = 0;
        peakNetWorth = startingCash;
        loanUseCount = 0;
        currentStrikes = 0;
        isPaused = false;
        boughtModelNames.Clear();

        GameEvents.ClearAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Advance to the next day after a successful day. Does NOT reload the scene.</summary>
    public void AdvanceDay(float endOfDayCash)
    {
        currentDay++;
        
        if (currentDay > dailyConfigs.Length)
        {
            GameEvents.OnGameWon?.Invoke();
            return;
        }

        currentCash = endOfDayCash;
        todaysProfitTarget = GetCurrentDayConfig().profitTarget;
        daysSurvived = currentDay - 1;

        if (endOfDayCash > peakNetWorth)
            peakNetWorth = endOfDayCash;

        // DayManager handles the in-scene transition — no scene reload needed
    }

    // ─────────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────────

    private void HandleGameOver(GameOverReason reason)
    {
        lastGameOverReason = reason;
        daysSurvived = currentDay;
        isPaused = true;

        Debug.Log($"[GameManager] GAME OVER — Reason: {reason} | Day: {currentDay} | Peak Net Worth: ${peakNetWorth:F0}");

        if (gameOverPanel != null)
        {
            gameOverPanel.ShowGameOver(reason);
        }
    }

    private void HandleGameWon()
    {
        daysSurvived = currentDay;
        isPaused = true;

        Debug.Log($"[GameManager] GAME WON! | Day: {currentDay} | Peak Net Worth: ${peakNetWorth:F0}");

        if (gameOverPanel != null)
        {
            gameOverPanel.ShowVictory();
        }
    }
}

[System.Serializable]
public class DayConfig
{
    [Header("Goals")]
    public float profitTarget = 500f;

    [Header("Spawn Pool")]
    public int legitCount = 5;
    public int fakeCount = 10;
    public int stolenCount = 3;
    
    [Tooltip("Window size for the 'Guarantee window' to avoid long streaks of fakes.")]
    public int windowSize = 3;
    
    [Tooltip("Max same models allowed to spawn today.")]
    public int maxModelsPerDay = 2;
}
