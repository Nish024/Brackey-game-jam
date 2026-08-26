using UnityEngine;
using TMPro;

/// <summary>
/// Listens for net worth changes and updates a TextMeshPro field.
/// No logic — purely presentation.
/// </summary>
public class NetWorthDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI netWorthText;

    [Tooltip("Format prefix shown before the number.")]
    [SerializeField] private string prefix = "$";

    void OnEnable()
    {
        GameEvents.OnNetWorthChanged += UpdateDisplay;
    }

    void OnDisable()
    {
        GameEvents.OnNetWorthChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(float newNetWorth)
    {
        netWorthText.text = $"{prefix}{newNetWorth:F0}";
    }
}
