# \# SPEC-011.5 — Automatic Mesh Setup and Authoring UX

# 

# \## Objective

# 

# Improve the Stylized Water Flow Baker authoring workflow so a typical water mesh can be configured with minimal manual setup.

# 

# Add an Editor-side automatic setup workflow capable of deriving as much useful configuration as reasonably possible from the current water mesh.

# 

# The intended workflow should become approximately:

# 

# 1\. Add StylizedWaterFlowBaker to a water mesh.

# 2\. Click Auto Setup From Mesh.

# 3\. Inspect the generated path.

# 4\. Reverse flow direction if necessary.

# 5\. Make optional artistic adjustments.

# 6\. Bake and assign.

# 

# The user should not normally need to manually calculate:

# 

# \- bake bounds;

# \- bake center;

# \- bake resolution;

# \- boundary points;

# \- initial centerline;

# \- basic steering scale;

# \- common target references.

# 

# This Spec is focused on authoring UX.

# 

# Do not change the validated visual behavior of SPEC-010 or SPEC-011.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated:

# 

# \- SPEC-010 — Flow Map Baker Tool

# \- SPEC-011 — Collider and Boundary-aware Flow Bake

# 

# Preserve:

# 

# \- centerline-based flow;

# \- Flow Map encoding;

# \- obstacle contour steering;

# \- generated flow-coordinate mesh;

# \- existing bake workflow;

# \- shader behavior;

# \- manual editing capability.

# 

# Automatic setup supplements manual authoring.

# 

# It must not remove it.

# 

# \---

# 

# \# Core Goal

# 

# Provide a primary Editor action:

# 

# Auto Setup From Mesh

# 

# The action should inspect the current water object and configure as many baker values as can be derived reliably.

# 

# Automatic setup should favor:

# 

# \- useful defaults;

# \- predictable results;

# \- easy correction;

# \- preservation of manual control.

# 

# Do not aim for perfect procedural river understanding.

# 

# A good editable starting point is preferable to an overly complex automatic solution.

# 

# \---

# 

# \# Quick Setup UI

# 

# Add a clearly separated section in the custom Inspector.

# 

# Suggested structure:

# 

# Quick Setup

# 

# \[ Auto Setup From Mesh ]

# 

# Optional setup controls / toggles

# 

# \[ Reverse Flow Direction ]

# 

# The exact UI may vary.

# 

# Keep the main workflow obvious.

# 

# Advanced/manual baker settings should remain available below.

# 

# \---

# 

# \# Auto Setup Scope

# 

# Auto Setup From Mesh should attempt to configure:

# 

# \- Target Renderer;

# \- Target Material;

# \- Output Name;

# \- Bake Center;

# \- Bake Size;

# \- Bake Resolution;

# \- water boundary;

# \- initial centerline / control points;

# \- Flow Coordinate settings where a reliable default can be estimated;

# \- Steering Distance where a useful geometric estimate is possible.

# 

# Do not overwrite unrelated material properties.

# 

# \---

# 

# \# Target Detection

# 

# Automatically detect the renderer associated with the water surface.

# 

# Prefer components on the same GameObject.

# 

# Configure:

# 

# Target Renderer

# 

# and, when appropriate:

# 

# Target Material

# 

# using the renderer's current shared material.

# 

# Do not instantiate or duplicate materials unnecessarily.

# 

# If the object does not contain a compatible renderer/mesh setup, report a useful error.

# 

# \---

# 

# \# Output Naming

# 

# Generate a predictable default output name.

# 

# Suggested format:

# 

# T\_FlowMap\_<WaterObjectName>

# 

# Sanitize invalid characters.

# 

# Do not overwrite unrelated generated assets with conflicting names.

# 

# Continue using the existing generated WaterShader folders.

# 

# \---

# 

# \# Mesh Source

# 

# Use the original water mesh as the geometric source for Auto Setup.

# 

# Do not modify the imported/source mesh.

# 

# Any mesh modifications required by the existing flow-coordinate or obstacle systems must continue to occur only on generated copies.

# 

# \---

# 

# \# Automatic Bake Bounds

# 

# Determine the water footprint from the mesh in the baker's local XZ space.

# 

# Calculate:

# 

# \- minimum X;

# \- maximum X;

# \- minimum Z;

# \- maximum Z.

