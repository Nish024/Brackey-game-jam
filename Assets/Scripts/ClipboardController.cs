using UnityEngine;
using System.Collections;

/// <summary>
/// Attached to the Clipboard prefab by ClipboardSpawner.
/// Handles clicking to inspect the clipboard and returning it to the counter.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ClipboardController : MonoBehaviour
{
    private Transform startPos;
    private Transform idlePos;
    private Transform viewPos;
    private float moveSpeed;

    private Coroutine currentMove;
    private bool isAtView = false;
    private bool isLeaving = false;

    public bool IsAtView => isAtView;

    public void Init(Transform start, Transform idle, Transform view, float speed)
    {
        startPos = start;
        idlePos = idle;
        viewPos = view;
        moveSpeed = speed;
    }

    public void MoveToIdle()
    {
        isAtView = false;
        BeginMove(idlePos.position, idlePos.rotation, false);
    }

    public void MoveToView()
    {
        isAtView = true;
        // The user specifically requested the X rotation to be 90 degrees and Z to be 180 when viewing
        Vector3 targetEuler = viewPos.eulerAngles;
        targetEuler.x = 90f;
        targetEuler.z = 180f;
        Quaternion targetRot = Quaternion.Euler(targetEuler);

        BeginMove(viewPos.position, targetRot, false);
    }

    public void LeaveAndDestroy()
    {
        isLeaving = true;
        BeginMove(startPos.position, startPos.rotation, true);
    }

    // Called when the player clicks the Clipboard collider
    void OnMouseDown()
    {
        if (isLeaving) return; // Can't interact if it's already leaving

        if (isAtView)
        {
            MoveToIdle();
        }
        else
        {
            // Drop gun if it's currently being viewed
            var pickup = FindObjectOfType<Pickup>();
            if (pickup != null) pickup.ForceReturnItem();
            

            
            MoveToView();
        }
    }

    // ─────────────────────────────────────────────────
    //  LERP MOVEMENT
    // ─────────────────────────────────────────────────

    private void BeginMove(Vector3 targetPos, Quaternion targetRot, bool destroyOnArrive)
    {
        if (currentMove != null) StopCoroutine(currentMove);
        currentMove = StartCoroutine(LerpToPoint(targetPos, targetRot, destroyOnArrive));
    }

    private IEnumerator LerpToPoint(Vector3 targetPos, Quaternion targetRot, bool destroyOnArrive)
    {
        Vector3 startP = transform.position;
        Quaternion startR = transform.rotation;
        float dist = Vector3.Distance(startP, targetPos);

        if (dist < 0.01f)
        {
            transform.SetPositionAndRotation(targetPos, targetRot);
            if (destroyOnArrive) Destroy(gameObject);
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

        if (destroyOnArrive)
        {
            Destroy(gameObject);
        }
    }
}
