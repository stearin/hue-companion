using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HueCompanion.Utilities;

internal static class ToggleSwitchStateHelper
{
    /// <summary>
    /// Displays the bound state immediately when a control is created or data finishes loading.
    /// Later state changes use the template's normal transitions.
    /// </summary>
    public static void ApplyInitialState(ToggleSwitch toggle)
    {
        toggle.ApplyTemplate();
        // Enter a different state so an in-flight transition to the current state is stopped.
        VisualStateManager.GoToState(toggle, "Initializing", useTransitions: false);
        VisualStateManager.GoToState(toggle, toggle.IsOn ? "On" : "Off", useTransitions: false);
    }
}
