# \# SPEC-009 — Stable Curved Flow and Dual-phase Advection

# 

# \## Objective

# 

# Use the spatial Flow Map support from SPEC-008 to animate the stylized water pattern along curved paths while preserving the clean visual language established in SPEC-006 and SPEC-007.

# 

# Implement stable dual-phase advection without introducing excessive pattern warping, curved-noise distortion, implausible speed changes, or visible temporal reset artifacts.

# 

# The result must look like stylized water following a curved current.

# 

# It must not look like the texture itself is being heavily twisted or deformed.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated:

# 

# \- SPEC-006

# \- SPEC-007

# \- SPEC-008

# 

# Preserve:

# 

# \- depth-based coloring;

# \- shallow/deep colors;

# \- opacity;

# \- shaped stylized pattern;

# \- Flow Strength behavior;

# \- uniform-flow fallback;

# \- Flow Map debug modes.

# 

# \---

# 

# \# Primary Visual Problem

# 

# The initial curved-flow implementation demonstrated technically correct spatial direction changes but produced excessive visual distortion.

# 

# Observed undesirable behavior included:

# 

# \- streaks bending too aggressively;

# \- pattern forms appearing warped rather than advected;

# \- local direction changes being visually exaggerated;

# \- speed differences that did not feel naturally related to the test geometry.

# 

# This Spec must correct those issues.

# 

# Technical correctness alone is not sufficient.

# 

# Visual plausibility is required.

# 

# \---

# 

# \# Core Principle

# 

# Flow Map direction should primarily determine:

# 

# \- where the pattern moves;

# \- the general orientation of the current.

# 

# It should not cause uncontrolled deformation of the pattern.

# 

# Flow Strength should determine the character/intensity of the flow but must remain within an artistically useful range.

# 

# \---

# 

# \# Local Flow Direction

# 

# When Flow Map mode is enabled:

# 

# use local Flow Map direction instead of the global uniform Flow Direction.

# 

# Each region of the surface should move according to its local vector.

# 

# However, local vectors must form a visually smooth field.

# 

# Avoid reacting strongly to high-frequency direction changes.

# 

# \---

# 

# \# Direction Smoothing

# 

# Curved flow must transition gradually between neighboring directions.

# 

# If the technical Flow Map contains overly abrupt vector transitions, smooth the data before or during use.

# 

# Acceptable approaches include:

# 

# \- smoother generated technical Flow Map data;

# \- filtered neighboring Flow Map samples;

# \- another lightweight method that produces a smooth local vector field.

# 

# Prefer fixing the source field when possible rather than adding unnecessary runtime sampling cost.

# 

# Do not create a large expensive blur operation in the shader solely for this test.

# 

# \---

# 

# \# Direction Change Requirement

# 

# A curved path should resemble:

# 

# → → → ↘ ↘ ↓ ↓

# 

# rather than:

# 

# → ↘ ↓ ↙ ←

# 

# across a very short distance.

# 

# Large angular changes over a small area should be treated as a technical problem with the test field or smoothing.

# 

# The visual goal is a broad readable current.

# 

# \---

# 

# \# Pattern Orientation

# 

# The stylized streaks should generally align with local flow direction.

# 

# However:

# 

# do not force the entire pattern to perfectly follow every tiny local vector change.

# 

# Orientation should feel smooth and coherent.

# 

# The result should resemble highlights traveling around a bend rather than brush marks being twisted into arcs.

# 

# \---

# 

# \# Pattern Shape Preservation

# 

# Preserve the clean shapes established in SPEC-006.

# 

# Curved Flow must not destroy:

# 

# \- negative space;

# \- separated highlight marks;

# \- readable streak shapes;

# \- large/medium stylized forms.

# 

# If curved flow causes the pattern to become:

# 

# \- marble-like;

# \- excessively wavy;

# \- spiraled;

# \- smeared;

# \- heavily bent;

# 

# reduce the influence of local orientation/deformation.

# 

# \---

# 

# \# Pattern Stretch

# 

# Pattern Stretch must continue to be driven primarily by Flow Strength.

# 

# Do not derive extreme stretch from direction changes or local curvature.

# 

# Flow direction determines orientation.

# 

# Flow Strength determines how elongated the pattern becomes.

# 

