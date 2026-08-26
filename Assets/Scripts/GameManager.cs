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

    [Tooltip("Profit target for Day 1.")]
    [SerializeField] private float baseProfitTarget = 500f;

    [Tooltip("How much the profit target increases each day.")]
    [SerializeField] private float profitTargetIncrease = 200f;

    // ── Session State (persists across scenes) ─────
    [HideInInspector] public int currentDay = 1;
    [HideInInspector] public float currentCash;
    [HideInInspector] public float todaysProfitTarget;
    [HideInInspector] public int daysSurvived;
    [HideInInspector] public float peakNetWorth;
    [HideInInspector] public GameOverReason lastGameOverReason;
    [HideInInspector] public int loanUseCount; // tracks loan interest escalation across days
    [HideInInspector] public bool isPaused;

    // ── Public Accessors ───────────────────────────
    public float StartingCash => startingCash;
    public float BaseProfitTarget => baseProfitTarget;
    public float ProfitTargetIncrease => profitTargetIncrease;

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

        // If we are starting directly in the Shop scene (for testing), initialize cash
        if (currentDay == 1 && currentCash == 0f)
        {
            currentCash = startingCash;
            todaysProfitTarget = baseProfitTarget;
        }
    }

    void OnEnable()
    {
        GameEvents.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnGameOver -= HandleGameOver;
    }

    // ─────────────────────────────────────────────────
    //  PUBLIC API — called by menu buttons, etc.
    // ─────────────────────────────────────────────────

    /// <summary>Start a brand-new game from Day 1.</summary>
    public void StartNewGame()
    {
        currentDay = 1;
        currentCash = startingCash;
        todaysProfitTarget = baseProfitTarget;
        daysSurvived = 0;
        peakNetWorth = startingCash;
        loanUseCount = 0;
        isPaused = false;

        GameEvents.ClearAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Advance to the next day after a successful day. Does NOT reload the scene.</summary>
    public void AdvanceDay(float endOfDayCash)
    {
        currentDay++;
        currentCash = endOfDayCash;
        todaysProfitTarget = baseProfitTarget + (profitTargetIncrease * (currentDay - 1));
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
}
