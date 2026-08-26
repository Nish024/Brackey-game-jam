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

    [Header("Loan Panel UI")]
    [SerializeField] private GameObject loanPanelRoot;
    [SerializeField] private TextMeshProUGUI loanAmountText;
    [SerializeField] private TextMeshProUGUI interestRateText;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button confirmLoanButton;
    [SerializeField] private Button closePanelButton;
    [SerializeField] private Button loanButton; // The trigger button in the main UI
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

    // ─────────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────────

    void Awake()
    {
        // Hide panel at start, but keep loan button visible and locked
        if (loanPanelRoot != null) loanPanelRoot.SetActive(false);
        if (loanButton != null)
        {
            loanButton.gameObject.SetActive(true);
            loanButton.interactable = false;
        }

        // Wire buttons
        if (plusButton   != null) plusButton.onClick.AddListener(OnPlus);
        if (minusButton  != null) minusButton.onClick.AddListener(OnMinus);
        if (confirmLoanButton != null) confirmLoanButton.onClick.AddListener(OnConfirmLoan);
        if (closePanelButton  != null) closePanelButton.onClick.AddListener(HidePanel);
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
    /// Wire this to the Loan Button's OnClick in the Inspector.
    /// Hard gate: only opens the panel if player genuinely can't afford the item
    /// and hasn't already used a loan this round.
    /// </summary>
    public void OpenLoanPanel()
    {
        // Hard guard 1: loan already used this round
        if (!CanOfferLoan())
        {
            Debug.Log("[LoanManager] Loan already used this round — ignoring.");
            return;
        }

        // Hard guard 2: player can actually afford the item — don't allow loan
        if (ledger != null && transactionController != null &&
            ledger.NetWorth >= transactionController.CurrentItemPrice)
        {
            Debug.Log($"[LoanManager] Player can afford ${transactionController.CurrentItemPrice:F0} — loan blocked.");
            return;
        }

        float itemPrice = transactionController != null ? transactionController.CurrentItemPrice : 0f;
        float rate = CurrentInterestRate();
        ShowLoanPanel(itemPrice, rate);
    }

    // ─────────────────────────────────────────────────
    //  PANEL LOGIC
    // ─────────────────────────────────────────────────

    private void ShowLoanPanel(float itemPrice, float interestRate)
    {
        pendingItemPrice = itemPrice;
        currentLoanAmount = 0; // start at 0, player decides how much to borrow

        gameObject.SetActive(true);
        if (loanPanelRoot != null)
        {
            loanPanelRoot.SetActive(true);
            loanPanelRoot.transform.SetAsLastSibling();
        }

        UpdatePanelUI();
        Debug.Log($"[LoanManager] Loan panel shown. Item: ${itemPrice:F0}, Rate: {interestRate * 100f:F0}%");
    }

    private void HidePanel()
    {
        if (loanPanelRoot != null) loanPanelRoot.SetActive(false);
    }

    private void OnPlus()
    {
        currentLoanAmount = Mathf.Min(currentLoanAmount + loanStep, maxLoanAmount);
        UpdatePanelUI();
    }

    private void OnMinus()
    {
        currentLoanAmount = Mathf.Max(currentLoanAmount - loanStep, 0);
        UpdatePanelUI();
    }

    private void UpdatePanelUI()
    {
        float rate = CurrentInterestRate();
        float repayment = Mathf.Round(currentLoanAmount * (1f + rate));

        if (loanAmountText  != null) loanAmountText.text  = $"{currentLoanAmount}";
        if (interestRateText != null) interestRateText.text = $"Interest: {rate * 100f:F0}% — Repay: ${repayment:F0}";
    }

    private void OnConfirmLoan()
    {
        if (currentLoanAmount <= 0)
        {
            Debug.Log("[LoanManager] Loan amount is 0, nothing to confirm.");
            HidePanel();
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

        HidePanel();
        if (loanButton != null) loanButton.interactable = false; // Lock trigger button since loan was taken

        // Tell TransactionController to retry the purchase now that funds are available
        GameEvents.OnLoanConfirmed?.Invoke();
    }

    private void OnShopOpened()
    {
        loanUsedThisRound = false;
        HidePanel();
        if (loanButton != null) loanButton.interactable = false; // always start locked
    }

    private void OnShopClosed()
    {
        HidePanel();
        if (loanButton != null) loanButton.interactable = false;
    }

    private void OnDecisionMade(bool bought)
    {
        // Once a decision is made on the item, lock the loan button
        if (loanButton != null) loanButton.interactable = false;
    }

    private void OnNetWorthChanged(float newWorth)
    {
        // Recheck every time cash changes — e.g. player buys a cheaper item, loan button should hide
        CheckAndToggleLoanButton();
    }

    private void CheckAndToggleLoanButton()
    {
        if (loanButton == null) return;

        // Conditions to UNLOCK the button:
        // 1. Loan not yet used this round
        // 2. A customer is present (transactionController has a price set)
        // 3. Player CANNOT afford the item
        bool loanAvailable = CanOfferLoan();
        bool hasCustomer   = transactionController != null && transactionController.CurrentItemPrice > 0f;
        bool cantAfford    = ledger != null && transactionController != null &&
                             ledger.NetWorth < transactionController.CurrentItemPrice;

        loanButton.interactable = (loanAvailable && hasCustomer && cantAfford);
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
