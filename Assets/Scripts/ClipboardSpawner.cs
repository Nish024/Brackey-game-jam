using UnityEngine;

/// <summary>
/// Spawns a clipboard document whenever a new customer arrives.
/// Sends the clipboard away to be destroyed when a decision is made.
/// </summary>
public class ClipboardSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("The Clipboard prefab to spawn.")]
    [SerializeField] private GameObject clipboardPrefab;

    [Header("Positions")]
    [Tooltip("Where the clipboard spawns when the customer arrives.")]
    [SerializeField] private Transform startPos;
    
    [Tooltip("Where the clipboard rests on the counter.")]
    [SerializeField] private Transform idlePos;
    
    [Tooltip("Where the clipboard is held when the player inspects it.")]
    [SerializeField] private Transform viewPos;

    [Header("Settings")]
    [Tooltip("How fast the clipboard moves between positions.")]
    [SerializeField] private float moveSpeed = 5f;

    private ClipboardController currentClipboard;

    void OnEnable()
    {
        GameEvents.OnCustomerReady += SpawnClipboard;
        GameEvents.OnDecisionMade += OnDecisionMade;
        GameEvents.OnShopClosed += OnShopClosed;
    }

    void OnDisable()
    {
        GameEvents.OnCustomerReady -= SpawnClipboard;
        GameEvents.OnDecisionMade -= OnDecisionMade;
        GameEvents.OnShopClosed -= OnShopClosed;
    }

    private void SpawnClipboard()
    {
        if (clipboardPrefab == null)
        {
            Debug.LogError("[ClipboardSpawner] No clipboard prefab assigned!");
            return;
        }

        // Clean up any existing clipboard just in case
        if (currentClipboard != null)
        {
            Destroy(currentClipboard.gameObject);
        }

        // Spawn the new clipboard at the start position
        GameObject obj = Instantiate(clipboardPrefab, startPos.position, startPos.rotation);
        
        // Add the controller logic and initialize it
        currentClipboard = obj.AddComponent<ClipboardController>();
        currentClipboard.Init(startPos, idlePos, viewPos, moveSpeed);

        // Tell it to move to the counter (idle position)
        currentClipboard.MoveToIdle();
    }

    private void OnDecisionMade(bool wasBought)
    {
        // When the player makes a decision, the clipboard leaves and is destroyed
        if (currentClipboard != null)
        {
            currentClipboard.LeaveAndDestroy();
            currentClipboard = null; // We lose ownership of it
        }
    }

    private void OnShopClosed()
    {
        // If the shop closes, instantly destroy the clipboard
        if (currentClipboard != null)
        {
            Destroy(currentClipboard.gameObject);
            currentClipboard = null;
        }
    }
}
