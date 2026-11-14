namespace HueCompanion.Core.Models;

/// <summary>
/// Provides color space conversion utilities for Hue XY colors.
/// </summary>
public static class ColorConverter
{
    /// <summary>
    /// Converts Hue XY color to HSV color space.
    /// </summary>
    /// <param name="xy">The XY color coordinates.</param>
    /// <param name="brightness">The brightness value (0.0 to 1.0).</param>
    /// <returns>HSV color as (hue: 0-360, saturation: 0-1, value: 0-1).</returns>
    public static (double Hue, double Saturation, double Value) XyToHsv(HueColor xy, double brightness = 1.0)
    {
        // Convert XY to RGB first
        var (r, g, b) = XyToRgb(xy, brightness);

        // Convert RGB to HSV
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        // Calculate hue
        double hue = 0;
        if (delta > 0)
        {
            if (max == r)
                hue = 60 * (((g - b) / delta) % 6);
            else if (max == g)
                hue = 60 * (((b - r) / delta) + 2);
            else
                hue = 60 * (((r - g) / delta) + 4);

            if (hue < 0)
                hue += 360;
        }

        // Calculate saturation
        var saturation = max > 0 ? delta / max : 0;

        // Value is the max component
        var value = max;

        return (hue, saturation, value);
    }

    /// <summary>
    /// Converts HSV color to Hue XY color space.
    /// </summary>
    /// <param name="hue">Hue (0-360 degrees).</param>
    /// <param name="saturation">Saturation (0-1).</param>
    /// <param name="value">Value/brightness (0-1).</param>
    /// <returns>XY color coordinates and brightness.</returns>
    public static (HueColor Xy, double Brightness) HsvToXy(double hue, double saturation, double value)
    {
        // Convert HSV to RGB
        var c = value * saturation;
        var x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
        var m = value - c;

        double r, g, b;
        if (hue < 60)
            (r, g, b) = (c, x, 0);
        else if (hue < 120)
            (r, g, b) = (x, c, 0);
        else if (hue < 180)
            (r, g, b) = (0, c, x);
        else if (hue < 240)
            (r, g, b) = (0, x, c);
        else if (hue < 300)
            (r, g, b) = (x, 0, c);
        else
            (r, g, b) = (c, 0, x);

        r += m;
        g += m;
        b += m;

        // Convert RGB to XY
        return RgbToXy(r, g, b);
    }

    /// <summary>
    /// Converts XY color to RGB.
    /// </summary>
    internal static (double R, double G, double B) XyToRgb(HueColor xy, double brightness)
    {
        var chromaticityX = xy.X;
        var chromaticityY = xy.Y;

        if (chromaticityY <= 0) return (1.0, 1.0, 1.0); // White fallback for degenerate input

        var chromaticityZ = 1.0 - chromaticityX - chromaticityY;

        var tristimulusY = brightness;
        var tristimulusX = (tristimulusY / chromaticityY) * chromaticityX;
        var tristimulusZ = (tristimulusY / chromaticityY) * chromaticityZ;

        // Convert XYZ to linear RGB using sRGB D65 matrix
        var linearRed = tristimulusX * 3.2404542 - tristimulusY * 1.5371385 - tristimulusZ * 0.4985314;
        var linearGreen = -tristimulusX * 0.9692660 + tristimulusY * 1.8760108 + tristimulusZ * 0.0415560;
        var linearBlue = tristimulusX * 0.0556434 - tristimulusY * 0.2040259 + tristimulusZ * 1.0572252;

        // Apply gamma correction
        var srgbRed = ApplyGamma(linearRed);
        var srgbGreen = ApplyGamma(linearGreen);
        var srgbBlue = ApplyGamma(linearBlue);

        // Clamp to [0, 1]
        srgbRed = Math.Clamp(srgbRed, 0, 1);
        srgbGreen = Math.Clamp(srgbGreen, 0, 1);
        srgbBlue = Math.Clamp(srgbBlue, 0, 1);

        return (srgbRed, srgbGreen, srgbBlue);
    }

