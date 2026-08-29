using UnityEngine;

/// <summary>
/// Handles the Buy / Reject decision for the current customer's item.
/// Wired to UI Buttons in the Inspector.
/// Buy checks Ledger for funds, spends money if affordable, then broadcasts.
/// Reject just broadcasts — no money changes.
/// </summary>
public class TransactionController : MonoBehaviour
{
    [SerializeField] private Ledger ledger;
    [SerializeField] private PurchasedInventory purchasedInventory;
    [SerializeField] private LoanManager loanManager;

    [Header("Testing Only - Item State Display")]
    [SerializeField] private TMPro.TextMeshProUGUI itemStateText;

    private float currentItemPrice;
    public float CurrentItemPrice => currentItemPrice;

    private string currentItemName = "Unknown Item";
    private bool decisionPending;
    private bool shopClosed;

    // Randomized states
    private bool currentIsFake;
    private bool currentIsStolen;

    private float honestBasePrice;
    private float askingPrice;

    /// <summary>Whether a customer is waiting for a Buy/Reject decision.</summary>
    public bool DecisionPending => decisionPending;

    void OnEnable()
    {
        GameEvents.OnCustomerReady  += OnCustomerArrived;
        GameEvents.OnShopClosed     += OnShopClosed;
        GameEvents.OnShopOpened     += OnShopOpened;
        GameEvents.OnLoanConfirmed  += OnLoanConfirmed;
    }

    void OnDisable()
    {
        GameEvents.OnCustomerReady  -= OnCustomerArrived;
        GameEvents.OnShopClosed     -= OnShopClosed;
        GameEvents.OnShopOpened     -= OnShopOpened;
        GameEvents.OnLoanConfirmed  -= OnLoanConfirmed;
    }

    /// <summary>
    /// Called by the spawner or item system to set the base price of the current item.
    /// In the future, this should probably be calculated inside TransactionController, but for now we'll just set it.
    /// Actually, let's let OnCustomerArrived generate the prices for testing.
    /// </summary>
    public void SetCurrentItemPrice(float price) => currentItemPrice = price;

    public void ApplyConfrontationDiscount()
    {
        askingPrice = honestBasePrice;
        currentItemPrice = askingPrice;
        // In the future, fire an event to update the UI specifically if needed.
    }

    public void SetCurrentItemName(string name) => currentItemName = name;

    /// <summary>
    /// Wire this to the Buy button's OnClick in the Inspector.
    /// </summary>
    public void Buy()
    {
        if (!decisionPending || shopClosed) return;

        if (ledger.Spend(currentItemPrice))
        {
            // Attach any pending loan data to this item
            LoanData loan = LoanManager.PendingLoanData;
            LoanManager.ClearPendingLoan();

            var item = new PurchasedItem
            {
                itemName         = currentItemName,
                purchasePrice    = currentItemPrice,
                isFake           = currentIsFake,
                isStolen         = currentIsStolen,
                loanAmount       = loan != null ? loan.amount : 0f,
                loanInterestRate = loan != null ? loan.rate   : 0f
            };
            purchasedInventory?.AddItem(item);

            decisionPending = false;
            if (itemStateText != null) itemStateText.text = "";
            GameEvents.OnDecisionMade?.Invoke(true);
        }
        else
        {
            // Can't afford — do nothing. The Loan Button handles this case.
            Debug.Log($"[Transaction] Can't afford ${currentItemPrice:F0}.");
        }
    }

    /// <summary>
    /// Wire this to the Reject / Nope button's OnClick in the Inspector.
    /// </summary>
    public void Reject()
    {
        if (!decisionPending || shopClosed) return;
        decisionPending = false;
        if (itemStateText != null) itemStateText.text = ""; // clear text
        GameEvents.OnDecisionMade?.Invoke(false);
    }

    private void OnCustomerArrived()
    {
        decisionPending = true;
    }

    /// <summary>
    /// Called by ItemSpawner after a gun is spawned.
    /// resolvedState comes from GunDataHolder (computed from logo pick at runtime).
    /// </summary>
    public void SetGunData(GunData data, GunState resolvedState)
    {
        currentIsFake = resolvedState == GunState.Fake;
        currentIsStolen = resolvedState == GunState.Stolen;

        honestBasePrice = currentIsFake ? 0f : data.minAskPrice;

        // Random asking price between min and max from the data card
        askingPrice = Random.Range(data.minAskPrice, data.maxAskPrice);
        currentItemPrice = askingPrice;

        if (itemStateText != null)
        {
            string actualStr = currentIsFake ? "FAKE" : currentIsStolen ? "STOLEN" : "LEGIT";
            itemStateText.text = $"Actual: {actualStr}\nClaim: LEGIT";
        }
    }

    private void OnShopClosed()
    {
        decisionPending = false;
        shopClosed = true;
        if (itemStateText != null) itemStateText.text = "";
    }

    private void OnShopOpened()
    {
        shopClosed = false;
    }

    /// <summary>Called when LoanManager confirms a loan — retry the buy with the new funds.</summary>
    private void OnLoanConfirmed()
    {
        Debug.Log("[Transaction] Loan confirmed — retrying Buy.");
        Buy();
    }
}
