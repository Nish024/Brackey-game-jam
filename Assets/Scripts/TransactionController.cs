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

    [Header("Testing Only - Item State Display")]
    [SerializeField] private TMPro.TextMeshProUGUI itemStateText;

    private float currentItemPrice;
    public float CurrentItemPrice => currentItemPrice;

    private string currentItemName = "Unknown Item";
    private bool decisionPending;
    private bool shopClosed;

    // Temporary randomized states for testing auction logic
    private bool currentIsFake;
    private bool currentIsDamaged;
    private bool currentIsStolen;

    /// <summary>Whether a customer is waiting for a Buy/Reject decision.</summary>
    public bool DecisionPending => decisionPending;

    void OnEnable()
    {
        GameEvents.OnCustomerReady += OnCustomerArrived;
        GameEvents.OnShopClosed    += OnShopClosed;
        GameEvents.OnShopOpened    += OnShopOpened;
    }

    void OnDisable()
    {
        GameEvents.OnCustomerReady -= OnCustomerArrived;
        GameEvents.OnShopClosed    -= OnShopClosed;
        GameEvents.OnShopOpened    -= OnShopOpened;
    }

    /// <summary>
    /// Called by the spawner or item system to set the price of the current item.
    /// </summary>
    public void SetCurrentItemPrice(float price) => currentItemPrice = price;

    public void SetCurrentItemName(string name) => currentItemName = name;

    /// <summary>
    /// Wire this to the Buy button's OnClick in the Inspector.
    /// </summary>
    public void Buy()
    {
        if (!decisionPending || shopClosed) return;

        if (ledger.Spend(currentItemPrice))
        {
            // File this item into today's purchased inventory
            var item = new PurchasedItem
            {
                itemName      = currentItemName,
                purchasePrice = currentItemPrice,
                isFake        = currentIsFake,
                isDamaged     = currentIsDamaged,
                isStolen      = currentIsStolen
            };
            purchasedInventory?.AddItem(item);

            decisionPending = false;
            if (itemStateText != null) itemStateText.text = ""; // clear text
            GameEvents.OnDecisionMade?.Invoke(true);
        }
        else
        {
            Debug.Log($"[Transaction] Can't afford ${currentItemPrice:F0}!");
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

        // --- TEMPORARY RANDOMIZED STATE FOR TESTING ---
        currentIsFake = false;
        currentIsDamaged = false;
        currentIsStolen = false;

        float r = Random.value;
        if (r < 0.2f)      currentIsStolen = true;  // 20% stolen
        else if (r < 0.4f) currentIsFake = true;    // 20% fake
        else if (r < 0.6f) currentIsDamaged = true; // 20% damaged
        // else 40% genuine

        if (itemStateText != null)
        {
            if (currentIsStolen)  itemStateText.text = "State: STOLEN";
            else if (currentIsFake)    itemStateText.text = "State: Fake";
            else if (currentIsDamaged) itemStateText.text = "State: Damaged";
            else                       itemStateText.text = "State: Genuine";
        }
        // ----------------------------------------------
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
}
