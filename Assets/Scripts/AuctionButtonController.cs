using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the "Go to Auction" button when the player's cash drops to 5% (or less)
/// of what they had at the start of today's round.
/// Clicking it fires OnDayEnd early.
/// </summary>
public class AuctionButtonController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Auction button UI object to show/hide.")]
    [SerializeField] private GameObject auctionButtonObject;

    [Header("Settings")]
    [Tooltip("Cash must drop to this fraction of starting cash to reveal the button.")]
    [SerializeField] private float thresholdFraction = 0.05f;

    private float startingCashThisDay;
    private bool buttonShown;

    void OnEnable()
    {
        GameEvents.OnShopOpened    += ResetForNewDay;
        GameEvents.OnNetWorthChanged += CheckThreshold;
        GameEvents.OnShopClosed    += HideButton;
    }

    void OnDisable()
    {
        GameEvents.OnShopOpened    -= ResetForNewDay;
        GameEvents.OnNetWorthChanged -= CheckThreshold;
        GameEvents.OnShopClosed    -= HideButton;
    }

    /// <summary>Called by DayManager right before the shop opens each day.</summary>
    public void InitialiseForDay(float startCash)
    {
        startingCashThisDay = startCash;
        buttonShown = false;
        if (auctionButtonObject != null)
            auctionButtonObject.SetActive(false);
    }

    /// <summary>Wire this to the Auction button's OnClick in the Inspector.</summary>
    public void OnAuctionButtonClicked()
    {
        Debug.Log("[AuctionButton] Early auction triggered by player.");
        GameEvents.OnDayEnd?.Invoke();
    }

    // ─────────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────────

    private void ResetForNewDay()
    {
        // Starting cash may have been updated by DayManager before this fires
        if (GameManager.Instance != null)
            startingCashThisDay = GameManager.Instance.currentCash;

        buttonShown = false;
        if (auctionButtonObject != null)
            auctionButtonObject.SetActive(false);
    }

    private void CheckThreshold(float currentCash)
    {
        if (buttonShown || startingCashThisDay <= 0f) return;

        if (currentCash <= startingCashThisDay * thresholdFraction)
        {
            buttonShown = true;
            if (auctionButtonObject != null)
                auctionButtonObject.SetActive(true);

            Debug.Log($"[AuctionButton] Cash at {thresholdFraction * 100f}% threshold — showing auction button.");
        }
    }

    private void HideButton()
    {
        if (auctionButtonObject != null)
            auctionButtonObject.SetActive(false);
    }
}
