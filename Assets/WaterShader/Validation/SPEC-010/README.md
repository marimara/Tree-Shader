# SPEC-010 — Flow Map Baker validation

SPEC-010 was implemented and validated before any SPEC-011 bake was run.

## Implementation

- `StylizedWaterFlowBaker` is an Editor-authored component with an ordered local-space centerline, explicit local-XZ bake bounds, selectable 128/256/512/1024 resolution, persistent output naming, constant strength, and optional authored path falloff.
- The custom Editor displays numbered position handles, a smooth Catmull–Rom preview, tangent arrows, and bake bounds.
- Every texel is projected onto the continuous curve, refined around its closest parameter, and receives the curve tangent converted into the same normalized UV space used by the shader.
- Re-baking overwrites the named `RGBAFloat` `.asset` in place. The asset is linear data, bilinear, clamp, uncompressed, and persistent under `Generated/FlowMaps`.
- The baker also regenerates a persistent mesh copy with UV0 map addressing, UV2 channel coordinates, and a UV3 smooth conversion frame from the same centerline. This was required to preserve the corrected SPEC-009 channel-coordinate representation when the authored path changes.
- Assignment is explicit: the configured validation material receives `_FlowMap` and `_UseFlowMap = 1`. The runtime shader was not modified.

## Validation

- Constant bake: `T_FlowMap_FlowCurve_Baked_Constant.asset`, B = 0.62.
- Direction debug is smooth and agrees with the straight → broad curve → straight channel (`SPEC-010_Baked_DebugDirection.png`).
- The final constant capture has straight marks on both straight sections and coherent marks through the bend (`SPEC-010_Constant_Final.png`).
- Falloff bake: `T_FlowMap_FlowCurve_Baked_Falloff.asset`, authored B = 0.82 → 0.28 after path t = 0.55.
- Automated comparison found maximum RG delta = 0 between Constant and Falloff, while B differed by up to 0.3399998. Strength therefore changes behavior without changing trajectory.
- Strength debug shows the expected downstream gradient (`SPEC-010_Falloff_DebugStrength.png`); the normal result is `SPEC-010_Falloff_Final.png`.
- An automated edit/re-bake moved the middle centerline point, re-baked the same persistent asset, and detected 22,359 / 65,536 changed direction texels. It then restored the point and re-baked again.
- Play Mode was allowed to advance in the background to 87.07 seconds. Captures at advancing time and 80+ seconds retain channel alignment and negative space (`SPEC-010_Play_T30_Advancing.png`, `SPEC-010_Play_T80Plus.png`).
- The uniform comparison object remained active and the shader's uniform-flow fallback was not changed.
- After the run, the WaterShader scripts/shader reported no errors or warnings. Unity AI `NoSubscription` messages seen earlier are unrelated to this module.

## Checkpoint result

Passed. Centerline editing, persistent bake, path-following direction, Constant Strength, direction-independent Strength Falloff, and edit/re-bake all behaved correctly. Only after this result was SPEC-011 started.

## Limitations

- Reliable SPEC-009 curved-pattern transport requires the generated UV2/UV3-compatible mesh; disabling compatible flow-coordinate generation falls back to the older arbitrary-mesh path with its documented limitations.
- A target mesh is expected to represent the water surface in the baker object's local XZ plane. Object translation, rotation, and scale are supported because authored data remains local and collider tests convert through the object transform.

