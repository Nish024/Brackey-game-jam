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
    [SerializeField] private ShutterDoor        shutterDoor;
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

        // If it's Day 1, we wait for MainMenuController to call StartDayCycle()
        // If it's Day 2 or higher, we start automatically!
        if (currentDay > 1)
        {
            StartDayCycle(false);
        }
    }

    public void StartDayCycle(bool skipIntro = false)
    {
        if (skipIntro)
        {
            OpenShop();
            return;
        }

        // Begin the day intro (door closed → "DAY X" → open door → open shop)
        if (shutterDoor != null)
            shutterDoor.ShowDayIntro(currentDay, OpenShop);
        else
            OpenShop();
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
        // Stop any competing door movements
        if (shutterDoor != null) shutterDoor.StopAllCoroutines();

        // Close door and wait for it to finish
        bool transitionComplete = false;
        if (shutterDoor != null)
        {
            shutterDoor.CloseDoor(() => transitionComplete = true);
            while (!transitionComplete) yield return null;
        }

        Debug.Log("[DayManager] Door closed. Resolving auction...");

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

        float currentNetWorth = ledger.NetWorth;
        float profitEarnedToday = currentNetWorth - netWorthAtDayStart;
        float profitTarget = GameManager.Instance != null ? GameManager.Instance.todaysProfitTarget : 0f;

        // Show auction panel (UI hidden array in ShutterDoor ensures it doesn't overlap behind)
        auctionResultsPanel.Show(results, totalEarned, profitEarnedToday, profitTarget, loanWasRepaid);
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

        // 1. Check for Fake gun penalties (Strikes)
        if (GameManager.Instance != null)
        {
            int fakesBoughtToday = 0;
            foreach (var item in purchasedInventory.Items)
            {
                if (item.isFake) fakesBoughtToday++;
            }

            if (fakesBoughtToday > 0)
            {
                GameManager.Instance.currentStrikes += fakesBoughtToday;
                Debug.Log($"[DayManager] Bought {fakesBoughtToday} fakes! Total strikes: {GameManager.Instance.currentStrikes}/{GameManager.Instance.maxStrikes}");
                
                if (GameManager.Instance.currentStrikes >= GameManager.Instance.maxStrikes)
                {
                    GameEvents.OnGameOver?.Invoke(GameOverReason.TooManyFakes);
                    return;
                }
            }
        }

        float profitEarnedToday = endOfDayCash - netWorthAtDayStart;

        // 2. Check for Financial Game Over conditions
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

        // 3. Advance the day in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdvanceDay(endOfDayCash);
            
            // If the game was won during AdvanceDay (currentDay > configs), stop here!
            if (GameManager.Instance.currentDay > GameManager.Instance.dailyConfigs.Length)
            {
                return;
            }
        }

        // 4. Reset for the new day in this scene
        currentDay++;
        purchasedInventory.Clear();
        roundTimer?.ResetTimer();

        // Show the next day intro (door closed → shows "DAY X" → opens door → OpenShop)
        if (shutterDoor != null)
            shutterDoor.ShowDayIntro(currentDay, OpenShop);
        else
            OpenShop();
    }
}