# 

# Use these values to derive:

# 

# Bake Center

# 

# and:

# 

# Bake Size

# 

# Add a small configurable or internally sensible padding so edge samples are not clipped.

# 

# The automatic bounds should closely encompass the actual water surface.

# 

# Do not assume the mesh is centered at the object's origin.

# 

# \---

# 

# \# Automatic Resolution

# 

# Select a sensible bake resolution based on physical water size and desired spatial detail.

# 

# The result must use the baker's existing supported resolution levels.

# 

# For example:

# 

# 128

# 256

# 512

# 1024

# 

# Do not simply assign one fixed resolution to every water surface.

# 

# Prefer an approach based on approximate world/local cell size or another geometry-aware heuristic.

# 

# The selected value remains artist-editable after setup.

# 

# Avoid choosing 1024 unnecessarily.

# 

# \---

# 

# \# Boundary Extraction

# 

# Attempt to generate the valid water boundary directly from the mesh footprint.

# 

# A suitable approach may inspect:

# 

# \- mesh topology;

# \- boundary edges;

# \- projected XZ geometry;

# \- another robust footprint representation.

# 

# The implementation method is not prescribed.

# 

# The important result is an editable polygon representing the water surface.

# 

# When successful:

# 

# Boundary Mode should become:

# 

# Explicit Polygon

# 

# and Boundary Points should describe the water region.

# 

# \---

# 

# \# Boundary Loops

# 

# Meshes may contain:

# 

# \- one external boundary;

# \- holes;

# \- islands;

# \- disconnected pieces;

# \- unusual topology.

# 

# The system does not need to perfectly support every arbitrary mesh.

# 

# For the initial version, prioritize ordinary single-channel / single-surface water meshes.

# 

# If multiple ambiguous boundary loops are detected:

# 

# \- choose a safe behavior;

# \- report the ambiguity;

# \- do not silently produce a clearly incorrect boundary.

# 

# If useful, the largest outer loop may be selected as the primary boundary.

# 

# Document any limitations.

# 

# \---

# 

# \# Boundary Simplification

# 

# Do not expose hundreds of boundary points when the mesh contains dense topology.

# 

# Simplify the extracted boundary while preserving its significant shape.

# 

# Use enough points to represent:

# 

# \- bends;

# \- river width;

# \- basin shape;

# 

# without reproducing every triangle edge.

# 

# The resulting points must remain manually editable.

# 

# \---

# 

# \# Automatic Centerline

# 

# Attempt to generate an initial centerline from the water footprint.

# 

# The centerline should broadly follow the geometric middle of the water shape.

# 

# It should work particularly well for:

# 

# \- straight rivers;

# \- curved rivers;

# \- S-shaped channels;

# \- river sections widening toward a basin.

# 

# Do not simply use the longest world axis if that would ignore the actual channel shape.

# 

# \---

# 

# \# Centerline Generation Freedom

# 

# The implementation is free to choose an appropriate Editor-time approach.

# 

# Possible techniques include:

# 

# \- medial-axis approximation;

# \- distance-to-boundary ridge analysis;

# \- sampled channel centers;

# \- skeletonization;

# \- graph-based footprint analysis;

# \- another lightweight geometric approach.

# 

# These are suggestions, not requirements.

# 

# Choose the simplest robust method for this project.

# 

# The centerline output matters more than the exact algorithm.

# 

# \---

# 

# \# Centerline Quality

# 

# The generated path should:

# 

# \- remain inside the valid water boundary;

# \- approximately follow the visual center of the channel;

# \- avoid unnecessary oscillations;

# \- preserve major bends;

# \- provide a useful downstream path for SPEC-010/011;

# \- use a manageable number of control points.

# 

# After generation, simplify the path into editable Control Points.

# 

# Do not create hundreds of control points.

# 

# Prefer a compact representation that the existing continuous curve system can smooth.

# 

# \---

# 

# \# Path Simplification

# 

# Reduce the automatically generated path to a practical number of control points.

# 

# Preserve meaningful:

# 

# \- bends;

# \- entry direction;

# \- exit direction;

# \- broad structural changes.

# 

# Remove small geometric noise.

# 

# The exact simplification algorithm is not prescribed.

# 

# \---

# 

# \# Flow Direction Ambiguity

# 

