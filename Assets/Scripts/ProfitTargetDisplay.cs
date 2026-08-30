using UnityEngine;
using TMPro;

public class ProfitTargetDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The text component to display the profit target (supports UI or 3D Text).")]
    [SerializeField] private TMP_Text targetText;

    void Start()
    {
        UpdateTargetDisplay();
    }

    void OnEnable()
    {
        // Whenever the shop opens (new day), update the target
        GameEvents.OnShopOpened += UpdateTargetDisplay;
    }

    void OnDisable()
    {
        GameEvents.OnShopOpened -= UpdateTargetDisplay;
    }

    private void UpdateTargetDisplay()
    {
        if (targetText != null && GameManager.Instance != null)
        {
            float target = GameManager.Instance.todaysProfitTarget;
            targetText.text = $"Target Profit: ${target:F0}";
        }
    }
}
