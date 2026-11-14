namespace HueCompanion.Core.Models;

/// <summary>
/// Transition style between keyframes.
/// </summary>
public enum TransitionStyle
{
    /// <summary>
    /// Linear interpolation.
    /// </summary>
    Linear,

    /// <summary>
    /// Ease in (slow start).
    /// </summary>
    EaseIn,

    /// <summary>
    /// Ease out (slow end).
    /// </summary>
    EaseOut,

    /// <summary>
    /// Ease in and out (slow start and end).
    /// </summary>
    EaseInOut,

    /// <summary>
    /// Instant jump (no interpolation).
    /// </summary>
    Instant
}
