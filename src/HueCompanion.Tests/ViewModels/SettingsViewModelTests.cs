using FluentAssertions;
using HueCompanion.Core.Models;
using HueCompanion.Core.Services.Interfaces;
using HueCompanion.Core.ViewModels;
using Moq;
using Xunit;

namespace HueCompanion.Tests.ViewModels;

public class SettingsViewModelTests
{
    [Theory]
    [InlineData("7.8.9")]
    [InlineData("7.8.9-beta.2+build.123")]
    public void AppVersion_UsesSuppliedVersionBeforeAndAfterLoadingSettings(string version)
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.Settings).Returns(new AppSettings());
        var bridges = new Mock<IMultiBridgeService>();
        bridges.SetupGet(b => b.ConfiguredBridges).Returns(Array.Empty<BridgeModel>());
        bridges.SetupGet(b => b.ConnectionStatus).Returns(new Dictionary<string, bool>());
        using var vm = new SettingsViewModel(settings.Object, bridges.Object,
            Mock.Of<IBridgeDiscoveryService>(), version);

        vm.AppVersion.Should().Be(version);

        vm.LoadSettings();
        vm.LoadSettings();

        vm.AppVersion.Should().Be(version);
    }
}
