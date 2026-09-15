# \# SPEC-008 — Flow Map Foundation

# 

# \## Objective

# 

# Introduce Flow Map support so flow direction and strength can vary spatially across a water surface.

# 

# This Spec establishes Flow Map decoding, sampling and debug visualization.

# 

# It does not yet attempt to create the final curved-river animation.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated SPEC-007.

# 

# Preserve the existing uniform-flow mode for comparison and fallback.

# 

# \---

# 

# \# Flow Map Convention

# 

# Use the following initial channel convention:

# 

# R = Flow Direction X

# G = Flow Direction Y

# B = Flow Strength

# A = reserved for future turbulence / foam data

# 

# Direction channels should be decoded from texture range:

# 

# 0–1

# 

# into vector range:

# 

# \-1 to +1

# 

# Conceptually:

# 

# flowDir =

# &#x20;   flowMap.rg \* 2 - 1

# 

# Normalize where appropriate.

# 

# \---

# 

# \# Flow Strength

# 

# Use Flow Map B as local Flow Strength.

# 

# This should feed the existing Flow Strength system created in SPEC-007.

# 

# Do not create a separate unrelated strength system.

# 

# \---

# 

# \# Properties

# 

# Add:

# 

# \_FlowMap

# \_UseFlowMap

# 

# Optional:

# 

# \_FlowMapTiling

# \_FlowMapOffset

# 

# Only add tiling/offset if useful for technical testing.

# 

# \---

# 

# \# Uniform Flow Fallback

# 

# When Flow Map is disabled:

# 

# retain the existing:

# 

# \_FlowDirection

# \_FlowStrength

# 

# behavior.

# 

# Do not remove the uniform-flow workflow.

# 

# It remains useful for:

# 

# \- waterfalls;

# \- simple channels;

# \- debugging;

# \- fallback.

# 

# \---

# 

# \# Debug Modes

# 

# Provide temporary debug views for:

# 

# \## Direction

# 

# Visualize RG direction in a clear way.

# 

# \## Strength

# 

# Display the B channel as grayscale or another readable debug representation.

# 

# The purpose is to verify map interpretation before using it for final animation.

# 

# \---

# 

# \# Test Flow Map

# 

# Create a simple technical Flow Map for validation.

# 

# It should demonstrate:

# 

# \- at least two different directions;

# \- varying Flow Strength;

# \- a calm region;

# \- a faster region.

# 

# The test map does not need to be artistically polished.

# 

# \---

# 

# \# Scene Test

# 

# Create or configure:

# 

# FlowMap\_Test

# 

# The surface should clearly show different map regions.

# 

# Do not build a final river environment yet.

# 

# \---

# 

# \# Validation

# 

# Confirm:

# 

# \- direction decoding is correct;

# \- local direction changes spatially;

# \- B correctly controls Flow Strength;

# \- calm and fast map regions produce expected values;

# \- uniform-flow fallback still works;

# \- debug modes match the source Flow Map.

# 

# \---

# 

# \# External Assets

# 

# SPEC-008 requires one technical Flow Map.

# 

# Suggested:

# 

# Assets/WaterShader/Textures/Flow/T\_FlowMap\_Test.png

# 

# This is a technical validation asset, not a final production Flow Map.

# 

# It may be generated procedurally or manually.

# 

# Do not create an elaborate artist tool yet.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-008 is complete when:

# 

# \- Flow Map direction is sampled correctly;

# \- local Flow Strength drives the existing transition system;

# \- direction and strength can be inspected through debug modes;

# \- uniform flow remains available;

# \- the test map produces spatially different values across one surface.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- final curved-river advection;

# \- Flow Map painting tools;

# \- foam;

# \- turbulence;

# \- waves;

# \- normals;

# \- reflection;

# \- refraction;

# \- VFX.