# Mesh geometry may determine the centerline but not necessarily which end is upstream.

# 

# Auto Setup may choose either end using a reasonable deterministic rule.

# 

# The user must have an easy way to correct the direction.

# 

# Provide:

# 

# Reverse Flow Direction

# 

# This action should reverse the current centerline/control-point order without rebuilding the entire setup.

# 

# The result must immediately be visible in the Scene View path preview.

# 

# \---

# 

# \# Flow Coordinate Scale

# 

# Attempt to estimate useful initial Flow Coordinate World Scale values from the generated geometry.

# 

# Possible useful inputs include:

# 

# \- centerline length;

# \- average channel width;

# \- mesh dimensions.

# 

# This is an artistic parameter as well as a geometric one.

# 

# Therefore:

# 

# \- use conservative defaults;

# \- do not aggressively overwrite manually tuned values on every Auto Setup;

# \- keep the result editable.

# 

# If reliable automatic estimation is not possible, preserve the current validated default rather than inventing unstable values.

# 

# \---

# 

# \# Steering Distance

# 

# Estimate a useful initial Steering Distance based on the scale of the water geometry.

# 

# Possible inputs include:

# 

# \- average channel width;

# \- median distance to boundary;

# \- mesh scale.

# 

# This value remains artist-editable.

# 

# Do not attempt to automatically determine final Steering Strength unless a clearly reliable default exists.

# 

# Steering Strength may remain primarily artistic.

# 

# \---

# 

# \# Obstacle Setup

# 

# Do not automatically treat every collider in the scene as an obstacle.

# 

# Preserve the SPEC-011 filtering model.

# 

# Auto Setup may:

# 

# \- preserve an existing Obstacle LayerMask;

# \- optionally apply a known project default if explicitly configured.

# 

# Do not scan arbitrary scene colliders and populate them without user intent.

# 

# \---

# 

# \# Auto Setup Options

# 

# If useful, expose individual toggles such as:

# 

# \- Fit Bake Bounds

# \- Extract Boundary

# \- Generate Centerline

# \- Choose Resolution

# \- Estimate Flow Coordinates

# \- Estimate Steering Distance

# 

# The exact UI may vary.

# 

# A single default Auto Setup action should configure the recommended set.

# 

# Advanced users may disable individual operations.

# 

# \---

# 

# \# Preserve Manual Work

# 

# Auto Setup must not unexpectedly destroy deliberate manual authoring.

# 

# If the baker already contains a manually edited:

# 

# \- centerline;

# \- boundary;

# \- flow scale;

# 

# the tool should either:

# 

# \- clearly indicate that Auto Setup will replace those values;

# \- provide per-feature toggles;

# \- or use Undo so the entire operation can be reverted.

# 

# Unity Undo support is required for authoring changes.

# 

# \---

# 

# \# Re-run Behavior

# 

# Auto Setup should be safe to run again after the water mesh changes.

# 

# Re-running may recalculate:

# 

# \- bounds;

# \- boundary;

# \- centerline;

# \- resolution;

# \- estimated values.

# 

# Do not create uncontrolled duplicate assets.

# 

# Auto Setup itself should not need to Bake unless explicitly designed as an optional combined action.

# 

# Keep:

# 

# Auto Setup

# 

# and:

# 

# Bake And Assign Flow Map

# 

# conceptually separate.

# 

# \---

# 

# \# Scene View Feedback

# 

# After Auto Setup, the Scene View should immediately show:

# 

# \- bake bounds;

# \- extracted boundary;

# \- centerline;

# \- control points;

# \- direction arrows.

# 

# The user should be able to visually answer:

# 

# "Did the tool understand this water mesh correctly?"

# 

# without first entering Play Mode.

# 

# \---

# 

# \# Validation Case A — Straight River

# 

# Use a simple straight water mesh.

# 

# Auto Setup should produce:

# 

# \- fitted bounds;

# \- correct boundary;

# \- centerline through the channel;

# \- sensible resolution;

# \- valid downstream path.

# 

# Reverse Flow Direction must work.

# 

# \---

# 

# \# Validation Case B — Curved River

# 

# Use the current curved technical river.

# 

# Auto Setup should generate a centerline that follows the major S-shaped bend.

# 

# It must not collapse to a straight line between the mesh extremes.

# 

