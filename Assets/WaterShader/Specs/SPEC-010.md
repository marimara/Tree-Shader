# \# SPEC-010 — Flow Map Baker Tool

# 

# \## Objective

# 

# Create an Editor-side Flow Map baking tool for the Stylized Water System.

# 

# The tool must automatically generate Flow Map data from an authored water path / centerline.

# 

# The user should not need to manually paint Flow Maps.

# 

# The baker should transform simple scene authoring data into the runtime Flow Map format already validated in SPEC-008 and visually proven in SPEC-009.

# 

# The primary goal is:

# 

# author a water path

# → bake

# → shader follows the path correctly.

# 

# This Spec establishes the core authoring and baking workflow.

# 

# Collider-aware routing, automatic width analysis and river-to-lake strength generation belong to later Specs.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated:

# 

# \- SPEC-008 — Flow Map Runtime Support

# \- SPEC-009 — Curved Flow / Stable Advection

# 

# Preserve the validated SPEC-009 behavior.

# 

# Do not redesign the runtime shader unless a compatibility problem with the baker is discovered.

# 

# The baker should generate data that works with the existing shader rather than compensating for poor data inside the shader.

# 

# \---

# 

# \# Lessons from SPEC-009

# 

# SPEC-009 validation demonstrated that Flow Map quality strongly affects the final visual result.

# 

# Important findings that must inform the baker:

# 

# \- local flow direction must correspond to the actual water path;

# \- arbitrary interpolation between directions can create visually incorrect curves;

# \- segmented or low-precision direction generation can introduce visible directional stepping;

# \- a continuous path representation produces better direction fields;

# \- Flow Strength should not unintentionally alter transverse orientation of the pattern;

# \- direction generation should be smooth enough that the shader does not need expensive blur or heavy correction.

# 

# The baker should generate clean source data.

# 

# Do not rely on the shader to repair a poor flow field.

# 

# \---

# 

# \# Workflow Goal

# 

# Target authoring workflow:

# 

# 1\. Create or select a water surface.

# 2\. Add a Flow Map Baker component.

# 3\. Define a centerline / water path.

# 4\. Configure basic bake settings.

# 5\. Bake Flow Map.

# 6\. Assign or automatically connect the generated Flow Map to the water material.

# 7\. Preview and rebake as needed.

# 

# The workflow should be simple enough for normal level-design iteration.

# 

# No manual texture painting should be required.

# 

# \---

# 

# \# Tool Architecture

# 

# Create an Editor-friendly component.

# 

# Suggested name:

# 

# StylizedWaterFlowBaker

# 

# or another clear equivalent.

# 

# Separate:

# 

# \- runtime/shared data;

# \- Editor-only UI;

# \- bake logic;

# 

# where appropriate.

# 

# Editor-only code must live in an Editor-safe location.

# 

# Keep the architecture modular enough for later Specs to add:

# 

# \- collider influence;

# \- boundary analysis;

# \- width-based strength;

# \- lake transitions.

# 

# Do not implement those systems yet.

# 

# \---

# 

# \# Primary Authoring Input — Centerline / Path

# 

# The primary directional input must be a continuous authored path through the water.

# 

# The baker should not depend primarily on a single global Flow Direction.

# 

# The path may be represented using:

# 

# \- editable control points;

# \- a spline;

# \- a lightweight custom polyline with smooth interpolation;

# \- another equivalent continuous representation.

# 

# Choose the simplest robust implementation suitable for Editor iteration.

# 

# The user must be able to define:

# 

# \- entry;

# \- bends;

# \- exit;

# 

# through scene-space control points.

# 

# \---

# 

# \# Path Editing

# 

# Provide a practical Editor workflow for editing the path.

# 

# At minimum:

# 

# \- control points visible in Scene View;

# \- points can be positioned in the scene;

# \- path order is clear;

# \- the resulting curve can be previewed.

# 

# Scene handles are strongly preferred if practical.

# 

# The tool does not need a complex spline-editor UX.

# 

# Keep the interaction lightweight.

