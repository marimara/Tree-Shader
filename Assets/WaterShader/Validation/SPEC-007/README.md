# SPEC-007 Flow Strength and Flow-to-Calm Transition

## Result

PASS. `_FlowStrength` now produces a continuous visual family from calm water to
fast directional flow.

## Flow Strength behavior

The uniform 0-1 property modulates:

- effective Flow Speed, with a small non-zero calm baseline;
- flow-relative Pattern Stretch;
- Pattern Strength;
- threshold and selective-mask coverage;
- Pattern Scale and edge softness for broader calm forms.

All changes use continuous interpolation. `_FlowSpeed` remains the artist-facing
maximum/base speed and all SPEC-006 controls remain available.

## Comparison setup

The technical scene contains four adjacent surfaces with identical base
properties and only `_FlowStrength` changed:

- `Flow_Calm`: 0.00
- `Flow_Slow`: 0.33
- `Flow_River`: 0.66
- `Flow_Fast`: 1.00

`SPEC-007_Calm_Slow_River_Fast_Top_Final.png` is the clearest still comparison:
calm is broad and sparse, slow introduces gentle directionality, river has clean
separated streaks, and fast has longer, stronger, more numerous marks while
retaining negative space. `SPEC-007_Final_MainCamera.png` confirms the comparison
inside the depth test environment.

Depth coloring, shallow/deep colors, opacity, Flow Direction, and Flow Speed are
preserved. No Flow Map, foam, wave, normal, reflection, refraction, waterfall VFX,
or interaction system was added.
