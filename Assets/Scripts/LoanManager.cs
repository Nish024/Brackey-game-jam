using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the loan system for the pawn shop game.
/// - Tracks total loan use count across the whole run (never resets mid-game).
/// - Tracks whether a loan was used this round (resets on OnShopOpened).
/// - Fires OnLoanOffered when the player can't afford an item.
/// - Manages the Loan Panel UI: +/- buttons, loan amount display, interest rate display.
/// - Confirms the loan by adding money to the Ledger and tagging the last purchased item.
/// 
/// Interest rate tiers (increases every 3 uses):
///   Use 1-3:   5%
///   Use 4-6:  10%
///   Use 7-9:  13%
///   Use 10+:  16%  (and continues adding 3% per tier)
/// </summary>
public class LoanManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ledger ledger;
    [SerializeField] private PurchasedInventory purchasedInventory;

    [SerializeField] private TransactionController transactionController;

    [Header("Settings")]
    [Tooltip("Step size for +/- buttons.")]
    [SerializeField] private int loanStep = 10;
    [Tooltip("Maximum amount the player can borrow in one transaction.")]
    [SerializeField] private int maxLoanAmount = 500;
    [Tooltip("Base interest rate on the very first use (e.g. 0.05 = 5%).")]
    [SerializeField] private float baseInterestRate = 0.05f;
    [Tooltip("How much the interest rate increases per tier (every 3 uses).")]
    [SerializeField] private float interestRateIncreasePerTier = 0.03f;

    // ── State ────────────────────────────────────────
    // Persists across days (never resets mid-game, only on StartNewGame scene reload)
    private int loanUseCount = 0;

    // Resets every new day
    private bool loanUsedThisRound = false;

    // Currently pending loan offer (set when buy fails)
    private float pendingItemPrice;
    private int currentLoanAmount;

    public int CurrentLoanAmount => currentLoanAmount;

    // ─────────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────────

    void Awake()
    {
        // Initialization if needed
    }

    void OnEnable()
    {
        GameEvents.OnLoanOffered    += ShowLoanPanel;
        GameEvents.OnShopOpened     += OnShopOpened;
        GameEvents.OnShopClosed     += OnShopClosed;
        GameEvents.OnCustomerReady  += CheckAndToggleLoanButton;
        GameEvents.OnDecisionMade   += OnDecisionMade;
        GameEvents.OnNetWorthChanged += OnNetWorthChanged; // recheck whenever cash changes
    }

    void OnDisable()
    {
        GameEvents.OnLoanOffered    -= ShowLoanPanel;
        GameEvents.OnShopOpened     -= OnShopOpened;
        GameEvents.OnShopClosed     -= OnShopClosed;
        GameEvents.OnCustomerReady  -= CheckAndToggleLoanButton;
        GameEvents.OnDecisionMade   -= OnDecisionMade;
        GameEvents.OnNetWorthChanged -= OnNetWorthChanged;
    }

    // ─────────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────────

    /// <summary>Returns true if a loan is allowed this round (one per round).</summary>
    public bool CanOfferLoan() => !loanUsedThisRound;

    /// <summary>Returns the current interest rate based on total loan use count.</summary>
    public float CurrentInterestRate()
    {
        int tier = loanUseCount / 3; // tier increases every 3 uses
        return baseInterestRate + (tier * interestRateIncreasePerTier);
    }

    /// <summary>
    /// Attempts to open the loan panel. Returns true if allowed, false if blocked.
    /// Hard gate: only opens if player genuinely can't afford the item
    /// and hasn't already used a loan this round.
    /// </summary>
    public bool TryOpenLoanPanel()
    {
        // Hard guard 1: loan already used this round
        if (!CanOfferLoan())
        {
            Debug.Log("[LoanManager] Loan already used this round — ignoring.");
            return false;
        }

        // Hard guard 2: player can actually afford the item — don't allow loan
        if (ledger != null && transactionController != null &&
            ledger.NetWorth >= transactionController.CurrentItemPrice)
        {
            Debug.Log($"[LoanManager] Player can afford ${transactionController.CurrentItemPrice:F0} — loan blocked.");
            return false;
        }

        // Hard guard 3: no customer / item price is 0
        if (transactionController == null || transactionController.CurrentItemPrice <= 0f)
        {
            Debug.Log("[LoanManager] No customer present — loan blocked.");
            return false;
        }

        float itemPrice = transactionController.CurrentItemPrice;
        float rate = CurrentInterestRate();
        ShowLoanPanel(itemPrice, rate);
        return true;
    }

    // ─────────────────────────────────────────────────
    //  PANEL LOGIC
    // ─────────────────────────────────────────────────

    private void ShowLoanPanel(float itemPrice, float interestRate)
    {
        pendingItemPrice = itemPrice;
        currentLoanAmount = 0; // start at 0, player decides how much to borrow
        Debug.Log($"[LoanManager] Loan started. Item: ${itemPrice:F0}, Rate: {interestRate * 100f:F0}%");
    }

    public void OnPlus()
    {
        currentLoanAmount = Mathf.Min(currentLoanAmount + loanStep, maxLoanAmount);
    }

    public void OnMinus()
    {
        currentLoanAmount = Mathf.Max(currentLoanAmount - loanStep, 0);
    }


    public void OnConfirmLoan()
    {
        if (currentLoanAmount <= 0)
        {
            Debug.Log("[LoanManager] Loan amount is 0, nothing to confirm.");
            return;
        }

        float rate = CurrentInterestRate();

        // Give money to player
        ledger.Add(currentLoanAmount);
        loanUsedThisRound = true;
        loanUseCount++;

        // Tag the loan onto the most-recently-pending item — it hasn't been added yet,
        // so store it for TransactionController to attach when it finalises the purchase.
        PendingLoanData = new LoanData { amount = currentLoanAmount, rate = rate };

        Debug.Log($"[LoanManager] Loan confirmed: ${currentLoanAmount} at {rate * 100f:F0}% (use #{loanUseCount})");

        // Tell TransactionController to retry the purchase now that funds are available
        GameEvents.OnLoanConfirmed?.Invoke();
    }

    private void OnShopOpened()
    {
        loanUsedThisRound = false;
    }

    private void OnShopClosed()
    {
        // cleanup if needed
    }

    private void OnDecisionMade(bool bought)
    {
        // cleanup if needed
    }

    private void OnNetWorthChanged(float newWorth)
    {
        CheckAndToggleLoanButton();
    }

    private void CheckAndToggleLoanButton()
    {
        // UI logic removed. 
    }

    // ─────────────────────────────────────────────────
    //  LOAN DATA HANDOFF (to TransactionController)
    // ─────────────────────────────────────────────────

    /// <summary>
    /// When a loan is confirmed, LoanManager stores the data here.
    /// TransactionController reads and clears it when finalising a purchase.
    /// </summary>
    public static LoanData PendingLoanData { get; private set; }

    public static void ClearPendingLoan() => PendingLoanData = null;
}

/// <summary>Simple data bag for a pending loan, passed from LoanManager to TransactionController.</summary>
public class LoanData
{
    public float amount;
    public float rate;
}
