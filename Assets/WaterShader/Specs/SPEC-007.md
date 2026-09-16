# \# SPEC-007 — Flow Strength and Flow-to-Calm Transition

# 

# \## Objective

# 

# Introduce Flow Strength as the central parameter controlling the perceived velocity and visual character of the stylized water pattern created in SPEC-006.

# 

# The same shader must transition continuously between:

# 

# \- nearly still water;

# \- slow-moving water;

# \- river water;

# \- strong directional flow.

# 

# Flow Strength must change the character of the pattern.

# 

# It must not simply slow down or accelerate the same visual texture.

# 

# The goal is to establish the first convincing:

# 

# Calm Water ↔ River ↔ Fast Flow

# 

# continuum.

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

# Use the shaped pattern architecture produced by SPEC-006.

# 

# Do not rebuild the pattern system from scratch.

# 

# Preserve:

# 

# \- depth-based coloring;

# \- Shallow Color;

# \- Deep Color;

# \- opacity;

# \- Flow Direction;

# \- Flow Speed;

# \- stylized pattern controls.

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

# 0 = nearly still / calm water

# 

# 1 = maximum strong directional flow

# 

# Flow Strength represents local perceived water movement.

# 

# For this Spec it is still a uniform material property.

# 

# Spatial Flow Map control belongs to later Specs.

# 

# \---

# 

# \# Core Principle

# 

# Flow Strength must affect more than animation speed.

# 

# At minimum, Flow Strength must drive:

# 

# 1\. effective flow speed;

# 2\. directional Pattern Stretch;

# 3\. Pattern Strength / visibility;

# 4\. effective pattern coverage or threshold behavior.

# 

# It may additionally influence:

# 

# \- Pattern Scale;

# \- Pattern Softness;

# \- procedural distortion;

# \- secondary pattern contribution;

# 

# if these improve the transition.

# 

# Do not connect every parameter to Flow Strength without visual justification.

# 

# \---

# 

# \# Base Flow Speed

# 

# \_FlowSpeed should remain the artist-facing maximum or base speed control.

# 

# Flow Strength should modulate the effective speed.

# 

# Conceptually:

# 

# effectiveFlowSpeed =

# &#x20;   function(FlowSpeed, FlowStrength)

# 

# Do not remove the ability to tune Flow Speed.

# 

# Flow Strength defines the state.

# 

# Flow Speed defines the overall speed range.

# 

# \---

# 

# \# Pattern Character Transition

# 

# Flow Strength should gradually transform the pattern.

# 

# The intended conceptual transition is:

# 

# low Flow Strength

# → broad / sparse / subtle / slow

# 

# medium Flow Strength

# → directional / readable / moderately stretched

# 

# high Flow Strength

# → elongated / strong / fast

# 

# Avoid changing only Time speed.

# 

# \---

# 

# \# Flow Strength = 0 — Calm

# 

# At:

# 

# Flow Strength = 0

# 

# The water should appear calm or nearly still.

# 

# Required characteristics:

# 

# \- very slow residual movement;

# \- broad pattern forms;

# \- minimal directional elongation;

# \- low highlight coverage;

# \- low Pattern Strength;

# \- strong visible areas of base water color;

# \- subtle visual activity.

# 

# The surface must not be completely frozen.

# 

# A minimal baseline motion is allowed and encouraged.

# 

# \---

# 

# \# Critical Calm-Water Rule

# 

# The calm state must not look like:

# 

# "the river pattern with Time almost stopped."

# 

# It should visually change shape and coverage.

# 

# Compared to River state, Calm should have:

# 

# \- broader marks;

# \- fewer visible directional streaks;

# \- less pattern coverage;

# \- softer visual directionality;

# \- lower contrast or intensity where useful.

# 

# The base depth-colored water should dominate.

# 

# \---

# 

# \# Flow Strength ≈ 0.25–0.35 — Slow Water

# 

# Expected characteristics:

# 

# \- gentle directional movement;

# \- broad highlight forms;

# \- slightly increased stretch;

# \- slightly increased coverage;

# \- clearly alive surface;

# \- no impression of rapids.

# 

# This range should feel appropriate for:

# 

# \- slow streams;

# \- gentle transitions;

# \- water approaching a lake.

# 

# \---

# 

# \# Flow Strength ≈ 0.55–0.70 — River

# 

# This is the primary River target.

# 

# Expected characteristics:

# 

# \- clearly readable directional movement;

# \- medium-to-strong Pattern Stretch;

# \- clearly separated highlight streaks;

# \- moderate Pattern Strength;

# \- clean negative space;

# \- medium animation speed.

# 

# The pattern should resemble the SPEC-006 River-like validation state.

# 

# \---

# 

# \# Flow Strength = 1 — Fast Flow

# 

# At:

# 

# Flow Strength = 1

# 

# Expected characteristics:

# 

# \- strong directional animation;

# \- high Pattern Stretch;

# \- clearly elongated streaks;

# \- increased pattern visibility;

# \- high motion readability.

# 

# This state should provide a useful visual foundation for:

# 

# \- rapids;

# \- waterfall surfaces;

# \- later foam systems.

# 

# Do not add foam or waterfall-specific effects.

# 

# \---

# 

# \# Smooth Transition

# 

# Flow Strength should behave continuously.

# 

# Avoid:

# 

# \- hard visual state switches;

# \- sudden threshold jumps;

# \- visible discontinuities;

# \- obvious popping.

# 

# Interpolation may be linear or shaped with smooth nonlinear functions if that produces better results.

# 

# Use Smoothstep or similar shaping where appropriate.

# 

# \---

# 

# \# Threshold Behavior

# 

# Because SPEC-006 uses pattern thresholding to create separated highlight marks, Flow Strength should control coverage intelligently.

