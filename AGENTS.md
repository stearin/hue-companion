# Repository Instructions

Read [CLAUDE.md](CLAUDE.md) for project-specific build workflows, WinUI styling, navigation, scene-builder, and MCP conventions. Its guidance applies to Codex as well as Claude. Prefer current source and project configuration when descriptive documentation is stale.

## Working in This Repository

- Keep platform-independent models, services, and view models in `src/HueCompanion.Core/`; Windows UI and OS integration belong in `src/HueCompanion/`.
- Implement bridge-control MCP tools in `src/HueCompanion.Mcp/`, reusing Core service interfaces. The Node screenshot server in `tools/screenshot-mcp/` is separate development tooling.
- Keep the Next.js website under `website/`; it has its own npm workflow outside the .NET solution.
- Preserve existing MVVM, dependency-injection, and WinUI styling conventions described in `CLAUDE.md`.

## Validation

- For .NET behavior changes, run `dotnet test src/HueCompanion.Tests/HueCompanion.Tests.csproj`.
- For desktop UI/build changes, also run `dotnet build src/HueCompanion/HueCompanion.csproj -p:Platform=x64` on Windows.
- For website changes, use the `lint` and `build` npm scripts from `website/`.
- For documentation-only changes, verify referenced paths/examples and run `git diff --check`; a full test run is unnecessary.
- Use xUnit v3 test cancellation tokens in cancellable test operations. Supply all arguments, including optional arguments, in Moq expression trees.

## Reference Guides

Read the guides relevant to the task:

- [Structure](.planning/codebase/STRUCTURE.md)
- [Stack](.planning/codebase/STACK.md)
- [Testing](.planning/codebase/TESTING.md)
- [Integrations](.planning/codebase/INTEGRATIONS.md)
- [Architecture](.planning/codebase/ARCHITECTURE.md)
