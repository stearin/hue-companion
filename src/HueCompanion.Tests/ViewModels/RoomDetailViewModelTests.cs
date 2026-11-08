using FluentAssertions;
using HueCompanion.Core.Models;
using HueCompanion.Core.Services.Interfaces;
using HueCompanion.Core.ViewModels;
using Moq;
using Xunit;

namespace HueCompanion.Tests.ViewModels;

public class RoomDetailViewModelTests
{
    [Theory]
    [InlineData(LightGroupType.Room, false)]
    [InlineData(LightGroupType.Room, true)]
    [InlineData(LightGroupType.Zone, false)]
    [InlineData(LightGroupType.Zone, true)]
    public async Task LoadRoomAsync_AndSliderInitialization_DoNotWriteToBridge(
        LightGroupType groupType, bool firstLightOn)
    {
        var bridge = new Mock<IHueBridgeService>(MockBehavior.Strict);
        var group = CreateGroup(groupType, firstLightOn);
        SetupGroupReads(bridge, group);
        using var vm = CreateViewModel(bridge.Object);

        await vm.LoadRoomAsync(group.Id, groupType);

        // ValueChanged can echo the displayed integer percentage on initial binding.
        await vm.SetBrightnessCommand.ExecuteAsync(vm.BrightnessPercent / 100.0);
        foreach (var light in vm.Lights)
            await light.SetBrightnessCommand.ExecuteAsync(light.BrightnessPercent / 100.0);

        vm.IsOn.Should().Be(firstLightOn);
        vm.Lights.Select(l => l.IsOn).Should().Equal(firstLightOn, false);
        vm.Brightness.Should().Be(group.Brightness);
        vm.Lights.Select(l => l.Brightness).Should().Equal(0.657, 0.423);
        if (groupType == LightGroupType.Room)
        {
            bridge.Verify(b => b.GetRoomAsync(group.Id), Times.Once);
            bridge.Verify(b => b.GetScenesForRoomAsync(group.Id), Times.Once);
        }
        else
        {
            bridge.Verify(b => b.GetZoneAsync(group.Id), Times.Once);
            bridge.Verify(b => b.GetScenesForZoneAsync(group.Id), Times.Once);
        }
        bridge.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(LightGroupType.Room)]
    [InlineData(LightGroupType.Zone)]
    public async Task SetBrightnessAsync_UserChangesValue_WritesToSelectedGroup(LightGroupType groupType)
    {
        var bridge = new Mock<IHueBridgeService>();
        var group = CreateGroup(groupType, false);
        SetupGroupReads(bridge, group);
        using var vm = CreateViewModel(bridge.Object);
        await vm.LoadRoomAsync(group.Id, groupType);
        bridge.Invocations.Clear();

        await vm.SetBrightnessCommand.ExecuteAsync(0.8);

        vm.Brightness.Should().Be(0.8);
        if (groupType == LightGroupType.Room)
            bridge.Verify(b => b.SetRoomBrightnessAsync(group.Id, 0.8, null), Times.Once);
        else
            bridge.Verify(b => b.SetZoneBrightnessAsync(group.Id, 0.8), Times.Once);
        bridge.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LightBrightness_RefreshAndEcho_DoNotWrite_ButUserEditDoes()
    {
        var bridge = new Mock<IHueBridgeService>();
        var id = Guid.NewGuid();
        var vm = new LightItemViewModel(new LightModel
        {
            Id = id, IsOn = false, Brightness = 0.657
        }, bridge.Object);

        vm.UpdateFromBridge(false, 0.423, HueColor.White);
        await vm.SetBrightnessCommand.ExecuteAsync(vm.BrightnessPercent / 100.0);
        await vm.SetBrightnessCommand.ExecuteAsync(vm.Brightness);

        vm.IsOn.Should().BeFalse();
        vm.Brightness.Should().Be(0.423);
        bridge.VerifyNoOtherCalls();

        await vm.SetBrightnessCommand.ExecuteAsync(0.8);

        bridge.Verify(b => b.SetLightBrightnessAsync(id, 0.8, null), Times.Once);
        bridge.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(LightGroupType.Room)]
    [InlineData(LightGroupType.Zone)]
    public async Task LightStateChanged_DisplayedLight_RefreshesLightAndGroup(LightGroupType groupType)
    {
        var bridge = new Mock<IHueBridgeService>();
        var multiBridge = new Mock<IMultiBridgeService>();
        var group = CreateGroup(groupType, false);
        SetupGroupReads(bridge, group);
        using var vm = CreateViewModel(bridge.Object, multiBridge);
        await vm.LoadRoomAsync(group.Id, groupType);
        var color = HueColor.FromRgb(20, 40, 60);

        multiBridge.Raise(b => b.LightStateChanged += null, new MultiBridgeLightStateChangedEventArgs
        {
            BridgeId = "bridge-1",
            LightId = group.Lights[0].Id,
            IsOn = true,
            Brightness = 0.8,
            Color = color
        });

        vm.Lights[0].IsOn.Should().BeTrue();
        vm.Lights[0].Brightness.Should().Be(0.8);
        vm.Lights[0].CurrentColor.Should().Be(color);
        vm.IsOn.Should().BeTrue();
        vm.Brightness.Should().Be(0.8);
        vm.LightColors.Should().ContainSingle();
        bridge.Verify(b => b.SetLightOnAsync(It.IsAny<Guid>(), It.IsAny<bool>(), null), Times.Never);
    }

    private static RoomModel CreateGroup(LightGroupType groupType, bool firstLightOn) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test group",
        GroupType = groupType,
        IsOn = firstLightOn,
        Brightness = 0.657,
        Lights = new()
        {
            new() { Id = Guid.NewGuid(), Name = "First", IsOn = firstLightOn, Brightness = 0.657 },
            new() { Id = Guid.NewGuid(), Name = "Second", IsOn = false, Brightness = 0.423 }
        }
    };

