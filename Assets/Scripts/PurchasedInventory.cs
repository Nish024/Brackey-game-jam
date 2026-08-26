using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The day's shopping basket.
/// TransactionController adds to it on Buy.
/// AuctionResolver reads it at end of day.
/// DayManager clears it at the start of each new day.
/// </summary>
public class PurchasedInventory : MonoBehaviour
{
    private readonly List<PurchasedItem> items = new List<PurchasedItem>();

    /// <summary>All items bought today.</summary>
    public IReadOnlyList<PurchasedItem> Items => items;

    /// <summary>How many items have been bought today.</summary>
    public int Count => items.Count;

    public void AddItem(PurchasedItem item)
    {
        items.Add(item);
        Debug.Log($"[Inventory] Added '{item.itemName}' (${item.purchasePrice:F0}). Total items: {items.Count}");
    }

    /// <summary>Wipe the list at the start of a new day.</summary>
    public void Clear()
    {
        items.Clear();
        Debug.Log("[Inventory] Cleared for new day.");
    }
}
