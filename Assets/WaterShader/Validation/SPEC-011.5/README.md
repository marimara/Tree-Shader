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

- Bounds remain in baker-local XZ, but their padding is chosen in world units and converted independently back through the transformed local X/Z axes. This avoids scale-dependent padding while preserving the baker's local mapping contract.
- Resolution uses transformed physical extents: up to 14 units = 128, 36 = 256, 90 = 512, larger = 1024.
- Physical centerline length is integrated from transformed samples. Average physical width is estimated from transformed centerline-to-boundary distances.
- Flow Coordinate World Scale uses approximately one along-flow repeat per third of physical centerline length and 40% of physical average channel width across-flow.
- Steering Distance uses 35% of physical average channel width. Steering Strength, obstacle layer filtering, explicit obstacles and Flow Strength remain artist-authored/preserved.
- Generated UV2 coordinates now store transformed physical along/across distances divided by Flow Coordinate World Scale. UV3 frames convert between the local Flow Map domain and that physical chart, including non-uniform scale.
- Boundary and obstacle distance fields, steering reach, blocked-cell clearance and adaptive mesh refinement use transformed physical cell/edge distances.
- Output naming is deterministic: `T_FlowMap_<WaterObjectName>`.

## Validation

- Straight procedural river: bounds 20.60 x 3.80, 4 boundary points, 3 centerline points, resolution 256. Reverse direction exchanged endpoints correctly.
- Curved technical river (`MESH_FlowCurve_Water`): bounds 20.60 x 9.70, 16 boundary points, 6 centerline points. The generated path follows the full -3 to +3 S bend instead of collapsing to a straight chord.
- Curved river with validated local obstacle: 256x256 Bake produced 21,655 valid-water samples and 435 blocked samples. Obstacle contour interpolation converged; the generated coordinate mesh contained 28,382 vertices. An RK2 audit sent 80/80 streamlines to the exit with 0 collider hits and 0 bank escapes. Flow Strength B error was 0.
- Widening/basin procedural river: bounds 20.60 x 10.20, usable simplified boundary, and a centered 3-point path through the widening region. No automatic strength transition was added.
- Ambiguous/disconnected footprint: warnings were emitted, bounds and the largest boundary were retained, and the existing centerline was preserved for manual editing.
- Undo restored all Auto Setup values in one operation. Reverse Flow Direction was independently undoable. Re-running setup reused the same component data and created no assets because Auto Setup does not Bake.

## Transform-aware regression

- A dedicated repeatable Editor fixture is available at `Tools/WaterShader/SPEC-011.5/Validate Transform Awareness`.
- Case A used a 20 x 4 source mesh with Transform Scale `(1,1,1)`. Case B used a 2 x 2 source mesh with non-uniform Scale `(10,1,2)`. Both were rotated 17 degrees and therefore occupied equivalent 20 x 4 world-space footprints.
- Auto Setup selected resolution 256 for both. Flow Coordinate World Scale was `(5.085, 1.582)` versus `(5.000, 1.566)` and Steering Distance was `1.385` versus `1.370`.
- Generated UV2 ranges were `(3.249, 2.528)` versus `(3.249, 2.555)`. Baked Flow Map RG mean/max delta was `0.00157 / 0.02174`.
- Reconstructed shader channel-motion vectors differed by only `0.00062` mean and `0.00198` maximum at exact corresponding source vertices.
- Equivalent physical obstacles produced identical trace results: 39/40 downstream arrivals, one numerical separatrix hit and zero bank escapes in each representation. The previously validated SPEC-011 focused audit remains 80/80 with zero hits.
- Re-running Auto Setup after changing the scaled case from `(10,1,2)` to `(15,1,1)` changed Flow Coordinate World Scale from `(5.000, 1.566)` to `(7.500, 0.783)`, Steering Distance from `1.370` to `0.685`, and transform-aware local padding from Bake Size `(2.060, 2.300)` to `(2.060, 2.900)`. Resolution correctly remained 256 because both physical longest extents stayed within the same threshold.
- Bake And Assign set both `_UseFlowMap` and `_UseFlowCoordinates` on the supported shader without manual material intervention.

## Captures

- `SPEC-011.5_Curved_AutoSetup_PreBake_Top.png`: top Scene View before Bake, showing fitted bounds, simplified boundary, editable centerline and direction arrows.
- `SPEC-011.5_CurvedObstacle_PostBake.png`: normal pattern after the auto-generated setup and obstacle-aware Bake.
- `SPEC-011.5_CurvedObstacle_Direction.png`: Direction Debug showing localized obstacle diversion and downstream recovery.
- `SPEC-011.5_TransformAware_Final.png`: isolated top-down comparison of the final-size and non-uniformly scaled equivalent surfaces after Auto Setup + obstacle-aware Bake And Assign.

## Limitations

- The supported target is a single, mostly planar, non-branching triangle surface in local XZ.
- Holes, multiple significant loops, disconnected pieces, open/non-manifold projection and strong branching are diagnosed rather than invented into one centerline.
- A pre-SPEC-011.5 baker whose MeshFilter already references only a generated mesh cannot recover the historical source reference. It is analyzed read-only with a warning until the user assigns `Source Mesh`.
- Upstream direction is not inferable from geometry alone; direction is deterministic and explicitly correctable with `Reverse Flow Direction`.
- The physical metric assumes a mostly planar local-XZ water surface, matching the existing baker contract. Strongly sheared hierarchies are handled through transformed basis vectors where practical, but remain less predictable than ordinary TRS hierarchies.
- The minimal transform-equivalence obstacle fixture contains one discrete separatrix trajectory that grazes the collider in both representations; this is not scale-dependent. The denser established SPEC-011 regression remains the authoritative obstacle-quality case.
- No SPEC-012 width-to-strength behavior was implemented.
