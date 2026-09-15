# \# SPEC-007 — Flow Strength and Flow-to-Calm Transition

# 

# \## Objective

# 

# Introduce Flow Strength as the central parameter controlling the perceived velocity and visual character of the water.

# 

# The same shader must transition continuously between:

# 

# \- nearly still water;

# \- slow-moving water;

# \- river water;

# \- strong directional flow.

# 

# The transition must alter the visual character of the pattern, not merely animation speed.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated:

# 

# \- SPEC-004

# \- SPEC-005

# \- SPEC-006

# 

# \---

# 

# \# Core Property

# 

# Add:

# 

# \_FlowStrength

# 

# Range:

# 

# 0 to 1

# 

# Meaning:

# 

# 0 = calm / nearly still

# 

# 1 = maximum directional flow

# 

# \---

# 

# \# Flow Strength Responsibilities

# 

# Flow Strength must affect at minimum:

# 

# 1\. effective flow speed;

# 2\. directional pattern stretch;

# 3\. pattern strength / visibility.

# 

# It may additionally influence:

# 

# \- threshold;

# \- secondary pattern contribution;

# \- distortion;

# \- pattern scale;

# 

# if this improves the transition.

# 

# Do not make unnecessary controls dependent on Flow Strength.

# 

# \---

# 

# \# Calm Water Requirement

# 

# At Flow Strength = 0:

# 

# Water must not look like a paused river texture.

# 

# Expected characteristics:

# 

# \- very slow residual motion;

# \- broad shapes;

# \- minimal directional streaking;

# \- lower pattern prominence;

# \- calm visual impression.

# 

# The surface should remain subtly alive.

# 

# \---

# 

# \# Slow Flow

# 

# Around:

# 

# Flow Strength = 0.3

# 

# Expected:

# 

# \- gentle directional movement;

# \- broad but slightly stretched shapes;

# \- clear indication that the surface is moving.

# 

# \---

# 

# \# River

# 

# Around:

# 

# Flow Strength = 0.6

# 

# Expected:

# 

# \- clearly directional pattern;

# \- moderate-to-strong stretch;

# \- visually readable current;

# \- medium animation speed.

# 

# \---

# 

# \# Fast Flow

# 

# At:

# 

# Flow Strength = 1

# 

# Expected:

# 

# \- strong directional animation;

# \- highly elongated pattern;

# \- strong flow readability;

# \- useful foundation for later rapids and waterfall behavior.

# 

# Do not add foam or particles.

# 

# \---

# 

# \# Transition Model

# 

# Flow Strength should interpolate smoothly between states.

# 

# Avoid obvious hard state switches.

# 

# The system should behave as a continuum.

# 

# \---

# 

# \# Test Setup

# 

# Create a dedicated technical comparison.

# 

# Suggested:

# 

# FlowTransition\_Test

# 

# Show at least four states:

# 

# 0.00

# 0.33

# 0.66

# 1.00

# 

# Possible implementation:

# 

# \- adjacent surfaces;

# \- separate material instances;

# \- controlled debug comparison.

# 

# A spatial Flow Map is not required yet.

# 

# \---

# 

# \# Validation

# 

# Verify:

# 

# \- animation progressively accelerates;

# \- pattern stretch progressively increases;

# \- pattern strength changes appropriately;

# \- calm water remains visually alive;

# \- fast water feels clearly directional;

# \- intermediate values interpolate smoothly;

# \- depth coloring remains correct.

# 

# A viewer should be able to identify relative flow strength without reading numeric labels.

# 

# \---

# 

# \# External Assets

# 

# No new external assets should be required.

# 

# Use the existing pattern system.

# 

# If Flow Strength reveals that another texture is necessary, stop and report before adding it.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-007 is complete when:

# 

# \- Flow Strength provides a convincing continuous visual family;

# \- calm water does not look like paused river water;

# \- river and fast-flow states are clearly different;

# \- intermediate states remain visually coherent;

# \- no hard visual transitions occur;

# \- existing depth and pattern systems remain functional.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- Flow Maps;

# \- curved flow;

# \- foam;

# \- waves;

# \- normals;

# \- reflection;

# \- refraction;

# \- waterfall VFX;

# \- interaction.

