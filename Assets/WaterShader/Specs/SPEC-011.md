# \# SPEC-011 — Collider and Boundary-aware Flow Bake

# 

# \## Objective

# 

# Extend the Flow Map Baker created in SPEC-010 so scene geometry and water boundaries can influence the generated flow field.

# 

# The centerline/path created in SPEC-010 remains the primary flow backbone.

# 

# Scene geometry should refine that field locally by:

# 

# \- identifying valid water regions;

# \- identifying obstacles;

# \- measuring distance from boundaries;

# \- steering flow around reasonable local obstructions.

# 

# This remains an Editor-time bake.

# 

# Do not introduce runtime fluid simulation.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated:

# 

# \- SPEC-010 — Flow Map Baker Tool

# 

# Preserve:

# 

# \- centerline/path authoring;

# \- continuous tangent generation;

# \- Flow Map RG encoding;

# \- Flow Strength B encoding;

# \- SPEC-009 advection;

# \- uniform-flow fallback.

# 

# \---

# 

# \# Core Principle

# 

# The authored centerline remains the main definition of where the river travels.

# 

# Collider and boundary information modifies the flow field locally.

# 

# Conceptually:

# 

# Centerline

# → primary direction field

# 

# Boundaries / Colliders

# → local constraints and steering

# 

# Combined result

# → final baked Flow Map

# 

# Do not replace the centerline with a general-purpose pathfinding system.

# 

# \---

# 

# \# Scope of Automatic Steering

# 

# The system should handle reasonable local obstacles such as:

# 

# \- rocks;

# \- pillars;

# \- small terrain intrusions;

# \- simple islands;

# \- local boundary irregularities.

# 

# The system is not required to solve arbitrary maze-like environments.

# 

# If an obstacle completely blocks the authored channel or makes the centerline invalid, the baker should report the problem rather than attempting complex automatic rerouting.

# 

# \---

# 

# \# Collider Filtering

# 

# Allow the baker to explicitly control which colliders influence water flow.

# 

# Preferred mechanism:

# 

# LayerMask

# 

# Optional additional mechanism:

# 

# explicit collider collection.

# 

# Do not automatically include every collider in the scene.

# 

# Provide a clear field such as:

# 

# Obstacle Layers

# 

# or equivalent.

# 

# \---

# 

# \# Water Boundary Source

# 

# The baker must establish which texels represent valid water.

# 

# Support at least one robust boundary source.

# 

# Possible approaches:

# 

# \- water surface mesh footprint;

# \- dedicated water boundary collider;

# \- explicit boundary geometry;

# \- generated mask from configured water region.

# 

# Choose the simplest solution compatible with the project.

# 

# The resulting internal representation should distinguish:

# 

# Valid Water

# Blocked / Outside Water

# Obstacle

# 

# \---

# 

# \# Technical Representation

# 

# Convert boundary and obstacle information into a bake-space representation.

# 

# Possible internal data:

# 

# \- occupancy grid;

# \- water mask;

# \- obstacle mask;

# \- distance field.

# 

# A grid-based Editor-time representation is acceptable.

# 

# Do not expose implementation complexity unless useful for debugging.

# 

# \---

# 

# \# Distance Field

# 

# Generate or approximate distance to the nearest invalid region / obstacle if technically useful.

# 

# This information is expected to become important for:

# 

# \- smooth obstacle steering;

# \- width estimation in SPEC-012;

# \- future intersection foam;

# \- future turbulence.

# 

# For SPEC-011, use it only where needed for routing and diagnostics.

# 

# Do not implement foam or turbulence.

# 

# \---

# 

# \# Relationship to Centerline

# 

# For each valid water sample:

# 

# 1\. determine its closest / relevant position on the centerline;

# 2\. obtain the centerline tangent;

# 3\. evaluate boundary / obstacle influence;

# 4\. modify the local direction only as much as required;

# 5\. smoothly return toward the centerline direction after the influence ends.

# 

# The centerline tangent should remain the dominant long-range direction.

# 

# \---

# 

# \# Obstacle Steering

# 

# Near an obstacle, flow should:

# 

# \- avoid pointing directly into blocked space;

# \- move around the obstacle using a smooth lateral component;

# \- preserve forward progression;

# \- gradually recover toward the centerline tangent afterward.

# 

# Avoid:

# 

# \- sudden 90-degree turns;

# \- vectors pointing backward without a strong reason;

# \- circular flow around static obstacles;

# \- high-frequency direction changes.

# 

# \---

# 

# \# Forward Progress Requirement

# 

# Obstacle avoidance must preserve meaningful downstream motion.

# 

# A local obstacle should not accidentally produce:

# 

# \- loops;

# \- backwards flow;

# \- spirals;

# \- stagnant regions;

# 

# unless such behavior is explicitly authored in a future system.

# 

# Prefer a combination of:

# 

# forward tangent

# \+

# obstacle avoidance influence

# 

# rather than replacing the forward direction completely.

# 

# \---

# 

# \# Boundary Influence

