namespace HueCompanion.Core.Models;

/// <summary>
/// Modifier keys for hotkeys.
/// </summary>
[Flags]
public enum HotkeyModifiers
{
    /// <summary>No modifiers.</summary>
    None = 0,
    /// <summary>Alt key.</summary>
    Alt = 0x0001,
    /// <summary>Ctrl key.</summary>
    Ctrl = 0x0002,
    /// <summary>Shift key.</summary>
    Shift = 0x0004,
    /// <summary>Windows key.</summary>
    Win = 0x0008
}
