# SPEC-011 — Boundary and collider-aware bake validation

SPEC-011 extends the validated SPEC-010 baker. The centerline remains the long-range backbone; boundary and collider data only modify nearby RG vectors. B is left unchanged.

## Implementation

- Boundary source: an explicit editable local-space polygon, with Bake Bounds retained as the simpler alternative.
- Obstacle selection: an explicit collider list plus optional `LayerMask`; the baker never includes every scene collider implicitly.
- Bake representation: valid-water mask, blocked-collider mask, and a two-pass eight-neighbor distance field at the Flow Map resolution.
- Boundary steering uses the distance-field inward gradient projected laterally against the centerline direction.
- Internal obstacles are handled separately with Editor-time obstacle-contour interpolation. The centerline transverse coordinate is constrained on each obstacle and smoothly interpolated through valid water, producing a downstream-preserving field that passes around the solid and returns to the centerline field.
- The generated compatible mesh stores obstacle-adapted flow coordinates and a frame derived from the final baked RG field. It is refined near obstacles so the chart can represent the redirected contours.
- Centerline validation warns when samples leave the boundary or intersect configured colliders. The system deliberately does not attempt maze solving or full rerouting.
- A persistent diagnostic texture is generated under `Generated/Diagnostics`: black = outside, magenta = blocked, cyan/green = valid water / increasing clearance. Scene View retains boundary, centerline, tangent and bounds visualization; shader Direction debug shows final RG.
- Runtime shader and Flow Strength logic were not changed. This remains an Editor-only bake.

## Validation

- Boundary-only bake produced 18,072 valid water texels and no blocked texels. The shader remains within the curved channel without unnecessary distortion (`SPEC-011_Boundary_Final.png`).
- Local obstacle bake identified 260 blocked texels. Visual marks divert on approach, preserve downstream motion, and return to the centerline route after the cylinder (`SPEC-011_Obstacle_Final.png`).
- Direction debug shows a localized change around the obstacle rather than a replacement of the channel field (`SPEC-011_Obstacle_DebugDirection.png`).
- Compared with the boundary baseline, the obstacle altered 1,261 RG texels, maximum deviation was 51.94 degrees, minimum forward dot was 0.616, no vector reversed, and maximum B delta was 0.
- The obstacle was moved from one side/later section of the channel to the other and re-baked. The moved bake changed 2,628 / 65,536 RG texels while 62,908 remained stable (`SPEC-011_ObstacleMoved_Final.png`).
- Play Mode captures at startup and at 24.78 seconds show the moved-obstacle field animating while the diversion and downstream recovery remain visually clean (`SPEC-011_Play_MovedObstacle_T00.png`, `SPEC-011_Play_MovedObstacle_T15.png`).
- Re-running boundary and obstacle bakes after the endpoint tolerance correction produced no centerline validity warnings.
- WaterShader compilation and final console audit produced no module errors or warnings.

## Obstacle correction revalidation

- The isolated straight regression case was re-baked after the contour-interpolation correction and explicit mesh-buffer upload fix.
- Direction Debug remained localized and smooth around the cylinder.
- All 80/80 traced streamlines reached the exit, with 0 collider hits, 0 bank escapes, 0 reversed samples, and no B-channel error (`ObstacleCorrection/AfterBufferFix_Metrics.txt`).
- The post-fix pattern capture follows both sides of the cylinder cleanly. Compared with the earlier `After_Pattern.png`, the stale/jagged renderer result is gone (`ObstacleCorrection/AfterBufferFix_Pattern.png`).
- The live renderer uses the regenerated mesh asset after rebake; explicit vertex/index/UV buffer updates resolved the discrepancy between serialized CPU mesh data and rendered geometry.
- The curved SPEC-011 case was re-baked and reloaded from disk. Its Direction Debug aligns with the configured obstacle, the normal pattern remains coherent through the channel, and an independent 80-streamline audit reached 80/80 with 0 collider hits, 0 bank escapes, and no reverse samples (`ObstacleCorrection/CurvedReloaded_Direction.png`, `ObstacleCorrection/CurvedReloaded_Pattern.png`).

## Limitations

- Boundary authoring is an explicit polygon in this Spec; automatic mesh-footprint extraction is not included.
- Collider projection assumes the collider intersects or closely spans the water plane. Completely blocked channels are reported, not automatically re-routed.
- The chamfer distance field is intentionally an Editor-time approximation. It supports smooth local obstacles, not arbitrary mazes, branches, recirculation, or fluid simulation.
- A 256x256 contour-interpolation rebake can take long enough for an external MCP request to time out even though the Unity Editor continues and completes the bake. This is an iteration-time caveat, not a correctness failure.
- No width-to-strength, lake inference, foam, waves, normals, reflection, refraction, VFX, or interaction was added.
