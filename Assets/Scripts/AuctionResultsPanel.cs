using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// The auction results screen shown at end of day.
/// Dynamically instantiates one AuctionResultRow per purchased item,
/// shows the total, and offers a Next Day button.
/// Shown/hidden by DayManager via Show() / Hide().
/// </summary>
public class AuctionResultsPanel : MonoBehaviour
{
    [Header("Row Prefab & Container")]
    [Tooltip("Prefab with AuctionResultRow component — one per purchased item.")]
    [SerializeField] private GameObject rowPrefab;
    [Tooltip("Parent with a Vertical Layout Group to auto-arrange rows.")]
    [SerializeField] private Transform rowContainer;

    [Header("Summary")]
    [SerializeField] private TextMeshProUGUI totalEarningsText;
    [SerializeField] private TextMeshProUGUI emptyMessageText; // shown when 0 items bought

    [Header("Buttons")]
    [SerializeField] private Button nextDayButton;

    [Header("Panel Root")]
    [Tooltip("The root GameObject of the entire auction panel — toggled by Show/Hide.")]
    [SerializeField] private GameObject panelRoot;

    void Awake()
    {
        // Add override canvas to the panel root so it always renders on top
        if (panelRoot != null)
        {
            Canvas overrideCanvas = panelRoot.GetComponent<Canvas>();
            if (overrideCanvas == null) overrideCanvas = panelRoot.AddComponent<Canvas>();
            overrideCanvas.overrideSorting = true;
            overrideCanvas.sortingOrder = 100;

            if (panelRoot.GetComponent<GraphicRaycaster>() == null)
                panelRoot.AddComponent<GraphicRaycaster>();

            panelRoot.SetActive(false);
        }

        // Add override canvas to the Next Day button if it's outside
        if (nextDayButton != null)
        {
            Canvas btnCanvas = nextDayButton.GetComponent<Canvas>();
            if (btnCanvas == null) btnCanvas = nextDayButton.gameObject.AddComponent<Canvas>();
            btnCanvas.overrideSorting = true;
            btnCanvas.sortingOrder = 101;

            if (nextDayButton.GetComponent<GraphicRaycaster>() == null)
                nextDayButton.gameObject.AddComponent<GraphicRaycaster>();

            nextDayButton.gameObject.SetActive(false);
            nextDayButton.onClick.AddListener(OnNextDayClicked);
        }
    }

    // ─────────────────────────────────────────────────
    //  PUBLIC API  (called by DayManager)
    // ─────────────────────────────────────────────────

    public void Show(List<PurchasedItem> items, float totalEarnings)
    {
        // Clear any previous rows
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        if (items == null || items.Count == 0)
        {
            if (emptyMessageText != null)
            {
                emptyMessageText.gameObject.SetActive(true);
                emptyMessageText.text = "Nothing was bought today.";
            }
        }
        else
        {
            if (emptyMessageText != null) emptyMessageText.gameObject.SetActive(false);

            foreach (var item in items)
            {
                GameObject row = Instantiate(rowPrefab, rowContainer);
                row.GetComponent<AuctionResultRow>()?.Setup(item);
            }
        }

        if (totalEarningsText != null)
            totalEarningsText.text = $"Total Earned: ${totalEarnings:F0}";

        // Activate the panel
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        // Show the Next Day button (even if it's outside the panel)
        if (nextDayButton != null)
        {
            nextDayButton.gameObject.SetActive(true);
        }

        Debug.Log($"[AuctionResultsPanel] Panel activated. Items: {items?.Count ?? 0}");
    }

    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (nextDayButton != null) nextDayButton.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────────

    private void OnNextDayClicked()
    {
        Debug.Log("[AuctionResultsPanel] Next Day clicked.");
        GameEvents.OnAuctionComplete?.Invoke();
    }
}
