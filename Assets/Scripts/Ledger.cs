using UnityEngine;

/// <summary>
/// Single source of truth for the player's net worth.
/// Only this script is allowed to mutate the value.
/// Broadcasts OnNetWorthChanged on every mutation.
/// </summary>
public class Ledger : MonoBehaviour
{
    [Header("Starting Cash")]
    [Tooltip("Only used if GameManager is not present (testing in isolation).")]
    [SerializeField] private float fallbackStartingCash = 1000f;

    private float netWorth;

    /// <summary>Current net worth (read-only).</summary>
    public float NetWorth => netWorth;

    void Start()
    {
        // Pull starting cash from GameManager if it exists, otherwise use fallback
        if (GameManager.Instance != null)
            netWorth = GameManager.Instance.currentCash;
        else
            netWorth = fallbackStartingCash;

        GameEvents.OnNetWorthChanged?.Invoke(netWorth);
    }

    /// <summary>
    /// Attempt to spend money. Returns true if affordable, false otherwise.
    /// </summary>
    public bool Spend(float amount)
    {
        if (amount <= 0f) return false;

        if (netWorth < amount)
        {
            Debug.Log($"[Ledger] Can't afford ${amount:F0} — only have ${netWorth:F0}");
            return false;
        }

        netWorth -= amount;
        GameEvents.OnNetWorthChanged?.Invoke(netWorth);
        Debug.Log($"[Ledger] Spent ${amount:F0} — Net worth: ${netWorth:F0}");
        return true;
    }

    /// <summary>
    /// Add money (auction proceeds, etc.)
    /// </summary>
    public void Add(float amount)
    {
        if (amount <= 0f) return;

        netWorth += amount;
        GameEvents.OnNetWorthChanged?.Invoke(netWorth);
        Debug.Log($"[Ledger] Added ${amount:F0} — Net worth: ${netWorth:F0}");
    }

    /// <summary>
    /// Force-set the net worth (used for loan shortfall / game-over edge cases).
    /// </summary>
    public void SetNetWorth(float value)
    {
        netWorth = value;
        GameEvents.OnNetWorthChanged?.Invoke(netWorth);
    }
}
