using UnityEngine;
using System.Collections.Generic;

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

    // Removed old Item Prefabs array

    [Tooltip("How fast the item moves between positions.")]
    [SerializeField] private float itemMoveSpeed = 5f;

    [Header("References")]
    [SerializeField] private TransactionController transactionController;

    [Header("Testing/Debug")]
    [Tooltip("If assigned, this prefab will always be the very first item spawned each day. Great for testing!")]
    [SerializeField] private GameObject forceFirstSpawnPrefab;

    [Header("Available Gun Models")]
    [Tooltip("Drag ONLY the LEGIT/REAL prefabs here. The spawner will extract their fake/stolen variations automatically.")]
    [SerializeField] private GameObject[] availableModels;

    private ItemController currentItem;
    private int lastSpawnedIndex = -1;
    private Queue<GameObject> dailyPool = new Queue<GameObject>();

    /// <summary>Read by Pickup.cs to find the active ItemController.</summary>
    public ItemController CurrentItem => currentItem;

    void OnEnable()
    {
        GameEvents.OnCustomerReady += SpawnItem;
        GameEvents.OnDecisionMade  += OnDecisionMade;
        GameEvents.OnShopClosed    += OnShopClosed;
        GameEvents.OnShopOpened    += OnShopOpened;
    }

    void OnDisable()
    {
        GameEvents.OnCustomerReady -= SpawnItem;
        GameEvents.OnDecisionMade  -= OnDecisionMade;
        GameEvents.OnShopClosed    -= OnShopClosed;
        GameEvents.OnShopOpened    -= OnShopOpened;
    }

    // ─────────────────────────────────────────────────
    //  SPAWN
    // ─────────────────────────────────────────────────

    private void SpawnItem()
    {
        if (availableModels == null || availableModels.Length == 0)
        {
            Debug.LogError("[ItemSpawner] No available models assigned!");
            return;
        }

        // Clean up any leftover item (safety)
        if (currentItem != null)
            Destroy(currentItem.gameObject);

        GameObject prefabToSpawn = null;

        // Try to dequeue from the pre-decided daily pool
        if (dailyPool != null && dailyPool.Count > 0)
        {
            prefabToSpawn = dailyPool.Dequeue();
        }
        else
        {
            // Fallback if pool is empty or not configured
            Debug.LogWarning("[ItemSpawner] Daily spawn pool is empty! Falling back to random selection.");
            if (availableModels == null || availableModels.Length == 0) return;
            
            int randomIndex = Random.Range(0, availableModels.Length);
            if (availableModels.Length > 1)
            {
                while (randomIndex == lastSpawnedIndex)
                {
                    randomIndex = Random.Range(0, availableModels.Length);
                }
            }
            lastSpawnedIndex = randomIndex;
            prefabToSpawn = availableModels[randomIndex];
        }

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
            transactionController?.SetGunData(dataHolder.Data, dataHolder.ResolvedState);
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

    private void OnShopOpened()
    {
        DayConfig config = GameManager.Instance != null 
            ? GameManager.Instance.GetCurrentDayConfig() 
            : new DayConfig(); // Fallback if no GameManager

        dailyPool = SpawnPoolBuilder.BuildPool(
            availableModels,
            config.maxModelsPerDay,
            config.legitCount,
            config.fakeCount,
            config.stolenCount,
            config.windowSize
        );

        // Debug override: Inject the forced prefab at the front of the queue
        if (forceFirstSpawnPrefab != null)
        {
            Queue<GameObject> injectedPool = new Queue<GameObject>();
            injectedPool.Enqueue(forceFirstSpawnPrefab);
            
            while (dailyPool.Count > 0)
            {
                injectedPool.Enqueue(dailyPool.Dequeue());
            }
            dailyPool = injectedPool;
        }

        Debug.Log($"[ItemSpawner] Daily pool generated with {dailyPool.Count} items.");
    }
}
