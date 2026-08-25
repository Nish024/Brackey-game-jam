using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Calculates the auction sale price for every item in PurchasedInventory.
/// Called by DayManager after the shop closes.
/// Returns the resolved item list for AuctionResultsPanel to display.
/// </summary>
public class AuctionResolver : MonoBehaviour
{
    [Header("Price Multipliers")]
    [Tooltip("Genuine items sell for this multiple of purchase price.")]
    [SerializeField] private float genuineMultiplier = 2.5f;

    [Tooltip("Damaged items sell for this fraction of purchase price (< 1 = loss).")]
    [SerializeField] private float damagedMultiplier = 0.9f;

    [Header("References")]
    [SerializeField] private PurchasedInventory inventory;
    [SerializeField] private Ledger ledger;

    /// <summary>
    /// Resolve the auction. Returns the priced item list.
    /// Returns null and fires OnGameOver(Arrest) if a stolen item is found.
    /// </summary>
    public List<PurchasedItem> Resolve()
    {
        var items = new List<PurchasedItem>(inventory.Items);

        // ── Stolen check first ───────────────────────
        foreach (var item in items)
        {
            if (item.isStolen)
            {
                Debug.Log($"[AuctionResolver] STOLEN ITEM FOUND: '{item.itemName}' — ARREST!");
                GameEvents.OnGameOver?.Invoke(GameOverReason.Arrest);
                return null; // Skip the rest
            }
        }

        // ── Price each item ──────────────────────────
        float totalEarnings = 0f;
        foreach (var item in items)
        {
            if (item.isFake)
                item.salePrice = 0f;
            else if (item.isDamaged)
                item.salePrice = Mathf.Round(item.purchasePrice * damagedMultiplier);
            else
                item.salePrice = Mathf.Round(item.purchasePrice * genuineMultiplier);

            totalEarnings += item.salePrice;
            Debug.Log($"[AuctionResolver] '{item.itemName}' ({item.StatusText}) — bought ${item.purchasePrice:F0}, sold ${item.salePrice:F0}");
        }

        // ── Add proceeds to Ledger ───────────────────
        if (totalEarnings > 0f)
            ledger.Add(totalEarnings);

        Debug.Log($"[AuctionResolver] Total auction earnings: ${totalEarnings:F0}");
        return items;
    }
}
