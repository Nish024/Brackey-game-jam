using UnityEngine;

/// <summary>
/// Spawns one item per customer.
/// When OnCustomerReady fires → instantiate item at itemSpawnStart, lerp to itemSpawnPos.
/// When OnDecisionMade fires → send item to bought destination or back to counter, then destroy it.
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("Positions")]
    [Tooltip("Where the item first appears (off-screen or at customer's hand).")]
    [SerializeField] private Transform itemSpawnStart;

    [Tooltip("Where the item rests on the counter for the player to inspect.")]
    [SerializeField] private Transform itemSpawnPos;

    [Tooltip("Where bought items slide to before being destroyed.")]
    [SerializeField] private Transform itemBoughtPos;

    [Tooltip("Where the camera looks when inspecting — set to a Transform in front of the camera.")]
    [SerializeField] private Transform viewPos;

    [Header("Items")]
    [Tooltip("List of item prefabs to randomly spawn. Must have at least 2 to avoid back-to-back duplicates.")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Tooltip("How fast the item moves between positions.")]
    [SerializeField] private float itemMoveSpeed = 5f;

    [Header("References")]
    [SerializeField] private TransactionController transactionController;

    private ItemController currentItem;
    private int lastSpawnedIndex = -1;

    /// <summary>Read by Pickup.cs to find the active ItemController.</summary>
    public ItemController CurrentItem => currentItem;

    void OnEnable()
    {
        GameEvents.OnCustomerReady += SpawnItem;
        GameEvents.OnDecisionMade  += OnDecisionMade;
        GameEvents.OnShopClosed    += OnShopClosed;
    }

    void OnDisable()
    {
        GameEvents.OnCustomerReady -= SpawnItem;
        GameEvents.OnDecisionMade  -= OnDecisionMade;
        GameEvents.OnShopClosed    -= OnShopClosed;
    }

    // ─────────────────────────────────────────────────
    //  SPAWN
    // ─────────────────────────────────────────────────

    private void SpawnItem()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogError("[ItemSpawner] No item prefabs assigned!");
            return;
        }

        // Clean up any leftover item (safety)
        if (currentItem != null)
            Destroy(currentItem.gameObject);

        // Pick a random item, but don't pick the same one twice in a row (if we have more than 1)
        int randomIndex = Random.Range(0, itemPrefabs.Length);
        if (itemPrefabs.Length > 1)
        {
            while (randomIndex == lastSpawnedIndex)
            {
                randomIndex = Random.Range(0, itemPrefabs.Length);
            }
        }
        lastSpawnedIndex = randomIndex;
        GameObject prefabToSpawn = itemPrefabs[randomIndex];

        GameObject obj = Instantiate(prefabToSpawn, itemSpawnStart.position, itemSpawnStart.rotation);
        obj.tag = "Item";

        // Tell TransactionController the item's name for PurchasedInventory
        transactionController?.SetCurrentItemName(prefabToSpawn.name);

        // Add ItemController at runtime and give it the scene positions
        currentItem = obj.AddComponent<ItemController>();
        currentItem.Init(itemSpawnPos, viewPos, itemBoughtPos, itemMoveSpeed);
        currentItem.MoveToCounter();

        // Check if the item has GunData, and pass it along
        GunDataHolder dataHolder = obj.GetComponent<GunDataHolder>();
        if (dataHolder != null && dataHolder.Data != null)
        {
            transactionController?.SetGunData(dataHolder.Data);
            GameEvents.OnGunDataLoaded?.Invoke(dataHolder.Data);
        }

        Debug.Log("[ItemSpawner] Item spawned.");
    }

    // ─────────────────────────────────────────────────
    //  DECISION
    // ─────────────────────────────────────────────────

    private void OnDecisionMade(bool wasBought)
    {
        if (currentItem == null) return;

        // Pickup.cs will have already released the item via its own OnDecisionMade listener.
        // We just tell the item where to go from wherever it currently is.
        currentItem.MoveToFinalDestination(wasBought);
        currentItem = null; // We no longer own it; it will self-destroy on arrival
    }

    private void OnShopClosed()
    {
        // Destroy any leftover item immediately
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
            currentItem = null;
        }
    }
}
