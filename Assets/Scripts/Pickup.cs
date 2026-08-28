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

    [Header("Zoom")]
    [SerializeField] private float zoomSensitivity = 30f;
    [SerializeField] private float minFOV = 20f;
    private float defaultFOV;
    private float targetFOV;
    private Camera mainCam;

    [Header("Inspection Objects")]
    [Tooltip("Objects that should only be active while inspecting an item (e.g. Spotlights, Scale Ruler).")]
    [SerializeField] private GameObject[] inspectionOnlyObjects;

    void Awake()
    {
        mainCam = Camera.main;
        if (mainCam != null)
        {
            defaultFOV = mainCam.fieldOfView;
            targetFOV = defaultFOV;
        }
    }

    void OnEnable()
    {
        GameEvents.OnDecisionMade += OnDecisionMade;
        GameEvents.OnShopClosed   += OnShopClosed;
        
        SetInspectionObjectsActive(false);
    }

    void OnDisable()
    {
        GameEvents.OnDecisionMade -= OnDecisionMade;
        GameEvents.OnShopClosed   -= OnShopClosed;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (inspectedItem == null)
                TryPickup();
            else
                ForceReturnItem();
        }

        // Safety fallback: if item was destroyed but we're still rotating
        if (inspectedItem == null && isRotating)
        {
            ReleaseItem();
        }

        HandleRotation();
        HandleZoom();
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
            // Drop clipboard if it's currently being viewed
            var clipboard = FindObjectOfType<ClipboardController>();
            if (clipboard != null && clipboard.IsAtView)
            {
                clipboard.MoveToIdle();
            }

            inspectedItem = item;
            item.MoveToView();
            SetInspectionObjectsActive(true);
            Debug.Log("[Pickup] Item picked up for inspection.");
        }
    }

    // ─────────────────────────────────────────────────
    //  RETURN
    // ─────────────────────────────────────────────────

    public void ForceReturnItem()
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
    //  ZOOM
    // ─────────────────────────────────────────────────

    private void HandleZoom()
    {
        if (mainCam == null) return;

        if (inspectedItem != null)
        {
            // Allow zooming only when item is picked up
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetFOV -= scroll * zoomSensitivity;
                targetFOV = Mathf.Clamp(targetFOV, minFOV, defaultFOV);
            }
        }
        else
        {
            // If no item is picked up, ensure target is default
            targetFOV = defaultFOV;
        }

        // Smoothly interpolate the camera's FOV to the target
        mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, targetFOV, Time.deltaTime * 15f);
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
        targetFOV = defaultFOV; // Reset zoom
        SetInspectionObjectsActive(false);
    }

    private void SetInspectionObjectsActive(bool active)
    {
        if (inspectionOnlyObjects == null) return;
        foreach (var obj in inspectionOnlyObjects)
        {
            if (obj != null) obj.SetActive(active);
        }
    }

    private void OnDecisionMade(bool wasBought)
    {
        // ItemSpawner handles moving the item — we just let go of our reference
        ReleaseItem();
    }

    private void OnShopClosed()
    {
        // Force drop item and unlock cursor when the day ends
        ReleaseItem();
    }
}
