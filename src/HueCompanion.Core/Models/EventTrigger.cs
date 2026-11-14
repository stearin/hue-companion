namespace HueCompanion.Core.Models;

/// <summary>
/// A single event trigger definition (e.g., a lightning flash or sparkle).
/// </summary>
public class EventTrigger
{
    /// <summary>
    /// List of state changes to apply in sequence.
    /// </summary>
    public List<EventTriggerState> States { get; set; } = new();

    /// <summary>
    /// Relative probability weight for this trigger (higher = more likely).
    /// </summary>
    public double Weight { get; set; } = 1.0;
}
