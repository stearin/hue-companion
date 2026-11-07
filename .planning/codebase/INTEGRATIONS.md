# External Integrations

**Analysis Date:** 2026-11-07

## APIs & External Services

**Philips Hue Bridge (CLIP v2 API):**
- Service: Hue Bridge on local network (LAN)
- What it's used for: Light control, scene management, real-time state updates
  - Query rooms, zones, lights, scenes
  - Toggle lights on/off, set brightness, color, color temperature
  - Execute scenes (built-in Hue scenes)
  - Real-time event streaming for state synchronization
- SDK/Client: HueApi 3.4.0 (NuGet package); `HueBridgeService` also uses a raw `HttpClient` for selected CLIP v2 updates.
- Authentication: Bridge app key obtained after a link button press; raw requests send the `hue-application-key` header.
- Protocol: HTTPS to the local bridge. The raw `HttpClient` explicitly accepts any server certificate through `ServerCertificateCustomValidationCallback`. SDK calls use `LocalHueApi`.
- Event streaming: Server-Sent Events (SSE) via `StartEventStream()` in `HueBridgeService`

**Bridge Discovery Services:**
- HTTP-based discovery: `discovery.meethue.com`, attempted first.
- mDNS discovery: Local multicast discovery, attempted after HTTP discovery even when HTTP succeeds; results are deduplicated by bridge ID.
- Both via `BridgeDiscoveryService` which uses HueApi's `HttpBridgeLocator` and `MdnsBridgeLocator`

