using System.ComponentModel;
using System.Windows.Input;
using HueCompanion.Core.Models;

namespace HueCompanion.Core.ViewModels;

/// <summary>
/// Interface for ViewModels that can be displayed in a RoomCard control.
/// </summary>
public interface IRoomCardViewModel : INotifyPropertyChanged
{
    string RoomName { get; }
    bool IsOn { get; }
    double Brightness { get; }
    string RoomIcon { get; }
    int LightCount { get; }
    int BrightnessPercent { get; }
    IReadOnlyList<RgbColor> LightColors { get; }
    RgbColor? BackgroundColorRgb { get; }
    bool UseBlackText { get; }
    bool SupportsColor { get; }
    ICommand TapRoomCommand { get; }
    ICommand ToggleCommand { get; }
    ICommand SetBrightnessCommand { get; }
    ICommand? SetColorCommand { get; }
}