# 

# Suggested behavior:

# 

# Calm:

# \- higher threshold / lower coverage.

# 

# River:

# \- moderate threshold / moderate coverage.

# 

# Fast:

# \- slightly more visible pattern.

# 

# Do not increase coverage so much that Fast Flow becomes visually filled with white noise.

# 

# Preserve negative space even at high Flow Strength.

# 

# \---

# 

# \# Stretch Behavior

# 

# Pattern Stretch must progressively increase with Flow Strength.

# 

# Example conceptual behavior:

# 

# FlowStrength 0.00

# → broad shapes

# 

# FlowStrength 0.33

# → slightly elongated

# 

# FlowStrength 0.66

# → clear river streaks

# 

# FlowStrength 1.00

# → long strong streaks

# 

# The exact interpolation is visual, not numerically fixed.

# 

# \---

# 

# \# Pattern Strength Behavior

# 

# Pattern contribution should gradually increase with Flow Strength.

# 

# However:

# 

# Flow Strength = 0

# 

# must not necessarily mean:

# 

# Pattern Strength = 0.

# 

# Calm water should retain subtle animated highlights.

# 

# Use a non-zero minimum pattern contribution if visually beneficial.

# 

# \---

# 

# \# Pattern Scale

# 

# Flow Strength may optionally affect effective Pattern Scale.

# 

# This is allowed if it helps calm water use:

# 

# \- broader;

# \- larger;

# \- less repetitive forms.

# 

# Do not make Pattern Scale variation strong enough to cause obvious popping or swimming.

# 

# \---

# 

# \# Distortion

# 

# If the hybrid pattern uses procedural distortion:

# 

# Calm:

# \- subtle distortion.

# 

# River:

# \- moderate distortion.

# 

# Fast:

# \- enough distortion to prevent mechanical repetition.

# 

# Do not use Flow Strength to create increasingly chaotic distortion.

# 

# Fast water should remain directional.

# 

# \---

# 

# \# Test Setup

# 

# Create a dedicated technical comparison.

# 

# Suggested object/group:

# 

# FlowTransition\_Test

# 

# Display four comparable states:

# 

# 0.00

# 0.33

# 0.66

# 1.00

# 

# Preferred setup:

# 

# \- four adjacent surfaces;

# \- same camera;

# \- same depth environment;

# \- same base colors;

# \- same Flow Direction;

# \- same maximum Flow Speed.

# 

# Only Flow Strength should differ unless another parameter is intentionally linked to Flow Strength by the shader.

# 

# \---

# 

# \# Optional Continuous Test

# 

# If practical, create an additional runtime validation that smoothly animates:

# 

# Flow Strength:

# 0 → 1 → 0

# 

# This is optional.

# 

# Its purpose is to reveal:

# 

# \- popping;

# \- threshold discontinuities;

# \- sudden stretch changes.

# 

# Do not add a permanent gameplay system solely for this test.

# 

# \---

# 

# \# Validation

# 

# Verify:

# 

# \## Flow Strength 0

# 

# \- movement remains subtle;

# \- base water dominates;

# \- pattern is broad and sparse;

# \- surface does not look frozen;

# \- surface does not look like a paused river.

# 

# \## Flow Strength 0.33

# 

# \- gentle directional movement is visible;

# \- highlight forms begin elongating;

# \- transition from calm remains coherent.

# 

# \## Flow Strength 0.66

# 

# \- clear river-like current;

# \- medium/high stretch;

# \- readable directional streaks;

# \- clean negative space remains.

# 

# \## Flow Strength 1

# 

# \- strong fast flow;

# \- long directional streaks;

# \- higher pattern prominence;

# \- no loss of readability.

# 

# \---

# 

# \# Cross-State Validation

# 

# Confirm:

# 

# \- speed progressively increases;

# \- Pattern Stretch progressively increases;

# \- pattern contribution changes progressively;

# \- coverage changes coherently;

# \- intermediate values do not pop;

# \- depth coloring remains correct;

# \- Shallow/Deep colors remain correct;

# \- opacity remains functional;

# \- Flow Direction remains functional.

# 

# A viewer should be able to identify the relative flow state without numeric labels.

# 

# \---

# 

# \# Visual Acceptance Target

# 

# The four states should visually communicate:

# 

# Calm

# → Slow

# → River

# → Fast

# 

# even in a still comparison screenshot.

# 

# Motion should reinforce that difference in Play Mode.

# 

# The visual distinction should not depend exclusively on animation speed.

# 

# \---

# 

# \# External Assets

# 

# No new external assets should be required.

# 

# Use the existing hybrid pattern and Noise 1 source validated through SPEC-005 and shaped in SPEC-006.

# 

# Do not create/import:

# 

# \- new noise textures;

# \- masks;

# \- alpha textures;

# \- foam textures;

# \- normal maps;

# \- Flow Maps.

# 

# If SPEC-007 reveals that an additional asset is genuinely required, stop and report before adding it.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-007 is complete when:

# 

# \- \_FlowStrength controls a continuous visual family from calm to fast water;

# \- Flow Strength affects speed, stretch, pattern strength, and pattern coverage/threshold;

# \- Calm water does not look like paused river water;

# \- calm water remains subtly alive;

# \- River state has clean directional streaks;

# \- Fast state has elongated strong directional marks;

# \- negative space remains visible across all states;

# \- no hard transitions or popping occur;

# \- existing depth and color systems remain functional;

# \- no Flow Maps or later features were implemented.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- Flow Maps;

# \- curved local flow;

# \- foam;

# \- intersection foam;

# \- waterfall foam;

# \- waterfall edge effects;

# \- waves;

# \- custom normals;

# \- reflection;

# \- refraction;

# \- waterfall VFX;

# \- interaction.

