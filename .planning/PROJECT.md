# Hue Companion

## Open Planning Milestone: v0.3 Scene Builder Bug Bash

**Source reconciliation:** 2026-11-07. These milestone labels describe the Scene Builder workstream, not the application package version. See [STATE.md](STATE.md) for implementation and verification status.

**Goal:** Fix color picker usability issues and keyframe interaction bugs discovered during v0.2 testing.

**Target fixes:**
- Replace unrestricted color wheel with hue ring + adaptive saturation (limits per hue angle)
- Simplify color picker UI (remove RGB fields, reduce size)
- Fix color picker popup positioning constraints
- Fix keyframe drag color corruption
- Change snap default to off

## What This Is

A polished DAW-style timeline editor for creating animated light scenes in Hue Companion. Features GPU-accelerated rendering via Win2D, continuous color gradient tracks showing light evolution over time, and professional interaction polish inspired by Ableton Live and Logic Pro.

## Core Value

The track representation must feel like a real DAW — continuous color gradients showing what lights will do over time, with precise time alignment that users can trust.

## Requirements

### Validated

Shipped in v0.2:

- ✓ Timeline renders via Win2D CanvasControl for GPU acceleration — v0.2
- ✓ Canvas uses layered architecture (ruler, tracks, keyframes, playhead) — v0.2
- ✓ Rendering scales correctly on high-DPI displays — v0.2
- ✓ Continuous gradient color strips showing interpolated color over time — v0.2
- ✓ Gradient strips have rounded edges and clean borders (Logic Pro aesthetic) — v0.2
- ✓ Keyframes show hover states with color-matched glow effects — v0.2
- ✓ Playhead has 44px WCAG 2.5.5 compliant hit area — v0.2
- ✓ Playhead shows hover highlight and cursor change — v0.2
- ✓ Time ruler aligns correctly with keyframes on resize — v0.2
- ✓ Compact icon toggles for snap/loop with screen reader support — v0.2
- ✓ Brightness slider debounced (150ms) to prevent API flooding — v0.2
- ✓ Color picker updates lights in real-time for preview — v0.2
- ✓ Keyboard shortcuts documented in tooltips — v0.2

Existing capabilities (pre-v0.2):

- ✓ Timeline with tracks per light
- ✓ Keyframe creation via click, deletion via right-click
- ✓ Keyframe drag repositioning
- ✓ Color picker for keyframe editing
- ✓ Event tracks for random effects (lightning, sparkle, flicker)
- ✓ Snap-to-grid with zoom-adaptive intervals
- ✓ 60fps playback with rate-limited light updates
- ✓ Scene save/load to JSON

### Active

For future milestones (v2+):

- [ ] Track height adjustment (presets or drag-to-resize)
- [ ] Track reordering via drag
- [ ] Ableton-style minimal transport layout
- [ ] Zoom presets via keyboard shortcuts (Z/X)
- [x] Marquee selection for multiple keyframes — implemented in SceneBuilderPage
- [ ] Complete multi-keyframe move/delete workflow — selection/deletion exist; coordinated group movement requires evaluation

### Out of Scope

- Audio integration or waveform display — this is lights-only
- Further changes to multi-track editing — existing selection/clipboard operations already span tracks
- Undo/redo redesign — existing TimelineCommandHistory already supports undo/redo
- New animation types or event presets — polish existing, don't expand

## Context

**Current state:** The historical v0.2 milestone records the Win2D migration. GamutColorPicker is now integrated in the editor side panel, but remaining v0.3 acceptance checks and default-off snapping are open. Historical change statistics are retained in [MILESTONES.md](MILESTONES.md).

**Codebase:** WinUI 3 app with MVVM pattern using CommunityToolkit.Mvvm.

**Key files:**
- `src/HueCompanion/Views/SceneBuilderPage.xaml` — Timeline UI and integrated gamut picker
- `src/HueCompanion/Views/SceneBuilderPage.xaml.cs` — Input, selection, playback preview, and invalidation
- `src/HueCompanion/Views/Rendering/` — Win2D renderer classes and hit testing
- `src/HueCompanion.Core/ViewModels/SceneBuilderViewModel.cs` — State, keyframes, clipboard, and undo/redo

**Graphics:** Win2D layered architecture with separate renderers:
- TimeRulerRenderer — Time scale and tick marks
- GradientTrackRenderer — Color gradient strips with easing interpolation
- KeyframeLayerRenderer — Keyframe circles with glow effects
- PlayheadRenderer — Playhead line with hover thickness

## Constraints

- **Platform:** WinUI 3 / Windows App SDK
- **Performance:** 60fps timeline rendering maintained during playback
- **Accessibility:** WCAG 2.5.5 compliance for touch targets, screen reader support

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Gradient strips behind keyframe markers | User prefers current keyframe circles, gradient adds context | ✓ Good |
| Logic Pro aesthetic (4px corner radius) | Subtle rounding, not pill shape — user preference | ✓ Good |
| Win2D layered renderer architecture | Separate classes per layer for independent invalidation | ✓ Good |
| 15 gradient stops | Smooth visual without excessive GPU overhead | ✓ Good |
| Win2D ShadowEffect for glow | GPU-accelerated, color-matched glow rendering | ✓ Good |
| 44px playhead hit area | WCAG 2.5.5 compliance, easier to grab | ✓ Good |
| AutomationProperties.AcceleratorKey | Screen reader keyboard shortcut announcements | ✓ Good |
| Color picker real-time preview | Users need to see colors on physical lights immediately | ✓ Good (adjusted from debounced) |
| Brightness slider 150ms debounce | Prevents API flooding while maintaining usable interaction | ✓ Good |

---
*Last reconciled: 2026-11-07 against source. Historical release and UAT claims above retain their original scope.*
