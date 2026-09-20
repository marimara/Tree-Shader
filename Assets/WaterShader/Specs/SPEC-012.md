# \# SPEC-012 — Automatic River-to-Lake Flow Strength Generation

# 

# \## Objective

# 

# Automatically generate Flow Strength during the Flow Map bake so a narrow directional river can transition naturally into slower broad water and eventually a calm lake-like region.

# 

# The system should produce:

# 

# Fast Flow

# → River

# → Slow Water

# → Calm Water

# 

# without requiring manual per-pixel Flow Strength painting.

# 

# The result does not need to be physically accurate.

# 

# It must be:

# 

# \- visually plausible;

# \- stable;

# \- smooth;

# \- artist-controllable.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated:

# 

# \- SPEC-010 — Flow Map Baker

# \- SPEC-011 — Collider and Boundary-aware Bake

# 

# Preserve:

# 

# \- centerline-based direction;

# \- boundary representation;

# \- collider steering;

# \- SPEC-007 Flow Strength visual behavior;

# \- SPEC-009 advection.

# 

# \---

# 

# \# Core Principle

# 

# Flow Strength B should be generated primarily as a function of progression along the water path.

# 

# Environmental measurements such as channel width may influence this longitudinal profile.

# 

# Avoid uncontrolled per-pixel variation across the width of the river.

# 

# This is critical.

# 

# SPEC-009 testing demonstrated that transverse per-pixel variation can visually tilt or distort streaks even when Flow Direction RG is correct.

# 

# \---

# 

# \# Separation of Direction and Strength

# 

# RG defines:

# 

# where the water travels.

# 

# B defines:

# 

# how strong the visual flow is.

# 

# Changing B must not modify:

# 

# \- local tangent;

# \- pattern coordinate frame;

# \- transverse orientation;

# \- centerline direction.

# 

# Flow Strength must feed only the existing SPEC-007 behavior.

# 

# \---

# 

# \# Width Measurement

# 

# Use the valid-water representation from SPEC-011 to estimate available channel width.

# 

# Preferred conceptual method:

# 

# centerline sample

# → determine local perpendicular direction

# → measure distance to each water boundary

# → combine both distances

# → local channel width

# 

# Exact implementation may vary.

# 

# Do not estimate width simply from arbitrary pixel density or world-axis dimensions.

# 

# Width should relate to the local path orientation.

# 

# \---

# 

# \# Centerline Width Profile

# 

# Evaluate width along the centerline at multiple samples.

# 

# Produce a longitudinal profile:

# 

# Path Parameter

# → Local Width

# 

# Example:

# 

# 0.00 → 3.0 m

# 0.25 → 3.3 m

# 0.50 → 3.1 m

# 0.70 → 5.5 m

# 0.85 → 10.0 m

# 1.00 → 18.0 m

# 

# This profile becomes an input for strength generation.

# 

# \---

# 

# \# Width-to-Strength Heuristic

# 

# Default artistic behavior:

# 

# narrower channel

# → stronger Flow Strength

# 

# broader channel

# → weaker Flow Strength

# 

# This is a stylized heuristic.

# 

# Do not claim physical fluid accuracy.

# 

# The mapping must be tunable.

# 

# \---

# 

# \# Strength Range

# 

# Expose a controllable output range.

# 

# Suggested:

# 

# Minimum Flow Strength

# Maximum Flow Strength

# 

# Generated strength must be mapped into this range.

# 

# Do not assume:

# 

# B = 0

# means absolutely motionless.

# 

# Do not assume:

# 

# B = 1

# means maximum possible shader motion.

# 

# The artist controls the useful visual range.

# 

# \---

# 

# \# Width Influence

# 

# Expose a control equivalent to:

# 

# Width Influence

# 

# At:

# 

# 0

# 

# channel width does not alter Flow Strength.

# 

# At:

# 

# 1

# 

# width strongly influences the generated profile.

# 

# Exact implementation may differ.

# 

# \---

# 

# \# Strength Smoothing

# 

# Raw width measurements may fluctuate because of:

# 

# \- collider edges;

# \- irregular terrain;

# \- grid sampling;

# \- small boundary details.

# 

# Do not translate those fluctuations directly into Flow Strength.

# 

# Apply longitudinal smoothing.

# 

# Possible methods:

# 

# \- moving average;

# \- Gaussian-like smoothing;

# \- curve smoothing;

# \- another lightweight Editor-time approach.

# 

# The final B profile should vary gradually unless an abrupt transition is intentionally configured.

# 

# \---

# 

# \# Small Geometry Rejection

# 

# Very small rocks or boundary details should not automatically create dramatic speed changes.

# 

# Provide a mechanism or smoothing behavior that prevents tiny local width differences from producing:

# 

# fast

# slow

# fast

# slow

# 

# over short distances.

# 

# The system should respond primarily to meaningful channel-scale changes.

# 

# \---

# 

# \# Cross-section Strength Consistency

# 

# For a given centerline parameter, Flow Strength should generally remain similar across the local cross-section.

# 

# Conceptually:

# 

# Left Bank    Center    Right Bank

# 

# 0.65         0.65      0.65

# 

# rather than:

# 

# 0.30         0.80      0.45

# 

# unless a future specialized system intentionally requires that behavior.

# 

# This protects pattern orientation and visual coherence.

# 

# \---

# 

# \# Propagating Strength to the Flow Map

# 

# For each valid Flow Map texel:

# 

