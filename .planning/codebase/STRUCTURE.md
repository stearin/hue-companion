# Codebase Structure

**Analysis Date:** 2026-11-07

## Directory Layout

Selected files and directories (generated output omitted):

```text
hue-companion/
|-- src/
|   |-- HueCompanion/                     # WinUI 3 presentation layer
|   |   |-- App.xaml(.cs)                 # Application startup, DI, crash logging
|   |   |-- MainWindow.xaml(.cs)          # Navigation shell, hotkey/tray integration
|   |   |-- Views/                        # Pages and code-behind
|   |   |   `-- Rendering/                # Win2D timeline canvas, renderers, hit testing
|   |   |-- Controls/                     # Cards, color pickers, effect flyouts
|   |   |-- Dialogs/                      # SaveEffectDialog
|   |   |-- Styles/                       # Global XAML theme resources
|   |   |-- Converters/                   # ValueConverters.cs
|   |   |-- Constants/                    # AppConstants.cs
|   |   |-- Helpers/                      # Navigation, command parsing, dispatching
|   |   |-- Services/                     # HotkeyService, SystemTrayService, McpServerManager
|   |   |-- Utilities/                    # Animation, brushes, effects, visual tree helpers
|   |   |-- Properties/                   # Launch settings and publish profiles
|   |   |-- Assets/Scenes/                # Built-in animated scene JSON files
|   |   |-- Package.appxmanifest          # Stable MSIX identity
|   |   `-- Package.Dev.appxmanifest      # Development MSIX identity
|   |-- HueCompanion.Core/                # Platform-independent domain and MVVM logic
|   |   |-- Services/                     # Bridge access, playback, persistence, undo/redo
|   |   |   `-- Interfaces/               # Service and UI abstraction contracts
|   |   |-- ViewModels/                   # Page, dashboard, scene, track/keyframe state
|   |   |-- Models/                       # Domain entities, settings, colors, animations
|   |   `-- Utilities/                    # RoomIconHelper, TaskExtensions
|   |-- HueCompanion.Mcp/                 # .NET MCP server using Core services
|   |   |-- Program.cs                    # Host startup, transports, DI registration
|   |   |-- Tools/                        # BridgeTools, LightTools, SceneTools, AnimationTools
|   |   `-- Services/                     # FileSettingsService, ColorParser, FuzzyMatcher
|   `-- HueCompanion.Tests/               # xUnit v3 tests; references Core and Mcp
|       |-- Models/                       # Color and Result tests
|       |-- Services/                     # Storage, settings, animation, assignment tests
|       |-- ViewModels/                   # SceneBuilderViewModel tests
|       `-- Mcp/                          # ColorParser and FuzzyMatcher tests
|-- website/                              # Separate Next.js website (outside .NET solution)
|   |-- src/app/                          # App Router pages, layout, metadata, global CSS
|   |-- src/components/                   # Website UI components
|   |-- src/lib/                          # Shared helpers and SEO utilities
|   `-- public/                           # Static assets and product screenshots
|-- tools/                                # CLI, launch, screenshots, releases, scene tooling
|   |-- screenshot-mcp/                   # Node screenshot MCP server
|   |-- hue-mcp/                          # MCP setup documentation
|   `-- generated/                        # Generated scene examples
|-- tests/visual-baselines/               # Reference screenshots for visual regression
|-- .github/workflows/                    # Main CI, PR validation, releases
|-- .planning/codebase/                   # Codebase reference documents
|-- docs/                                 # Development, Store setup, implementation plans
|-- assets/                               # Project-level branding assets
|-- CLAUDE.md                             # Developer instructions
|-- README.md                             # Project overview
|-- HueCompanion.sln                       # Four .NET projects
`-- Directory.Build.props                # Shared C# compiler settings
```

## Directory Purposes

