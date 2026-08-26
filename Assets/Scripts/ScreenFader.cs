using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Controls the full-screen black overlay used for transitions and the day-intro.
/// Supports CanvasGroup (preferred), Image, or RawImage. Auto-detects components on Awake.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    [Header("References")]
    [Tooltip("CanvasGroup on the BlackScreen (preferred). Auto-detected if left empty.")]
    [SerializeField] private CanvasGroup blackCanvasGroup;

    [Tooltip("Fallback RawImage or Image component. Auto-detected if left empty.")]
    [SerializeField] private Graphic blackScreenGraphic;

    [Tooltip("The TextMeshPro that shows 'DAY X'.")]
    [SerializeField] private TMPro.TextMeshProUGUI dayText;

    [Header("Timings (seconds)")]
    [SerializeField] private float fadeDuration  = 0.8f;
    [SerializeField] private float dayHoldTime   = 1.5f; // how long 'DAY X' is shown

    void Awake()
    {
        // Auto-detect components on this object if not manually assigned in Inspector
        if (blackCanvasGroup == null)
            blackCanvasGroup = GetComponent<CanvasGroup>();

        if (blackScreenGraphic == null && blackCanvasGroup == null)
            blackScreenGraphic = GetComponent<Graphic>();

        // Start fully black — DayManager will fade us out on the first day intro
        SetAlpha(1f);
        if (dayText != null) dayText.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────────

    /// <summary>
    /// Shows "DAY X" on the black screen, holds, then fades to transparent.
    /// Calls onComplete when fully faded out.
    /// </summary>
    public void ShowDayIntro(int dayNumber, System.Action onComplete)
    {
        gameObject.SetActive(true);
        StartCoroutine(DayIntroCo(dayNumber, onComplete));
    }

    /// <summary>Fades screen from transparent to black. Calls onComplete when done.</summary>
    public void FadeToBlack(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeCo(0f, 1f, fadeDuration, onComplete));
    }

    /// <summary>Fades screen from black to transparent. Calls onComplete when done.</summary>
    public void FadeFromBlack(System.Action onComplete = null)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeCo(1f, 0f, fadeDuration, onComplete));
    }

    /// <summary>Enable/disable raycast blocking on the black screen overlay.</summary>
    public void SetBlocksRaycasts(bool block)
    {
        if (blackCanvasGroup != null)
            blackCanvasGroup.blocksRaycasts = block;
        else if (blackScreenGraphic != null)
            blackScreenGraphic.raycastTarget = block;
    }

    // ─────────────────────────────────────────────────
    //  COROUTINES
    // ─────────────────────────────────────────────────

    private IEnumerator DayIntroCo(int dayNumber, System.Action onComplete)
    {
        // Ensure we start fully black
        SetAlpha(1f);

        // Show day text
        if (dayText != null)
        {
            dayText.text = $"DAY {dayNumber}";
            dayText.gameObject.SetActive(true);
        }

        // Hold
        yield return new WaitForSeconds(dayHoldTime);

        // Hide day text mid-fade
        if (dayText != null)
            dayText.gameObject.SetActive(false);

        // Fade from black to clear
        yield return StartCoroutine(FadeCo(1f, 0f, fadeDuration, null));

        SetAlpha(0f);
        onComplete?.Invoke();
    }

    private IEnumerator FadeCo(float from, float to, float duration, System.Action onComplete)
    {
        float elapsed = 0f;
        SetAlpha(from);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetAlpha(to);
        onComplete?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        if (blackCanvasGroup != null)
        {
            blackCanvasGroup.alpha = alpha;
            blackCanvasGroup.blocksRaycasts = (alpha > 0.01f);
        }
        else if (blackScreenGraphic != null)
        {
            Color c = blackScreenGraphic.color;
            c.a = alpha;
            blackScreenGraphic.color = c;
            blackScreenGraphic.raycastTarget = (alpha > 0.01f);
        }
    }
}
