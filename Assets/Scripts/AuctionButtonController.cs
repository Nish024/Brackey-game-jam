using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the "Go to Auction" button when the player's cash drops to a threshold percentage (or less)
/// of what they had at the start of today's round.
/// Clicking it fires OnDayEnd early.
/// </summary>
public class AuctionButtonController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Auction button UI object to show/hide (Canvas UI).")]
    [SerializeField] private GameObject auctionButtonObject;

    [Tooltip("If using a 3D button, assign its MeshRenderer here to toggle emission.")]
    [SerializeField] private MeshRenderer buttonMeshRenderer;
    
    [Tooltip("If using a 3D button, assign the Button3D script here to toggle interactability.")]
    [SerializeField] private Button3D button3DScript;

    [Header("Settings")]
    [Tooltip("Cash threshold percentage or fraction (e.g., 0.1 for 10%, or 50 for 50%).")]
    [SerializeField] private float thresholdFraction = 0.10f;

    [Tooltip("The emission color when the 3D button is active (HDR).")]
    [SerializeField] [ColorUsage(true, true)] private Color activeEmissionColor = new Color(1f, 0.9f, 0f) * 3f;

    private float startingCashThisDay;
    private bool buttonShown;
    private CanvasGroup canvasGroup;
    private Material buttonMaterialInstance;

    void Awake()
    {
        if (auctionButtonObject == null)
            auctionButtonObject = gameObject;

        // If the script is attached to the button object itself, use CanvasGroup so the script doesn't disable itself!
        if (auctionButtonObject == gameObject && GetComponent<RectTransform>() != null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (buttonMeshRenderer != null)
        {
            // Create a material instance so we don't permanently modify the project asset
            buttonMaterialInstance = buttonMeshRenderer.material;
            // Ensure emission is enabled on the material keyword just in case
            buttonMaterialInstance.EnableKeyword("_EMISSION");
        }
    }

    void OnEnable()
    {
        GameEvents.OnShopOpened      += ResetForNewDay;
        GameEvents.OnNetWorthChanged += CheckThreshold;
        GameEvents.OnShopClosed      += HideButton;
    }

    void OnDisable()
    {
        GameEvents.OnShopOpened      -= ResetForNewDay;
        GameEvents.OnNetWorthChanged -= CheckThreshold;
        GameEvents.OnShopClosed      -= HideButton;
    }

    /// <summary>Called by DayManager right before the shop opens each day.</summary>
    public void InitialiseForDay(float startCash)
    {
        gameObject.SetActive(true); // Ensure GameObject is active so Awake and OnEnable run!
        startingCashThisDay = startCash;
        buttonShown = false;
        SetButtonVisible(false);
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
        if (GameManager.Instance != null && GameManager.Instance.currentCash > 0)
            startingCashThisDay = GameManager.Instance.currentCash;

        buttonShown = false;
        SetButtonVisible(false);
    }

    private void CheckThreshold(float currentCash)
    {
        if (buttonShown || startingCashThisDay <= 0f) return;

        // Automatically convert 50 -> 0.50 (50%) if user entered a percentage > 1 in Inspector
        float fraction = thresholdFraction > 1.0f ? thresholdFraction / 100.0f : thresholdFraction;
        float targetCashThreshold = startingCashThisDay * fraction;

        Debug.Log($"[AuctionButton] Checking threshold: Current=${currentCash}, Starting=${startingCashThisDay}, TargetThreshold=${targetCashThreshold}");

        if (currentCash <= targetCashThreshold)
        {
            buttonShown = true;
            SetButtonVisible(true);
            Debug.Log($"[AuctionButton] Cash is below ${targetCashThreshold:F0} ({fraction * 100f}%) — showing auction button.");
        }
    }

    private void HideButton()
    {
        SetButtonVisible(false);
    }

    private void SetButtonVisible(bool visible)
    {
        // 1. Canvas UI support
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        else if (auctionButtonObject != null && buttonMeshRenderer == null)
        {
            auctionButtonObject.SetActive(visible);
        }

        // 2. 3D Button support
        if (button3DScript != null)
        {
            button3DScript.interactable = visible;
        }

        if (buttonMaterialInstance != null)
        {
            Color targetEmission = visible ? activeEmissionColor : Color.black; // Color.black is 0 intensity
            buttonMaterialInstance.SetColor("_EmissionColor", targetEmission);
        }
    }
}