# These responsibilities should remain conceptually separate.

# 

# \---

# 

# \# Flow Strength Remapping

# 

# Flow Map B represents relative local Flow Strength.

# 

# Do not assume the full encoded 0–1 range must directly become the full visual 0–1 Flow Strength range.

# 

# Provide a controllable remapping so technical Flow Map values can operate inside a visually useful range.

# 

# Conceptually:

# 

# encoded strength

# → artist-controlled range

# → existing SPEC-007 Flow Strength system

# 

# Possible controls:

# 

# \_FlowMapMinStrength

# \_FlowMapMaxStrength

# 

# or an equivalent compact representation.

# 

# Example:

# 

# Flow Map B = 0

# does not necessarily need to produce completely still water.

# 

# Flow Map B = 1

# does not necessarily need to produce the absolute maximum possible shader motion.

# 

# Avoid redundant controls.

# 

# \---

# 

# \# Effective Strength

# 

# The remapped local strength must continue feeding the existing SPEC-007 system.

# 

# Do not duplicate:

# 

# \- speed logic;

# \- stretch logic;

# \- pattern visibility logic;

# \- threshold/coverage logic.

# 

# SPEC-007 remains the source of truth for how Flow Strength changes the visual water state.

# 

# \---

# 

# \# Speed Behavior

# 

# Local speed differences must be visually plausible.

# 

# Avoid dramatic velocity changes unless the Flow Map clearly represents such a transition.

# 

# For the validation scene:

# 

# \- constant-width sections should have relatively coherent speed;

# \- intentionally slower or faster sections should transition smoothly;

# \- curved direction alone must not automatically imply a large speed change.

# 

# \---

# 

# \# Dual-phase Advection

# 

# Implement or preserve repeating dual-phase flow sampling.

# 

# Conceptually:

# 

# Phase A

# → advects along local flow.

# 

# Phase B

# → uses the same flow with approximately half-cycle temporal offset.

# 

# Blend between the phases so resets are hidden.

# 

# The exact implementation may vary.

# 

# \---

# 

# \# Dual-phase Visual Requirement

# 

# Dual-phase advection must solve temporal looping without creating obvious spatial distortion.

# 

# If technically stable dual-phase sampling produces noticeably worse visual shapes than the validated SPEC-006 pattern, adjust the implementation.

# 

# Temporal stability must not come at the cost of destroying the visual style.

# 

# \---

# 

# \# Stability Requirements

# 

# Avoid:

# 

# \- unlimited UV offsets;

# \- visible phase reset pops;

# \- increasing temporal deformation;

# \- pattern collapse;

# \- excessive local twisting;

# \- severe stretching;

# \- discontinuous temporal artifacts.

# 

# The animation must remain stable during extended Play Mode testing.

# 

# \---

# 

# \# Technical Validation Scene

# 

# Create or revise the technical validation scene so curved flow can be judged visually.

# 

# The Codex may design the scene setup that best demonstrates the system.

# 

# The scene does not need final environment art.

# 

# However, it should contain a visually understandable water path.

# 

# Recommended characteristics:

# 

# \- a clear channel or river shape;

# \- one broad gradual curve;

# \- visible banks/boundaries or simple geometry providing spatial context;

# \- a mostly constant-width section for direction testing;

# \- optionally a wider section for later slow-flow preview.

# 

# The viewer should be able to look at the geometry and intuitively understand where the water should travel.

# 

# Avoid validating curved flow only on a featureless rectangular plane.

# 

# \---

# 

# \# Scene Design Freedom

# 

# The implementation may:

# 

# \- create simple banks from primitive geometry;

# \- use simple materials;

# \- reposition the camera;

# \- create a dedicated FlowCurve\_Test setup;

# \- revise technical Flow Map data.

# 

# Keep all work inside the WaterShader test/validation scope.

# 

# Do not create decorative environment art.

# 

# \---

# 

# \# Curved Flow Test

# 

# The primary test should demonstrate:

# 

# straight flow

# → gradual bend

# → straight or gently continuing flow

# 

# The bend should be broad enough to judge orientation and distortion clearly.

# 

# The current must visually follow the channel.

# 

# \---

# 

# \# Strength Test

# 

# Do not combine every possible test into the same region.

# 

