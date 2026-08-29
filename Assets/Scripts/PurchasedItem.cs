using UnityEngine;

/// <summary>
/// Data record for a single item purchased during the trading day.
/// Created by TransactionController on Buy, stored in PurchasedInventory,
/// read by AuctionResolver and AuctionResultsPanel.
/// </summary>
[System.Serializable]
public class PurchasedItem
{
    public string itemName;
    public float purchasePrice;
    public float salePrice; // calculated by AuctionResolver

    // Hidden truth flags — set by the item system (all false = genuine for now)
    public bool isFake;
    public bool isStolen;

    // Loan data — set by LoanManager if the item was purchased with a loan
    public float loanAmount;        // principal borrowed (0 = no loan)
    public float loanInterestRate;  // e.g. 0.05 = 5%
    public float LoanRepaymentDue => loanAmount > 0f ? Mathf.Round(loanAmount * (1f + loanInterestRate)) : 0f;

    /// <summary>Human-readable status for the auction results UI.</summary>
    public string StatusText
    {
        get
        {
            if (isStolen)  return "STOLEN";
            if (isFake)    return "Fake";
            return "Genuine";
        }
    }
}
