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
    [SerializeField] private float genuineMultiplier = 1.5f;

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

        // ── Price each item and deduct loans ─────────
        float totalEarnings = 0f;
        bool loanNotRepaid = false;

        foreach (var item in items)
        {
            if (item.isFake)
                item.salePrice = 0f;
            else
                item.salePrice = Mathf.Round(item.purchasePrice * genuineMultiplier);

            // Deduct loan repayment from this item's sale price
            float repayment = item.LoanRepaymentDue;
            if (repayment > 0f)
            {
                Debug.Log($"[AuctionResolver] Loan repayment due on '{item.itemName}': ${repayment:F0}");
                if (item.salePrice >= repayment)
                {
                    item.salePrice -= repayment;
                    item.loanAmount = 0f; // mark as repaid for UI
                    Debug.Log($"[AuctionResolver] Loan repaid for '{item.itemName}'. Net sale: ${item.salePrice:F0}");
                }
                else
                {
                    // Can't repay from sale proceeds
                    loanNotRepaid = true;
                    Debug.Log($"[AuctionResolver] LOAN NOT REPAID for '{item.itemName}'! Sale ${item.salePrice:F0} < repayment ${repayment:F0}");
                }
            }

            totalEarnings += item.salePrice;
            Debug.Log($"[AuctionResolver] '{item.itemName}' ({item.StatusText}) — bought ${item.purchasePrice:F0}, sold ${item.salePrice:F0}");
        }

        // ── Add proceeds to Ledger ───────────────────
        if (totalEarnings > 0f)
            ledger.Add(totalEarnings);

        Debug.Log($"[AuctionResolver] Total auction earnings: ${totalEarnings:F0}");

        // ── Check unpaid loan after earnings are in ──
        if (loanNotRepaid)
        {
            GameEvents.OnGameOver?.Invoke(GameOverReason.LoanNotRepaid);
            return null; // Stop further processing
        }

        return items;
    }
}
