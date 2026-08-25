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
    [SerializeField] private Button button;

    private bool timerHasExpired;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        // Start disabled — no decision has been made yet
        button.interactable = false;
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
        button.interactable = false;

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
        button.interactable = true;
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
        button.interactable = false;
    }

    private void OnShopOpened()
    {
        timerHasExpired = false;
        button.interactable = false;
    }

}