# Validate:

# 

# \- boundary;

# \- centerline;

# \- bounds;

# \- generated flow after Bake.

# 

# The final flow should remain visually comparable to the manually configured SPEC-011 setup.

# 

# \---

# 

# \# Validation Case C — River With Obstacle

# 

# Use the validated cylinder-obstacle setup.

# 

# Auto Setup should configure the underlying water geometry without breaking obstacle steering.

# 

# After Bake:

# 

# \- the river path should remain valid;

# \- the obstacle should still be avoided;

# \- the generated pattern should remain coherent.

# 

# The auto-generated centerline does not need to route around the obstacle itself.

# 

# SPEC-011 obstacle steering remains responsible for local obstacle avoidance.

# 

# \---

# 

# \# Validation Case D — Wider Water / Basin

# 

# Use a mesh containing a narrow section opening into a wider region.

# 

# Auto Setup should still generate:

# 

# \- valid bounds;

# \- usable boundary;

# \- a reasonable centerline.

# 

# Do not implement automatic Flow Strength changes yet.

# 

# That belongs to SPEC-012.

# 

# \---

# 

# \# Difficult / Unsupported Geometry

# 

# Detect or warn when the geometry is outside the reliable scope of automatic setup.

# 

# Examples:

# 

# \- multiple disconnected water surfaces;

# \- strong river branching;

# \- highly non-manifold water meshes;

# \- ambiguous centerline topology;

# \- insufficient mesh data.

# 

# Do not silently create nonsense.

# 

# When automatic centerline generation is unreliable:

# 

# preserve all other successful Auto Setup results and request manual path editing.

# 

# \---

# 

# \# Branching Water

# 

# A single centerline is not sufficient to fully represent a branching river network.

# 

# If the footprint contains meaningful branches:

# 

# \- detect this where practical;

# \- warn the user;

# \- avoid pretending a single automatically generated path represents all branches correctly.

# 

# Do not implement multi-branch flow architecture in this Spec.

# 

# \---

# 

# \# Diagnostics

# 

# After Auto Setup, provide a short summary.

# 

# Example:

# 

# Auto Setup:

# Renderer: found

# Bounds: fitted

# Boundary: 18 points

# Centerline: 7 control points

# Resolution: 256

# Flow Scale: estimated

# Steering Distance: estimated

# 

# Warnings:

# Centerline direction is inferred. Use Reverse Flow Direction if necessary.

# 

# Exact wording may vary.

# 

# \---

# 

# \# Performance

# 

# All analysis occurs in the Editor.

# 

# Auto Setup may perform moderate geometry processing.

# 

# It does not need to run every frame.

# 

# Prioritize:

# 

# 1\. useful output;

# 2\. predictable behavior;

# 3\. reasonable Editor iteration time.

# 

# Avoid heavyweight processing when a simpler solution provides a good authoring starting point.

# 

# \---

# 

# \# External Assets

# 

# No external textures or art assets are required.

# 

# No manually authored Flow Map is required.

# 

# The system derives setup information from the existing water mesh and scene configuration.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-011.5 is complete when:

# 

# \- Auto Setup From Mesh exists;

# \- Renderer and Material can be configured automatically;

# \- Bake bounds fit the water mesh;

# \- an appropriate bake resolution is selected automatically;

# \- the water boundary can be generated from a typical water mesh;

# \- a useful initial centerline can be generated for straight and curved non-branching channels;

# \- generated paths use a manageable number of editable points;

# \- Reverse Flow Direction works;

# \- automatic estimates remain manually editable;

# \- Auto Setup can be undone;

# \- rerunning setup does not create uncontrolled assets;

# \- Scene View clearly previews the generated setup;

# \- existing SPEC-010/011 flow behavior remains functional;

# \- unsupported/ambiguous geometry produces useful warnings rather than silent failure.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- automatic multi-branch river flow;

# \- runtime mesh analysis;

# \- runtime Auto Setup;

# \- runtime rebaking;

# \- automatic river-to-lake Flow Strength;

# \- SPEC-012 behavior;

# \- Flow Strength override zones;

# \- terrain carving;

# \- mesh generation;

# \- fluid simulation;

# \- foam;

# \- waves;

# \- normals;

# \- reflection;

# \- refraction;

# \- VFX;

# \- interaction.

