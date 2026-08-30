using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the customer lifecycle:
///  - First customer auto-spawns on Start.
///  - When Buy/Reject is made → current customer immediately walks to leavingSP and is destroyed.
///  - When Next is clicked → a brand-new customer spawns (unless timer expired).
/// </summary>
public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform startingSP;
    [SerializeField] private Transform middleSP;
    [SerializeField] private Transform leavingSP;

    [Header("Customer")]
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("Rotation offset applied when spawning/moving. E.g. (-90, 0, 0) to fix sideways models.")]
    [SerializeField] private Vector3 spawnRotationOffset = new Vector3(-90f, 0f, 0f);

    [Header("Item Prices (Temporary — replaced by item system)")]
    [SerializeField] private float minItemPrice = 50f;
    [SerializeField] private float maxItemPrice = 500f;

    [Header("References")]
    [SerializeField] private TransactionController transactionController;

    private GameObject currentCustomer;
    private bool timerExpired;
    private List<GameObject> activeCustomers = new List<GameObject>(); // Tracks ALL customers, even leaving ones

    void OnEnable()
    {
        GameEvents.OnDecisionMade          += OnDecisionMade;
        GameEvents.OnNextCustomerRequested += OnNextRequested;
        GameEvents.OnTimerExpired          += OnTimerExpired;
        GameEvents.OnShopOpened            += SpawnCustomer; // first customer
        GameEvents.OnShopClosed            += OnShopClosed;
    }

    void OnDisable()
    {
        GameEvents.OnDecisionMade          -= OnDecisionMade;
        GameEvents.OnNextCustomerRequested -= OnNextRequested;
        GameEvents.OnTimerExpired          -= OnTimerExpired;
        GameEvents.OnShopOpened            -= SpawnCustomer;
        GameEvents.OnShopClosed            -= OnShopClosed;
    }

    // ─────────────────────────────────────────────────
    //  EVENT HANDLERS
    // ─────────────────────────────────────────────────

    // Decision made → current customer walks out immediately
    private void OnDecisionMade(bool wasBought)
    {
        if (currentCustomer != null)
            StartCoroutine(CustomerLeave());
    }

    // Next clicked → spawn a new customer (the previous one is already leaving/gone)
    private void OnNextRequested()
    {
        // Foolproof check: only spawn if the timer hasn't expired AND there is no customer currently at the counter.
        // (When a customer leaves, currentCustomer is set to null immediately, allowing the next spawn).
        if (!timerExpired && currentCustomer == null)
            SpawnCustomer();
    }

    private void OnTimerExpired()
    {
        timerExpired = true;
    }

    private void OnShopClosed()
    {
        // Destroy ALL leftover customers immediately (including those walking out)
        StopAllCoroutines(); // stop any active lerps

        foreach (var customer in activeCustomers)
        {
            if (customer != null) Destroy(customer);
        }
        activeCustomers.Clear();
        currentCustomer = null;

        timerExpired = false; // reset for next day
    }

    // ─────────────────────────────────────────────────
    //  SPAWN & MOVEMENT
    // ─────────────────────────────────────────────────

    private void SpawnCustomer()
    {
        if (customerPrefab == null) { Debug.LogError("[CustomerSpawner] No prefab!"); return; }
        Quaternion rot = startingSP.rotation * Quaternion.Euler(spawnRotationOffset);
        currentCustomer = Instantiate(customerPrefab, startingSP.position, rot);
        activeCustomers.Add(currentCustomer);
        
        StartCoroutine(MoveToCounter(currentCustomer));
    }

    private IEnumerator MoveToCounter(GameObject customer)
    {
        yield return StartCoroutine(LerpToPoint(customer, middleSP.position, middleSP.rotation));

        if (customer == null) yield break; // destroyed mid-walk (shouldn't happen)

        // Assign a random item price (temporary)
        float price = Mathf.Round(Random.Range(minItemPrice, maxItemPrice));
        transactionController.SetCurrentItemPrice(price);

        Debug.Log($"[CustomerSpawner] Customer ready. Item: ${price:F0}");
        GameEvents.OnCustomerReady?.Invoke();
    }

    private IEnumerator CustomerLeave()
    {
        GameObject leaving = currentCustomer;
        currentCustomer = null;

        yield return StartCoroutine(LerpToPoint(leaving, leavingSP.position, leavingSP.rotation));
        
        if (leaving != null)
        {
            activeCustomers.Remove(leaving);
            Destroy(leaving);
        }

        GameEvents.OnCustomerLeft?.Invoke();
    }

    // ─────────────────────────────────────────────────
    //  LERP HELPER
    // ─────────────────────────────────────────────────

    private IEnumerator LerpToPoint(GameObject obj, Vector3 targetPos, Quaternion targetRot)
    {
        if (obj == null) yield break;
        Transform t = obj.transform;

        float dist = Vector3.Distance(t.position, targetPos);
        Quaternion finalRot = targetRot * Quaternion.Euler(spawnRotationOffset);

        if (dist < 0.01f) { t.SetPositionAndRotation(targetPos, finalRot); yield break; }

        float duration = dist / moveSpeed;
        float elapsed  = 0f;
        Vector3    startPos = t.position;
        Quaternion startRot = t.rotation;

        while (elapsed < duration)
        {
            if (obj == null) yield break;
            elapsed += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            t.position = Vector3.Lerp(startPos, targetPos, s);
            t.rotation = Quaternion.Slerp(startRot, finalRot, s);
            yield return null;
        }

        if (obj != null) t.SetPositionAndRotation(targetPos, targetRot);
    }
}
