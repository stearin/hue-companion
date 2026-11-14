namespace HueCompanion.Core.Models;

/// <summary>
/// Type of animation.
/// </summary>
public enum AnimationType
{
    /// <summary>
    /// Keyframe-based smooth animation (interpolated transitions).
    /// </summary>
    Keyframe,

    /// <summary>
    /// Event-based random triggers (e.g., lightning flashes, sparkles).
    /// </summary>
    Event,

    /// <summary>
    /// Native Hue effect (fire, candle, etc.).
    /// </summary>
    NativeEffect
}