**Bridge-Control MCP Server (`src/HueCompanion.Mcp/`):**
- Purpose: Expose bridge queries, light control, scenes, and animation editing to MCP clients through `Tools/BridgeTools.cs`, `LightTools.cs`, `SceneTools.cs`, and `AnimationTools.cs`.
- SDKs: `ModelContextProtocol` and `ModelContextProtocol.AspNetCore` 2.3.1.
- Host: ASP.NET Core/Kestrel, configured in `Program.cs`; reuses Core services and connects configured bridges on startup.
- Transports: `--stdio` selects standard input/output. Otherwise the server listens on loopback at `https://localhost:5680`; `--http` selects HTTP and `--port` overrides 5680.
- Routing: `app.MapMcp()` uses an empty prefix with the configured SDK, so connect to the root URL. The startup hint that appends `/mcp` is inconsistent with the route. The configured [SDK v2.3.1 route implementation](https://github.com/modelcontextprotocol/csharp-sdk/blob/v2.3.1/src/ModelContextProtocol.AspNetCore/McpEndpointRouteBuilderExtensions.cs) defaults the prefix to an empty string.
- HTTPS uses Kestrel HTTPS configuration. No MCP authentication middleware or OAuth provider is configured in `Program.cs`; the HTTP listener is restricted to `IPAddress.Loopback`.
- Lifecycle: The WinUI app uses `Services/McpServerManager.cs` to launch a hidden child process when enabled and stop it when requested. It locates the executable next to the app or in the MCP project's Debug output.
- Settings: `Services/FileSettingsService.cs` reads an optional nonempty `bridges.json` list first, otherwise falls back to `settings.json`. A successful `bridges.json` load returns without reading other settings from `settings.json`.
- Enablement: The host checks `Settings.McpEnabled` before serving tools; the app exposes the setting in its Settings page.

**Screenshot MCP Server (`tools/screenshot-mcp/`):**
- Separate Node.js development tool using `@modelcontextprotocol/sdk` ^1.0.0 and standard input/output transport.
- Tools: `list_screenshots`, `get_screenshot`, `get_latest_screenshot`, and `capture_screenshot`.
- Reads images under the repository's `screenshots/` directory; capture invokes `tools/Capture-AppScreenshot.ps1` through Windows PowerShell and returns image data to the MCP client.

**Windows Integration:**
- `Services/SystemTrayService.cs`: Win32 `Shell_NotifyIcon` tray integration.
- `Services/HotkeyService.cs`: Win32 global hotkey registration.
- Dashboard/room/zone pages use `Windows.System.Launcher` to open the Hue app, with a Microsoft Store URI fallback.

**Website (`website/`):**
- Next.js product website with GitHub, Microsoft Store, and MCP setup links; canonical URL is configured in `website/src/lib/seo.ts`.
- `website/src/app/layout.tsx` uses `next/font/google` for Inter and Space Grotesk, adding a font retrieval dependency during website builds.
- `next.config.ts` selects standalone output; no website deployment workflow or hosting provider configuration is established by the inspected .NET workflows.

## Data Storage

**Local File Storage:**
- Settings: `%LOCALAPPDATA%\HueCompanion\settings.json` (AppSettings model as JSON)
  - Contains: Bridge credentials, user preferences, pinned items, scene assignments
  - Service: `SettingsService` (read/write via System.Text.Json); MCP uses its own `FileSettingsService` to load shared configuration.
- Optional MCP bridge export: `%LOCALAPPDATA%\HueCompanion\bridges.json` (`List<BridgeModel>`). The MCP reader supports this file; no writer was found in the current WinUI source.

**Scene Storage:**
- Built-in scenes: Bundled in application package at `Assets\Scenes\scene_*.json`
- User-created scenes: `%LOCALAPPDATA%\HueCompanion\Scenes\*.json`; saving uses the scene ID as the filename, so the `scene_` prefix is not required.
- Format: AnimatedSceneModel (JSON with camelCase naming policy)
- Service: `SceneStorageService` with separate paths for built-in vs user scenes

**Crash Logs:**
- Location: `%LOCALAPPDATA%\HueCompanion\crash.log`
- Format: Plain text with timestamp, source, exception details
- Usage: Unhandled exception logging (set in App.xaml.cs exception handlers)

**Databases:**
- None - application uses local file storage only (JSON-based)

**Caching:**
- In-memory collections in ViewModels (no persistent caching layer).
- `HueBridgeService` caches room and zone lists for five seconds for individual lookup methods; relevant mutations invalidate those caches.
- Scene library cached in `SceneLibraryViewModel`
- Light/room state cached in `RoomDetailViewModel`

## Authentication & Identity

**Auth Provider:**
- No application account/sign-in provider; bridge registration and app-key authentication are local to the bridge. HTTP discovery separately contacts Philips discovery infrastructure.
- Hue Bridge registration process:
  - User must press physical link button on bridge
  - App calls `HueApi.LocalHueApi.RegisterAsync()` to request app key
  - App key stored in settings for future connections
  - Implementation: `BridgeDiscoveryService.RegisterAsync()`

**Credential Storage:**
- Bridge IP address + app key stored in `settings.json` (plain text in LocalAppData)
- Multi-bridge support: List of `BridgeModel` objects in `AppSettings.ConfiguredBridges`
- No encryption is applied by the settings serializers; credentials are local JSON data, including the optional MCP bridge export.

## Monitoring & Observability

**Error Tracking:**
- None (no remote error tracking)
- Local crash logging: See "Crash Logs" under Data Storage

**Logs:**
- Crash logs written to `%LOCALAPPDATA%\HueCompanion\crash.log`
- Debug output via `System.Diagnostics.Debug.WriteLine()` in Visual Studio debugger.
- MCP host uses console logging and writes startup/status messages to standard error; standard input/output mode directs console logs to standard error to preserve protocol output.
- Three-layer exception handling in App.xaml.cs:
  - `UnhandledException` (XAML layer)
  - `AppDomain.CurrentDomain.UnhandledException`
  - `UnobservedTaskException` (async/await)

## CI/CD & Deployment

**Hosting:**
- Desktop distribution: MSIX via Microsoft Store or sideloading; release workflow also prepares portable ZIP artifacts for x64 and ARM64.
- MCP bridge-control hosting: Local child process or separately started .NET host, with loopback HTTP(S) or standard input/output transport.
- Website: Separate Next.js app configured for standalone output; its hosting provider is not specified in the inspected repository configuration.
- Store integration is configured in `release.yml`; this documents repository configuration, not a verified live submission or deployment.

**CI Pipeline:**
- GitHub Actions via `.github\workflows\`:
  - `main-ci.yml` - Main branch: build (x64/ARM64), unit tests, UI tests, code coverage, visual regression
  - `pr-validation.yml` - Pull requests: build, tests with coverage, screenshot checks, and visual comparison
  - `release.yml` - Tag-triggered (`v*.*.*`): build, test, publish x64/ARM64 ZIP and MSIX artifacts to GitHub releases; optional Store submission
- Build platforms: Both x64 and ARM64 via `<Platforms>x64;ARM64</Platforms>`
- Test framework: xUnit v3; `coverlet.collector` produces Cobertura coverage uploaded through `codecov/codecov-action@v4`.
- UI screenshot and visual-comparison steps use `continue-on-error: true` in main and PR workflows.
- Artifacts: ZIP and MSIX release downloads, plus CI test results, coverage, screenshots, and visual diffs.
- Store submission: `microsoft/store-submission@v1`, gated by `vars.STORE_ENABLED == 'true'`, uses Store product and tenant/client credentials.
- `tools/Deploy-ToStore.ps1` is a setup/reference script; its example token/submission API calls are commented out.

## Environment Configuration

**Application Runtime:**
- Bridge credentials, preferences, and MCP enablement are file-based in LocalAppData, not supplied through required environment variables.
- MCP transport and port are command-line options in `HueCompanion.Mcp/Program.cs`; HTTPS additionally depends on Kestrel certificate configuration.

**CI Secrets and Variables:**
- `CODECOV_TOKEN`: Main and PR workflow coverage uploads.
- `GITHUB_TOKEN`: GitHub release creation (workflow-provided token).
- `STORE_ENABLED`: Repository variable; enables the optional Store publishing job when set to `true`.
- `STORE_PRODUCT_ID`, `STORE_TENANT_ID`, `STORE_CLIENT_ID`, `STORE_CLIENT_SECRET`: Secrets passed to the Store submission action.
- Definitions are referenced by `.github/workflows/*.yml`; these are CI/deployment settings, not desktop runtime requirements.

## Webhooks & Callbacks

**Incoming:**
- No application webhook receiver is implemented.
- The optional MCP host accepts local MCP requests at `/` or via standard input/output.

**Bridge Events:**
- The application opens an SSE connection to the bridge; updates flow from the bridge into `HueBridgeService.OnEventStreamMessage()`.
- Parsed changes are raised through `LightStateChanged` for consumers to update state.
- State includes on/off, normalized brightness, XY color, and color temperature converted to a `HueColor` when XY color is absent.

## Event Streaming

**Server-Sent Events (SSE):**
- Bridge maintains open SSE connection for real-time updates
- Started via `HueBridgeService.StartEventStreamAsync()` after connection
- Received events deserialized as `EventStreamResponse` list
- Each callback iterates the received event batch and light resources, extracts state from extension data, and raises `LightStateChanged` when relevant values are present.
- Shutdown unsubscribes the SDK callback and calls `_hueApi.StopEventStream()`. The local `_eventStreamCts` is cancelled/disposed but its token is not passed to the SDK stream.

## Bridge Discovery Methods

**HTTP-based Discovery:**
- Endpoint: `discovery.meethue.com` (Philips cloud discovery)
- Method: `HttpBridgeLocator.LocateBridgesAsync()`
- Returns: BridgeId and IP address for bridges on network
- Timeout: Supplied by the caller to `DiscoverBridgesAsync` and passed to each locator.

**mDNS Discovery:**
- Protocol: Multicast DNS (local network only)
- Method: `MdnsBridgeLocator.LocateBridgesAsync()`
- Attempted after the HTTP locator regardless of success; results from both methods are combined and deduplicated.
- No external service dependency

## Multi-Bridge Architecture

**Service:**
- `MultiBridgeService` - Manages multiple bridge connections simultaneously
- `GetDefaultBridgeService()` - Returns primary bridge service
- `GetBridgeService(bridgeId)` - Returns specific bridge service by ID
- Bridge credentials stored in `AppSettings.ConfiguredBridges` list

---

*Integration audit: 2026-11-07*