**HueCompanion (Presentation Layer):**
- Purpose: WinUI 3 UI components, page navigation, user interaction
- Contains: XAML views, code-behind, converters, styles, UI-specific services
- Key files: `App.xaml.cs` (DI setup), `MainWindow.xaml.cs` (shell), Views/*.xaml

**HueCompanion.Core (Business Logic Layer):**
- Purpose: Domain logic, bridge communication, data models, view models
- Contains: Service implementations, interfaces, models, viewmodels, animation engine
- Key files: `Services/HueBridgeService.cs` (API wrapper), `Services/MultiBridgeService.cs` (aggregation), `Services/AnimationEngine.cs` (approximately 30 FPS keyframe updates)

**HueCompanion.Mcp (MCP Server):**
- Purpose: Expose bridge, light, scene, and animation operations as MCP tools.
- Contains: `Program.cs` for hosting and DI, `Tools/` for tool definitions, `Services/` for configuration loading and parsing helpers.
- Transports: Standard input/output with `--stdio`; otherwise loopback HTTPS by default, or HTTP with `--http`.
- References: `HueCompanion.Core`; the WinUI app starts and stops the server through `Services/McpServerManager.cs`.

**website/ (Website):**
- Purpose: Product information, feature pages, privacy, and MCP setup instructions.
- Contains: Next.js App Router pages in `src/app/`, reusable React components in `src/components/`, helpers in `src/lib/`, and static assets in `public/`.
- Build configuration: `package.json`, `next.config.ts`, `tsconfig.json`, and `eslint.config.mjs`; independent of the .NET solution.

**tools/ and tests/visual-baselines/ (Development Tooling):**
- Purpose: CLI control, launching, screenshots, visual comparisons, scene generation, and Store deployment.
- Key files: `hue-cli.ps1`, `Launch-App.ps1`, `Run-UITests.ps1`, `Compare-Screenshots.ps1`, and `Deploy-ToStore.ps1`.
- The Node server under `tools/screenshot-mcp/` is separate from the .NET server under `src/HueCompanion.Mcp/`.

**HueCompanion.Tests (Testing):**
- Purpose: Automated coverage for core business logic and MCP parsing helpers
- Contains: xUnit v3 test cases under `Services/`, `Models/`, `ViewModels/`, and `Mcp/`
- Key files: `HueCompanion.Tests.csproj`, `Services/AnimationEngineTests.cs`, `ViewModels/SceneBuilderViewModelTests.cs`; see `TESTING.md` for commands and conventions.

**Views/**
- Purpose: Page definitions for navigation destinations
- Contains: XAML markup (layout) + code-behind (event handlers)
- Pattern: Pages use view models from `HueCompanion.Core/ViewModels`; some pages share a view model (for example, `RoomsPage` uses `DashboardViewModel`).
- Files: `DashboardPage.xaml`, `RoomDetailPage.xaml`, `LightDetailPage.xaml`, `ScenesPage.xaml`, `SceneBuilderPage.xaml`, `SettingsPage.xaml`, `SetupPage.xaml`, `RoomsPage.xaml`, `ZonesPage.xaml`, `MyDashboardPage.xaml`, `SceneLibraryPage.xaml`

**Controls/**
- Purpose: Reusable UI components
- Contains: Card controls (RoomCard, LightCard), color pickers, effect flyouts, animated previews
- Files: `RoomCard.xaml` (room/zone card widget), `LightCard.xaml` (light row), `ColorPickerFlyout.xaml`, `GamutColorPicker.xaml`, `NativeEffectFlyout.xaml`

**Dialogs/**
- Purpose: Modal UI dialogs
- Files: `SaveEffectDialog.xaml` and its code-behind; dialogs live separately from reusable controls.

**Styles/**
- Purpose: Global XAML theme resources and control customizations
- Contains: Color brushes, control template overrides, animations
- Files: `AppStyles.xaml` (ToggleSwitchStyle, ToggleSwitch overrides, theme colors)

**Converters/**
- Purpose: XAML value converters for binding transformations
- Contains: Bool-to-opacity, color-to-brush, color-to-gradient converters
- Files: `ValueConverters.cs` contains the converter classes, including `BoolToOpacityConverter`, `HexToGradientBackgroundConverter`, and `HexToGradientBorderConverter`.

**Constants/**
- Purpose: Centralized constant values
- Contains: Animation durations, layout dimensions, icon glyphs, default colors
- Files: `AppConstants.cs` (StandardDurationMs = 300, InactiveIconAlpha = 128, window size)

**Helpers/**
- Purpose: UI infrastructure utilities
- Contains: Navigation service, command-line parsing, dispatcher marshaling
- Files: `NavigationService.cs`, `CommandLineParser.cs`, `DispatcherHelper.cs`, `NavigationTag.cs`

**Services/** (HueCompanion.Core)
- Purpose: Core business logic
- Contains: Bridge API wrapper, animation engine, scene storage, settings persistence
- Interfaces: `Interfaces/I*.cs` (IHueBridgeService, IMultiBridgeService, IAnimationService, ISettingsService, etc.)
- Implementations: `*.cs` files for each interface

**ViewModels/** (HueCompanion.Core)
- Purpose: MVVM state and commands for each page
- Contains: ObservableProperty state, RelayCommand handlers, service orchestration
- Pattern: Inherit from ObservableObject (MVVM Toolkit), one ViewModel per page/major feature
- Files: `DashboardViewModel.cs`, `RoomDetailViewModel.cs`, `LightDetailViewModel.cs`, `SceneBuilderViewModel.cs`

**Models/** (HueCompanion.Core)
- Purpose: Domain entities and value objects
- Contains: Light, Room, Scene definitions; animation keyframes; configuration models
- Files: `RoomModel.cs`, `LightModel.cs`, `SceneModel.cs`, `AnimatedSceneModel.cs`, `AnimationDefinition.cs`, `HueColor.cs`, `Result.cs`, `AppSettings.cs`

## Key File Locations

**Entry Points:**
- `src/HueCompanion/App.xaml.cs`: Application initialization, DI setup, crash logging
- `src/HueCompanion/MainWindow.xaml.cs`: Window shell, navigation, hotkey/tray integration
- `src/HueCompanion.Mcp/Program.cs`: MCP host, transport selection, and service registration
- `website/src/app/layout.tsx` and `website/src/app/page.tsx`: Website root layout and home page
- `HueCompanion.sln`: Solution file referencing the WinUI app, Core, MCP server, and Tests projects

**Configuration:**
- `src/HueCompanion/Constants/AppConstants.cs`: UI animation durations, layout sizes, icon glyphs
- `src/HueCompanion.Core/Models/AppSettings.cs`: Settings object schema
- `src/HueCompanion/Properties/launchSettings.json`: Packaged debugging profile (`commandName: MsixPackage`)
- `src/HueCompanion/HueCompanion.csproj`: Windows target, MSIX packaging, architectures, and development build identity
- `src/HueCompanion/Package.appxmanifest` and `Package.Dev.appxmanifest`: Package identities and capabilities
- `Directory.Build.props`: Shared language version, nullable references, implicit usings, and warning settings

**Core Logic:**
- `src/HueCompanion.Core/Services/HueBridgeService.cs`: Single bridge HueApi wrapper
- `src/HueCompanion.Core/Services/MultiBridgeService.cs`: Multi-bridge orchestration
- `src/HueCompanion.Core/Services/AnimationEngine.cs`: Keyframe/event animation executor; keyframe loop delays 33 ms between updates (approximately 30 FPS)
- `src/HueCompanion.Core/Services/SceneStorageService.cs`: JSON scene persistence
- `src/HueCompanion.Core/Services/SettingsService.cs`: AppSettings JSON storage

**View Layer:**
- `src/HueCompanion/MainWindow.xaml`: Navigation shell XAML
- `src/HueCompanion/Views/DashboardPage.xaml`: Home page with room/zone cards
- `src/HueCompanion/Views/RoomDetailPage.xaml`: Room control + scene playback
- `src/HueCompanion/Views/LightDetailPage.xaml`: Individual light control
- `src/HueCompanion/Views/SceneBuilderPage.xaml`: Timeline editor UI
- `src/HueCompanion/Views/Rendering/`: Win2D timeline rendering, caching, and hit testing
- `src/HueCompanion.Core/ViewModels/SceneBuilderViewModel.cs`: Editor state, synchronous `LoadSceneFromModel`, and scene import/export orchestration
- `src/HueCompanion/Styles/AppStyles.xaml`: Global theme resources

**Testing:**
- `src/HueCompanion.Tests/Services/`: Service unit tests
- `src/HueCompanion.Tests/Models/`: Model unit tests
- `src/HueCompanion.Tests/ViewModels/`: Scene builder view-model tests
- `src/HueCompanion.Tests/Mcp/`: MCP parsing and matching tests
- `tools/Run-UITests.ps1`, `tools/Compare-Screenshots.ps1`, and `tests/visual-baselines/`: UI screenshot checks and baseline images

## Naming Conventions

**Files:**
- Views: `[PageName]Page.xaml` and `[PageName]Page.xaml.cs` (e.g., DashboardPage.xaml)
- ViewModels: `[FeatureName]ViewModel.cs` (e.g., RoomDetailViewModel.cs)
- Models: `[EntityName]Model.cs` (e.g., RoomModel.cs)
- Services: `[ServiceName]Service.cs` (e.g., HueBridgeService.cs)
- Interfaces: `I[ServiceName].cs` (e.g., IHueBridgeService.cs)
- Converters: `[Conversion]Converter` classes consolidated in `Converters/ValueConverters.cs`

**Directories:**
- Feature/domain areas: PascalCase (Views, Controls, Services, ViewModels, Models)
- Cross-cutting concerns: PascalCase (Helpers, Converters, Constants, Styles, Utilities)
- Platform-specific: Grouped by concern, not platform (all Views in single Views/ folder, not separated by platform)

**Namespaces:**
- Namespace prefixes: `HueCompanion`, `HueCompanion.Core`, `HueCompanion.Mcp`, and `HueCompanion.Tests`
- Layers: `HueCompanion.Core.Services.Interfaces`, `HueCompanion.Core.ViewModels`, `HueCompanion.Core.Models`
- Features: `HueCompanion.Helpers`, `HueCompanion.Services`, `HueCompanion.Converters`, `HueCompanion.Constants`

**C# Code:**
- Classes/Methods: PascalCase
- Properties: PascalCase; private backing fields use `_camelCase`
- Parameters: camelCase
- Constants: PascalCase (for example, `StandardDurationMs`)
- Private fields: `_camelCase`
- Task-returning methods: Suffix `Async`; synchronous helpers omit the suffix (for example, `LoadSceneFromModel`).

## Where to Add New Code

**New Page/Feature:**
- XAML View: `src/HueCompanion/Views/[FeatureName]Page.xaml`
- Code-behind: `src/HueCompanion/Views/[FeatureName]Page.xaml.cs`
- ViewModel: `src/HueCompanion.Core/ViewModels/[FeatureName]ViewModel.cs`
- Register ViewModel in DI: `App.xaml.cs` ConfigureServices() with `services.AddTransient<[FeatureName]ViewModel>()`
- Add navigation route: `MainWindow.xaml.cs` NavView_ItemInvoked() switch statement

**New Control:**
- XAML Control: `src/HueCompanion/Controls/[ControlName].xaml`
- Code-behind: `src/HueCompanion/Controls/[ControlName].xaml.cs`
- Keep reusable controls in `Controls/`; put modal dialogs in `Dialogs/` and consume them from pages.

**New Service:**
- Interface: `src/HueCompanion.Core/Services/Interfaces/I[ServiceName].cs`
- Implementation: `src/HueCompanion.Core/Services/[ServiceName].cs`
- Register in DI: `App.xaml.cs` ConfigureServices() with appropriate lifetime (Singleton for stateful, Transient for stateless)
- Document in CLAUDE.md service architecture section

**New MCP Tool:**
- Tool definition: `src/HueCompanion.Mcp/Tools/[FeatureName]Tools.cs`, using MCP tool attributes.
- Shared domain operations: Reuse interfaces and implementations in `HueCompanion.Core/`.
- Registration: Assembly scanning in `src/HueCompanion.Mcp/Program.cs`; add dependencies to that host's DI registration as needed.
- Parsing/helper tests: `src/HueCompanion.Tests/Mcp/[ClassName]Tests.cs`.

**New Website Page:**
- Route: `website/src/app/[route]/page.tsx`.
- Reusable components: `website/src/components/`.
- Shared helpers: `website/src/lib/`; static assets: `website/public/`.

**New Model:**
- Definition: `src/HueCompanion.Core/Models/[ModelName].cs`
- Use: Reference in services and viewmodels
- Persist if needed: Add to SceneStorageService or SettingsService JSON handling

**New Utility/Helper:**
- Presentation helpers: `src/HueCompanion/Helpers/[HelperName].cs`
- UI utilities: `src/HueCompanion/Utilities/[UtilityName].cs`
- Platform-independent utilities: `src/HueCompanion.Core/Utilities/[UtilityName].cs`
- Converters: Add `[Conversion]Converter` to `src/HueCompanion/Converters/ValueConverters.cs`

**Tests:**
- Service tests: `src/HueCompanion.Tests/Services/[ServiceName]Tests.cs`
- Model tests: `src/HueCompanion.Tests/Models/[ModelName]Tests.cs`
- View-model tests: `src/HueCompanion.Tests/ViewModels/[ViewModelName]Tests.cs`
- MCP tests: `src/HueCompanion.Tests/Mcp/[ClassName]Tests.cs`
- Helpers and factories currently live inside their test classes; shared fixture files can be added if needed.

## Special Directories

**Assets/ (src/HueCompanion/Assets/)**
- Purpose: Application resources (icons, images, embedded scene templates)
- Generated: No
- Committed: Yes
- Contents: `app.ico`, `Scenes/` (built-in animated scenes), PNG/image files

**Assets/Scenes/ (src/HueCompanion/Assets/Scenes/)**
- Purpose: Embedded JSON scene templates (built-in scenes)
- Format: AnimatedSceneModel JSON structure
- Committed: Yes, as template seed data
- Usage: `SceneStorageService.LoadBuiltInScenesAsync()` reads these files; `AnimationService` combines built-in and user scenes when listing scenes.

**(HueCompanion.Core) bin/ and obj/**
- Purpose: Build output and intermediate files
- Generated: Yes
- Committed: No (.gitignored)
- Pattern: Organized by configuration (Debug, Release) and platform (x64, ARM64)

**(HueCompanion) bin/ and obj/**
- Purpose: Build output for presentation layer
- Generated: Yes
- Committed: No (.gitignored)
- Pattern: WinUI specific: net8.0-windows10.0.22621.0 directory

**%LOCALAPPDATA%/HueCompanion/ (Runtime)**
- Purpose: Application data directory (settings, scenes, logs)
- Structure:
  - `settings.json`: Serialized AppSettings object (also the MCP settings fallback)
  - `bridges.json`: Optional shared bridge configuration read by the MCP server before falling back to `settings.json`
  - `Scenes/`: User-created AnimatedSceneModel JSON files
  - `crash.log`: Unhandled exception log
- Lifetime: Persistent across application launches
- Creation: Automatic when services first write to directory

---

*Structure analysis: 2026-11-07*
