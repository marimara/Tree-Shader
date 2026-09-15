# AGENTS.md

## Project Context

This Unity project contains a stylized vegetation shader development task focused on tree foliage built with foliage cards and alpha clipping.

The shader should target a highly stylized visual result with:

- soft, readable light and shadow masses
- rounded canopy volume
- spherical/radial lighting behaviour
- saturated artistic shadow colors
- foliage-card support
- alpha clipping
- two-sided rendering
- controllable color palette
- subtle normal/detail variation
- canopy interior darkening
- optional wind in a later milestone

The implementation should prioritize artistic control and readability over physically accurate vegetation rendering.

---

# Source of Truth

For the Stylized Tree Shader task, the source of truth is:

`Assets/TreeShader/Specs/`

The specifications must be read in numerical order.

Current spec order:

1. `001-project-goal.md`
2. `002-test-scene.md`
3. `003-foliage-foundation.md`
4. `004-radial-normals.md`
5. `005-stylized-lighting.md`
6. `006-color-system.md`
7. `007-canopy-depth.md`
8. `008-detail-variation.md`
9. `009-normal-map-integration.md`
10. `010-distance-behaviour.md`
11. `011-wind.md`
12. `012-validation.md`

Do not skip ahead.

Do not implement features from later specs unless the current spec explicitly requires them.

If a later feature would make the current implementation cleaner, document the idea but do not implement it yet.

---

# Visual References

Visual references are stored in:

`Assets/TreeShader/References/`

Use them as artistic guidance.

The references have different purposes:

## Technical Reference

The technical reference demonstrates ideas such as:

- radial/spherical normals
- fake Lambert lighting
- foliage-card shading
- stylized sun response
- custom color mixing
- fake canopy density
- AO-like interior shading
- normal variation
- distance-based detail control

Use these ideas as inspiration for implementation.

Do not attempt to reproduce the reference shader line-for-line.

## Target Art Direction

The target art reference defines the desired visual result.

Prioritize:

- soft light transitions
- bright exposed foliage
- saturated green midtones
- cool green/teal shadows
- darker canopy interiors
- rounded foliage volumes
- readable large-scale lighting
- stylized rather than realistic appearance

Do not copy the reference colors literally.

Material properties should allow the artist to recreate or alter the palette.

---

# Unity MCP Usage

Use the Unity MCP whenever interaction with the Unity project is required.

The MCP should be used to:

- inspect the current project hierarchy
- inspect existing assets
- inspect imported models
- inspect materials
- inspect shaders
- inspect render pipeline configuration
- inspect scene contents
- create and edit project assets when supported
- open or inspect the shader test scene
- apply test materials
- inspect the Unity Console
- verify shader compilation
- verify the visual result
- save modified assets and scenes

Do not assume the Unity project state.

Inspect it before making changes.

---

# Before Starting Any Spec

Before implementing a specification:

1. Read this `AGENTS.md`.
2. Read `001-project-goal.md`.
3. Read the current specification.
4. Read any earlier specification required to understand existing systems.
5. Inspect the current Unity project through MCP.
6. Inspect existing TreeShader assets before creating new ones.
7. Inspect the Console before starting.
8. Confirm that the previous milestone is still functional.

Do not create duplicate shaders, materials, folders, scenes or scripts when an appropriate asset already exists.

---

# Development Strategy

Work incrementally.

Prefer small, reversible changes.

Do not attempt to implement the complete stylized foliage shader in a single step.

Each specification represents a milestone.

Preserve already validated behaviour when implementing new systems.

Examples:

- Spec 004 must not break alpha clipping from Spec 003.
- Spec 005 must not break radial normals from Spec 004.
- Spec 009 must not destroy large-scale canopy shading established by Specs 004 and 005.
- Spec 011 must not break lighting, alpha, shadows or silhouette.

Do not rewrite working shader systems without a clear technical reason.

---

# Source Asset Safety

Imported source tree assets must be treated as read-only reference assets whenever possible.

Do not destructively modify the original imported tree files.

Prefer:

- material overrides
- prefab instances
- duplicated test assets
- new materials
- new shaders
- separate test scenes
- non-destructive import settings

