# SPEC-011.5 — Automatic Mesh Setup and Authoring UX

## Objective

Improve the Stylized Water Flow Baker authoring workflow so a typical water mesh can be configured with minimal manual setup.

Add an Editor-side automatic setup workflow capable of deriving as much useful configuration as reasonably possible from:

- the source water mesh;
- the object's actual Transform;
- the existing validated baker architecture.

The intended workflow should become approximately:

1. Add StylizedWaterFlowBaker to a water mesh.
2. Click Auto Setup From Mesh.
3. Inspect the generated bounds, boundary and path.
4. Reverse flow direction if necessary.
5. Make optional artistic adjustments.
6. Bake and assign.
7. The generated material/mesh configuration should immediately use the correct flow-coordinate path.

The user should not normally need to manually calculate:

- bake bounds;
- bake center;
- bake resolution;
- boundary points;
- initial centerline;
- basic steering scale;
- initial flow-coordinate scale;
- common target references;
- required flow-coordinate material state.

This Spec is focused on authoring UX.

Do not change the validated visual behavior of SPEC-010 or SPEC-011.

---

# Dependencies

Requires completed and validated:

- SPEC-010 — Flow Map Baker Tool
- SPEC-011 — Collider and Boundary-aware Flow Bake

Preserve:

- centerline-based flow;
- Flow Map encoding;
- obstacle contour steering;
- generated flow-coordinate mesh;
- existing bake workflow;
- shader behavior;
- manual editing capability.

Automatic setup supplements manual authoring.

It must not remove it.

---

# Core Principle — Geometry Space vs Physical Space

The tool must distinguish between:

## Local authoring data

Data that belongs to the baker's local coordinate system may remain in local space:

- Bake Center;
- Bake Size;
- Boundary Points;
- Centerline Control Points;
- Flow Map UV mapping.

## Physical / metric-derived settings

Any value intended to represent actual spatial size, visual scale or distance must account for the object's Transform.

Examples include:

- physical channel length;
- physical channel width;
- Flow Coordinate World Scale;
- Steering Distance;
- bake-resolution heuristics.

Do not assume:

Transform Scale = (1, 1, 1)

Do not assume uniform X/Z scaling.

A water mesh scaled differently on X and Z must still receive sensible automatic settings.

---

# Transform Awareness

Auto Setup must inspect the current object Transform.

At minimum account for:

- local/world scale;
- non-uniform X/Z scale;
- rotation where relevant to world-space measurements.

Use appropriate Transform conversion rather than multiplying unrelated scalar values blindly.

Possible approaches include:

- TransformPoint;
- TransformVector;
- TransformDirection;
- transformed sample distances;
- another equivalent robust method.

The exact implementation is not prescribed.

The important requirement is:

two visually identical water surfaces should receive comparable physical setup values even if one was modeled at final size and the other reached the same size using Transform scale.

---

# Quick Setup UI

Provide a clearly separated Editor section.

Suggested workflow:

Quick Setup

[ Auto Setup From Mesh ]

optional per-feature setup toggles

[ Reverse Flow Direction ]

Advanced/manual settings remain available.

After Auto Setup, provide a summary of inferred values and relevant warnings.

---

# Auto Setup Scope

Auto Setup From Mesh should attempt to configure:

- Target Renderer;
- Target Material;
- Output Name;
- Source Mesh;
- Bake Center;
- Bake Size;
- Bake Resolution;
- Water Boundary;
- Initial Centerline;
- Flow Coordinate settings;
- Steering Distance;
- required material flow-coordinate state when applicable.

Do not overwrite unrelated material properties.

---

# Target Detection

Automatically detect the water Renderer and source Mesh.

Prefer components on the same GameObject.

Configure:

- Target Renderer;
- Target Material;
- Source Mesh.

Do not modify the imported/source mesh.

Generated flow-coordinate meshes must remain separate generated assets.

---

# Automatic Bake Bounds

Determine the footprint from the source mesh in baker local XZ space.

Derive:

- Bake Center;
- Bake Size.