# 1\. determine its related centerline parameter;

# 2\. evaluate the smoothed strength profile at that parameter;

# 3\. encode that value into B.

# 

# This should create broad coherent strength bands following river progression.

# 

# \---

# 

# \# River-to-Lake Detection

# 

# A lake-like transition may be inferred when width increases substantially and remains broad for a meaningful distance.

# 

# Do not treat every temporary widening as a lake.

# 

# Possible signals:

# 

# \- sustained width increase;

# \- large width relative to previous river sections;

# \- configurable broad-water threshold.

# 

# Keep detection heuristic and artist-controllable.

# 

# \---

# 

# \# Gradual Transition

# 

# When river width opens into a broad basin:

# 

# do not immediately jump:

# 

# 0.8 → 0.2

# 

# Use a configurable transition/falloff.

# 

# Target:

# 

# 0.8

# → 0.72

# → 0.60

# → 0.45

# → 0.30

# → 0.20

# 

# Exact values depend on artist settings.

# 

# \---

# 

# \# Artist Controls

# 

# Keep controls compact.

# 

# Recommended:

# 

# Minimum Flow Strength

# 

# Maximum Flow Strength

# 

# Width Influence

# 

# Strength Smoothing

# 

# Calm Bias

# 

# Optional:

# 

# Lake Width Threshold

# 

# Avoid exposing large numbers of low-level mathematical parameters.

# 

# \---

# 

# \# Manual Strength Profile

# 

# Preserve the useful SPEC-010 ability to test:

# 

# \- Constant Strength;

# \- authored Strength Falloff.

# 

# Automatic generation should be an additional mode.

# 

# Suggested modes:

# 

# Constant

# Manual/Profile

# Automatic Width

# 

# or equivalent.

# 

# This allows debugging and art direction.

# 

# \---

# 

# \# Optional Override Zones

# 

# A lightweight override mechanism is allowed but not required for initial completion.

# 

# Possible future-friendly overrides:

# 

# Calm Zone

# Fast Zone

# Strength Multiplier

# 

# If implementation becomes large, defer override zones to a separate Spec.

# 

# Do not turn SPEC-012 into a node graph.

# 

# \---

# 

# \# River-to-Lake Validation Scene

# 

# Construct or extend a technical scene containing:

# 

# \- narrow upstream channel;

# \- curved river section;

# \- widening downstream section;

# \- broad final basin.

# 

# The geometry should make the intended behavior obvious.

# 

# \---

# 

# \# Validation A — Direction Isolation

# 

# First verify the generated RG field with constant strength.

# 

# Confirm direction remains correct.

# 

# Do not diagnose direction and strength simultaneously.

# 

# \---

# 

# \# Validation B — Automatic Strength

# 

# Enable automatic width-based strength.

# 

# Expected:

# 

# narrow region

# → stronger flow

# 

# normal river

# → medium flow

# 

# widening region

# → progressively slower

# 

# broad basin

# → calm-like flow

# 

# \---

# 

# \# Validation C — Geometry Change

# 

# Modify the river geometry.

# 

# Examples:

# 

# \- make a narrow section wider;

# \- enlarge the basin;

# \- move a boundary.

# 

# Rebake.

# 

# Confirm Flow Strength responds predictably.

# 

# \---

# 

# \# Visual Validation

# 

# The water shader should visibly change through the existing SPEC-007 system:

# 

# stronger B

# → faster animation

# → longer stretch

# → stronger directional read

# 

# weaker B

# → slower animation

# → broader / calmer shapes

# → reduced pattern prominence

# 

# Do not implement a separate calm-water shader.

# 

# \---

# 

# \# No Transverse Distortion

# 

# Specifically verify that lowering Flow Strength does not cause:

# 

# \- streaks to tilt sideways;

# \- local orientation changes unrelated to RG;

# \- sudden transverse scaling;

# \- pattern deformation.

# 

# This was a previously observed failure mode and must remain fixed.

# 

# \---

# 

# \# Debug Visualization

# 

# Provide useful inspection for:

# 

# \- measured centerline width;

# \- raw width profile;

# \- smoothed width profile;

# \- generated Flow Strength profile;

# \- final B texture/channel.

# 

# The developer should be able to determine why a section became faster or slower.

# 

# \---

# 

# \# External Assets

# 

# No external assets are required.

# 

# All strength information is generated by the baker.

# 

# \---

# 

# \# Performance

# 

# All analysis occurs during Editor bake.

# 

# Runtime continues to consume only the generated Flow Map.

# 

# Do not introduce runtime width detection or geometry analysis.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-012 is complete when:

# 

# \- local channel width can be estimated along the centerline;

# \- raw width measurements are converted into a smooth longitudinal profile;

# \- narrow sections generate higher Flow Strength;

# \- sustained broad sections generate lower Flow Strength;

# \- strength remains coherent across each river cross-section;

# \- river-to-lake transition is gradual;

# \- changing geometry and rebaking updates strength;

# \- B continues driving the existing SPEC-007 behavior;

# \- RG direction remains unchanged by strength generation;

# \- no manual per-pixel painting is required;

# \- no runtime fluid simulation is introduced.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- physically accurate fluid conservation;

# \- pressure solving;

# \- Navier-Stokes;

# \- shallow-water simulation;

# \- runtime geometry analysis;

# \- runtime rebaking every frame;

# \- foam;

# \- waves;

# \- normals;

# \- reflection;

# \- refraction;

# \- VFX.

