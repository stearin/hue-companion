namespace HueCompanion.Core.Models;

/// <summary>
/// An sRGB color with gamma-encoded red, green, and blue channels from 0 to 255.
/// </summary>
public readonly record struct RgbColor(byte R, byte G, byte B);
