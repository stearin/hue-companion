namespace HueCompanion.Core.Models;

/// <summary>
/// A single state change within an event trigger.
/// </summary>
public class EventTriggerState
{
    /// <summary>
    /// Duration in seconds for this state.
    /// </summary>
    public double DurationSeconds { get; set; }

    /// <summary>
    /// Target brightness (0.0 to 1.0), or null to leave unchanged.
    /// </summary>
    public double? Brightness { get; set; }

    /// <summary>
    /// Target color, or null to leave unchanged.
    /// </summary>
    public HueColor? Color { get; set; }

    /// <summary>
    /// Target color temperature in mirek (153-500), or null to leave unchanged.
    /// </summary>
    public int? ColorTemperature { get; set; }

    /// <summary>
    /// Whether the light should be on during this state.
    /// </summary>
    public bool? IsOn { get; set; }

    /// <summary>
    /// Transition style for entering this state.
    /// </summary>
    public TransitionStyle TransitionStyle { get; set; } = TransitionStyle.Instant;
}