# First validate curved direction using a mostly coherent Flow Strength.

# 

# Then, if direction is visually correct, validate a smooth strength transition separately.

# 

# This makes it easier to distinguish:

# 

# \- direction problems;

# \- strength problems;

# \- advection problems.

# 

# \---

# 

# \# Optional River-to-Calm Preview

# 

# After curved-flow validation passes, the scene may include:

# 

# River

# → slower region

# → calm region

# 

# This remains a technical preview.

# 

# Automatic Flow Strength generation belongs to SPEC-012.

# 

# \---

# 

# \# Validation — Direction

# 

# Confirm:

# 

# \- Flow Map direction follows the intended channel;

# \- the bend is smooth;

# \- pattern orientation follows the broad current;

# \- streaks do not become excessively curved;

# \- negative space remains intact;

# \- direction does not visibly jitter between neighboring regions.

# 

# \---

# 

# \# Validation — Pattern Quality

# 

# Compare against the validated uniform-flow pattern.

# 

# Curved Flow is acceptable only if the pattern still retains:

# 

# \- clean stylized highlights;

# \- readable individual streaks;

# \- controlled elongation;

# \- limited noise;

# \- visible base water between marks.

# 

# The curved version should look like the same shader following a bend.

# 

# It should not look like a different distorted shader.

# 

# \---

# 

# \# Validation — Flow Strength

# 

# Test at least:

# 

# \- a mostly constant-strength curved channel;

# \- a smooth moderate-to-low strength transition.

# 

# Confirm:

# 

# \- speed transitions smoothly;

# \- stretch changes smoothly;

# \- pattern coverage changes smoothly;

# \- no unexpected region becomes dramatically faster/slower.

# 

# \---

# 

# \# Validation — Temporal Stability

# 

# Run extended Play Mode validation.

# 

# Confirm:

# 

# \- no reset pop;

# \- no increasing deformation over time;

# \- no pattern collapse;

# \- no progressive UV stretching;

# \- direction remains stable;

# \- pattern quality remains visually consistent.

# 

# \---

# 

# \# Uniform Flow Comparison

# 

# Retain a uniform-flow comparison.

# 

# Confirm that:

# 

# \- uniform mode still matches the previously validated appearance;

# \- enabling Flow Map does not alter unrelated shader behavior.

# 

# \---

# 

# \# Debugging

# 

# Retain SPEC-008 debug modes for:

# 

# \- direction;

# \- strength.

# 

# Use them to diagnose unexpected behavior before modifying the stylized pattern.

# 

# Temporary additional debug visualization is allowed if useful.

# 

# Do not leave intrusive debug behavior enabled by default.

# 

# \---

# 

# \# Technical Flow Map Data

# 

# The temporary Flow Map may be regenerated if needed.

# 

# Prefer a smooth, intentionally designed technical field appropriate for the validation scene.

# 

# Do not consider the previous SPEC-008 technical Flow Map visually authoritative.

# 

# It was created to verify data decoding, not final curved-water quality.

# 

# No manually painted production Flow Map is required.

# 

# \---

# 

# \# External Assets

# 

# No new visual water textures are required.

# 

# Reuse:

# 

# \- Noise 1;

# \- existing stylized pattern system.

# 

# Technical Flow Map data may be regenerated by code.

# 

# Do not add:

# 

# \- foam textures;

# \- normals;

# \- distortion textures;

# \- environment art assets.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-009 is complete when:

# 

# \- curved flow visually follows an understandable channel;

# \- local direction changes smoothly;

# \- stylized streaks align with the broad flow without excessive bending;

# \- the pattern retains the visual quality established in SPEC-006;

# \- Flow Strength is remapped into a controllable useful range;

# \- speed changes are visually plausible;

# \- dual-phase animation remains stable over extended runtime;

# \- no visible reset pop occurs;

# \- uniform-flow fallback remains unchanged;

# \- the result is visually approved, not merely technically functional.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- Flow Map Baker;

# \- collider-aware automatic routing;

# \- boundary-aware automatic baking;

# \- automatic river-to-lake strength generation;

# \- runtime fluid simulation;

# \- foam;

# \- intersection foam;

# \- waves;

# \- custom normals;

# \- reflection;

# \- refraction;

# \- waterfall VFX;

# \- interaction.

