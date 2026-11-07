# Codebase Concerns

**Source review:** 2026-11-07. These are source-observed limitations and verification gaps, not a report of reproduced production failures.

## Confirmed limitations

| Area | Current evidence | Follow-up |
|------|------------------|-----------|
| Credential storage | `SettingsService` and MCP `FileSettingsService` serialize app keys as plaintext JSON. | Assess protected credential storage if the persistence design changes. |
| MCP routing | `Program.cs` maps the root with `app.MapMcp()` but prints a `/mcp` setup hint. | Align the hint and route; current setup docs use the root URL. |
| Animation layering | `AnimationEngine.Start()` starts tasks concurrently without using `Priority` to arbitrate writes. | Avoid documenting guaranteed overlays; design arbitration before relying on it. |
| Native-effect scene parameters | The engine's native-effect path calls `ApplyEffectAsync` with only the light ID and effect name. | Forwarding saved speed/brightness requires a behavior change and regression coverage. Direct UI application has a separate parameterized path. |
| Scene Builder defaults | `_isSnapEnabled` starts as `true`. | The planned default-off requirement remains unimplemented. |
| Picker integration | `GamutColorPicker` is integrated in a side panel and defaults to Gamut C; the page does not bind a selected bulb's gamut. | Verify bulb-specific limits, color accuracy, sizing, and positioning before closing the color-picker milestone. |
| Settings writes | `SettingsService.SaveAsync()` writes directly to the settings file; loading catches errors and falls back to defaults. | Consider atomic replacement and diagnostics; test interrupted writes and permission errors. |
| Bridge command contracts | Several light-setting methods return `Task`, while queries/CRUD often return `Result`. | Check individual failure paths; exception logging alone does not handle unsuccessful result values. |

## Verification gaps

- The test project includes models, services, Scene Builder state, and MCP parsing helpers. There are no dedicated `HueBridgeService` or `MultiBridgeService` test files in the current suite.
- HueApi request-shape migrations, physical bridge behavior, reconnection stress, and API throttling need targeted regression coverage or real-device verification.
- Editor pause/resume, rapid selection changes, multiple-keyframe operations, picker positioning, and rendering performance need Windows UI verification. A nominal timer interval does not establish measured FPS.
- Main and PR screenshot/comparison steps use `continue-on-error: true`; they do not enforce a blocking visual quality gate.
- There is no current benchmark supporting the old startup-time, memory, or maximum-keyframe claims. Measure representative workloads before publishing numeric limits.
- Full settings/bridge backup remains proposed. Scene Builder already supports scene JSON import/export, and scene cards have `AnimatedPreviewControl`; neither is equivalent to full offline scene authoring or configuration backup.

## Findings superseded by current source

The [February review](../../docs/plans/2026-02-07-code-review.md) remains a historical record. Do not reopen these items solely from its original checklist:

- `EnsureConnectedAsync()` uses a reconnection semaphore.
- `RoomSceneAssignmentService` guards shared state with a semaphore.
- Room/Light cards unsubscribe from previous view models and on unload; several pages also clean up on navigation.
- `ColorConverter.XyToRgb` guards degenerate `y` input.
- Storage/settings tests instantiate production services with temporary-directory overrides. Moq is used by animation and view-model tests.
- Room/zone lookup caching and Win2D timeline renderers already exist. The old proposal to migrate the timeline away from XAML shapes is superseded.
- The old claim that PR validation also triggers on pushes to main is obsolete; it currently triggers on pull requests to main.

Other historical findings require individual inspection before being marked resolved. Package versions belong in [STACK.md](STACK.md); package maintenance and ecosystem claims require fresh upstream evidence.