    private static void SetupGroupReads(Mock<IHueBridgeService> bridge, RoomModel group)
    {
        var result = Result<RoomModel>.Success(group);
        var scenes = Result<IReadOnlyList<SceneModel>>.Success(Array.Empty<SceneModel>());
        if (group.GroupType == LightGroupType.Room)
        {
            bridge.Setup(b => b.GetRoomAsync(group.Id)).ReturnsAsync(result);
            bridge.Setup(b => b.GetScenesForRoomAsync(group.Id)).ReturnsAsync(scenes);
        }
        else
        {
            bridge.Setup(b => b.GetZoneAsync(group.Id)).ReturnsAsync(result);
            bridge.Setup(b => b.GetScenesForZoneAsync(group.Id)).ReturnsAsync(scenes);
        }
    }

    private static RoomDetailViewModel CreateViewModel(IHueBridgeService bridge, Mock<IMultiBridgeService>? multiBridge = null)
    {
        multiBridge ??= new Mock<IMultiBridgeService>();
        multiBridge.Setup(b => b.GetDefaultBridgeService()).Returns(bridge);
        multiBridge.SetupGet(b => b.ConnectionStatus).Returns(new Dictionary<string, bool>());
        var storage = new Mock<ISceneStorageService>();
        var emptyScenes = Result<IReadOnlyList<AnimatedSceneModel>>.Success(Array.Empty<AnimatedSceneModel>());
        storage.Setup(s => s.LoadBuiltInScenesAsync()).ReturnsAsync(emptyScenes);
        storage.Setup(s => s.LoadUserScenesAsync()).ReturnsAsync(emptyScenes);
        var assignments = new Mock<IRoomSceneAssignmentService>();
        assignments.Setup(a => a.GetAssignedScenesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(Array.Empty<string>());
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.Settings).Returns(new AppSettings());
        return new RoomDetailViewModel(multiBridge.Object, Mock.Of<IAnimationService>(),
            storage.Object, assignments.Object, settings.Object);
    }
}
