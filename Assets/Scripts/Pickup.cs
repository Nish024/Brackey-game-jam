using UnityEngine;

/// <summary>
/// Attach this script to the Main Camera.
/// Click on any 3D object (with a Collider) to pick it up.
/// While holding, the object follows a point in front of the camera.
/// Hold R + move the mouse to rotate/inspect the object.
/// Click again (or press E) to release.
/// </summary>
public class Pickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Maximum distance to pick up an object.")]
    [SerializeField] private float pickupRange = 10f;

    [Tooltip("How far in front of the camera the held object floats.")]
    [SerializeField] private float holdDistance = 3f;

    [Tooltip("How smoothly the object follows the hold point.")]
    [SerializeField] private float followSpeed = 12f;

    [Header("Inspection / Rotation")]
    [Tooltip("Mouse sensitivity when rotating the held object.")]
    [SerializeField] private float rotationSensitivity = 5f;

    [Header("Layer Mask (optional)")]
    [Tooltip("Which layers can be picked up. Leave to 'Everything' for all.")]
    [SerializeField] private LayerMask pickupMask = ~0; // Everything by default

    // Runtime state
    private GameObject heldObject;
    private Rigidbody heldRb;
    private bool isInspecting;
    private float lockedY;

    // Stored physics state so we can restore on release
    private bool originalUseGravity;
    private bool originalIsKinematic;
    private CollisionDetectionMode originalCollisionMode;

    void Update()
    {
        // --- Pick up / Release toggle ---
        if (Input.GetMouseButtonDown(0))
        {
            if (heldObject == null)
                TryPickup();
            else
                Release();
        }

        // Alternate release with E
        if (heldObject != null && Input.GetKeyDown(KeyCode.E))
        {
            Release();
        }

        // --- Inspect rotation ---
        if (heldObject != null)
        {
            if (Input.GetKey(KeyCode.R))
            {
                isInspecting = true;
                RotateHeldObject();
            }
            else
            {
                isInspecting = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (heldObject == null) return;

        if (isInspecting)
        {
            if (heldRb != null)
                heldRb.linearVelocity = Vector3.zero;
            return;
        }

        // Move the held object towards the hold point
        MoveHeldObject();
    }

    // ─────────────────────────────────────────────
    //  PICKUP
    // ─────────────────────────────────────────────
    private void TryPickup()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask))
        {
            GameObject target = hit.collider.gameObject;
            lockedY = target.transform.position.y;

            // Pick up the root of the collider's Rigidbody if it has one,
            // otherwise just use the collider's gameObject.
            Rigidbody rb = target.GetComponentInParent<Rigidbody>();

            if (rb != null)
            {
                heldObject = rb.gameObject;
                heldRb = rb;

                // Save original physics state
                originalUseGravity = heldRb.useGravity;
                originalIsKinematic = heldRb.isKinematic;
                originalCollisionMode = heldRb.collisionDetectionMode;

                // Disable physics while held
                heldRb.useGravity = false;
                heldRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                heldRb.linearVelocity = Vector3.zero;
                heldRb.angularVelocity = Vector3.zero;
            }
            else
            {
                // No Rigidbody — just move via transform
                heldObject = target;
                heldRb = null;
            }
        }
    }

    // ─────────────────────────────────────────────
    //  RELEASE
    // ─────────────────────────────────────────────
    private void Release()
    {
        if (heldRb != null)
        {
            // Restore original physics state
            heldRb.useGravity = originalUseGravity;
            heldRb.collisionDetectionMode = originalCollisionMode;
            heldRb.isKinematic = originalIsKinematic;
        }

        heldObject = null;
        heldRb = null;
        isInspecting = false;
    }

    // ─────────────────────────────────────────────
    //  MOVE HELD OBJECT (follows mouse cursor)
    // ─────────────────────────────────────────────
    private void MoveHeldObject()
    {
        // Project the mouse cursor into world space at holdDistance from the camera
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 holdPoint = mouseRay.origin + mouseRay.direction * holdDistance;
        
        // Lock the Y axis
        holdPoint.y = lockedY;

        if (heldRb != null)
        {
            // Smooth physics-based movement
            Vector3 direction = holdPoint - heldObject.transform.position;
            heldRb.linearVelocity = direction * followSpeed;
        }
        else
        {
            // Non-Rigidbody: lerp transform directly
            heldObject.transform.position = Vector3.Lerp(
                heldObject.transform.position,
                holdPoint,
                Time.fixedDeltaTime * followSpeed
            );
        }
    }

    // ─────────────────────────────────────────────
    //  ROTATE / INSPECT
    // ─────────────────────────────────────────────
    private void RotateHeldObject()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSensitivity;

        // Rotate around world axes so it feels natural regardless of camera angle
        heldObject.transform.Rotate(Camera.main.transform.up, -mouseX, Space.World);
        heldObject.transform.Rotate(Camera.main.transform.right, mouseY, Space.World);

        // Kill angular velocity so it doesn't spin after releasing R
        if (heldRb != null)
        {
            heldRb.angularVelocity = Vector3.zero;
        }
    }
}