These values are intentionally local because they define the Flow Map domain.

Add a small sensible padding.

Do not convert Bake Center / Bake Size into arbitrary world coordinates if the rest of the baker expects local coordinates.

Transform awareness for other derived metrics must not break this local-space contract.

---

# Automatic Resolution

Choose a sensible bake resolution based on the actual spatial size and required detail of the water surface.

The implementation may consider:

- transformed physical size;
- desired approximate cell size;
- obstacle/boundary detail;
- another appropriate metric.

Do not simply select resolution from the unscaled source mesh dimensions.

The exact thresholds are implementation decisions.

Avoid unnecessarily high resolution.

The selected result remains manually editable.

---

# Boundary Extraction

Extract a usable water footprint from the source mesh.

Possible methods include:

- mesh boundary edges;
- projected topology;
- another robust footprint representation.

Keep the resulting Boundary Points in baker local space.

Simplify dense boundaries while preserving meaningful shape.

Handle ambiguous loops conservatively and emit warnings rather than silently generating invalid data.

---

# Automatic Centerline

Generate an initial centerline that broadly follows the middle of the water footprint.

Support typical:

- straight rivers;
- curved rivers;
- S-shaped channels;
- widening channels.

The centerline remains stored in baker local space.

Its geometric generation may use local-space topology because uniform coordinate representation is useful for authoring.

However, when evaluating:

- physical path length;
- physical widths;
- downstream metric distances;

use Transform-aware measurements.

Do not use a simple longest-axis line when it ignores the actual channel shape.

The exact centerline-generation algorithm is not prescribed.

---

# Flow Direction Ambiguity

Geometry may determine the path but not upstream/downstream.

Use a deterministic default.

Provide:

Reverse Flow Direction

This reverses the existing Control Points without rebuilding the entire setup.

---

# Flow Coordinate World Scale

This setting must reflect the actual physical proportions of the water surface.

Do not derive it exclusively from unscaled local mesh dimensions.

Automatic estimation should consider transformed measurements such as:

- physical centerline length;
- physical average channel width;
- effective X/Z scale;
- other reliable physical metrics.

Non-uniform object scale must not cause the resulting pattern to become unintentionally:

- extremely thin;
- extremely wide;
- compressed;
- stretched;
- visually inconsistent with an equivalent Scale = 1 water mesh.

The exact formula is not prescribed.

Treat this as a geometry-informed artistic default rather than a physically exact value.

Preserve manual editability.

Do not aggressively overwrite a deliberately tuned value unless the user explicitly reruns the relevant Auto Setup option.

---

# Steering Distance

Estimate Steering Distance using actual physical water dimensions.

Do not derive it only from unscaled local width.

Possible inputs include:

- transformed average channel width;
- physical distance to boundaries;
- actual world-space obstacle/channel proportions.

The exact mapping is not prescribed.

Steering Strength remains primarily artistic unless a reliable automatic default is found.

---

# Material / Flow Coordinate Integration

If:

- Generate Compatible Flow Coordinates is enabled;
- a compatible generated flow-coordinate mesh is produced;
- the target shader exposes the expected channel-coordinate option;

Bake And Assign should ensure the material is configured to use the generated channel coordinates.

The user should not need to manually discover that:

Use Channel Coordinates

must be enabled after a successful compatible bake.

Do not enable unsupported shader properties blindly.

Validate property existence before changing material state.

Preserve the uniform-flow fallback.

---

# Obstacles

Do not automatically treat arbitrary scene colliders as water obstacles.

Preserve the SPEC-011 filtering system.

Auto Setup may preserve:

- Obstacle LayerMask;
- Explicit Obstacles.

If boundary/obstacle processing is enabled but no obstacle source is configured, provide a clear informational warning rather than implying obstacle avoidance is fully configured.

---

# Preserve Manual Work

Auto Setup must not silently destroy intentional manual authoring.

Use:

- per-feature toggles;
- clear replacement behavior;
- Unity Undo.

This especially applies to:

