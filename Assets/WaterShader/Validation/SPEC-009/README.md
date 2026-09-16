# SPEC-009 Validation — Visual Correction

SPEC-008 remains the validated runtime-data baseline. This correction addresses the visual quality of curved flow without changing the uniform-flow path.

## Root cause

The first SPEC-009 implementation rotated absolute pattern coordinates directly by the per-fragment Flow Map direction. Because that basis changed across the surface, the coordinate transform itself became nonlinear. Direction curvature was therefore amplified into bent streaks, arcing noise, and a marble-like deformation.

The original dual-phase blend was temporally bounded, but its full-pattern-unit travel distance and triangular weights made the two phases visually less correlated and gave the transition a harder cadence.

## Shader and advection correction

- Flow Map direction still drives local motion at full strength.
- Pattern orientation uses a centred UV basis so direction changes cannot create large origin-relative displacement.
- `_FlowMapOrientationInfluence` blends the broad local orientation with `_FlowDirection` as a stable reference. The technical materials use `0.68`.
- Curvature no longer contributes to stretch. Effective Flow Strength remains the only driver of speed factor, stretch, visibility, and coverage.
- Dual-phase travel is limited to `0.45` pattern units and the cycle rate is adjusted to preserve perceived speed.
- Sin-squared complementary weights replace triangular weights, giving zero derivative at phase resets and a softer handoff.
- The uniform-flow branch retains its original uncentred coordinates and behavior.
- Direction and encoded-strength debug modes from SPEC-008 remain intact.

## Flow Strength remap

Flow Map B is retained as the encoded debug value and is remapped once before entering the SPEC-007 system:

`effectiveStrength = lerp(_FlowMapMinStrength, _FlowMapMaxStrength, encodedB)`

The corrected technical materials use `0.46 -> 0.78`. The variable test map transitions smoothly from encoded `0.82` to `0.28`, producing an effective range of approximately `0.72 -> 0.55`. This keeps neighboring regions coherent while still showing progressively slower motion, shorter streaks, and lower coverage.

## Technical Flow Maps

Two 256 x 256 linear, uncompressed, bilinear, clamp textures were generated:

- `T_FlowMap_CurveConstant.png`: broad smooth direction turn with encoded strength `0.62` throughout.
- `T_FlowMap_CurveStrength.png`: the same direction field with a separate smooth strength transition.

The validated SPEC-008 map was not overwritten.

## Validation scene

`WaterShader_TestScene` now contains a `SPEC009_Validation` setup:

- a constant-width curved water mesh with a broad gradual bend;
- matching left/right bank meshes and a recessed river bed;
- a simple technical ground plane;
- a separate straight, banked uniform-flow comparison strip;
- a diagnostic top camera that makes the intended route immediately readable.

The previous scene objects remain present but inactive. No environment art or out-of-scope water feature was added.

## Staged visual validation

### 1. Constant strength

`MAT_Water_FlowCurve_Constant` isolates direction at nearly constant strength. The streaks follow the broad channel orientation while remaining separated and predominantly straight within each local region. The previous arcing/marble deformation is absent.

### 2. Strength transition

`MAT_Water_FlowCurve_Strength` uses the same direction field and changes only B. The transition is gradual; speed, stretch, and coverage change moderately without abrupt fast/slow islands.

### 3. Uniform comparison

`MAT_Water_Uniform_Comparison` uses `_UseFlowMap = 0` and matches the clean SPEC-006/SPEC-007 pattern family. Curved flow reads as the same shader following a bend, not as a different distorted pattern.

### 4. Temporal stability

A valid Play Mode run reached `130.19 s` with background execution explicitly enabled so Unity time continued while the Editor was unfocused. Captures at the start, after 60 seconds, and after 120 seconds contain different animation phases while retaining the same pattern quality.

Confirmed:

- no visible reset pop during observation;
- no increasing deformation or UV drift;
- no pattern collapse;
- no directional jitter;
- no progressive stretch;
- Play Mode console: 0 errors and 0 warnings;
- no `StylizedWater` or shader compiler errors after the final shader import.

Unity AI Assistant later emitted unrelated `NoSubscription` exceptions after an Editor reload; they are outside `Assets/WaterShader` and did not occur during the valid Play Mode run.

## Captures

- `SPEC-009_Corrected_ConstantStrength.png`
- `SPEC-009_Corrected_StrengthTransition_Final.png`
- `SPEC-009_Corrected_UniformComparison.png`
- `SPEC-009_Corrected_DebugDirection.png`
- `SPEC-009_Corrected_DebugStrengthConstant.png`
- `SPEC-009_Corrected_DebugStrengthTransition.png`
- `SPEC-009_Corrected_PlayBG_T00.png`
- `SPEC-009_Corrected_PlayBG_T03.png`
- `SPEC-009_Corrected_PlayBG_T60Plus.png`
- `SPEC-009_Corrected_PlayBG_T120Plus.png`

## Remaining limitation

This is still a technical UV-space Flow Map and not a production baker. Extremely abrupt or discontinuous artist-authored vector fields can still produce poor orientation; the shader deliberately avoids an expensive runtime blur, so source-field quality remains important.

Result: PASS for corrected SPEC-009 visual and temporal criteria.
