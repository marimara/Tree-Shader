# AGENTS.md — Stylized Water Shader

## Scope

This file governs all work inside:

Assets/WaterShader/

The WaterShader module is an isolated stylized water system developed for the current Unity URP project.

Do not modify unrelated systems, packages, scenes, shaders, materials, project settings, vegetation assets, grass assets, or files outside Assets/WaterShader unless a Spec explicitly requires it.

---

# Project Goal

Create a stylized water system capable of representing the same visual language across:

- waterfalls;
- fast rivers;
- slow rivers;
- transitions from flowing water into calm water;
- lakes and other mostly still water.

The core visual goal is not physical realism.

The shader should look graphic, painterly, readable and stylized.

Primary visual inspiration is stored in:

Assets/WaterShader/References/

Read:

Assets/WaterShader/References/REF_Links.md

before making major visual decisions.

---

# Core Design Principle

Flow must influence both:

1. animation;
2. appearance.

Water must not simply use the same texture at different speeds.

As flow becomes stronger, the surface should progressively support:

- faster movement;
- stronger directional stretching;
- more visible directional patterns;
- greater turbulence;
- eventually stronger foam and waterfall effects.

As flow approaches zero, the water should progressively become:

- slower;
- broader in pattern;
- less directional;
- less turbulent;
- visually calmer.

The intended progression is:

Waterfall -> Rapids -> River -> Calm Lake

These states should belong to the same visual system.

---

# Shader Technology

The main water shader must be implemented as a handwritten URP shader.

Do not use Shader Graph for the main water implementation.

Primary shader:

Assets/WaterShader/Shaders/StylizedWater.shader

Reusable shader logic may gradually be extracted into:

Assets/WaterShader/Shaders/Includes/

Use .hlsl include files only when the amount of logic justifies separating it.

Do not prematurely create many empty or unnecessary include files.

---

# Shader Architecture

The main shader should remain readable.

Long reusable systems should eventually be separated by responsibility.

Likely future modules include:

- WaterCommon.hlsl
- WaterDepth.hlsl
- WaterFlow.hlsl
- WaterPatterns.hlsl
- WaterFoam.hlsl
- WaterNormals.hlsl
- WaterWaves.hlsl
- WaterLighting.hlsl

These names are architectural guidance, not mandatory files to create immediately.

---

# Development Method

Development is Spec-driven.

Each Spec must:

1. introduce a small, clearly defined capability;
2. preserve previous validated behavior;
3. include explicit validation criteria;
4. avoid implementing unrelated future features;
5. keep the project compiling;
6. leave the technical test scene usable.

Do not jump ahead to later Specs.

If a future feature would require large structural changes, document that need instead of silently implementing it early.

---

# Technical Test Scene

All primary development and validation must happen in:

Assets/WaterShader/Scenes/WaterShader_TestScene.unity

Do not use the project's main environment scene as the primary development scene.

The test scene should remain lightweight and technical.

Its purpose is to make shader behavior easy to inspect.

The test scene may eventually contain:

- Lake_Test;
- River_Test;
- Waterfall_Test;
- transition meshes;
- test rocks;
- ground;
- lighting;
- camera;
- debug objects.

Do not decorate the scene beyond what is needed for validation.

---

# References

Files inside:

Assets/WaterShader/References/

are read-only references.

Do not:

- rename them;
- modify them;
- reimport them with destructive changes;
- move them;
- overwrite them.

The commercial/reference shader must not be copied or reverse-engineered.

Use references only to understand:

- visual language;
- motion;
- shape language;
- foam distribution;
- color relationships;
- transition between moving and calm water.

---

# Materials

Water materials belong in:

Assets/WaterShader/Materials/

Use clear names.

Examples:

MAT_Water_Test
MAT_Water_Lake
MAT_Water_River
MAT_Water_Waterfall

Avoid creating duplicate materials without a clear purpose.

---

# Textures

Water textures belong in:

Assets/WaterShader/Textures/

Create subfolders only when needed.

Expected future categories include:

- Flow
- Foam
- Normals
- Masks
- Gradients

Do not import random texture assets without documenting their purpose.

Prefer procedural shader logic where practical, but do not force procedural solutions when a small authored texture produces a significantly better stylized result.

---

# Meshes

Water-specific test meshes belong in:

Assets/WaterShader/Meshes/

Do not modify unrelated imported environment meshes.

For early Specs, simple Unity primitives or simple generated meshes are preferred.

---

# Validation

Validation output belongs in:

Assets/WaterShader/Validation/

When useful, organize validation by Spec:

Validation/
    SPEC-001/
    SPEC-002/
    ...

Validation may contain:

- screenshots;
- comparison images;
- short videos;
- debug captures.

Do not delete validation from previous Specs unless explicitly requested.

---

# Flow System

The long-term water system is expected to support:

- flow direction;
- flow speed / flow strength;
- painterly directional patterns;
- transitions between fast and calm water;
- eventually flow maps.

Do not assume that UV direction alone will be sufficient for the final river system.

However, early Specs may use a simple uniform flow direction before Flow Maps are introduced.

---

# Flow Map Direction

The intended future Flow Map convention is:

R = Flow X
G = Flow Y
B = Flow Strength
A = optional turbulence / foam data

This convention may be revised by a future Spec if technical testing shows a better format.

Do not implement the full Flow Map system before the relevant Spec.

---

# Visual Style

Target visual characteristics:

- saturated cyan / turquoise water;
- readable large shapes;
- painterly or brush-like streaks;
- strong directional motion in flowing water;
- smooth broad shapes in calm water;
- graphic highlights;
- stylized white foam;
- minimal dependence on physically realistic water behavior.

Avoid:

- noisy realistic ocean water;
- excessive micro-detail;
- physically accurate simulation for its own sake;
- overly glossy transparent glass-like water;
- generic realistic PBR water appearance.

---

# Performance

The shader should remain practical for a real-time Unity game.

Prefer:

- reusable calculations;
- minimal unnecessary texture samples;
- simple configurable features;
- predictable shader variants.

Do not optimize prematurely if doing so would obscure correctness.

First make each system correct and visually understandable, then optimize it in a later Spec.

---

# URP Compatibility

The shader must follow the URP setup already used by the project.

Do not modify:

- render pipeline assets;
- renderer configuration;
- global project graphics settings;
- package versions;

unless a Spec explicitly requires it.

If a desired feature requires a project-level URP setting, stop and report the requirement before changing it.

---

# Editing Existing Files

Before replacing existing shader behavior:

1. inspect the current implementation;
2. preserve validated features;
3. identify which Spec introduced them;
4. make the smallest reasonable change.

Do not rewrite the entire shader simply because a new feature is being added.

---

# Debugging

Temporary debug modes are encouraged when they make shader data easier to understand.

Useful future debug outputs may include:

- flow direction;
- flow strength;
- depth;
- painterly pattern;
- foam mask;
- normals.

Debug functionality should be removable or disabled in normal material use.

---

# Failure Handling

If Unity, MCP, compilation, scene loading, or another required development tool becomes unavailable:

- stop;
- report what succeeded;
- report what remains unvalidated;
- do not attempt unrelated infrastructure repair unless requested.

Never claim visual validation when the scene could not actually be inspected.

---

# Completion Standard

A Spec is complete only when:

- requested files exist;
- Unity compiles without new shader/script errors;
- previous validated features still work;
- current acceptance criteria are met;
- the WaterShader test scene remains usable;
- no unrelated files were modified.