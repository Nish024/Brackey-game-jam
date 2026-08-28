using System;

/// <summary>
/// Static event hub — every script fires and listens through here.
/// No MonoBehaviour, no instance needed. Just static Actions.
/// </summary>
public static class GameEvents
{
    // ── Timer ──────────────────────────────────────
    /// <summary>Fired every frame by RoundTimer with the current clock time string.</summary>
    public static Action<string> OnTimerTick;

    /// <summary>Fired once when the timer reaches zero.</summary>
    public static Action OnTimerExpired;

    // ── Transaction ────────────────────────────────
    /// <summary>Fired when the player clicks Buy (true) or Reject (false).</summary>
    public static Action<bool> OnDecisionMade;

    // ── Customer ───────────────────────────────────
    /// <summary>Fired when the Next button is clicked, requesting a new customer.</summary>
    public static Action OnNextCustomerRequested;

    /// <summary>Fired when a customer has arrived at the counter and is ready.</summary>
    public static Action OnCustomerReady;

    /// <summary>Fired by ItemSpawner when a gun with GunData is spawned.</summary>
    public static Action<GunData> OnGunDataLoaded;

    /// <summary>Fired when the item has physically arrived at the counter.</summary>
    public static Action OnItemArrivedAtCounter;

    /// <summary>Fired when the current customer has fully left the screen.</summary>
    public static Action OnCustomerLeft;

    /// <summary>Fired when the mouse pointer enters the customer's 3D collider.</summary>
    public static Action OnCustomerHoverEnter;

    /// <summary>Fired when the mouse pointer exits the customer's 3D collider.</summary>
    public static Action OnCustomerHoverExit;

    // ── Money ──────────────────────────────────────
    /// <summary>Fired whenever net worth changes, with the new value.</summary>
    public static Action<float> OnNetWorthChanged;

    // ── Loan ───────────────────────────────────────
    /// <summary>Fired by TransactionController when a buy fails due to insufficient funds.
    /// Parameters: item price gap the player needs, current interest rate.</summary>
    public static Action<float, float> OnLoanOffered;

    /// <summary>Fired by LoanManager when player confirms a loan — TransactionController should retry Buy.</summary>
    public static Action OnLoanConfirmed;

    // ── Day / Game State ───────────────────────────
    /// <summary>Fired when the day's auction phase should begin.</summary>
    public static Action OnDayEnd;

    /// <summary>Fired by DayManager after the day-intro fade finishes — starts timer and spawning.</summary>
    public static Action OnShopOpened;

    /// <summary>Fired by DayManager when the day is over — disables all shop interactions.</summary>
    public static Action OnShopClosed;

    /// <summary>Fired by AuctionResultsPanel when the player clicks Next Day.</summary>
    public static Action OnAuctionComplete;

    /// <summary>Fired when the game is over, with the reason.</summary>
    public static Action<GameOverReason> OnGameOver;

    /// <summary>Clears all listeners. Call on scene transitions to prevent stale references.</summary>
    public static void ClearAll()
    {
        OnTimerTick = null;
        OnTimerExpired = null;
        OnDecisionMade = null;
        OnNextCustomerRequested = null;
        OnCustomerReady = null;
        OnItemArrivedAtCounter = null;
        OnCustomerLeft = null;
        OnCustomerHoverEnter = null;
        OnCustomerHoverExit = null;
        OnGunDataLoaded = null;
        OnNetWorthChanged = null;
        OnDayEnd = null;
        OnShopOpened = null;
        OnShopClosed = null;
        OnAuctionComplete = null;
        OnGameOver = null;
        OnLoanOffered = null;
        OnLoanConfirmed = null;
    }
}

public enum GameOverReason
{
    Arrest,
    Bankruptcy,
    MissedQuota,
    LoanNotRepaid
}
