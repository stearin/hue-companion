using System.Text.Json;
using HueCompanion.Core.Models;
using HueCompanion.Core.Services.Interfaces;

namespace HueCompanion.Core.Services;

/// <summary>
/// Service for persisting application settings to local storage.
/// </summary>
public class SettingsService : ISettingsService
{
    private const string SettingsFileName = "settings.json";
    private readonly string _settingsPath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <inheritdoc/>
    public AppSettings Settings { get; set; } = new();

    public SettingsService(string? basePath = null)
    {
        string appFolder;
        if (basePath != null)
        {
            appFolder = basePath;
        }
        else
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            appFolder = Path.Combine(localAppData, "HueCompanion");
        }
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, SettingsFileName);
    }

    /// <inheritdoc/>
    public async Task LoadAsync()
    {
        BridgeModel? legacyBridge = null;

        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath);
                Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                legacyBridge = JsonSerializer.Deserialize<LegacyBridgeSettings>(json)?.ConfiguredBridge;
            }
            else
            {
                Settings = new AppSettings();
            }
        }
        catch
        {
            // If settings are corrupted, start fresh
            Settings = new AppSettings();
        }

        // Migrate legacy single bridge to multi-bridge list (runs once at startup)
        await MigrateLegacyBridgeAsync(legacyBridge);
    }

    /// <summary>
    /// Migrates legacy single bridge configuration to the multi-bridge list.
    /// </summary>
    private async Task MigrateLegacyBridgeAsync(BridgeModel? legacyBridge)
    {
        if (legacyBridge != null && Settings.ConfiguredBridges.Count == 0)
        {
            Settings.ConfiguredBridges.Add(legacyBridge);
            await SaveAsync();
        }
    }

    /// <inheritdoc/>
    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(Settings, JsonOptions);
        await File.WriteAllTextAsync(_settingsPath, json);
    }

    /// <inheritdoc/>
    public Task<bool> HasConfiguredBridgeAsync()
    {
        // Check new multi-bridge list first
        var hasMultiBridge = Settings.ConfiguredBridges.Any(b =>
            !string.IsNullOrEmpty(b.AppKey) && !string.IsNullOrEmpty(b.IpAddress));

        if (hasMultiBridge)
            return Task.FromResult(true);

        return Task.FromResult(false);
    }

    private sealed class LegacyBridgeSettings
    {
        public BridgeModel? ConfiguredBridge { get; init; }
    }
}
