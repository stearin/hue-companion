using System.Text.Json.Serialization;

namespace HueCompanion.Core.Models;

/// <summary>
/// Settings for the global hotkey feature.
/// </summary>
public class HotkeySettings
{
    /// <summary>
    /// Whether the global hotkey is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// The modifier keys for the hotkey.
    /// </summary>
    public HotkeyModifiers Modifiers { get; set; } = HotkeyModifiers.Win | HotkeyModifiers.Shift;

    /// <summary>
    /// The virtual key code for the hotkey.
    /// </summary>
    public VirtualKey Key { get; set; } = VirtualKey.H;

    /// <summary>
    /// Gets a human-readable string representation of the hotkey.
    /// </summary>
    [JsonIgnore]
    public string DisplayString
    {
        get
        {
            var parts = new List<string>();

            if (Modifiers.HasFlag(HotkeyModifiers.Win))
                parts.Add("Win");
            if (Modifiers.HasFlag(HotkeyModifiers.Ctrl))
                parts.Add("Ctrl");
            if (Modifiers.HasFlag(HotkeyModifiers.Alt))
                parts.Add("Alt");
            if (Modifiers.HasFlag(HotkeyModifiers.Shift))
                parts.Add("Shift");

            parts.Add(Key.ToString());

            return string.Join(" + ", parts);
        }
    }
}
