using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to the AuctionResultRow UI prefab.
/// AuctionResultsPanel calls Setup() to fill in the row's fields.
/// </summary>
public class AuctionResultRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI purchasePriceText;
    [SerializeField] private TextMeshProUGUI salePriceText;

    void Awake()
    {
        // Ensure a LayoutElement exists so the Vertical Layout Group sizes this row properly
        var le = GetComponent<LayoutElement>();
        if (le == null) le = gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 50f;
        le.minHeight = 50f;

        // Force all text fields to not wrap and to overflow instead
        ForceTextSettings(itemNameText);
        ForceTextSettings(statusText);
        ForceTextSettings(purchasePriceText);
        ForceTextSettings(salePriceText);
    }

    private void ForceTextSettings(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        // Add a LayoutElement with flexible width so the HorizontalLayoutGroup
        // distributes space evenly
        var le = tmp.GetComponent<LayoutElement>();
        if (le == null) le = tmp.gameObject.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.minWidth = 60f;
    }

    public void Setup(PurchasedItem item)
    {
        if (itemNameText    != null) itemNameText.text    = FormatItemName(item.itemName);
        if (statusText      != null) statusText.text      = item.StatusText;
        if (purchasePriceText != null) purchasePriceText.text = $"${item.purchasePrice:F0}";
        if (salePriceText   != null) salePriceText.text   = $"${item.salePrice:F0}";

        // Colour-code the status text for quick reading
        if (statusText != null)
        {
            statusText.color = item.isFake    ? new Color(0.9f, 0.2f, 0.2f) // red
                             : item.isStolen  ? new Color(0.9f, 0.5f, 0.0f) // orange
                                              : new Color(0.3f, 0.9f, 0.3f); // green for genuine
        }
    }

    public void SetupHeader(string itemCol = "Item Name", string statusCol = "Status", string boughtCol = "Bought Price", string soldCol = "Sold Price")
    {
        if (itemNameText != null)
        {
            itemNameText.text = itemCol;
            itemNameText.fontStyle = FontStyles.Bold;
            itemNameText.color = new Color(1f, 0.85f, 0.4f); // Golden accent
        }
        if (statusText != null)
        {
            statusText.text = statusCol;
            statusText.fontStyle = FontStyles.Bold;
            statusText.color = new Color(1f, 0.85f, 0.4f);
        }
        if (purchasePriceText != null)
        {
            purchasePriceText.text = boughtCol;
            purchasePriceText.fontStyle = FontStyles.Bold;
            purchasePriceText.color = new Color(1f, 0.85f, 0.4f);
        }
        if (salePriceText != null)
        {
            salePriceText.text = soldCol;
            salePriceText.fontStyle = FontStyles.Bold;
            salePriceText.color = new Color(1f, 0.85f, 0.4f);
        }
    }

    private string FormatItemName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        string clean = name.Replace("_real", "").Replace("_fake", "").Replace("_stolen", "").Replace("_", " ");
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(clean);
    }
}
