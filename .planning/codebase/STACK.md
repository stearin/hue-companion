# Technology Stack

**Analysis Date:** 2026-11-07

Versions below come from the checked-in project files and package manifests. npm ranges are shown as declared, rather than as exact installed versions.

## Languages

**Primary:**
- C# on .NET 8 - WinUI application, core services, MCP server, and tests.
- Shared C# configuration uses `LangVersion=latest`, nullable reference types, and implicit usings in `Directory.Build.props`; the language version is not pinned to C# 12.

**Supporting:**
- TypeScript/TSX and CSS - Next.js website under `website/`.
- JavaScript ES modules - Screenshot MCP server under `tools/screenshot-mcp/`.
- PowerShell and Windows batch - Build, launch, screenshot, and deployment scripts; `hue.cmd` invokes Windows PowerShell.

## Runtime

**Environment:**
- WinUI app target: `net8.0-windows10.0.22621.0`.
- Core, MCP server, and Tests targets: `net8.0`.
- Windows target SDK: 10.0.22621.0; this target is not a record of tested OS versions.
- Declared minimum Windows version: 10.0.17763.0 (Windows 10 1809).
- App architectures: x64 and ARM64, with runtime identifiers `win-x64` and `win-arm64`.
- MCP HTTP hosting: ASP.NET Core/Kestrel; standard input/output transport is also supported.
- Node.js: Required for website development and the separate screenshot MCP server. The screenshot server declares Node >=18.0.0; the website has no Node version pinned in its own `package.json`.

**Package Managers:**
- NuGet for .NET dependencies; versions are declared directly in each `.csproj`. No `packages.lock.json` is present in the repository.
- npm for `website/` and `tools/screenshot-mcp/`; each has a `package.json` and `package-lock.json`.

## Frameworks

**Core UI:**
- `Microsoft.WindowsAppSDK` 2.6.5 - WinUI 3 desktop framework.
- `Microsoft.Graphics.Win2D` 1.4.0 - Timeline graphics rendering.

