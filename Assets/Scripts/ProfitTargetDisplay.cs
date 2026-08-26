using UnityEngine;
using TMPro;

public class ProfitTargetDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetText;

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
            targetText.text = $"Target: ${target:F0}";
        }
    }
}