If the source model requires structural changes, report the issue before modifying it.

Do not silently edit the original mesh.

---

# Test Assets

The shader development should use the dedicated TreeShader test setup.

Expected locations:

`Assets/TreeShader/Test/`

`Assets/TreeShader/Materials/`

`Assets/TreeShader/Shaders/`

The main test scene should be:

`Assets/TreeShader/Test/TreeShader_TestScene.unity`

If the scene does not exist yet, create it according to `002-test-scene.md`.

Use the imported tree asset intended for testing, preferably `tree1` unless project state indicates a better existing choice.

When available, also use a foliage-only mesh such as `leaves1`.

---

# Shader Design Rules

The shader must be designed primarily for foliage cards.

It should support:

- alpha clipping
- two-sided foliage
- alpha-clipped shadow casting
- URP Main Light
- realtime shadows
- stylized lighting
- radial/spherical normal behaviour
- artist-controlled color transitions
- optional normal-map detail
- optional variation
- optional wind

Avoid relying on realistic PBR behaviour as the primary visual model.

Avoid:

- black shadow multiplication
- excessive specular highlights
- physically accurate subsurface scattering unless explicitly required later
- per-leaf lighting noise dominating the canopy
- unnecessary shader complexity
- unnecessary texture samples
- unnecessary branching
- hidden hard-coded artistic constants

Important artistic controls should be exposed through the material.

Internal implementation details do not need to be exposed.

---

# Lighting Rules

The Main Directional Light should drive the large-scale foliage lighting.

Do not fake all lighting independently of the scene unless the current spec explicitly requires it.

Stylized lighting should use the scene light as input while allowing artistic remapping.

Large-scale canopy shading is more important than physically accurate leaf response.

The intended hierarchy is:

1. canopy volume
2. light/shadow mass
3. artistic color
4. foliage detail

Do not allow small-scale leaf detail to overpower the first three.

---

# Radial / Spherical Normals

When implementing stylized normals:

- treat the foliage canopy as a larger rounded volume
- avoid exposing individual foliage-card orientation
- keep calculations stable under object translation and rotation
- prefer object-space calculations when practical
- document limitations involving non-uniform scale if they exist
- expose artistic control over the blend between mesh normals and stylized normals

The goal is visual coherence, not mathematical purity.

---

# Material Property Rules

Property names should be clear and artist-friendly.

Prefer names such as:

- `_BaseMap`
- `_AlphaMap`
- `_BaseColor`
- `_AlphaClipThreshold`
- `_StylizedNormalStrength`
- `_CanopyCenterOffset`
- `_ShadowThreshold`
- `_ShadowSoftness`
- `_ShadowStrength`
- `_LightColor`
- `_MidColor`
- `_ShadowColor`
- `_DeepShadowColor`
- `_InteriorStrength`
- `_AOStrength`
- `_NormalMap`
- `_NormalStrength`
- `_NormalNoiseStrength`
- `_ColorVariationStrength`
- `_WindStrength`

Do not expose redundant properties.

Remove obsolete properties when appropriate.

Do not rename established public properties casually once materials depend on them.

---

# Validation Rules

A specification is not complete merely because code was written.

A specification is not complete merely because the shader compiles.

Every milestone must be validated in Unity.

After implementing a spec:

1. Save all modified assets.
2. Save modified scenes.
3. Allow Unity to import and compile.
4. Inspect the Unity Console.
5. Fix shader compilation errors.
6. Fix material/property errors.
7. Open or inspect `TreeShader_TestScene`.
8. Apply the current test material.
9. Visually inspect the feature.
10. Verify previously completed features still work.
11. Report the result.

Do not declare completion before visual validation.

---

# Console Requirements

At the end of a milestone, there should be no new errors caused by the TreeShader work.

Specifically verify:

- zero shader compilation errors
- zero missing material property errors
- zero broken shader references
- zero missing texture references caused by the implementation
- zero exceptions introduced by TreeShader tooling or test setup

Warnings should be reviewed.

Do not ignore recurring warnings without explaining them.

---

# Visual Validation

Visual validation must use the test scene.

