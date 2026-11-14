namespace HueCompanion.Core.Models;

/// <summary>
/// How lights should be targeted for the animation.
/// </summary>
public enum LightTargeting
{
    /// <summary>
    /// Target all lights in a specific room.
    /// </summary>
    Room,

    /// <summary>
    /// Target all lights in a specific zone.
    /// </summary>
    Zone,

    /// <summary>
    /// Target specific individual lights.
    /// </summary>
    Lights
}
