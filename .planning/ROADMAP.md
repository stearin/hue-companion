# Roadmap: Hue Companion v0.3

## Overview

Scene Builder bug bash planning milestone, reconciled against source on 2026-11-07. Phase 5 uses a hue/saturation picker constrained by a configured gamut; integration exists but several acceptance checks remain open. Phase 6 proposes changing the snap-to-grid default. This roadmap does not claim a release or completed physical-bulb validation.

## Phases

### Phase 5: Color Picker Overhaul

**Goal:** Users select colors via hue ring with adaptive saturation limits where every choice is realizable on physical Hue bulbs.

**Dependencies:** None (first phase of v0.3)

**Plans:** 4 plans

Plans:
- [x] 05-01-PLAN.md — Consolidate XY-to-RGB conversions (implementation exists; COLOR-01 visual acceptance remains open)
- [x] 05-02-PLAN.md — Create ColorGamut class with triangle math
- [x] 05-03-PLAN.md — Build GamutColorPicker custom control with Win2D
- [ ] 05-04-PLAN.md — Partially integrated: GamutColorPicker is in the editor side panel; popup positioning and acceptance verification remain open

**Requirements:**
- CPICK-01: Color picker uses hue ring with adaptive saturation limit per hue angle (no impossible selections)
- CPICK-02: All selectable colors are realizable on Philips Hue bulbs
- CPICK-03: RGB text input fields removed from color picker
- CPICK-04: Color picker circle reduced to ~50% of current size
- CPICK-05: Color picker popup can appear outside timeline area bounds
- COLOR-01: Selected color on picker accurately reflects in gradient bar
- COLOR-02: Dragging keyframe preserves user-selected color (no corruption)

**Success Criteria:**
1. User can select any hue on the ring; saturation automatically limits to what the bulb can reproduce
2. Every color selectable in the picker displays correctly on physical Hue bulb
3. Selected color in picker matches the gradient bar segment for that keyframe
4. Dragging a keyframe to a new time position preserves the exact color value
5. Color picker popup positions intelligently and can extend beyond timeline bounds

---

### Phase 6: Interaction Polish

**Goal:** Scene Builder interactions match user expectations on first use without configuration.

**Dependencies:** Phase 5 (ensures color system is stable before changing interaction defaults)

**Requirements:**
- INTER-01: Snap-to-grid disabled by default

**Success Criteria:**
1. New Scene Builder sessions start with snap-to-grid toggle in off state
2. User can still enable snap-to-grid manually when needed

---

## Progress

| Phase | Status | Requirements |
|-------|--------|--------------|
| Phase 5: Color Picker Overhaul | Implementation present; integration/acceptance incomplete | 7 |
| Phase 6: Interaction Polish | Outstanding: snap still defaults to on | 1 |

**Total Requirements:** 8
**Mapped:** 8
**Coverage:** 100% mapped to phases; this is not implementation or test coverage. See [REQUIREMENTS.md](REQUIREMENTS.md) for individual acceptance statuses.

---
*Roadmap created: 2026-01-21*
*Milestone: v0.3 Scene Builder Bug Bash*
