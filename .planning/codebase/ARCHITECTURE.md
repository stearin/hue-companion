# Architecture

**Source review:** 2026-11-07. Source and project configuration take precedence over this overview.

## Projects and boundaries

| Project | Responsibility |
|---------|----------------|
| `src/HueCompanion/` | WinUI 3 views, controls, navigation, Windows hotkeys/tray, and MCP process management |
| `src/HueCompanion.Core/` | Platform-independent models, MVVM state, service interfaces, bridge access, storage, and animation |
| `src/HueCompanion.Mcp/` | ASP.NET Core MCP host and bridge-control tools using Core interfaces |
| `src/HueCompanion.Tests/` | xUnit v3 tests referencing Core and Mcp |
| `website/` | Independent Next.js product website with its own npm workflow |
| `tools/screenshot-mcp/` | Independent Node development tool for screenshots |

The desktop app uses CommunityToolkit.Mvvm observable properties and relay commands. `App.Services` exposes its DI container; constructors receive service dependencies. The MCP host registers its own Core service instances in `Program.cs`.

## Commands and state updates

1. A XAML binding or page handler invokes a view-model command.
2. The view model resolves the relevant bridge through `IMultiBridgeService`.
3. `HueBridgeService` uses HueApi or selected raw CLIP v2 HTTP requests.
4. Bridge SSE events raise `LightStateChanged`; consumers marshal UI updates through the dispatcher abstractions where needed.

`MultiBridgeService` manages bridge instances and aggregates rooms/zones. `HueBridgeService.EnsureConnectedAsync()` serializes reconnection with a semaphore. Individual room/zone lookups use five-second caches; relevant mutations invalidate those caches.

Bridge discovery attempts HTTP discovery and then mDNS, merging results by bridge ID. Registration obtains an app key through the bridge link-button flow.

## Persistence

- `%LOCALAPPDATA%/HueCompanion/settings.json`: `AppSettings`, including configured bridges, preferences, pinned items, and scene assignments.
- `%LOCALAPPDATA%/HueCompanion/Scenes/`: User scenes serialized as `AnimatedSceneModel`; filenames use scene IDs.
- `src/HueCompanion/Assets/Scenes/`: Built-in scene templates.
- `%LOCALAPPDATA%/HueCompanion/crash.log`: App-level exception reports.

Settings and bridge credentials are plain JSON; the serializers do not encrypt app keys. Services and transient UI state are recreated on launch, while saved settings and scenes persist.

The MCP `FileSettingsService` first accepts a nonempty optional `bridges.json` list, otherwise reads `settings.json`. A successful bridge-list load returns before other saved settings are read. No current WinUI writer for `bridges.json` was found.

## Scene Builder and playback

`SceneBuilderViewModel` owns tracks, keyframes, event tracks, selection, clipboard operations, and `TimelineCommandHistory` for undo/redo. Scene loading through `LoadSceneFromModel` is synchronous; `ImportFromJsonAsync` retains a task-based result API.

`SceneBuilderPage.xaml.cs` handles input, selection, scrubbing, playback preview, and invalidation. Win2D renderers under `Views/Rendering/` draw the timeline, gradients, markers, events, ruler, and playhead. `RenderCache` supports rendering reuse. The page integrates `GamutColorPicker` in a side panel and supports marquee selection and multiple-keyframe deletion.

The editor's playback timer targets about 60 display updates per second and rate-limits its light preview updates separately. Library playback uses `AnimationService` and `AnimationEngine`; the engine delays 33 ms between keyframe iterations (about 30 iterations per second before API latency). These are separate playback paths.

Animation definitions include keyframe, event, and native-effect types. The engine starts animation tasks concurrently; it does not arbitrate overlapping writes by `Priority`. Its native-effect path passes the effect name to the bridge without forwarding `EffectSpeed` or `EffectBrightness`.

## MCP hosting

- `--stdio`: The client launches the host and exchanges protocol messages over standard input/output.
- Default: Loopback HTTPS on port 5680; Kestrel certificate configuration is required.
- `--http`: Loopback HTTP; `--port` overrides the port.
- `app.MapMcp()` maps the HTTP endpoint at the root, e.g. `https://localhost:5680`. The startup hint's `/mcp` suffix does not match the route.
- Startup checks `McpEnabled` and connects saved bridges. The WinUI app can manage the host as a child process; the host can also run independently.

See [INTEGRATIONS.md](INTEGRATIONS.md) for configuration and lifecycle details.

## Navigation and dependency injection

`MainWindow` hosts the navigation frame. `Helpers/NavigationService.cs` implements Core's `INavigationService`. Rooms and zones share `RoomDetailPage`; navigation tags carry identifiers and types.

`App.ConfigureServices()` registers settings, discovery, multi-bridge access, navigation, storage, animation, pins, scene assignments, hotkeys, tray, and MCP management as singletons. Page view models are transient. MVVM messaging uses `IMessenger` with `WeakReferenceMessenger.Default`.

Command-line navigation is parsed by `Helpers/CommandLineParser.cs`:

```powershell
HueCompanion.exe --page room --name "Living Room"
HueCompanion.exe --page dashboard --screenshot --delay 8000
```

Valid page names are `dashboard`, `mydashboard`, `rooms`, `zones`, `room`, `zone`, `light`, `settings`, and `setup`. Room, zone, and light pages require `--name` or `--id`. `hue.cmd` manages builds and processes; use `tools/Launch-App.ps1` for its documented navigation options.

## Errors, logging, and validation

Many queries and CRUD operations return `Result<T>` or `Result`. Several light-setting operations return `Task`; inspect each interface's contract rather than assuming all failures are returned as results. Debug logging and app-level crash logs coexist with MCP console logging. In stdio mode, MCP logs go to standard error to preserve protocol output.

Storage/import paths perform structural validation, but unit tests do not establish physical-bridge correctness, performance, or complete numeric validation. See [TESTING.md](TESTING.md) for test conventions and [CONCERNS.md](CONCERNS.md) for remaining limitations.