# 

# \---

# 

# \# Continuous Path Requirement

# 

# Flow direction must be derived from a smooth/continuous representation of the path.

# 

# Avoid visible direction stepping caused by simply assigning each region to a raw line segment tangent.

# 

# If using a polyline internally, apply an appropriate continuous interpolation or tangent calculation.

# 

# The field should transition approximately like:

# 

# → → → ↘ ↘ ↓ ↓ ↘ → →

# 

# rather than abrupt:

# 

# → → ↓ ↓ → →

# 

# unless the authored path intentionally contains such a sharp turn.

# 

# \---

# 

# \# Flow Direction Generation

# 

# For every Flow Map texel inside the bake region:

# 

# 1\. determine its relationship to the authored path;

# 2\. find the relevant closest point / parameter on the continuous path;

# 3\. evaluate the local path tangent;

# 4\. encode that tangent as Flow Direction.

# 

# Conceptually:

# 

# world/sample position

# → closest position on centerline

# → path parameter

# → local tangent

# → Flow Direction

# → RG encoding

# 

# Exact implementation may vary.

# 

# The important requirement is that direction corresponds to the actual authored water path.

# 

# \---

# 

# \# Direction Encoding

# 

# Use the SPEC-008 convention:

# 

# R = Flow Direction X

# G = Flow Direction Y

# B = Flow Strength

# A = reserved

# 

# Encode normalized direction:

# 

# \-1 → 0

# &#x20;0 → 0.5

# +1 → 1

# 

# Conceptually:

# 

# encodedDirection = direction \* 0.5 + 0.5

# 

# Use the coordinate convention already validated by SPEC-008/009.

# 

# Do not introduce a new incompatible mapping.

# 

# \---

# 

# \# Coordinate Spaces

# 

# Be explicit and consistent about coordinate spaces.

# 

# The baker must correctly convert between:

# 

# \- water surface local space;

# \- world space;

# \- Flow Map UV space;

# \- path coordinates.

# 

# Do not assume that the water object is always:

# 

# \- at world origin;

# \- axis aligned;

# \- unscaled;

# \- unrotated;

# 

# unless a limitation is explicitly documented for this version.

# 

# Prefer a solution robust enough for normal transformed scene objects.

# 

# \---

# 

# \# Bake Bounds

# 

# The baker must know which world region maps to the Flow Map.

# 

# Support an explicit bake region associated with the water surface.

# 

# The region should be visible in Scene View.

# 

# Provide:

# 

# \- bounds visualization;

# \- predictable mapping from bounds to texture UV.

# 

# Avoid hidden or ambiguous mapping.

# 

# \---

# 

# \# Bake Resolution

# 

# Expose configurable bake resolution.

# 

# Suggested options may include:

# 

# 128

# 256

# 512

# 1024

# 

# Exact UI may vary.

# 

# Use a sensible default such as 256 or 512 for technical testing.

# 

# Do not force unnecessarily large textures.

# 

# \---

# 

# \# Base Flow Strength

# 

# For SPEC-010, Flow Strength generation should remain intentionally simple.

# 

# The goal is to validate direction generation and the baker workflow.

# 

# Support at least a constant Flow Strength value for the whole bake.

# 

# Example:

# 

# Base Flow Strength = 0.7

# 

# This value populates Flow Map B.

# 

# Do not implement automatic width-based strength yet.

# 

# \---

# 

# \# Optional Simple Strength Profile

# 

# A simple path-based strength profile is allowed only if it remains lightweight.

# 

# For example:

# 

# \- Start Strength

# \- End Strength

# 

# with smooth interpolation along the path.

# 

# This may be useful to reproduce the two validated technical cases from SPEC-009:

# 

# 1\. constant Flow Strength;

# 2\. gradual or strong reduction toward the final section.

# 

# However:

# 

# automatic analysis of river width / lake regions belongs to SPEC-012.

# 

# Do not make automatic environmental inference part of SPEC-010.

# 

# \---

# 

# \# Critical Strength Rule

# 