- centerline;
- boundary;
- Flow Coordinate Scale;
- Steering Distance.

---

# Re-run Behavior

Auto Setup must remain safe to run again after:

- editing the source mesh;
- changing object scale;
- changing object rotation;
- changing proportions.

Re-running relevant automatic options should respond to the new Transform.

Do not create uncontrolled duplicate assets.

Keep:

Auto Setup

and:

Bake And Assign

conceptually separate.

---

# Validation — Transform Equivalence

Add a dedicated validation for Transform awareness.

Create two visually equivalent water surfaces:

## Case A

Mesh modeled at approximately final dimensions.

Transform Scale:

(1, 1, 1)

## Case B

Same or equivalent source mesh using a substantially non-uniform Transform scale.

Example conceptually:

X much larger than Z.

Run Auto Setup on both.

They do not need identical serialized numbers because their local coordinate systems differ.

They should, however, produce comparable visual results after Bake.

Compare:

- pattern thickness;
- pattern density;
- apparent longitudinal movement;
- obstacle steering reach;
- bake detail.

The non-uniformly scaled object must not produce noticeably thinner or compressed streaks simply because its source mesh was authored at another size.

---

# Validation — Scale Change

On an already configured test surface:

1. run Auto Setup;
2. record generated settings;
3. change X/Z Transform scale substantially;
4. run Auto Setup again.

Verify that Transform-dependent estimates respond appropriately.

Local authoring data should remain internally coherent.

Physical/visual settings should adapt.

---

# Validation — Channel Coordinate Integration

Perform Auto Setup followed by Bake And Assign on a fresh supported water mesh.

Without manually changing the material afterward:

- Flow Map should be active;
- generated channel coordinates should be used when available;
- animation should move continuously;
- it should not fall back unexpectedly to the compatibility Flow Map path.

---

# Existing Validation Cases

Continue validating:

- straight river;
- curved/S-shaped river;
- river with obstacle;
- widening water/basin.

Also verify that the validated SPEC-011 obstacle behavior does not regress.

---

# Difficult / Unsupported Geometry

Warn rather than guess when encountering:

- disconnected surfaces;
- meaningful branching;
- highly non-manifold topology;
- ambiguous centerline;
- insufficient geometry.

Automatic setup should provide a good editable starting point, not solve arbitrary river topology.

---

# Diagnostics

After Auto Setup, report useful inferred information.

Suggested information:

- source mesh;
- Transform scale;
- whether non-uniform scale was detected;
- local bake bounds;
- estimated physical dimensions;
- boundary point count;
- centerline point count;
- chosen resolution;
- Flow Coordinate Scale;
- Steering Distance;
- whether channel-coordinate material mode will be used.

Warnings should clearly identify any setting left for manual configuration.

---

# Acceptance Criteria

SPEC-011.5 is complete when:

- Auto Setup From Mesh exists;
- target references are detected;
- source mesh is preserved;
- local Bake Bounds fit the mesh;
- Boundary and Centerline are generated for supported geometry;
- Reverse Flow Direction works;
- resolution considers actual spatial size;
- Flow Coordinate World Scale behaves sensibly under non-uniform Transform scale;
- Steering Distance behaves sensibly under non-uniform Transform scale;
- equivalent world-space water surfaces produce broadly comparable visual pattern scale regardless of source-mesh Transform scale;
- Bake And Assign automatically enables compatible channel-coordinate usage when appropriate;
- Transform changes can be handled by rerunning Auto Setup;
- artist-tuned settings remain editable;
- Undo works;
- obstacle steering from SPEC-011 remains functional;
- unsupported geometry produces useful warnings instead of silent failure.

---

# Out of Scope

Do not implement:

- automatic multi-branch river flow;
- runtime mesh analysis;
- runtime Auto Setup;
- runtime rebaking;
- automatic river-to-lake Flow Strength;
- SPEC-012 behavior;
- Flow Strength override zones;
- terrain carving;
- fluid simulation;
- foam;
- waves;
- normals;
- reflection;
- refraction;
- VFX;
- interaction.