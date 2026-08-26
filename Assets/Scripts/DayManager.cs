using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The shop-scene hub. Lives in the shop scene alongside everything else.
/// Drives the day lifecycle:
///   Day Intro → Shop Open → (timer/button) Day End → Auction → Next Day Intro → repeat
/// </summary>
public class DayManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private ScreenFader        screenFader;
    [SerializeField] private RoundTimer         roundTimer;
    [SerializeField] private PurchasedInventory purchasedInventory;
    [SerializeField] private Ledger             ledger;
    [SerializeField] private AuctionResolver    auctionResolver;
    [SerializeField] private AuctionResultsPanel auctionResultsPanel;
    [SerializeField] private AuctionButtonController auctionButtonController;

    private int currentDay;
    private float netWorthAtDayStart;
    private bool dayEnded;

    // ─────────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────────

    void OnEnable()
    {
        GameEvents.OnDayEnd          += HandleDayEnd;
        GameEvents.OnAuctionComplete += HandleAuctionComplete;
    }

    void OnDisable()
    {
        GameEvents.OnDayEnd          -= HandleDayEnd;
        GameEvents.OnAuctionComplete -= HandleAuctionComplete;
    }

    void Start()
    {
        // Read day number from GameManager; default to 1 for isolated scene testing
        currentDay = GameManager.Instance != null ? GameManager.Instance.currentDay : 1;

        // Begin the day intro (black screen → "DAY X" → fade out → open shop)
        screenFader.ShowDayIntro(currentDay, OpenShop);
    }

    // ─────────────────────────────────────────────────
    //  SHOP OPEN
    // ─────────────────────────────────────────────────

    private void OpenShop()
    {
        netWorthAtDayStart = ledger.NetWorth;
        dayEnded = false;

        // Tell AuctionButtonController what the starting cash is
        auctionButtonController?.InitialiseForDay(netWorthAtDayStart);

        Debug.Log($"[DayManager] Day {currentDay} open. Starting cash: ${netWorthAtDayStart:F0}");

        // Fire the signal — RoundTimer and CustomerSpawner both listen to this
        GameEvents.OnShopOpened?.Invoke();
    }

    // ─────────────────────────────────────────────────
    //  DAY END → AUCTION
    // ─────────────────────────────────────────────────

    private void HandleDayEnd()
    {
        if (dayEnded) return; // guard against double-fire
        dayEnded = true;

        roundTimer?.PauseTimer();
        GameEvents.OnShopClosed?.Invoke(); // disables Buy/Reject/Next

        Debug.Log("[DayManager] Day ended — starting auction sequence.");

        // Use a coroutine so we can explicitly wait for the fade to finish
        StartCoroutine(DayEndSequence());
    }

    private IEnumerator DayEndSequence()
    {
        // Stop any competing fades
        screenFader.StopAllCoroutines();

        // Fade to black and wait for it to finish
        bool fadeComplete = false;
        screenFader.FadeToBlack(() => fadeComplete = true);
        while (!fadeComplete) yield return null;

        Debug.Log("[DayManager] Fade to black complete. Resolving auction...");

        // Resolve auction
        List<PurchasedItem> results = auctionResolver.Resolve();

        // Resolve() returns null if there was an arrest
        if (results == null)
        {
            Debug.Log("[DayManager] Arrest triggered — stopping auction sequence.");
            yield break;
        }

        float totalEarned = 0f;
        bool loanWasRepaid = false;
        foreach (var item in results)
        {
            totalEarned += item.salePrice;
            // loanAmount is zeroed out by AuctionResolver when repaid
            if (item.loanAmount == 0f && item.loanInterestRate > 0f)
                loanWasRepaid = true;
        }

        Debug.Log($"[DayManager] Showing auction panel with {results.Count} items. Total: ${totalEarned:F0} | Loan repaid: {loanWasRepaid}");

        // Show auction panel ON TOP of the black screen
        auctionResultsPanel.Show(results, totalEarned, loanWasRepaid);

        // Disable blocksRaycasts so clicks pass through the black screen to the Auction panel
        screenFader.SetBlocksRaycasts(false);
    }

    // ─────────────────────────────────────────────────
    //  NEXT DAY TRANSITION
    // ─────────────────────────────────────────────────

    private void HandleAuctionComplete()
    {
        Debug.Log("[DayManager] Advancing to next day.");

        float endOfDayCash = ledger.NetWorth;

        // Hide the auction panel FIRST so it doesn't overlap with Game Over screens
        auctionResultsPanel.Hide();

        // Re-enable blocksRaycasts so the black screen covers everything (like buttons behind it)
        screenFader.SetBlocksRaycasts(true);

        float profitEarnedToday = endOfDayCash - netWorthAtDayStart;

        // Check for Game Over conditions
        if (endOfDayCash < 0)
        {
            GameEvents.OnGameOver?.Invoke(GameOverReason.Bankruptcy);
            return;
        }

        if (GameManager.Instance != null && profitEarnedToday < GameManager.Instance.todaysProfitTarget)
        {
            Debug.Log($"[DayManager] Missed profit quota! Earned: ${profitEarnedToday:F0}, Target: ${GameManager.Instance.todaysProfitTarget:F0}");
            GameEvents.OnGameOver?.Invoke(GameOverReason.MissedQuota);
            return;
        }

        // Advance the day in GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.AdvanceDay(endOfDayCash);

        // Reset for the new day
        currentDay++;
        purchasedInventory.Clear();
        roundTimer?.ResetTimer();

        // Show the next day intro (stays black → shows "DAY X" → fades out → OpenShop)
        screenFader.ShowDayIntro(currentDay, OpenShop);
    }
}
