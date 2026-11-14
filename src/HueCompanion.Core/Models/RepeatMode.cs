namespace HueCompanion.Core.Models;

/// <summary>
/// How an animation repeats.
/// </summary>
public enum RepeatMode
{
    /// <summary>
    /// Play once and stop.
    /// </summary>
    Once,

    /// <summary>
    /// Loop continuously.
    /// </summary>
    Loop,

    /// <summary>
    /// Loop forward then backward (ping-pong).
    /// </summary>
    PingPong
}