    /// <summary>
    /// Converts RGB to XY color.
    /// </summary>
    private static (HueColor Xy, double Brightness) RgbToXy(double srgbRed, double srgbGreen, double srgbBlue)
    {
        // Apply inverse gamma correction
        var linearRed = RemoveGamma(srgbRed);
        var linearGreen = RemoveGamma(srgbGreen);
        var linearBlue = RemoveGamma(srgbBlue);

        // Convert linear RGB to XYZ using sRGB D65 matrix
        var tristimulusX = linearRed * 0.4124564 + linearGreen * 0.3575761 + linearBlue * 0.1804375;
        var tristimulusY = linearRed * 0.2126729 + linearGreen * 0.7151522 + linearBlue * 0.0721750;
        var tristimulusZ = linearRed * 0.0193339 + linearGreen * 0.1191920 + linearBlue * 0.9503041;

        // Convert XYZ to xy
        var tristimulusSum = tristimulusX + tristimulusY + tristimulusZ;
        if (tristimulusSum == 0)
            return (HueColors.WarmWhite, 0);

        var chromaticityX = tristimulusX / tristimulusSum;
        var chromaticityY = tristimulusY / tristimulusSum;

        // Clamp chromaticity coordinates to [0, 1]
        chromaticityX = Math.Clamp(chromaticityX, 0, 1);
        chromaticityY = Math.Clamp(chromaticityY, 0, 1);

        return (new HueColor(chromaticityX, chromaticityY), tristimulusY);
    }

    /// <summary>
    /// Applies gamma correction for sRGB.
    /// </summary>
    private static double ApplyGamma(double value)
    {
        return value <= 0.0031308
            ? 12.92 * value
            : 1.055 * Math.Pow(value, 1.0 / 2.4) - 0.055;
    }

    /// <summary>
    /// Removes gamma correction for sRGB.
    /// </summary>
    private static double RemoveGamma(double value)
    {
        return value <= 0.04045
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
    }

    /// <summary>
    /// Interpolates between two colors directly in CIE xy chromaticity space.
    /// This avoids the lossy XY→RGB→HSV→RGB→XY round-trip that can shift hues,
    /// and matches how the Hue bridge itself transitions between colors.
    /// </summary>
    /// <param name="start">Starting XY color.</param>
    /// <param name="end">Ending XY color.</param>
    /// <param name="t">Interpolation factor (0.0 to 1.0).</param>
    /// <returns>Interpolated XY color.</returns>
    public static HueColor InterpolateXy(HueColor start, HueColor end, double t)
    {
        if (t <= 0) return start;
        if (t >= 1) return end;

        var x = start.X + (end.X - start.X) * t;
        var y = start.Y + (end.Y - start.Y) * t;

        return new HueColor(x, y);
    }

    /// <summary>
    /// Interpolates between two colors in HSV color space to avoid muddy colors.
    /// Brightness is handled separately and should not affect the color interpolation.
    /// </summary>
    /// <param name="start">Starting XY color.</param>
    /// <param name="end">Ending XY color.</param>
    /// <param name="t">Interpolation factor (0.0 to 1.0).</param>
    /// <param name="startBrightness">Starting brightness (not used for color, kept for API compatibility).</param>
    /// <param name="endBrightness">Ending brightness (not used for color, kept for API compatibility).</param>
    /// <returns>Interpolated XY color.</returns>
    public static HueColor InterpolateHsv(HueColor start, HueColor end, double t, double startBrightness = 1.0, double endBrightness = 1.0)
    {
        // Short-circuit: return exact XY color at endpoints or when colors are identical
        // to avoid lossy XY→RGB→HSV→RGB→XY round-trip that shifts hues
        if (t <= 0) return start;
        if (t >= 1) return end;
        if (Math.Abs(start.X - end.X) < 0.0001 && Math.Abs(start.Y - end.Y) < 0.0001)
            return start;

        // Convert to HSV at full brightness to preserve color saturation
        // Brightness is handled separately in the keyframe system
        var (h1, s1, _) = XyToHsv(start, 1.0);
        var (h2, s2, _) = XyToHsv(end, 1.0);

        // Interpolate hue using shortest path around the color wheel
        var hueDiff = h2 - h1;
        if (Math.Abs(hueDiff) > 180)
        {
            if (hueDiff > 0)
                h1 += 360;
            else
                h2 += 360;
        }

        var hue = h1 + (h2 - h1) * t;
        if (hue < 0)
            hue += 360;
        if (hue >= 360)
            hue -= 360;

        // Interpolate saturation
        var saturation = s1 + (s2 - s1) * t;

        // Always use full value (1.0) for color - brightness is separate
        var value = 1.0;

        // Convert back to XY
        var (xy, _) = HsvToXy(hue, saturation, value);
        return xy;
    }
}
