using CommunityToolkit.Mvvm.Input;
using FluentAssertions;
using HueCompanion.Core.Models;
using HueCompanion.Core.Services.Interfaces;
using HueCompanion.Core.ViewModels;
using Moq;
using Xunit;

namespace HueCompanion.Tests.ViewModels;

public class CardStateTests
{
    [Theory]
    [InlineData(false, PinnedItemType.Room)]
    [InlineData(false, PinnedItemType.Zone)]
    [InlineData(true, PinnedItemType.Room)]
    [InlineData(true, PinnedItemType.Zone)]
    [InlineData(true, PinnedItemType.Light)]
    public async Task BridgeEventsOnlyUpdateState_AndUserCommandsWriteToCorrectEndpoint(
        bool dashboard, PinnedItemType type)
    {
        var bridge = new Mock<IHueBridgeService>();
        var multiBridge = new Mock<IMultiBridgeService>();
        multiBridge.Setup(b => b.GetDefaultBridgeService()).Returns(bridge.Object);
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.Settings).Returns(new AppSettings());
        var light = new LightModel { Id = Guid.NewGuid(), IsOn = false, Brightness = 0.4 };
        var room = new RoomModel
        {
            Id = Guid.NewGuid(),
            GroupType = type == PinnedItemType.Zone ? LightGroupType.Zone : LightGroupType.Room,
            Lights = new() { light }
        };
        IRoomCardViewModel vm = dashboard
            ? type == PinnedItemType.Light
                ? new DashboardCardViewModel(light, multiBridge.Object, Mock.Of<IPinnedItemsService>(), settings.Object)
                : new DashboardCardViewModel(room, multiBridge.Object, Mock.Of<IPinnedItemsService>(), settings.Object)
            : new RoomCardViewModel(room, bridge.Object);
        var notifications = new List<string?>();
        vm.PropertyChanged += (_, e) => notifications.Add(e.PropertyName);
        var update = new LightStateChangedEventArgs { LightId = light.Id, IsOn = true, Brightness = 0.657 };

        if (vm is DashboardCardViewModel card)
            card.OnLightStateChanged(update);
        else
            ((RoomCardViewModel)vm).OnLightStateChanged(update);

        vm.IsOn.Should().BeTrue();
        vm.BrightnessPercent.Should().Be(65);
        notifications.Should().Contain(new[] { "IsOn", "BrightnessPercent", "BrightnessDisplayText", "LightColors" });
        bridge.VerifyNoOtherCalls();

        var id = type == PinnedItemType.Light ? light.Id : room.Id;
        await ((IAsyncRelayCommand)vm.ToggleCommand).ExecuteAsync(null);
        vm.IsOn.Should().BeFalse();
        if (type == PinnedItemType.Light)
            bridge.Verify(b => b.SetLightOnAsync(id, false, null), Times.Once);
        else if (type == PinnedItemType.Zone)
            bridge.Verify(b => b.SetZoneOnAsync(id, false), Times.Once);
        else
            bridge.Verify(b => b.SetRoomOnAsync(id, false, null), Times.Once);
        bridge.VerifyNoOtherCalls();
        bridge.Invocations.Clear();

        await ((IAsyncRelayCommand)vm.SetBrightnessCommand).ExecuteAsync(0.8);
        vm.IsOn.Should().BeTrue();
        if (type == PinnedItemType.Light)
            bridge.Verify(b => b.SetLightBrightnessAsync(id, 0.8, null), Times.Once);
        else if (type == PinnedItemType.Zone)
            bridge.Verify(b => b.SetZoneBrightnessAsync(id, 0.8), Times.Once);
        else
            bridge.Verify(b => b.SetRoomBrightnessAsync(id, 0.8, null), Times.Once);
        bridge.VerifyNoOtherCalls();
    }
}
