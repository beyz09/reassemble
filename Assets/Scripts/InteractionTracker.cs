using System;

/// <summary>
/// Tracks whether the player has interacted with anything this level.
/// Other systems can query InteractionTracker.HasInteracted or subscribe to OnInteracted.
/// </summary>
public static class InteractionTracker
{
    public static bool HasInteracted { get; private set; } = false;

    public static event Action OnInteracted;

    public static void ReportInteraction()
    {
        if (!HasInteracted)
        {
            HasInteracted = true;
            OnInteracted?.Invoke();
        }
    }

    public static void Reset()
    {
        HasInteracted = false;
    }
}
