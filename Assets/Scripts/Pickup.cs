using UnityEngine;

/// <summary>
/// Attach this script to the Main Camera.
/// Click on any 3D object (with a Collider) to pick it up.
/// While holding, the object follows a point in front of the camera.
/// Scroll the mouse wheel while holding to zoom the object closer/further.
/// Hold R + move the mouse to rotate/inspect the object.
/// Click again (or press E) to release — the object will smoothly drift
/// back to the exact position/rotation it was picked up from before
/// physics takes back over.
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

    [Header("Height (Scroll) Settings")]
    [Tooltip("How much each scroll tick raises/lowers the held object.")]
    [SerializeField] private float heightSensitivity = 1.5f;

    [Tooltip("Lowest the object can be scrolled to, relative to its pickup height.")]
    [SerializeField] private float minHeightOffset = -3f;

    [Tooltip("Highest the object can be scrolled to, relative to its pickup height.")]
    [SerializeField] private float maxHeightOffset = 3f;

    [Header("Inspection / Rotation")]
    [Tooltip("Mouse sensitivity when rotating the held object.")]
    [SerializeField] private float rotationSensitivity = 5f;

    [Header("Return To Origin Settings")]
    [Tooltip("How quickly the object drifts back to its original spot after release.")]
    [SerializeField] private float returnSpeed = 4f;

    [Tooltip("How close (position) and aligned (rotation, degrees) the object must get before physics resumes.")]
    [SerializeField] private float returnPositionThreshold = 0.05f;
    [SerializeField] private float returnRotationThreshold = 1f;

    [Header("Layer Mask (optional)")]
    [Tooltip("Which layers can be picked up. Leave to 'Everything' for all.")]
    [SerializeField] private LayerMask pickupMask = ~0; // Everything by default

    // Runtime state
    private GameObject heldObject;
    private Rigidbody heldRb;
    private bool isInspecting;
    private bool isReturning;
    private float lockedY;
    private float baseHoldY;

    // Original transform, restored to on release
    private Vector3 originalPosition;
    private Quaternion originalRotation;

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
            else if (!isReturning)
                BeginReturn();
        }

        // Alternate release with E
        if (heldObject != null && !isReturning && Input.GetKeyDown(KeyCode.E))
        {
            BeginReturn();
        }

        // --- Height (scroll) while actively holding (not while returning) ---
        if (heldObject != null && !isReturning)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > Mathf.Epsilon)
            {
                lockedY += scroll * heightSensitivity;
                lockedY = Mathf.Clamp(
                    lockedY,
                    baseHoldY + minHeightOffset,
                    baseHoldY + maxHeightOffset
                );
            }
        }

        // --- Inspect rotation ---
        if (heldObject != null && !isReturning)
        {
            if (Input.GetKey(KeyCode.R))
            {
                if (!isInspecting)
                {
                    isInspecting = true;
                    // Lock the cursor so its screen position stays put while we
                    // rotate via mouse delta — prevents a jump when R is released.
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                RotateHeldObject();
            }
            else if (isInspecting)
            {
                isInspecting = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (heldObject == null) return;

        if (isReturning)
        {
            MoveTowardsOrigin();
            return;
        }

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
            baseHoldY = lockedY;

            // Pick up the root of the collider's Rigidbody if it has one,
            // otherwise just use the collider's gameObject.
            Rigidbody rb = target.GetComponentInParent<Rigidbody>();

            GameObject pickedObject = (rb != null) ? rb.gameObject : target;

            // Remember where it started so we can send it back later
            originalPosition = pickedObject.transform.position;
            originalRotation = pickedObject.transform.rotation;

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

            isReturning = false;
            isInspecting = false;
        }
    }

    // ─────────────────────────────────────────────
    //  BEGIN RETURN (instead of releasing instantly)
    // ─────────────────────────────────────────────
    private void BeginReturn()
    {
        isInspecting = false;
        isReturning = true;

        // Make sure the cursor is freed in case we were mid-rotation
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (heldRb != null)
        {
            // Keep gravity off and let us drive velocity manually until it arrives home
            heldRb.useGravity = false;
            heldRb.angularVelocity = Vector3.zero;
        }
    }

    // ─────────────────────────────────────────────
    //  MOVE TOWARDS ORIGIN, THEN FINALIZE RELEASE
    // ─────────────────────────────────────────────
    private void MoveTowardsOrigin()
    {
        Vector3 currentPos = heldObject.transform.position;
        Quaternion currentRot = heldObject.transform.rotation;

        float posDistance = Vector3.Distance(currentPos, originalPosition);
        float rotAngle = Quaternion.Angle(currentRot, originalRotation);

        bool arrived = posDistance <= returnPositionThreshold && rotAngle <= returnRotationThreshold;

        if (arrived)
        {
            // Snap exactly into place, then hand control back to physics
            heldObject.transform.position = originalPosition;
            heldObject.transform.rotation = originalRotation;
            FinalizeRelease();
            return;
        }

        if (heldRb != null)
        {
            Vector3 direction = originalPosition - currentPos;
            heldRb.linearVelocity = direction * returnSpeed;
            heldObject.transform.rotation = Quaternion.Slerp(
                currentRot,
                originalRotation,
                Time.fixedDeltaTime * returnSpeed
            );
        }
        else
        {
            heldObject.transform.position = Vector3.Lerp(
                currentPos,
                originalPosition,
                Time.fixedDeltaTime * returnSpeed
            );
            heldObject.transform.rotation = Quaternion.Slerp(
                currentRot,
                originalRotation,
                Time.fixedDeltaTime * returnSpeed
            );
        }
    }

    // ─────────────────────────────────────────────
    //  FINALIZE RELEASE (restore physics state)
    // ─────────────────────────────────────────────
    private void FinalizeRelease()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (heldRb != null)
        {
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;

            // Restore original physics state
            heldRb.useGravity = originalUseGravity;
            heldRb.collisionDetectionMode = originalCollisionMode;
            heldRb.isKinematic = originalIsKinematic;
        }

        heldObject = null;
        heldRb = null;
        isInspecting = false;
        isReturning = false;
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
