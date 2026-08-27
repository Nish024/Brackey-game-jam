using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;

/// <summary>
/// Replaces ScreenFader. Controls a physical 3D shutter door that lerps up and down.
/// Hides UI elements when the door is closed to prevent clipping/overlapping.
/// </summary>
public class ShutterDoor : MonoBehaviour
{
    [Header("Door References")]
    [Tooltip("The 3D model of the shutter door to move.")]
    [SerializeField] private Transform doorTransform;
    [Tooltip("The position/rotation where the door is fully OPEN (Up).")]
    [SerializeField] private Transform spawnUp;
    [Tooltip("The position/rotation where the door is fully CLOSED (Down).")]
    [SerializeField] private Transform spawnDown;

    [Header("UI References")]
    [Tooltip("The TextMeshPro that shows 'DAY X'.")]
    [SerializeField] private TextMeshProUGUI dayText;
    [Tooltip("UI Canvases/Panels to hide when the door is closed, so they don't clip through.")]
    [SerializeField] private GameObject[] uiToHide;

    [Header("Settings")]
    [SerializeField] private float moveDuration = 1.2f;
    [SerializeField] private float dayHoldTime = 1.5f;

    [Header("Events")]
    [Tooltip("Fired when the door starts moving (useful for playing a sound effect).")]
    public UnityEvent onDoorMove;

    private void Awake()
    {
        // Hide Day Text initially
        if (dayText != null) dayText.gameObject.SetActive(false);

        // Ensure door starts closed (down) at game launch
        if (doorTransform != null && spawnDown != null)
        {
            doorTransform.position = spawnDown.position;
            doorTransform.rotation = spawnDown.rotation;
        }

        // We start with the UI hidden since the door is closed
        SetUIHidden(true);
    }

    private void OnEnable()
    {
        GameEvents.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameEvents.OnGameOver -= HandleGameOver;
    }

    // ─────────────────────────────────────────────────
    //  PUBLIC API (Mimics ScreenFader)
    // ─────────────────────────────────────────────────

    /// <summary>
    /// Starts with the door CLOSED. Shows "DAY X", holds, then OPENS the door.
    /// Calls onComplete when fully open.
    /// </summary>
    public void ShowDayIntro(int dayNumber, System.Action onComplete)
    {
        gameObject.SetActive(true);
        StartCoroutine(DayIntroCo(dayNumber, onComplete));
    }

    /// <summary>
    /// Closes the door (moves to spawnDown). Hides UI. Calls onComplete when done.
    /// </summary>
    public void CloseDoor(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        StartCoroutine(MoveDoorCo(spawnDown, true, onComplete));
    }

    /// <summary>
    /// Opens the door (moves to spawnUp). Restores UI. Calls onComplete when done.
    /// </summary>
    public void OpenDoor(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        StartCoroutine(MoveDoorCo(spawnUp, false, onComplete));
    }

    // ─────────────────────────────────────────────────
    //  COROUTINES & HELPERS
    // ─────────────────────────────────────────────────

    private IEnumerator DayIntroCo(int dayNumber, System.Action onComplete)
    {
        // 1. Ensure door is completely closed
        if (doorTransform != null && spawnDown != null)
        {
            doorTransform.position = spawnDown.position;
            doorTransform.rotation = spawnDown.rotation;
        }

        // 2. Hide background UI
        SetUIHidden(true);

        // 3. Show "DAY X" overlay
        if (dayText != null)
        {
            dayText.text = $"DAY {dayNumber}";
            dayText.gameObject.SetActive(true);
        }

        // 4. Hold for the intro duration
        yield return new WaitForSeconds(dayHoldTime);

        // 5. Hide the Day text
        if (dayText != null)
            dayText.gameObject.SetActive(false);

        // 6. Open the door
        yield return StartCoroutine(MoveDoorCo(spawnUp, false, null));

        onComplete?.Invoke();
    }

    private IEnumerator MoveDoorCo(Transform targetPoint, bool hidingUI, System.Action onComplete)
    {
        if (doorTransform == null || targetPoint == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        // Fire the sound effect event!
        onDoorMove?.Invoke();

        // If we are closing the door, hide the UI immediately before it shuts
        if (hidingUI) SetUIHidden(true);

        float elapsed = 0f;
        Vector3 startPos = doorTransform.position;
        Quaternion startRot = doorTransform.rotation;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));
            
            doorTransform.position = Vector3.Lerp(startPos, targetPoint.position, t);
            doorTransform.rotation = Quaternion.Slerp(startRot, targetPoint.rotation, t);
            
            yield return null;
        }

        doorTransform.position = targetPoint.position;
        doorTransform.rotation = targetPoint.rotation;

        // If we are opening the door, restore the UI after it finishes opening
        if (!hidingUI) SetUIHidden(false);

        onComplete?.Invoke();
    }

    private void SetUIHidden(bool isHidden)
    {
        foreach (var uiElement in uiToHide)
        {
            if (uiElement != null)
            {
                // If it's hidden, we disable it. If it's not hidden, we enable it.
                uiElement.SetActive(!isHidden);
            }
        }
    }

    private void HandleGameOver(GameOverReason reason)
    {
        // If a game over occurs, instantly snap the door shut and hide UI to provide a clean background
        if (doorTransform != null && spawnDown != null)
        {
            doorTransform.position = spawnDown.position;
            doorTransform.rotation = spawnDown.rotation;
        }
        SetUIHidden(true);
    }
}
