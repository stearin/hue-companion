using FluentAssertions;
using HueCompanion.Core.Models;
using HueCompanion.Core.Services.Interfaces;
using HueCompanion.Core.ViewModels;
using Moq;
using Xunit;

namespace HueCompanion.Tests.ViewModels;

public class LightDetailViewModelTests
{
    [Fact]
    public async Task UserCommands_WriteOnce_WhilePropertyAssignmentsOnlyUpdateState()
    {
        var id = Guid.NewGuid();
        var bridge = new Mock<IHueBridgeService>();
        bridge.Setup(b => b.GetLightAsync(id)).ReturnsAsync(Result<LightModel>.Success(new() { Id = id }));
        var multiBridge = new Mock<IMultiBridgeService>();
        multiBridge.Setup(b => b.GetDefaultBridgeService()).Returns(bridge.Object);
        var vm = new LightDetailViewModel(multiBridge.Object);
        await vm.LoadLightAsync(id);
        bridge.Invocations.Clear();

        vm.IsOn = true;
        vm.Brightness = 0.657;
        vm.BrightnessPercent.Should().Be(65);
        bridge.VerifyNoOtherCalls();

        await vm.ToggleCommand.ExecuteAsync(null);
        vm.IsOn.Should().BeFalse();
        bridge.Verify(b => b.SetLightOnAsync(id, false, null), Times.Once);
        bridge.VerifyNoOtherCalls();
        bridge.Invocations.Clear();

        await vm.SetBrightnessCommand.ExecuteAsync(0.8);
        vm.IsOn.Should().BeTrue();
        bridge.Verify(b => b.SetLightBrightnessAsync(id, 0.8, null), Times.Once);
        bridge.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LoadLightAsync_UpdatesDisplayedPercentage_WithoutWritingToBridge(bool isOn)
    {
        var lightId = Guid.NewGuid();
        var bridge = new Mock<IHueBridgeService>(MockBehavior.Strict);
        bridge.Setup(b => b.GetLightAsync(lightId)).ReturnsAsync(Result<LightModel>.Success(new()
        {
            Id = lightId,
            Brightness = 0.657,
            IsOn = isOn
        }));
        var multiBridge = new Mock<IMultiBridgeService>();
        multiBridge.Setup(b => b.GetDefaultBridgeService()).Returns(bridge.Object);
        var vm = new LightDetailViewModel(multiBridge.Object);
        var displayedPercent = vm.BrightnessPercent;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.BrightnessPercent))
                displayedPercent = vm.BrightnessPercent;
        };

        await vm.LoadLightAsync(lightId);

        displayedPercent.Should().Be(65);
        vm.Brightness.Should().Be(0.657);
        vm.IsOn.Should().Be(isOn);
        bridge.Verify(b => b.GetLightAsync(lightId), Times.Once);
        bridge.VerifyNoOtherCalls();
    }
}
