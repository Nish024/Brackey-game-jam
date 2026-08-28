using UnityEngine;
using TMPro;

/// <summary>
/// Displays the price of the current item, but only once it has fully arrived at the counter.
/// Hides the price when a decision is made or the next customer is requested.
/// </summary>
public class PriceDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The text component that shows the price (supports UI or 3D text).")]
    [SerializeField] private TMP_Text priceText;
    
    [Tooltip("Needed to read the current price.")]
    [SerializeField] private TransactionController transactionController;

    [Header("Settings")]
    [SerializeField] private string prefix = "Asking: $";

    void Awake()
    {
        if (priceText != null)
            priceText.enabled = false; // Hide at start
    }

    void OnEnable()
    {
        GameEvents.OnCustomerHoverEnter += ShowPrice;
        GameEvents.OnCustomerHoverExit += HidePrice;
        GameEvents.OnDecisionMade += HidePrice;
        GameEvents.OnNextCustomerRequested += HidePrice;
    }

    void OnDisable()
    {
        GameEvents.OnCustomerHoverEnter -= ShowPrice;
        GameEvents.OnCustomerHoverExit -= HidePrice;
        GameEvents.OnDecisionMade -= HidePrice;
        GameEvents.OnNextCustomerRequested -= HidePrice;
    }

    private void ShowPrice()
    {
        if (priceText == null || transactionController == null) return;

        priceText.text = $"{prefix}{transactionController.CurrentItemPrice:F0}";
        priceText.enabled = true;
    }

    private void HidePrice(bool wasBought)
    {
        HidePrice();
    }

    private void HidePrice()
    {
        if (priceText != null)
            priceText.enabled = false;
    }
}
