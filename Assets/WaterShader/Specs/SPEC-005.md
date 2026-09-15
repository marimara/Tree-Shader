# \# SPEC-005 — Flow Pattern Source Exploration

# 

# \## Objective

# 

# Determine the most suitable base pattern source for the Stylized Water Shader.

# 

# The project references suggest that the final water appearance should be created primarily through shader shaping rather than by using a fully authored final-looking water texture.

# 

# This Spec must compare several pattern-source strategies before committing the shader architecture to one solution.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated SPEC-004.

# 

# Preserve:

# 

# \- uniform flow direction;

# \- flow speed;

# \- depth coloring;

# \- opacity.

# 

# \---

# 

# \# Goal

# 

# Compare at least three approaches:

# 

# 1\. procedural noise;

# 2\. sampled grayscale noise texture;

# 3\. hybrid texture + procedural shaping.

# 

# The purpose is to identify which approach provides the best foundation for:

# 

# \- calm water;

# \- rivers;

# \- waterfalls;

# \- directional stretching;

# \- thresholding;

# \- future Flow Strength control.

# 

# \---

# 

# \# Pattern Modes

# 

# Introduce a temporary technical way to switch between pattern-source implementations.

# 

# This may be:

# 

# \_PatternSourceMode

# 

# or an equivalent debug/development mechanism.

# 

# It does not need to become a permanent production-facing material property.

# 

# \---

# 

# \# Candidate A — Procedural Noise

# 

# Implement a simple procedural noise source.

# 

# Preferred characteristics:

# 

# \- medium-to-large shapes;

# \- no excessive high-frequency grain;

# \- stable animation;

# \- suitable for directional stretching.

# 

# Do not create an unnecessarily expensive multi-octave procedural system.

# 

# The goal is comparison, not maximum complexity.

# 

# \---

# 

# \# Candidate B — Noise Texture

# 

# Support one grayscale seamless noise texture.

# 

# Suggested asset:

# 

# T\_WaterNoise\_01

# 

# Expected characteristics:

# 

# \- grayscale;

# \- seamless;

# \- medium-frequency abstract forms;

# \- no baked lighting;

# \- no perspective;

# \- no obvious final water streaks;

# \- suitable for transformation in the shader.

# 

# \---

# 

# \# Candidate C — Hybrid

# 

# Combine the sampled noise texture with simple procedural modification.

# 

# Possible techniques include:

# 

# \- procedural UV distortion;

# \- remapping;

# \- thresholding;

# \- contrast shaping;

# \- small secondary noise influence.

# 

# Keep this version reasonably simple.

# 

# \---

# 

# \# Flow Integration

# 

# All three pattern-source candidates must use the directional flow system from SPEC-004.

# 

# Changing:

# 

# Flow Direction

# 

# or:

# 

# Flow Speed

# 

# must affect all candidates consistently.

# 

# \---

# 

# \# Comparison Controls

# 

# Expose only the controls needed for fair comparison.

# 

# Useful controls may include:

# 

# \_NoiseScale

# \_NoiseContrast

# \_NoiseStretch

# 

# Exact property names may differ.

# 

# Avoid prematurely exposing the full final shader interface.

# 

# \---

# 

# \# Visual Evaluation

# 

# Evaluate each candidate for:

# 

# \## Calm Water

# 

# Does it remain visually useful at low speed?

# 

# Does it avoid looking like frozen river noise?

# 

# \## River

# 

# Does it produce readable directional movement?

# 

# Can it form larger stylized shapes?

# 

# \## Waterfall

# 

# Can it be stretched strongly without becoming visually broken or overly repetitive?

# 

# \---

# 

# \# Performance Evaluation

# 

# Record approximate relative complexity.

# 

# Consider:

# 

# \- texture samples;

# \- procedural calculations;

# \- repeated noise evaluations;

# \- shader readability.

# 

# Do not optimize aggressively yet.

# 

# The goal is to avoid selecting a pattern source that is unnecessarily expensive.

# 

# \---

# 

# \# Test Setup

# 

# Use the existing technical scene.

# 

# If useful, create three adjacent comparison surfaces:

# 

# Pattern\_Procedural

# Pattern\_Texture

# Pattern\_Hybrid

# 

# They should use comparable scale and flow settings.

# 

# Do not create a decorative environment.

# 

# \---

# 

# \# Validation

# 

# Capture comparison images or video showing:

# 

# \- procedural;

# \- texture;

# \- hybrid.

# 

# Evaluate all three at:

# 

# \- low flow speed;

# \- medium flow speed;

# \- high flow speed.

# 

# At the end of the Spec, clearly document which source is selected as the basis for SPEC-006 and why.

# 

# Do not silently select one without reporting the comparison.

# 

# \---

# 

# \# External Assets

# 

# SPEC-005 may use one authored grayscale noise texture:

# 

# Assets/WaterShader/Textures/Flow/T\_WaterNoise\_01.png

# 

# Do not add additional textures unless comparison demonstrates that they are necessary.

# 

# The texture must not already contain the final painterly streak appearance.

# 

# It should serve as raw pattern data.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-005 is complete when:

# 

# \- procedural noise is tested;

# \- texture noise is tested;

# \- a hybrid approach is tested;

# \- all three respond to flow direction and speed;

# \- a preferred source is selected and documented;

# \- the chosen approach is suitable for calm water, rivers, and strong directional flow;

# \- existing depth coloring remains functional.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- final painterly styling;

# \- Flow Strength;

# \- Flow Maps;

# \- foam;

# \- waves;

# \- normals;

# \- reflections;

# \- refraction;

# \- waterfall VFX.

