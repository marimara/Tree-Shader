# SPEC-008 Validation Checkpoint

Validated before starting SPEC-009.

## Runtime data

- `_UseFlowMap = 1` samples `_FlowMap`; `_UseFlowMap = 0` preserves the uniform `_FlowDirection` and `_FlowStrength` fallback.
- Flow Map convention: `RG * 2 - 1` is local direction, `B` is the existing Flow Strength input, and `A` remains unused.
- Near-zero decoded directions use a magnitude guard and resolve to no directional flow.
- `_FlowDebugMode` values: `0 = Off`, `1 = Direction`, `2 = Strength`; default is Off.

## Technical generated data

`T_FlowMap_Technical.png` is a generated 256 x 256 validation texture, not an artistic/manual Flow Map. It turns smoothly from +X to +Y, reduces strength from fast to calm, and ends with a near-zero direction region.

Importer validation:

- sRGB: Off
- Compression: None / Uncompressed
- Filter: Bilinear
- Wrap: Clamp
- Mip maps: Off

Decoded samples:

- Entry: direction `(1.000, 0.004)`, strength `1.000`
- Curve midpoint: direction `(0.702, 0.710)`, strength `0.518`
- Calm end: encoded near-zero direction `(0.004, 0.004)`, strength `0.039`

## Validation results

- Shader compiler messages: 0.
- Flow Map mode enabled/disabled successfully.
- Direction debug matches generated RG data.
- Strength debug matches generated B data.
- Local B drives the SPEC-007 speed, stretch, pattern contribution, and coverage logic.
- Uniform fallback remains animated and directional.
- Depth color gradient remains visible and `_Opacity` remains connected (`0.72` in the technical material).
- Play Mode runtime validation completed without new shader/script errors.

## Captures

- `SPEC-008_Debug_Direction.png`
- `SPEC-008_Debug_Strength.png`
- `SPEC-008_FlowMap_Runtime.png`
- `SPEC-008_Uniform_Fallback.png`
- `SPEC-008_PlayMode_Runtime.png`

Checkpoint result: PASS. SPEC-009 may begin.