# Flow Strength data must remain conceptually independent from flow direction.

# 

# Changing B must not alter:

# 

# \- path tangent;

# \- transverse coordinate basis;

# \- direction field.

# 

# B should only feed the existing SPEC-007 Flow Strength behavior.

# 

# Do not encode orientation tricks through the strength channel.

# 

# \---

# 

# \# Bake Output

# 

# Generate a persistent Flow Map compatible with SPEC-008.

# 

# Preferred output:

# 

# Texture2D asset

# 

# or another persistent texture asset appropriate for runtime use.

# 

# The generated texture must use suitable import/settings for data:

# 

# \- sRGB Off;

# \- no destructive compression;

# \- bilinear filtering unless validation indicates otherwise.

# 

# Use the exact data format expected by the shader.

# 

# \---

# 

# \# Generated Asset Location

# 

# Generated assets should be isolated under:

# 

# Assets/WaterShader/Generated/

# 

# Recommended structure:

# 

# Assets/WaterShader/Generated/FlowMaps/

# 

# Create folders only when needed.

# 

# Do not place generated assets under:

# 

# \- References;

# \- source noise textures;

# \- Validation screenshots.

# 

# \---

# 

# \# Naming

# 

# Use predictable names.

# 

# Example:

# 

# T\_FlowMap\_<WaterObjectName>

# 

# Avoid cryptic generated filenames.

# 

# If multiple versions are needed during validation, use clear suffixes such as:

# 

# \_Constant

# \_Falloff

# \_Test

# 

# Do not create uncontrolled duplicate assets on every rebake.

# 

# \---

# 

# \# Re-baking

# 

# The baker must support repeated iteration.

# 

# When path points or settings change:

# 

# Bake again

# 

# should update the expected generated asset or clearly create a new intentional version.

# 

# Avoid requiring manual deletion or reassignment for routine iteration.

# 

# Do not overwrite unrelated assets.

# 

# \---

# 

# \# Material Assignment

# 

# The tool should make it easy to use the generated Flow Map.

# 

# Preferred behavior:

# 

# \- assign generated map to the intended water material;

# \- enable Flow Map mode if appropriate;

# 

# or provide an explicit button/action to do so.

# 

# Do not silently modify unrelated materials.

# 

# If the selected object uses a shared production material, avoid destructive global changes without clear intent.

# 

# A dedicated material instance may be preferable for technical validation.

# 

# \---

# 

# \# Preview / Debugging

# 

# Provide lightweight Scene View or Inspector diagnostics.

# 

# Useful preview information includes:

# 

# \- path / centerline;

# \- path control points;

# \- bake bounds;

# \- direction arrows along the path;

# \- optionally sampled field direction;

# \- optionally Flow Strength preview.

# 

# Do not attempt to render thousands of gizmos if it makes the Scene View unusable.

# 

# Keep debug visualization practical.

# 

# \---

# 

# \# Direction Debug

# 

# The existing shader Flow Direction debug mode should be usable with the generated map.

# 

# After baking, the developer should be able to inspect the generated RG field directly in the water shader.

# 

# The debug result should visibly correspond to the authored path.

# 

# \---

# 

# \# Validation Scene

# 

# Use or extend the SPEC-009 technical curved channel.

# 

# The baker validation should contain a clearly understandable path:

# 

# Straight

# → broad curve

# → second direction

# → exit

# 

# The channel geometry should make it obvious what the expected water trajectory is.

# 

# The centerline should follow this geometry.

# 

# Do not validate only on a featureless rectangular plane.

# 

# \---

# 

# \# Validation Case A — Constant Strength

# 

# First bake:

# 

# \- curved centerline;

# \- constant Flow Strength.

# 

# Validate only:

# 

# \- Flow Direction;

# \- curved advection;

# \- path accuracy.

# 

# Expected:

# 

# \- straight section remains visually straight;

# \- streaks follow the first curve;

# \- streaks follow the next segment;

# \- no unexplained extra curves appear;

# \- direction debug agrees with the path.

# 

# This test must pass before testing strength variation.

