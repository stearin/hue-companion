using System.Globalization;
using System.Text.RegularExpressions;
using HueCompanion.Core.Models;

namespace HueCompanion.Mcp.Services;

/// <summary>
/// Parses color strings (hex, RGB, named colors) into HueColor (CIE xy).
/// </summary>
public static partial class ColorParser
{
    private static readonly Dictionary<string, RgbColor> NamedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["red"] = new(255, 0, 0),
        ["green"] = new(0, 255, 0),
        ["blue"] = new(0, 0, 255),
        ["yellow"] = new(255, 255, 0),
        ["orange"] = new(255, 165, 0),
        ["purple"] = new(128, 0, 128),
        ["magenta"] = new(255, 0, 255),
        ["pink"] = new(255, 192, 203),
        ["cyan"] = new(0, 255, 255),
        ["white"] = new(255, 255, 255),
        ["warm white"] = new(255, 244, 229),
        ["cool white"] = new(200, 210, 255),
        ["soft white"] = new(255, 240, 220),
        ["warm orange"] = new(255, 140, 0),
        ["turquoise"] = new(64, 224, 208),
        ["teal"] = new(0, 128, 128),
        ["coral"] = new(255, 127, 80),
        ["salmon"] = new(250, 128, 114),
        ["lavender"] = new(230, 230, 250),
        ["indigo"] = new(75, 0, 130),
        ["violet"] = new(238, 130, 238),
        ["lime"] = new(0, 255, 0),
        ["gold"] = new(255, 215, 0),
        ["crimson"] = new(220, 20, 60),
        ["navy"] = new(0, 0, 128),
        ["olive"] = new(128, 128, 0),
        ["maroon"] = new(128, 0, 0),
        ["aqua"] = new(0, 255, 255),
        ["peach"] = new(255, 218, 185),
        ["mint"] = new(152, 255, 152),
        ["rose"] = new(255, 0, 127),
    };

    [GeneratedRegex(@"^#?([0-9a-fA-F]{6})$")]
    private static partial Regex HexRegex();

    [GeneratedRegex(@"^rgb\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*\)$", RegexOptions.IgnoreCase)]
    private static partial Regex RgbRegex();

    /// <summary>
    /// Parse a color string into a HueColor.
    /// Supports: hex (#FF0000), RGB (rgb(255,0,0)), and named colors (red, warm white, etc.)
    /// </summary>
    public static HueColor? Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim();

        // Try hex
        var hexMatch = HexRegex().Match(input);
        if (hexMatch.Success)
        {
            var hex = hexMatch.Groups[1].Value;
            byte r = byte.Parse(hex[..2], NumberStyles.HexNumber);
            byte g = byte.Parse(hex[2..4], NumberStyles.HexNumber);
            byte b = byte.Parse(hex[4..6], NumberStyles.HexNumber);
            return HueColor.FromRgb(r, g, b);
        }

        // Try rgb()
        var rgbMatch = RgbRegex().Match(input);
        if (rgbMatch.Success)
        {
            // Clamp to 0-255 to prevent overflow from out-of-range values
            byte r = (byte)Math.Clamp(int.Parse(rgbMatch.Groups[1].Value), 0, 255);
            byte g = (byte)Math.Clamp(int.Parse(rgbMatch.Groups[2].Value), 0, 255);
            byte b = (byte)Math.Clamp(int.Parse(rgbMatch.Groups[3].Value), 0, 255);
            return HueColor.FromRgb(r, g, b);
        }

        // Try named color
        if (NamedColors.TryGetValue(input, out var named))
        {
            return HueColor.FromRgb(named);
        }

        return null;
    }

    /// <summary>
    /// Convert a HueColor to hex string for display.
    /// </summary>
    public static string ToHex(HueColor color, double brightness = 1.0)
    {
        var (r, g, b) = color.ToRgb(brightness);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
