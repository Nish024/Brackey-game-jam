using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the movement of the Phone between its idle position on the desk and the view position.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PhoneController : MonoBehaviour
{
    [Header("Positions")]
    [Tooltip("The position where the phone rests on the counter.")]
    [SerializeField] private Transform idlePos;
    [Tooltip("The position where the phone moves to when inspected.")]
    [SerializeField] private Transform viewPos;

    [Header("UI")]
    [Tooltip("The UI canvas/panel that should only be visible when the phone is viewed.")]
    [SerializeField] private GameObject phoneUI;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    private Coroutine currentMove;
    private bool isAtView = false;
    private RectTransform phoneUIRect;
    private Vector2 originalAnchoredPos;

    public bool IsAtView => isAtView;

    // Optional: Start at idle position if set
    void Start()
    {
        if (idlePos != null)
        {
            transform.position = idlePos.position;
            transform.rotation = idlePos.rotation;
        }

        if (phoneUI != null)
        {
            phoneUIRect = phoneUI.GetComponent<RectTransform>();
            if (phoneUIRect != null) originalAnchoredPos = phoneUIRect.anchoredPosition;
            phoneUI.SetActive(false);
        }
    }

    void Update()
    {
        // Scale the UI dynamically to match the camera zoom
        if (isAtView && phoneUI != null)
        {
            phoneUI.transform.localScale = Vector3.one * Pickup.ZoomRatio;
            
            // Also scale the position so it doesn't slide sideways!
            if (phoneUIRect != null)
            {
                phoneUIRect.anchoredPosition = originalAnchoredPos * Pickup.ZoomRatio;
            }
        }
    }

    public void MoveToIdle()
    {
        if (idlePos == null || !isAtView) return;
        
        isAtView = false;
        if (phoneUI != null) phoneUI.SetActive(false);

        BeginMove(idlePos.position, idlePos.rotation);
    }

    public void MoveToView()
    {
        if (viewPos == null || isAtView) return;

        isAtView = true;
        
        // Reset to home screen whenever the phone is picked up
        var appManager = FindObjectOfType<PhoneAppManager>();
        if (appManager != null) appManager.GoToHome();

        if (phoneUI != null) phoneUI.SetActive(true);
        
        Vector3 targetEuler = viewPos.eulerAngles;
        targetEuler.x = -180f;
        targetEuler.y = 0f;
        targetEuler.z = -180f;
        Quaternion targetRot = Quaternion.Euler(targetEuler);

        BeginMove(viewPos.position, targetRot);
    }

    void OnMouseDown()
    {
        // Only allow clicking the 3D model to pick it up. 
        // Putting it down is handled by a UI button calling MoveToIdle().
        if (!isAtView)
        {
            // Drop gun if it's currently being viewed
            var pickup = FindObjectOfType<Pickup>();
            if (pickup != null) pickup.ForceReturnItem();

            MoveToView();
        }
    }

    private void BeginMove(Vector3 targetPos, Quaternion targetRot)
    {
        if (currentMove != null) StopCoroutine(currentMove);
        currentMove = StartCoroutine(LerpToPoint(targetPos, targetRot));
    }

    private IEnumerator LerpToPoint(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startP = transform.position;
        Quaternion startR = transform.rotation;
        float dist = Vector3.Distance(startP, targetPos);

        if (dist < 0.01f)
        {
            transform.SetPositionAndRotation(targetPos, targetRot);
            yield break;
        }

        float duration = dist / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            
            transform.position = Vector3.Lerp(startP, targetPos, t);
            transform.rotation = Quaternion.Slerp(startR, targetRot, t);
            
            yield return null;
        }

        transform.SetPositionAndRotation(targetPos, targetRot);
    }
}
