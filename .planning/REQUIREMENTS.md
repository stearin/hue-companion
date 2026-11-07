# Requirements: Hue Companion

**Defined:** 2026-01-21
**Core Value:** The track representation must feel like a real DAW — continuous color gradients showing what lights will do over time, with precise time alignment that users can trust.

**Source reconciliation:** 2026-11-07. Checked boxes mean implementation exists in source; physical-bulb and Windows UI acceptance remain separate.

## v0.3 Requirements

Bug fixes and usability improvements for Scene Builder color picker and interactions.

### Color Picker Redesign

- [x] **CPICK-01**: Color picker uses hue ring with adaptive saturation limit per hue angle (no impossible selections)
- [ ] **CPICK-02**: All selectable colors are realizable on Philips Hue bulbs
- [x] **CPICK-03**: RGB text input fields removed from color picker
- [ ] **CPICK-04**: Color picker circle reduced to ~50% of current size
- [ ] **CPICK-05**: Color picker popup can appear outside timeline area bounds

### Color Accuracy

- [ ] **COLOR-01**: Selected color on picker accurately reflects in gradient bar
- [ ] **COLOR-02**: Dragging keyframe preserves user-selected color (no corruption)

### Interaction Defaults

- [ ] **INTER-01**: Snap-to-grid disabled by default

## Future Requirements

Remaining proposals and capabilities reconciled from the original v0.2 deferred list:

### Scene Builder v2

- **SCENE-01**: Track height adjustment (presets or drag-to-resize)
- **SCENE-02**: Track reordering via drag
- **SCENE-03**: Ableton-style minimal transport layout
- **SCENE-04**: Zoom presets via keyboard shortcuts (Z/X)
- **SCENE-05 — Implemented**: Marquee selection for multiple keyframes
- **SCENE-06 — Partial**: Multiple-keyframe selection/deletion exists; coordinated group movement still needs separate evaluation

## Out of Scope

| Feature | Reason |
|---------|--------|
| Unrestricted color wheel | Hue bulbs can't reproduce all sRGB colors; wheel must be clipped to Hue gamut |
| Audio integration | This is lights-only |
| Further undo/redo changes | A command-history implementation already exists; no redesign is planned in this milestone |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| CPICK-01 | Phase 5 | Implemented: configured-gamut saturation limiting; bulb-specific integration needs verification |
| CPICK-02 | Phase 5 | Needs physical-bulb verification; page currently uses default Gamut C |
| CPICK-03 | Phase 5 | Implemented: GamutColorPicker has no RGB input fields |
| CPICK-04 | Phase 5 | Needs acceptance verification: current canvas is 240px with a 110px wheel radius; relative-size target unverified |
| CPICK-05 | Phase 5 | Outstanding: picker uses a side panel, not the proposed floating popup |
| COLOR-01 | Phase 5 | Shared conversion/picker paths exist; visible agreement needs Windows verification |
| COLOR-02 | Phase 5 | Time-only move path exists; drag color preservation needs Windows regression verification |
| INTER-01 | Phase 6 | Outstanding: _isSnapEnabled initializes to true |

**Coverage:**
- v0.3 requirements: 8 total
- Mapped to phases: 8
- Unmapped: 0

---
*Requirements defined: 2026-01-21*
*Last reconciled: 2026-11-07 against current source; no new UAT performed.*