# 

# Flow near the edge of the water region should remain inside valid water.

# 

# Vectors should not point strongly out of the water mask.

# 

# Use boundary influence to gently steer the field inward where necessary.

# 

# Do not create an obvious artificial "wall-following" effect.

# 

# \---

# 

# \# Smoothness

# 

# The resulting direction field must remain compatible with the visual requirements discovered in SPEC-009.

# 

# Neighboring vectors should change smoothly.

# 

# Avoid producing data that requires expensive shader-side blur or correction.

# 

# If the baked map produces visually incorrect curves:

# 

# debug the bake data first.

# 

# Do not immediately modify the validated runtime shader.

# 

# \---

# 

# \# Centerline Validity

# 

# Provide validation for the authored centerline.

# 

# Warn if:

# 

# \- the centerline leaves the valid water region;

# \- a control point lies inside a blocking collider;

# \- a large obstacle completely blocks the intended path;

# \- the channel becomes too narrow for reliable bake resolution.

# 

# Do not silently generate obviously invalid data.

# 

# \---

# 

# \# Bake Resolution

# 

# Reuse the resolution controls created in SPEC-010.

# 

# Obstacle and boundary sampling should use the same predictable bake-space mapping.

# 

# Do not create an unrelated secondary resolution unless technically necessary.

# 

# \---

# 

# \# Debug Visualization

# 

# Provide useful diagnostics for:

# 

# \- valid water mask;

# \- obstacles;

# \- distance-to-boundary / obstacle if implemented;

# \- centerline;

# \- final local direction;

# \- blocked cells.

# 

# The user should be able to understand why a region received a particular flow direction.

# 

# Avoid rendering excessive Scene View gizmos by default.

# 

# \---

# 

# \# Validation Case A — Boundary Following

# 

# Create a technical water channel containing:

# 

# \- a clear centerline;

# \- curved boundaries;

# \- no internal obstacle.

# 

# Confirm:

# 

# \- flow remains inside the channel;

# \- flow broadly follows the centerline;

# \- boundary influence does not create unnecessary distortion.

# 

# \---

# 

# \# Validation Case B — Local Obstacle

# 

# Add a simple obstacle such as a rock/cylinder/cube that partially obstructs the channel.

# 

# Bake again.

# 

# Expected:

# 

# before obstacle:

# → follows centerline

# 

# near obstacle:

# → smoothly diverts

# 

# after obstacle:

# → returns toward centerline

# 

# The resulting shader motion must remain visually clean.

# 

# \---

# 

# \# Validation Case C — Moved Obstacle

# 

# Move the obstacle.

# 

# Rebake.

# 

# Confirm that:

# 

# \- affected vectors change;

# \- unaffected areas remain largely stable;

# \- shader responds to the new map;

# \- generated field still follows the channel.

# 

# \---

# 

# \# Flow Strength

# 

# SPEC-011 should avoid introducing complex automatic strength behavior.

# 

# Prefer constant or authored strength from SPEC-010 during direction validation.

# 

# Obstacle steering must not unintentionally create large Flow Strength changes.

# 

# Automatic width / constriction-driven Flow Strength belongs to SPEC-012.

# 

# \---

# 

# \# Critical Separation of Responsibilities

# 

# RG:

# 

# Flow Direction

# 

# B:

# 

# Flow Strength

# 

# Obstacle steering should primarily affect RG.

# 

# Do not encode directional correction by manipulating B.

# 

# Do not let local obstacle distance arbitrarily change Pattern Stretch through unintended strength changes.

# 

# \---

# 

# \# Generated Data

# 

# Continue generating persistent Flow Maps through the SPEC-010 baker.

# 

# No manually authored texture is required.

# 

# Generated assets remain under:

# 

# Assets/WaterShader/Generated/

# 

# \---

# 

# \# Performance

# 

# This remains an Editor-time operation.

# 

# It is acceptable to perform:

# 

# \- grid analysis;

# \- distance calculations;

# \- iterative smoothing;

# 

# during Bake.

# 

# Prioritize field quality over extremely fast bake time.

# 

# However, keep iteration practical for normal level design.

# 

# Do not perform this analysis continuously every frame.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-011 is complete when:

# 

# \- water boundaries are represented in bake space;

# \- relevant colliders can be selected/filterable;

# \- generated flow remains inside valid water;

# \- simple obstacles influence the baked field;

# \- flow smoothly moves around local obstacles;

# \- forward progression is preserved;

# \- flow returns toward the centerline after obstacles;

# \- moving an obstacle and rebaking changes the expected area;

# \- resulting maps remain visually compatible with SPEC-009;

# \- no runtime fluid simulation is introduced.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- full automatic route planning;

# \- NavMesh-style river routing;

# \- arbitrary maze solving;

# \- runtime collider recalculation;

# \- automatic width-to-strength generation;

# \- automatic lake detection;

# \- foam;

# \- waves;

# \- normals;

# \- reflection;

# \- refraction;

# \- VFX;

# \- runtime fluid simulation.

