# \# SPEC-006 — Stylized Flow Pattern Shaping

# 

# \## Objective

# 

# Transform the pattern source selected in SPEC-005 into the project's stylized directional water pattern.

# 

# This is the first Spec focused on the actual visual language of moving water.

# 

# The shader should begin to resemble the visual references without implementing foam or waterfall-specific effects.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated SPEC-005.

# 

# Read the SPEC-005 completion report before implementation.

# 

# Use the pattern-source approach selected during SPEC-005.

# 

# Do not restart the pattern-source architecture unless validation showed a technical problem.

# 

# \---

# 

# \# Visual Target

# 

# Use the visual references stored in:

# 

# Assets/WaterShader/References/

# 

# Target characteristics:

# 

# \- clear directional movement;

# \- elongated highlight-like shapes;

# \- large readable forms;

# \- limited fine noise;

# \- irregular but controlled shapes;

# \- graphic stylized appearance;

# \- clean negative space;

# \- suitable for both river and waterfall stretching.

# 

# Avoid:

# 

# \- realistic ocean detail;

# \- tiny noisy grain;

# \- marble appearance;

# \- cloud-like patterns;

# \- uniform sine-wave bands;

# \- overly dense surface detail.

# 

# \---

# 

# \# Pattern Shaping

# 

# Create controls that transform the base noise into stylized water marks.

# 

# Expected techniques may include:

# 

# \- contrast adjustment;

# \- remapping;

# \- Smoothstep;

# \- thresholding;

# \- directional scaling;

# \- UV stretching;

# \- selective masking.

# 

# Do not assume all techniques are required.

# 

# Use the simplest combination that achieves the target.

# 

# \---

# 

# \# Required Controls

# 

# Expose useful controls for:

# 

# \_PatternScale

# \_PatternStrength

# \_PatternStretch

# \_PatternColor

# 

# Also expose threshold / softness controls if required by the selected shaping technique.

# 

# Example:

# 

# \_PatternThreshold

# \_PatternSoftness

# 

# Avoid redundant controls.

# 

# \---

# 

# \# Directional Stretch

# 

# Pattern Stretch must act relative to flow direction.

# 

# Low values:

# 

# \- broader;

# \- softer;

# \- less directional.

# 

# High values:

# 

# \- elongated;

# \- clearly directional;

# \- suitable for rapid water / waterfall-like streaks.

# 

# Changing Flow Direction should also rotate/reorient the effective pattern behavior appropriately.

# 

# \---

# 

# \# Pattern Color

# 

# The pattern should act as a stylized highlight/detail layer over the existing depth-based water color.

# 

# Use a light cyan / near-white default.

# 

# Do not replace the shallow/deep color system.

# 

# \---

# 

# \# Layering

# 

# If the chosen pattern source benefits from multiple samples, a second sample is allowed.

# 

# Possible differences:

# 

# \- scale;

# \- speed;

# \- offset;

# \- threshold.

# 

# Do not use multiple samples by default unless they materially improve the visual result.

# 

# Keep the pattern readable.

# 

# \---

# 

# \# Calm / River / Waterfall Test Presets

# 

# Validate manually using at least three temporary configurations.

# 

# \## Calm-like

# 

# \- low speed;

# \- low stretch;

# \- low pattern strength.

# 

# \## River-like

# 

# \- medium speed;

# \- medium stretch;

# \- medium pattern strength.

# 

# \## Waterfall-like

# 

# \- high speed;

# \- high stretch;

# \- stronger pattern visibility.

# 

# These are manual test states only.

# 

# Automatic transition belongs to SPEC-007.

# 

# \---

# 

# \# Validation

# 

# Confirm:

# 

# \- patterns follow flow direction;

# \- patterns stretch in the flow direction;

# \- pattern scale remains controllable;

# \- threshold / shaping produces clean forms;

# \- the result remains readable at a distance;

# \- the river configuration begins resembling the intended visual references;

# \- strong stretch produces useful waterfall-like streaks;

# \- low stretch does not look obviously broken.

# 

# Capture comparison validation.

# 

# \---

# 

# \# External Assets

# 

# Use only assets selected during SPEC-005.

# 

# Do not add another pattern/noise texture unless the existing source demonstrably cannot produce the required result.

# 

# If another external texture appears necessary, stop and report the need before creating/importing it.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-006 is complete when:

# 

# \- the raw noise source has been transformed into deliberate stylized water marks;

# \- direction and stretch behave correctly;

# \- calm-like, river-like, and waterfall-like manual presets are visually distinct;

# \- the pattern remains part of the same visual family across these states;

# \- depth-based coloring remains functional;

# \- no foam or other future effects were added.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- automatic Flow Strength transition;

# \- Flow Maps;

# \- foam;

# \- intersection foam;

# \- waves;

# \- normals;

# \- refraction;

# \- reflection;

# \- waterfall particles;

# \- interaction.

