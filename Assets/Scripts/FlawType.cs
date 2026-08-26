/// <summary>
/// Enum representing a detectable item flaw.
/// Used by ClueEvidenceTracker, InterrogationResolver, and InterrogationPanel.
///
/// NOTE: Stolen is intentionally excluded from the Interrogation checklist.
/// It resolves through the database hard-reject path, not a price-reduction flow.
/// Add it here and to the panel in a future pass if needed.
/// </summary>
public enum FlawType
{
    Damaged,
    Fake,
}
