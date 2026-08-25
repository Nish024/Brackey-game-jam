using UnityEngine;

/// <summary>
/// Attach to the Main Camera.
/// Click on any GameObject tagged "Item" to send it to the view position for inspection.
/// Hold R + move mouse to rotate it.
/// Click again to send it back to the counter.
/// When a Buy/Reject decision is made, the item is automatically released.
/// </summary>
public class Pickup : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSensitivity = 5f;

    [Header("Raycast")]
    [SerializeField] private float pickupRange = 15f;
    [SerializeField] private LayerMask pickupMask = ~0;

    private ItemController inspectedItem;
    private bool isRotating;

    void OnEnable()
    {
        GameEvents.OnDecisionMade += OnDecisionMade;
    }

    void OnDisable()
    {
        GameEvents.OnDecisionMade -= OnDecisionMade;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (inspectedItem == null)
                TryPickup();
            else
                ReturnToCounter();
        }

        HandleRotation();
    }

    // ─────────────────────────────────────────────────
    //  PICKUP
    // ─────────────────────────────────────────────────

    private void TryPickup()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask)) return;
        if (!hit.collider.CompareTag("Item")) return;

        // Find ItemController on the hit object or its parent
        ItemController item = hit.collider.GetComponent<ItemController>()
                           ?? hit.collider.GetComponentInParent<ItemController>();
        if (item == null) return;

        if (item.State == ItemState.AtCounter)
        {
            inspectedItem = item;
            item.MoveToView();
            Debug.Log("[Pickup] Item picked up for inspection.");
        }
    }

    // ─────────────────────────────────────────────────
    //  RETURN
    // ─────────────────────────────────────────────────

    private void ReturnToCounter()
    {
        if (inspectedItem == null) return;

        inspectedItem.ReturnToCounter();
        ReleaseItem();
        Debug.Log("[Pickup] Item returned to counter.");
    }

    // ─────────────────────────────────────────────────
    //  ROTATION
    // ─────────────────────────────────────────────────

    private void HandleRotation()
    {
        if (inspectedItem == null) return;

        // Only allow rotation once the item has fully arrived at view position
        if (inspectedItem.State != ItemState.AtView) return;

        if (Input.GetKey(KeyCode.R))
        {
            if (!isRotating)
            {
                isRotating = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            float mouseX = Input.GetAxis("Mouse X") * rotationSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSensitivity;

            inspectedItem.transform.Rotate(Camera.main.transform.up,    -mouseX, Space.World);
            inspectedItem.transform.Rotate(Camera.main.transform.right,  mouseY, Space.World);
        }
        else if (isRotating)
        {
            isRotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // ─────────────────────────────────────────────────
    //  FORCE RELEASE (decision was made externally)
    // ─────────────────────────────────────────────────

    private void ReleaseItem()
    {
        if (isRotating)
        {
            isRotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        inspectedItem = null;
    }

    private void OnDecisionMade(bool wasBought)
    {
        // ItemSpawner handles moving the item — we just let go of our reference
        ReleaseItem();
    }
}
