using UnityEngine;
using System.Collections;

public enum ItemState
{
    MovingToCounter,
    AtCounter,
    MovingToView,
    AtView,
    ReturningToCounter,
    Leaving
}

/// <summary>
/// Attached to the item prefab by ItemSpawner.
/// Manages the item's movement between counter, view, and final destination.
/// ItemSpawner calls Init() to inject the scene positions.
/// </summary>
public class ItemController : MonoBehaviour
{
    private Transform counterPos;
    private Transform viewPos;
    private Transform boughtPos;
    private float moveSpeed = 4f;

    private Coroutine currentMove;

    public ItemState State { get; private set; } = ItemState.MovingToCounter;

    // ── Called by ItemSpawner after instantiation ──
    public void Init(Transform counter, Transform view, Transform bought, float speed)
    {
        counterPos = counter;
        viewPos    = view;
        boughtPos  = bought;
        moveSpeed  = speed;
    }

    // ─────────────────────────────────────────────────
    //  PUBLIC MOVEMENT API
    // ─────────────────────────────────────────────────

    public void MoveToCounter()
    {
        BeginMove(counterPos.position, counterPos.rotation,
                  ItemState.MovingToCounter, ItemState.AtCounter);
    }

    /// <summary>Called by Pickup when player clicks the item at the counter.</summary>
    public void MoveToView()
    {
        if (State != ItemState.AtCounter) return;
        BeginMove(viewPos.position, viewPos.rotation,
                  ItemState.MovingToView, ItemState.AtView);
    }

    /// <summary>Called by Pickup when player clicks again to put item back.</summary>
    public void ReturnToCounter()
    {
        if (State != ItemState.AtView && State != ItemState.MovingToView) return;
        BeginMove(counterPos.position, counterPos.rotation,
                  ItemState.ReturningToCounter, ItemState.AtCounter);
    }

    /// <summary>Called by ItemSpawner after Buy/Reject decision. Works from any position.</summary>
    public void MoveToFinalDestination(bool wasBought)
    {
        Vector3    target    = wasBought ? boughtPos.position  : counterPos.position;
        Quaternion targetRot = wasBought ? boughtPos.rotation  : counterPos.rotation;
        BeginMove(target, targetRot, ItemState.Leaving, ItemState.Leaving, destroyOnArrive: true);
    }

    // ─────────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────────

    private void BeginMove(Vector3 targetPos, Quaternion targetRot,
                           ItemState duringState, ItemState arrivedState,
                           bool destroyOnArrive = false)
    {
        if (currentMove != null) StopCoroutine(currentMove);
        State = duringState;
        currentMove = StartCoroutine(LerpToPoint(targetPos, targetRot, arrivedState, destroyOnArrive));
    }

    private IEnumerator LerpToPoint(Vector3 targetPos, Quaternion targetRot,
                                    ItemState arrivedState, bool destroyOnArrive)
    {
        Vector3    startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float dist = Vector3.Distance(startPos, targetPos);

        if (dist < 0.01f)
        {
            transform.SetPositionAndRotation(targetPos, targetRot);
            State = arrivedState;
            if (arrivedState == ItemState.AtCounter)
                GameEvents.OnItemArrivedAtCounter?.Invoke();
            if (destroyOnArrive) Destroy(gameObject);
            yield break;
        }

        float duration = dist / moveSpeed;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.SetPositionAndRotation(targetPos, targetRot);
        State = arrivedState;
        if (arrivedState == ItemState.AtCounter)
            GameEvents.OnItemArrivedAtCounter?.Invoke();

        if (destroyOnArrive) Destroy(gameObject);
    }
}
