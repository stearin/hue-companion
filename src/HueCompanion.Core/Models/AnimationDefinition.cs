namespace HueCompanion.Core.Models;

/// <summary>
/// Defines a single animation within a scene (keyframe-based or event-based).
/// </summary>
public class AnimationDefinition
{
    /// <summary>
    /// Unique identifier for this animation.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this animation.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of animation (keyframe or event-based).
    /// </summary>
    public AnimationType Type { get; set; }

    /// <summary>
    /// How lights are assigned for this animation.
    /// </summary>
    public LightAssignment LightAssignment { get; set; }

    /// <summary>
    /// Light indices to target (if using Subset or Random assignment).
    /// </summary>
    public List<int> TargetLightIndices { get; set; } = new();

    /// <summary>
    /// Percentage of lights to target (if using Random assignment).
    /// </summary>
    public double? RandomPercentage { get; set; }

    /// <summary>
    /// List of keyframes for keyframe-based animations.
    /// </summary>
    public List<AnimationKeyframe> Keyframes { get; set; } = new();

    /// <summary>
    /// Event pattern for event-based animations.
    /// </summary>
    public EventPattern? EventPattern { get; set; }

    /// <summary>
    /// Total duration of the animation in seconds (for keyframe animations).
    /// </summary>
    public double DurationSeconds { get; set; }

    /// <summary>
    /// How the animation should repeat.
    /// </summary>
    public RepeatMode RepeatMode { get; set; }

    /// <summary>
    /// Priority/layer for animation execution (higher numbers play on top).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Effect speed (0.0-1.0) for native effect animations.
    /// </summary>
    public double? EffectSpeed { get; set; }

    /// <summary>
    /// Effect brightness (0.0-1.0) for native effect animations.
    /// </summary>
    public double? EffectBrightness { get; set; }

    /// <summary>
    /// Event preset identifier for event-based animations.
    /// </summary>
    public string? EventPreset { get; set; }
}
