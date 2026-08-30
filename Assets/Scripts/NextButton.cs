using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The Next button — gatekeeper between customers.
/// Starts disabled when a new customer appears.
/// Enables itself when OnDecisionMade fires.
/// On click, requests the next customer (or triggers day-end if timer expired).
/// </summary>
public class NextButton : MonoBehaviour
{
    [SerializeField] private Button uiButton;
    [SerializeField] private Button3D button3D;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip nextSfx;

    private bool timerHasExpired;

    void Awake()
    {
        if (uiButton == null) uiButton = GetComponent<Button>();
        if (button3D == null) button3D = GetComponent<Button3D>();

        // Start disabled — no decision has been made yet
        SetInteractable(false);
    }

    void OnEnable()
    {
        GameEvents.OnDecisionMade += OnDecisionMade;
        GameEvents.OnTimerExpired += OnTimerExpired;
        GameEvents.OnCustomerReady += OnNewCustomer;
        GameEvents.OnShopOpened += OnShopOpened;
    }

    void OnDisable()
    {
        GameEvents.OnDecisionMade -= OnDecisionMade;
        GameEvents.OnTimerExpired -= OnTimerExpired;
        GameEvents.OnCustomerReady -= OnNewCustomer;
        GameEvents.OnShopOpened -= OnShopOpened;
    }

    public void OnNextClicked()
    {
        if (audioSource != null && nextSfx != null)
        {
            audioSource.PlayOneShot(nextSfx);
        }

        SetInteractable(false);

        if (timerHasExpired)
        {
            // Timer already expired — this was the last customer.
            // Don't spawn a new one. End the day now since the shop is clear!
            Debug.Log("[NextButton] Timer expired — ending day immediately.");
            GameEvents.OnDayEnd?.Invoke();
        }
        else
        {
            // Always request the current customer to leave
            // (CustomerSpawner listens and will spawn next if timer hasn't expired)
            GameEvents.OnNextCustomerRequested?.Invoke();
        }
    }

    // ─────────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────────

    private void OnDecisionMade(bool wasBought)
    {
        // Player made a decision — enable the Next button
        SetInteractable(true);
    }

    private void OnTimerExpired()
    {
        timerHasExpired = true;
        // Don't disable the button — if a decision is pending, let them finish.
        // If a decision was already made, the button is already enabled.
    }

    private void OnNewCustomer()
    {
        // A new customer just arrived — disable until decision is made
        SetInteractable(false);
    }

    private void OnShopOpened()
    {
        timerHasExpired = false;
        SetInteractable(false);
    }

    private void SetInteractable(bool state)
    {
        if (uiButton != null) uiButton.interactable = state;
        if (button3D != null) button3D.interactable = state;
    }
}
