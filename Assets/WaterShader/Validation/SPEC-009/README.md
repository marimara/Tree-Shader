# SPEC-009 — Current diagnosis and channel-coordinate correction

The previous visual approval below is **superseded**. The user reported persistent
trajectory errors, and fresh captures reproduced streaks crossing the channel.

Read [Diagnosis_2026-09-16.md](Diagnosis_2026-09-16.md) for the current implementation,
tested hypotheses, evidence, performance and limitations. The current technical
materials enable `_UseFlowCoordinates`, requiring mesh UV2 and a UV3 flow frame.
This is a correction for a parameterized channel, not a general arbitrary-field solver.

Current comparison: `ChannelChart_Final.png`. Before: `Diagnosis_Before.png`.

---

## Historical streamline report — superseded, not current approval

SPEC-008 remains the runtime-data baseline. This revision replaces the previous per-fragment coordinate rotation with bounded streamline integration and rebuilds the technical Flow Maps from the channel centerline.

## Confirmed root cause

The previous correction reduced deformation but still oriented pattern coordinates independently at each fragment. A local Flow Map tangent describes only the direction at one point; it does not describe the integrated route that water took to reach that point. Rotating centred UVs by that tangent therefore produced locally plausible marks whose full trajectories could curve independently of the channel.

The shader now obtains curved motion from repeated Flow Map sampling along a backward trace. Local coordinate rotation is no longer responsible for the bend.

## Centerline and validation channel

The primary channel uses the following piecewise centerline in XZ:

- `x = lerp(-10, 10, t)`;
- `z = -3` for the straight entry (`t <= 0.25`);
- `z = lerp(-3, 3, smoothstep(0, 1, (t - 0.25) / 0.50))` through the broad bend;
- `z = 3` for the straight exit (`t >= 0.75`).

The water, bed, left bank, right bank, and orange `Centerline_Path_Reference` mesh are generated from this same path and its analytic tangent. The water strip uses planar XZ UVs, so the Flow Map and scene geometry share one coordinate space instead of double-counting curvature through longitudinal mesh UVs.

The scene defaults to the constant-strength material with the centerline visible. The separate straight uniform-flow strip remains above the channel for direct comparison.

## Technical Flow Maps

For every texel:

1. convert texture UV to the validation channel's XZ bounds;
2. find the nearest point on a 320-segment centerline approximation;
3. evaluate the local analytic centerline tangent at that path parameter;
4. convert that world tangent into planar texture-UV units and normalize it;
5. encode the result into RG.

This removes the former arbitrary UV interpolation between direction vectors. Both maps are 256 x 256, linear, uncompressed, bilinear, clamp textures:

- `T_FlowMap_CurveConstant.png`: encoded B is `0.62` throughout;
- `T_FlowMap_CurveStrength.png`: identical direction field, with a smooth `0.82 -> 0.28` B transition beginning after the bend.

## Backward advection

Each dual-phase sample starts at the fragment's base UV and traces upstream using four fixed Euler steps. Direction is re-sampled after each of the first three steps; the initial Flow Map sample already computed by `GetFlowData` is reused for step zero.

The two phases therefore use six additional Flow Map samples in total, or seven Flow Map samples per fragment including the initial direction/strength lookup. The pattern-source sample count is unchanged. This is a moderate, predictable SPEC-009 validation cost; it is not a simulation and does not accumulate state.

The loop travel distance is bounded to `0.24` UV units. Flow Strength still controls speed factor, pattern stretch, visibility, and coverage. Curvature does not feed any stretch calculation.

## Dual-phase adaptation

Both half-cycle-offset phases independently backtrace the same field. Sin-squared weights preserve the soft handoff and hide each reset. Because the trace starts again from `baseUV` every frame and travel is bounded, there is no unlimited offset, accumulated deformation, or progressive phase shear.

## `_FlowMapOrientationInfluence`

This property no longer blends the global direction against the local Flow Map direction and cannot weaken the curved trajectory. It is retained for material compatibility as a small phase-independent integrated pre-roll (up to 18% of the bounded loop distance), providing secondary artistic settling without local UV rotation. The technical materials use `0.8`.

The uniform-flow branch is unchanged and does not depend on this control.

## Visual validation

### Constant strength first

- the straight entry remains straight;
- the direction debug changes only where the centerline bends;
- streaks follow the broad S bend between the banks;
- the exit returns to the new straight heading;
- marks remain separated with visible negative space;
- no unjustified counter-curve, marble pattern, or local directional jitter was observed.

### Strength transition second

The separate strength map was tested only after direction passed. Speed, stretch, coverage, and visibility decrease smoothly toward the exit while the trajectory remains identical to the constant-strength map.

### Uniform comparison

The straight comparison continues using `_UseFlowMap = 0` and retains the SPEC-006/007 appearance. The curved channel reads as the same shaped pattern transported through a spatial path rather than a separately twisted shader.

### Temporal stability

Play Mode ran to `132.8 s` with valid captures at startup, approximately 60 seconds, and beyond 120 seconds. Animation phases changed while pattern quality and channel alignment remained stable.

Confirmed:

- no visible reset pop during observation;
- no increasing deformation or UV drift;
- no pattern collapse or progressive stretch;
- no directional jitter or discontinuity;
- Unity console: 0 errors and 0 warnings after shader import and the extended run.

## Current captures

- `SPEC-009_Streamline_Constant_WithCenterline.png`
- `SPEC-009_Streamline_Play_T00.png`
- `SPEC-009_Streamline_Play_T60.png`
- `SPEC-009_Streamline_Play_T120Plus.png`
- `SPEC-009_Streamline_StrengthTransition.png`
- `SPEC-009_Streamline_DebugDirection.png`
- `SPEC-009_Streamline_DebugStrength.png`

## Remaining limitations

- Four Euler steps are tuned for this broad, smooth field. Tighter bends or discontinuous production Flow Maps may require a smaller bounded travel distance, better source data, or a later quality/cost control.
- This is a fixed technical validation generator result, not a Flow Map Baker or boundary-aware routing system.
- The field follows the centerline and does not yet account for obstacles, colliders, bank proximity, or automatic river-to-lake strength.

Result: corrected SPEC-009 implementation is technically and visually validated against the explicit centerline channel; final artistic approval remains with the project owner.
