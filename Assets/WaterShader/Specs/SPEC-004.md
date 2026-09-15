# \# SPEC-004 — Uniform Directional Water Flow

# 

# \## Objective

# 

# Introduce the first animated flow system.

# 

# This Spec establishes directional motion without Flow Maps.

# 

# The entire test surface may use one uniform flow direction.

# 

# \---

# 

# \# Dependencies

# 

# Requires SPEC-001 through SPEC-003.

# 

# \---

# 

# \# Shader Properties

# 

# Add:

# 

# \_FlowDirection

# \_FlowSpeed

# 

# Suggested Inspector names:

# 

# Flow Direction

# Flow Speed

# 

# Flow Direction should operate as a 2D direction.

# 

# Normalize internally where appropriate.

# 

# \---

# 

# \# Flow Coordinate System

# 

# Create reusable flow UV logic.

# 

# The system should allow texture coordinates to move along:

# 

# Flow Direction \* Flow Speed \* Time

# 

# Do not yet create complex Flow Map logic.

# 

# \---

# 

# \# Temporary Flow Visualization

# 

# Because no final painterly texture exists yet, create a procedural or very simple temporary flow visualization.

# 

# Acceptable options include:

# 

# \- procedural stripes;

# \- simple noise;

# \- generated gradient;

# \- minimal temporary texture.

# 

# The purpose is only to verify:

# 

# \- direction;

# \- speed;

# \- UV stability.

# 

# Do not spend time polishing the visual pattern yet.

# 

# \---

# 

# \# Recommended Architecture

# 

# If flow logic becomes substantial enough, create:

# 

# Assets/WaterShader/Shaders/Includes/WaterFlow.hlsl

# 

# Otherwise it may remain inside StylizedWater.shader for this Spec.

# 

# Do not create an include simply for a few trivial lines.

# 

# \---

# 

# \# Test Scene

# 

# Create or duplicate a test surface specifically suitable for flow inspection.

# 

# Suggested name:

# 

# River\_Test

# 

# It may simply be a long rectangular plane.

# 

# Keep Water\_Test available for depth comparison if useful.

# 

# \---

# 

# \# Validation

# 

# Verify:

# 

# \- positive X flow;

# \- negative X flow;

# \- positive Y flow;

# \- diagonal flow;

# \- Flow Speed = 0 produces no movement;

# \- high speed is clearly visible;

# \- depth coloring still works.

# 

# \---

# 

# \# Acceptance Criteria

# 

# The shader has a reliable configurable directional animation system.

# 

# The implementation should be suitable as a foundation for the later painterly flow pattern.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- Flow Maps;

# \- river bends;

# \- Flow Strength masks;

# \- painterly final streaks;

# \- foam;

# \- waves;

# \- refraction.