**MVVM & DI:**
- `CommunityToolkit.Mvvm` 8.4.2 - ObservableObject, generated observable properties, RelayCommand, and messaging.
- `Microsoft.Extensions.DependencyInjection` 9.0.22 - WinUI service registration and resolution.
- `Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.22 - Core service abstractions.
- `Microsoft.Extensions.Hosting` 9.0.22 - Application hosting infrastructure.

**MCP Server:**
- `ModelContextProtocol` 2.3.1 - MCP server and tool definitions.
- `ModelContextProtocol.AspNetCore` 2.3.1 - HTTP transport integration.
- `HueCompanion.Mcp` uses `Microsoft.NET.Sdk.Web` and references `HueCompanion.Core`.
- `Program.cs` selects standard input/output with `--stdio`; otherwise it uses loopback HTTPS on port 5680 by default. `--http` selects HTTP and `--port` overrides the port. HTTP tools are mapped at the server root (`/`) by the parameterless `app.MapMcp()` call. The startup hint’s `/mcp` suffix does not match this mapping.

**Website (`website/package.json`):**
- Next.js 16.1.3 - App Router pages, metadata, and standalone server output (`next.config.ts`).
- React and React DOM 19.2.3 - Component rendering.
- TypeScript ^5 - Type checking.
- Tailwind CSS and `@tailwindcss/postcss` ^4 - Styling.
- `framer-motion` ^12.27.1 - Animation.
- `lucide-react` ^0.562.0 - Icons.
- `clsx` ^2.1.1 and `tailwind-merge` ^3.4.0 - Class name composition.
- ESLint ^9 and `eslint-config-next` 16.1.3 - Linting.
- npm scripts: `dev`, `build`, `start`, and `lint`.

**Testing:**
- `xunit.v3` 4.0.2 - Unit test framework; migrated from xUnit v2.
- `xunit.runner.visualstudio` 4.0.1 - Visual Studio/VSTest integration.
- `Moq` 4.21.0 - Service mocks and call verification.
- `FluentAssertions` 8.11.0 - Assertions.
- `Microsoft.NET.Test.Sdk` 18.11.0 - Test SDK infrastructure.
- `coverlet.collector` 10.1.0 - Coverage through `--collect:"XPlat Code Coverage"`.
- `HueCompanion.Tests` references both Core and Mcp; tests cover models, services, view models, and MCP parsing helpers. See `TESTING.md` for commands and xUnit v3 migration patterns.
- PowerShell screenshot capture and comparison scripts provide UI checks with baselines under `tests/visual-baselines/`.

## Key Dependencies

**Bridge Communication:**
- `HueApi` 3.4.0 - Philips Hue CLIP v2 client, referenced by the app and Core.
- Color conversion is implemented in `HueCompanion.Core/Models/ColorConverter.cs` and `HueColor.cs`; `HueApi.ColorConverters` is not referenced by the projects.
- `HueBridgeService` uses the updated HueApi models for nested scene palettes, power-on settings, `EffectsV2.Action`, and `CreateUpdateRoom` requests.

**Build Infrastructure:**
- `Microsoft.Windows.SDK.BuildTools` 10.0.28000.2957 - Windows SDK build and packaging tools.
- MSBuild and NuGet restore - Four projects in `HueCompanion.sln`.
- GitHub Actions - Main CI, PR validation, and releases; .NET workflows currently request SDK `8.0.x`.

**Screenshot Tooling:**
- `@modelcontextprotocol/sdk` ^1.0.0 - Node screenshot MCP server in `tools/screenshot-mcp/`.
- This is a separate tool from the .NET bridge-control MCP server in `src/HueCompanion.Mcp/`.

## Configuration

**Environment and Settings:**
- Application configuration is stored in JSON under `%LOCALAPPDATA%/HueCompanion/`; no required environment variables are declared for normal app configuration.
- `settings.json`: AppSettings, including bridge configuration and MCP settings.
- `Scenes/`: User-created scene JSON files; built-in templates are under `src/HueCompanion/Assets/Scenes/`.
- MCP `FileSettingsService` reads an optional `bridges.json` export first and falls back to `settings.json`.
- The MCP host checks `McpEnabled` before running. Its default HTTPS mode uses Kestrel HTTPS configuration; HTTP and standard input/output are alternative transports.

**Build Identity and Version:**
- `DevBuild=true`: Uses `Package.Dev.appxmanifest`, sets `PackageId=HueCompanion.Dev`, and sets the application title to `Hue Companion (Dev)`.
- The default build uses the stable package manifest and `Hue Companion` title.
- `HueCompanion.csproj` declares `Version=1.0.3` and `BaseVersion=0.1.0`. `InformationalVersion` uses BaseVersion plus an optional VersionSuffix.
- `tools/hue-cli.ps1` supplies `VersionSuffix=dev+<git hash>` for development builds; the hash is version metadata, not part of the configured application title.

**Build and Packaging:**
- Project files: `src/HueCompanion/HueCompanion.csproj`, `src/HueCompanion.Core/HueCompanion.Core.csproj`, `src/HueCompanion.Mcp/HueCompanion.Mcp.csproj`, and `src/HueCompanion.Tests/HueCompanion.Tests.csproj`.
- `Directory.Build.props`: Shared compiler settings; warnings are not treated as errors.
- WinUI settings: `UseWinUI=true`, `EnableMsixTooling=true`, `WindowsPackageType=MSIX`, and `WindowsAppSDKSelfContained=true`.
- `WindowsAppSDKSelfContained` bundles the Windows App SDK; it does not itself establish that the .NET runtime is bundled. The CLI's stable publish command separately passes `--self-contained true`.
- Packaged debugging requires `commandName: MsixPackage` in `src/HueCompanion/Properties/launchSettings.json` (the file is ignored by Git).
- The existing publish profile is `Properties/PublishProfiles/FolderProfile.pubxml` (Release, ARM64, filesystem output). The project also selects `win-$(Platform).pubxml`, but corresponding `win-x64.pubxml` / `win-ARM64.pubxml` files are absent; builds can report a missing-profile warning.
- Website and Node screenshot tooling use their own npm configuration outside the .NET solution.

## Platform Requirements

**Development:**
- .NET SDK capable of building the .NET 8 targets; no repository `global.json` pins an exact SDK.
- Windows with WinUI/MSIX build tooling for the desktop app; Visual Studio 2022 or a compatible toolchain.
- PowerShell for CLI and UI screenshot scripts. `hue.cmd` and CI screenshot steps use `powershell`; release steps use `pwsh`.
- Node.js and npm for website or screenshot-server work; choose a Node runtime compatible with the relevant package dependencies.

**Production:**
- Desktop app declares Windows 10 1809+ as its minimum and supports x64/ARM64.
- MSIX distribution via Microsoft Store or sideloading.
- Runtime prerequisites depend on the publish options; Windows App SDK self-containment and .NET self-containment are configured separately.
- Hue Bridge accessible over the local network for lighting operations.
- The standalone .NET MCP host and Node website have separate hosting/runtime requirements from the desktop MSIX.

## Architecture Notes

**Application Structure:**
- Four .NET projects: `HueCompanion` (WinUI UI), `HueCompanion.Core` (domain services and MVVM state), `HueCompanion.Mcp` (MCP host), and `HueCompanion.Tests` (automated tests).
- WinUI DI is configured in `App.xaml.cs`; MCP DI is configured separately in `HueCompanion.Mcp/Program.cs`.
- The website and Node screenshot server are independent npm projects.

**Core Services (`HueCompanion.Core.Services`):**
- `HueBridgeService`: Single-bridge API communication and event streaming.
- `BridgeDiscoveryService`: HTTP and mDNS discovery.
- `MultiBridgeService`: Multi-bridge aggregation.
- `SettingsService` and `SceneStorageService`: JSON configuration and scene persistence.
- `AnimationService` and `AnimationEngine`: Scene playback and keyframe/event execution; the keyframe loop delays 33 ms between updates (approximately 30 FPS).
- `RoomSceneAssignmentService`, `PinnedItemsService`, and timeline command/history classes: Scene assignments, dashboard pins, and editor undo/redo.

**Presentation Services and Views (`HueCompanion`):**
- `Services/SystemTrayService.cs` and `HotkeyService.cs`: Windows tray and global hotkey integration; their interfaces live in Core.
- `Services/McpServerManager.cs`: MCP server process lifecycle.
- Pages include dashboards, rooms/zones, light detail, scene library/builder, settings, and setup.
- `Views/Rendering/`: Win2D timeline canvas and renderers.
- `Controls/` and `Dialogs/`: Reusable WinUI controls and modal dialogs.
- `Styles/AppStyles.xaml` and `Converters/ValueConverters.cs`: Shared XAML resources and value converters.

---

*Stack analysis: 2026-11-07*