When relevant, test the tree under multiple sun directions.

At minimum inspect:

- front lighting
- side lighting
- opposite side lighting
- higher sun angle
- lower sun angle

Check that:

- foliage cards remain visually hidden as individual planes
- the canopy reads as a coherent volume
- alpha clipping remains clean
- backfaces remain visible when required
- cast shadows use the foliage alpha
- shadows remain colored rather than black
- the tree does not unexpectedly darken
- lighting transitions remain stable
- materials remain editable
- silhouette remains readable

---

# Comparison Testing

When adding an adjustable feature, compare meaningful extremes.

Examples:

For stylized normals:

`_StylizedNormalStrength = 0`

versus:

`_StylizedNormalStrength = 1`

For normal maps:

`_NormalStrength = 0`

versus a visible but reasonable value.

For canopy interior darkening:

`_InteriorStrength = 0`

versus the configured default.

For wind:

`_WindStrength = 0`

versus the intended default.

Use comparisons to verify that each control is actually working.

---

# Performance Guidelines

Keep the shader appropriate for realtime vegetation.

Prefer:

- simple vector math
- smoothstep/remap operations
- inexpensive procedural masks
- minimal texture samples
- vertex-stage work where appropriate

Avoid:

- loops in fragment shading
- expensive ray-based thickness calculations
- unnecessarily complex noise
- multiple redundant normal conversions
- excessive dynamic branching
- features with no visible artistic benefit

Do not prematurely optimize at the cost of correctness, but avoid obviously wasteful implementation.

---

# Spec Completion Report

When completing a spec, report:

## Implemented

Briefly describe what was added.

## Assets Modified

List relevant asset paths.

## Material Properties Added

List new exposed properties.

## Validation

Report:

- compilation status
- Console status
- scene tested
- object tested
- visual checks performed

## Known Limitations

List any current limitation.

## Next Spec

State which specification should be implemented next.

Do not begin the next spec unless explicitly requested.

---

# Failure Handling

If the requested feature cannot be implemented correctly because of:

- mesh structure
- missing UV data
- missing vertex data
- inappropriate pivots
- incorrect source textures
- render pipeline limitations
- MCP limitations
- Unity version differences

stop and report the actual constraint.

Do not silently replace the requested approach with a substantially different one.

Propose the smallest practical solution.

---

# Scope Control

Do not add unrelated features.

Do not add:

- seasons
- snow
- rain
- interaction bending
- complex translucency
- GPU instancing systems
- LOD generation
- SpeedTree integration
- terrain integration
- global wind managers
- custom editors

unless a specification explicitly requests them.

Keep the implementation focused on the current milestone.

---

# Current Workflow

The intended implementation sequence is:

## Phase 1 — Foundation

`001-project-goal.md`

`002-test-scene.md`

`003-foliage-foundation.md`

Goal:

Correct foliage cards, alpha clipping, two-sided rendering and leaf-shaped shadows.

## Phase 2 — Canopy Form

`004-radial-normals.md`

Goal:

Make the foliage read as rounded canopy volumes instead of independent planes.

## Phase 3 — Main Art Direction

`005-stylized-lighting.md`

`006-color-system.md`

Goal:

Establish the main stylized lighting and artistic color palette.

This is the first major visual target.

## Phase 4 — Depth and Variation

`007-canopy-depth.md`

`008-detail-variation.md`

Goal:

Add interior depth and controlled natural variation without losing the main canopy shape.

## Phase 5 — Fine Surface Detail

`009-normal-map-integration.md`

Goal:

Restore small leaf-surface detail while preserving stylized canopy lighting.

## Phase 6 — Distance Refinement

`010-distance-behaviour.md`

Goal:

Reduce unnecessary fine detail at distance and maintain visual stability.

## Phase 7 — Animation

`011-wind.md`

Goal:

Add subtle foliage movement after the static appearance is validated.

## Phase 8 — Final Validation

`012-validation.md`

Goal:

Verify the complete system across lighting angles, assets and material settings.

---

# Important Rule

Never treat "shader compiles" as equivalent to "task complete".

For this project:

visual correctness in Unity is part of the specification.