# 

# \---

# 

# \# Validation Case B — Strength Falloff

# 

# Create a second bake or configuration.

# 

# Use the same direction/path field.

# 

# Only change B.

# 

# Suggested behavior:

# 

# \- normal/strong strength through the river;

# \- clear reduction after a later curve;

# \- slower region toward the end.

# 

# This test should reproduce the useful behavior discovered during SPEC-009:

# 

# moving river

# → visibly slowing water

# 

# without changing the directional path.

# 

# The final automatic river-to-lake logic is still out of scope.

# 

# \---

# 

# \# Pattern Preservation

# 

# Generated maps must preserve the visual quality established in SPEC-006/007/009.

# 

# With the generated Flow Map:

# 

# \- streaks remain clean;

# \- negative space remains visible;

# \- shapes are not excessively bent;

# \- flow direction matches the path;

# \- no marble-like deformation appears.

# 

# If the baked field causes obvious deformation:

# 

# debug the generated field before adding shader-side corrections.

# 

# \---

# 

# \# Precision

# 

# Use sufficient precision in direction generation to avoid visible stepping.

# 

# Do not quantize path parameter or tangents unnecessarily.

# 

# If generated texture precision becomes visibly problematic, investigate:

# 

# \- path sampling;

# \- tangent calculation;

# \- texture format;

# \- bake resolution;

# 

# before adding shader complexity.

# 

# \---

# 

# \# Performance

# 

# SPEC-010 is an Editor-time tool.

# 

# Bake operations do not need to run every frame.

# 

# Prioritize:

# 

# 1\. correctness;

# 2\. predictable output;

# 3\. reasonable iteration speed.

# 

# A bake taking a short moment in Editor is acceptable.

# 

# Do not introduce continuous runtime flow solving.

# 

# \---

# 

# \# Error Handling

# 

# Provide useful feedback for invalid setups.

# 

# Examples:

# 

# \- fewer than two path points;

# \- missing water renderer/material;

# \- invalid bake bounds;

# \- invalid resolution;

# \- missing output folder/path;

# \- degenerate path section.

# 

# Do not silently produce invalid Flow Maps.

# 

# \---

# 

# \# Validation

# 

# Confirm:

# 

# \- a path can be authored in the scene;

# \- path tangents are smooth;

# \- a Flow Map is generated automatically;

# \- RG direction matches the path;

# \- B contains expected constant or authored strength;

# \- shader uses generated output;

# \- curved flow matches the scene geometry;

# \- rebaking after editing path points updates the field;

# \- uniform-flow fallback remains functional;

# \- generated map remains stable in extended Play Mode;

# \- no manual texture painting is required.

# 

# \---

# 

# \# External Assets

# 

# No manually authored Flow Map is required.

# 

# No new visual water textures are required.

# 

# The baker generates all Flow Map data required by this Spec.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-010 is complete when:

# 

# \- the user can author a water centerline/path in the scene;

# \- the path can be edited without manually editing a texture;

# \- the baker converts the path into a persistent Flow Map;

# \- generated RG direction follows the actual path;

# \- generated B supports at least constant Flow Strength;

# \- a simple strength falloff configuration can be tested without changing direction;

# \- generated Flow Maps work with the validated SPEC-009 advection;

# \- editing the path and rebaking updates the water;

# \- generated flow remains visually coherent through curves;

# \- no manual Flow Map painting is required;

# \- no collider-aware routing or automatic width inference has been introduced.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement yet:

# 

# \- collider-aware obstacle avoidance;

# \- automatic routing around rocks/obstacles;

# \- automatic water boundary extraction;

# \- distance fields;

# \- automatic river-width analysis;

# \- automatic narrow = fast behavior;

# \- automatic lake detection;

# \- automatic river-to-lake Flow Strength generation;

# \- runtime fluid simulation;

# \- runtime rebaking every frame;

# \- foam;

# \- intersection foam;

# \- waves;

# \- custom normals;

# \- reflection;

# \- refraction;

# \- VFX;

# \- interaction.

