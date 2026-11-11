using FluentAssertions;
using HueCompanion.Core.Models;
using HueCompanion.Core.Services.Interfaces;
using HueCompanion.Core.ViewModels;
using Moq;
using Xunit;

namespace HueCompanion.Tests.ViewModels;

public class LightDetailViewModelTests
{
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
