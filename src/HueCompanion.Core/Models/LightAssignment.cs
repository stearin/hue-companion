namespace HueCompanion.Core.Models;

/// <summary>
/// How lights are assigned for an animation.
/// </summary>
public enum LightAssignment
{
    /// <summary>
    /// All available lights.
    /// </summary>
    All,

    /// <summary>
    /// Specific subset of lights by index.
    /// </summary>
    Subset,

    /// <summary>
    /// Random selection of lights.
    /// </summary>
    Random,

    /// <summary>
    /// Alternating lights (every other light).
    /// </summary>
    Alternating
}
