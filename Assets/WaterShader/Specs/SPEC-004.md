# \# SPEC-004 — Uniform Directional Water Flow

# 

# \## Objective

# 

# Introduce the first animated water-flow system.

# 

# This Spec establishes a stable directional UV animation system that later Specs can reuse for stylized patterns and Flow Maps.

# 

# The entire test surface may use one uniform flow direction.

# 

# This Spec is technical infrastructure, not final visual styling.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated:

# 

# \- SPEC-001

# \- SPEC-002

# \- SPEC-003

# 

# Preserve:

# 

# \- depth-based coloring;

# \- shallow/deep colors;

# \- opacity;

# \- existing WaterShader\_TestScene.

# 

# \---

# 

# \# Required Shader Properties

# 

# Add:

# 

# \_FlowDirection

# \_FlowSpeed

# 

# Inspector names:

# 

# Flow Direction

# Flow Speed

# 

# Flow Direction should represent a 2D direction.

# 

# Normalize internally where appropriate.

# 

# Flow Speed should control animation velocity independently from direction.

# 

# \---

# 

# \# Flow Coordinate System

# 

# Create reusable flow UV logic.

# 

# Conceptually:

# 

# flowOffset =

# &#x20;   normalizedFlowDirection

# &#x20;   \* FlowSpeed

# &#x20;   \* Time

# 

# animatedUV =

# &#x20;   baseUV + flowOffset

# 

# The implementation should be structured so future Specs can replace the uniform direction with data from a Flow Map.

# 

# Avoid tightly coupling flow calculations to temporary visualization logic.

# 

# \---

# 

# \# Temporary Flow Visualization

# 

# Create a simple procedural visualization to make direction and speed obvious.

# 

# Preferred solution:

# 

# procedural stripes or bands.

# 

# Do not use an authored texture for this Spec.

# 

# The visualization exists only to validate:

# 

# \- direction;

# \- speed;

# \- UV movement;

# \- stability.

# 

# Do not polish it into the final water pattern.

# 

# \---

# 

# \# Architecture

# 

# If the flow logic becomes large enough to justify separation, create:

# 

# Assets/WaterShader/Shaders/Includes/WaterFlow.hlsl

# 

# Do not create an include merely for a few trivial expressions.

# 

# If created, WaterFlow.hlsl should contain reusable flow-related calculations rather than visual styling.

# 

# \---

# 

# \# Test Scene

# 

# Add or configure a technical surface suitable for flow inspection.

# 

# Suggested name:

# 

# River\_Test

# 

# A long rectangular plane is sufficient.

# 

# Keep existing depth validation geometry available.

# 

# Do not build a decorative river environment.

# 

# \---

# 

# \# Debugging

# 

# A temporary flow-direction visualization is allowed if useful.

# 

# For example:

# 

# \- UV bands;

# \- directional color;

# \- animated procedural stripes.

# 

# Do not make debug visualization the default final shader appearance.

# 

# \---

# 

# \# Validation

# 

# Verify:

# 

# Flow Direction:

# \- positive X;

# \- negative X;

# \- positive Y;

# \- negative Y;

# \- diagonal directions.

# 

# Flow Speed:

# \- 0 produces no movement;

# \- low speed is visibly slow;

# \- high speed is clearly faster.

# 

# Also confirm:

# 

# \- depth coloring still works;

# \- opacity still works;

# \- no visual jitter appears;

# \- Unity reports no new shader errors.

# 

# \---

# 

# \# External Assets

# 

# SPEC-004 requires no external textures or meshes.

# 

# Do not create or import:

# 

# \- noise textures;

# \- flow textures;

# \- foam textures;

# \- distortion textures;

# \- masks;

# \- normal maps.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-004 is complete when:

# 

# \- configurable uniform flow direction works;

# \- configurable flow speed works;

# \- the flow system is reusable by future Specs;

# \- direction can be changed without rewriting pattern logic;

# \- depth coloring remains functional;

# \- the shader compiles cleanly.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- Flow Maps;

# \- painterly final patterns;

# \- noise exploration;

# \- Flow Strength;

# \- foam;

# \- waves;

# \- normals;

# \- refraction;

# \- reflection;

# \- waterfall VFX.

