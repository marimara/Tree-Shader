# SPEC-011.5 — Automatic Mesh Setup validation

## Architecture

- `Auto Setup From Mesh` is an Editor-only operation and remains separate from Bake.
- The baker now retains an explicit `Source Mesh`. Generated flow-coordinate meshes are still separate persistent copies and the source/imported mesh is never changed.
- The footprint is projected into baker-local XZ. Vertices are conservatively welded for topology analysis, one-use triangle edges form closed boundary loops, and the largest outer loop is selected.
- The boundary is simplified with closed-loop Ramer-Douglas-Peucker splitting while preserving both arcs of the loop. Output remains directly editable.
- Centerline generation rasterizes the selected polygon, builds an internal clearance field, infers deterministic end regions from the footprint principal axis, then finds a clearance-weighted path between them. This uses the axis only to identify ends; the path itself follows the interior medial region, including S bends.
- The dense path is smoothed only when the candidate remains inside the polygon and is simplified to at most 24 editable control points.
- Disconnected surfaces, significant secondary loops, non-manifold/open projected boundaries and persistent split cross-sections produce warnings. Ambiguous centerline generation is skipped while successful bounds/boundary setup is preserved.

## Automatic estimates

- Bounds use the source-mesh XZ min/max plus 1.5% padding, with a 0.05 local-unit minimum.
- Resolution uses transformed world extent: up to 14 units = 128, 36 = 256, 90 = 512, larger = 1024.
- Flow Coordinate World Scale uses approximately one along-flow repeat per third of centerline length and 40% of average channel width across-flow.
- Steering Distance uses 35% of average channel width. Steering Strength, obstacle layer filtering, explicit obstacles and Flow Strength remain artist-authored/preserved.
- Output naming is deterministic: `T_FlowMap_<WaterObjectName>`.

## Validation

- Straight procedural river: bounds 20.60 x 3.80, 4 boundary points, 3 centerline points, resolution 256. Reverse direction exchanged endpoints correctly.
- Curved technical river (`MESH_FlowCurve_Water`): bounds 20.60 x 9.70, 16 boundary points, 6 centerline points. The generated path follows the full -3 to +3 S bend instead of collapsing to a straight chord.
- Curved river with validated local obstacle: 256x256 Bake produced 21,655 valid-water samples and 435 blocked samples. Obstacle contour interpolation converged; the generated coordinate mesh contained 28,382 vertices. An RK2 audit sent 80/80 streamlines to the exit with 0 collider hits and 0 bank escapes. Flow Strength B error was 0.
- Widening/basin procedural river: bounds 20.60 x 10.20, usable simplified boundary, and a centered 3-point path through the widening region. No automatic strength transition was added.
- Ambiguous/disconnected footprint: warnings were emitted, bounds and the largest boundary were retained, and the existing centerline was preserved for manual editing.
- Undo restored all Auto Setup values in one operation. Reverse Flow Direction was independently undoable. Re-running setup reused the same component data and created no assets because Auto Setup does not Bake.

## Captures

- `SPEC-011.5_Curved_AutoSetup_PreBake_Top.png`: top Scene View before Bake, showing fitted bounds, simplified boundary, editable centerline and direction arrows.
- `SPEC-011.5_CurvedObstacle_PostBake.png`: normal pattern after the auto-generated setup and obstacle-aware Bake.
- `SPEC-011.5_CurvedObstacle_Direction.png`: Direction Debug showing localized obstacle diversion and downstream recovery.

## Limitations

- The supported target is a single, mostly planar, non-branching triangle surface in local XZ.
- Holes, multiple significant loops, disconnected pieces, open/non-manifold projection and strong branching are diagnosed rather than invented into one centerline.
- A pre-SPEC-011.5 baker whose MeshFilter already references only a generated mesh cannot recover the historical source reference. It is analyzed read-only with a warning until the user assigns `Source Mesh`.
- Upstream direction is not inferable from geometry alone; direction is deterministic and explicitly correctable with `Reverse Flow Direction`.
- No SPEC-012 width-to-strength behavior was implemented.
