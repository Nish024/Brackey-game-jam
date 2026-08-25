using UnityEngine;
using System.Collections;

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

    [Header("Item Prices (Temporary — replaced by item system)")]
    [SerializeField] private float minItemPrice = 50f;
    [SerializeField] private float maxItemPrice = 500f;

    [Header("References")]
    [SerializeField] private TransactionController transactionController;

    private GameObject currentCustomer;
    private bool timerExpired;

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
        if (!timerExpired)
            SpawnCustomer();
    }

    private void OnTimerExpired()
    {
        timerExpired = true;
    }

    private void OnShopClosed()
    {
        // Destroy any leftover customer immediately
        if (currentCustomer != null)
        {
            StopAllCoroutines(); // stop any active lerps
            Destroy(currentCustomer);
            currentCustomer = null;
        }
        timerExpired = false; // reset for next day
    }

    // ─────────────────────────────────────────────────
    //  SPAWN & MOVEMENT
    // ─────────────────────────────────────────────────

    private void SpawnCustomer()
    {
        if (customerPrefab == null) { Debug.LogError("[CustomerSpawner] No prefab!"); return; }
        currentCustomer = Instantiate(customerPrefab, startingSP.position, startingSP.rotation);
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
        if (leaving != null) Destroy(leaving);

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
        if (dist < 0.01f) { t.SetPositionAndRotation(targetPos, targetRot); yield break; }

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
            t.rotation = Quaternion.Slerp(startRot, targetRot, s);
            yield return null;
        }

        if (obj != null) t.SetPositionAndRotation(targetPos, targetRot);
    }
}
