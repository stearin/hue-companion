# Coding Conventions

**Source review:** 2026-11-07. Follow nearby source and [CLAUDE.md](../../CLAUDE.md) for feature-specific patterns.

## Naming and formatting

- Use PascalCase for C# types, methods, properties, enum members, and constants (`SettingsFileName`, `StandardDurationMs`).
- Prefix interfaces with `I` and private fields with `_`; use camelCase for locals and parameters.
- Use the `Async` suffix for asynchronous methods. Synchronous helpers such as `LoadSceneFromModel` omit it.
- Prefer file-scoped namespaces and four-space indentation, following nearby code.
- Prefer one principal type per file. Existing related types sometimes share a file, including track/keyframe view models in `SceneBuilderViewModel.cs` and converters in `ValueConverters.cs`.
- Name files for their principal type or feature; there is no rule requiring plural enum filenames.
- Keep import ordering consistent with the surrounding file. Current source does not enforce a single grouping order.

`Directory.Build.props` enables nullable references and implicit usings, selects the latest language version, and does not treat all warnings as errors. Package-provided analyzers and generators include MVVM Toolkit and xUnit v3; address their diagnostics when changing relevant code. No root `.editorconfig` is present.

## Project boundaries

- Put platform-independent models, services, and view models in `src/HueCompanion.Core/`.
- Put Windows UI and OS integration in `src/HueCompanion/`; modal dialogs belong in `Dialogs/`.
- Implement bridge-control MCP tools in `src/HueCompanion.Mcp/`, reusing Core interfaces.
- Keep the website and Node screenshot server in their separate npm projects.

## MVVM and services

- Observable view models generally derive from `ObservableObject` and use `[ObservableProperty]` fields with `[RelayCommand]` methods.
- Inject dependencies through constructors. Register desktop dependencies in `App.ConfigureServices()` and MCP dependencies in its `Program.cs`.
- Resolve bridge services through `IMultiBridgeService`; the desktop DI container does not register a single global `IHueBridgeService`.
- Match event subscription lifetimes with cleanup on navigation, unloading, or disposal. Follow the actual lifecycle of the page/control being changed.
- Use UI dispatcher abstractions when service events update bound state from background threads.
- Keep WinUI lightweight theme styling and navigation conventions described in `CLAUDE.md`.

## Errors and persistence

- Preserve existing `Result<T>` / `Result` contracts for fallible operations. Some bridge setters return `Task`, so inspect the specific contract.
- Prefer awaited operations. Where an event requires fire-and-forget work, use the existing `TaskExtensions` helpers where appropriate and consider both thrown exceptions and returned failures.
- Desktop services use debug output and app-level crash logs; the MCP host uses console logging directed to standard error in stdio mode.
- Settings and scenes use `System.Text.Json`. Settings retain their existing property naming; scene serializers use camelCase properties and string enums.
- App keys are persisted as plaintext in local settings. Do not describe the serializer as encrypted or secure credential storage.
- Add XML summaries to public APIs and comments explaining non-obvious domain logic. Avoid duplicating implementation details in comments or embedding unstable source line numbers in examples.

## Testing and validation

- Use xUnit v3 with FluentAssertions and Moq; pass `TestContext.Current.CancellationToken` to cancellable test operations.
- Supply every argument, including optional parameters, inside Moq expression trees.
- Test production storage/settings services directly with unique temporary directories through their constructor overrides.
- Run `dotnet test src/HueCompanion.Tests/HueCompanion.Tests.csproj` for .NET behavior changes.
- Also build `src/HueCompanion/HueCompanion.csproj -p:Platform=x64` on Windows for desktop changes.
- Run `npm run lint` and `npm run build` in `website/` for website code changes.
- For documentation-only changes, verify paths/examples and run `git diff --check`.

See [TESTING.md](TESTING.md) for examples and the limits of current automated coverage.
