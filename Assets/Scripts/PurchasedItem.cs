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
    public bool isDamaged;
    public bool isStolen;

    /// <summary>Human-readable status for the auction results UI.</summary>
    public string StatusText
    {
        get
        {
            if (isStolen)  return "STOLEN";
            if (isFake)    return "Fake";
            if (isDamaged) return "Damaged";
            return "Genuine";
        }
    }
}
