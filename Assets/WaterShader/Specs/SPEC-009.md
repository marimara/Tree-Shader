# \# SPEC-009 — Flow Map Advection and Curved River Validation

# 

# \## Objective

# 

# Use the Flow Map from SPEC-008 to drive the actual animated stylized pattern across changing flow directions.

# 

# The system must support curved water paths without obvious UV breakdown.

# 

# This Spec completes the initial flow system.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated SPEC-008.

# 

# \---

# 

# \# Local Flow Direction

# 

# Replace the uniform direction used by pattern animation with local Flow Map direction when Flow Map mode is enabled.

# 

# Each sampled region of the surface should move according to its local direction.

# 

# \---

# 

# \# Pattern Orientation

# 

# Stylized pattern orientation and stretch should follow local flow direction.

# 

# Curving flow should not merely change translation direction while leaving visual streaks incorrectly oriented.

# 

# \---

# 

# \# UV Advection

# 

# Implement a robust repeating flow-animation technique suitable for Flow Maps.

# 

# Avoid continuously offsetting UVs in a way that causes visible stretching or degradation over time.

# 

# Preferred strategy:

# 

# dual-phase flow sampling.

# 

# Conceptually:

# 

# \- sample the pattern using two flow phases;

# \- offset phases by approximately half a cycle;

# \- blend between them;

# \- periodically reset progression without visible popping.

# 

# Exact implementation may vary.

# 

# \---

# 

# \# Dual-Phase Requirement

# 

# The flow animation should remain stable during extended Play Mode testing.

# 

# Avoid:

# 

# \- unlimited UV deformation;

# \- visible phase reset popping;

# \- severe stretching;

# \- discontinuous temporal artifacts.

# 

# \---

# 

# \# Flow Strength Integration

# 

# Flow Map B must continue driving:

# 

# \- speed;

# \- pattern stretch;

# \- pattern visibility;

# 

# through the SPEC-007 system.

# 

# Do not duplicate this logic.

# 

# \---

# 

# \# Curved River Test

# 

# Create a simple technical curved-flow test.

# 

# The goal is to clearly demonstrate:

# 

# \- current entering from one direction;

# \- gradually turning;

# \- continuing in another direction;

# \- optionally reducing strength toward a calm region.

# 

# The geometry may remain simple.

# 

# The key validation target is Flow Map behavior, not final environment art.

# 

# \---

# 

# \# Transition Target

# 

# If practical, the technical test should include:

# 

# Fast Flow

# → Curved River

# → Slower Flow

# → Calm Region

# 

# This is the first test that should conceptually resemble the intended:

# 

# river → lake

# 

# transition.

# 

# \---

# 

# \# Debugging

# 

# Retain useful Flow Map debug modes.

# 

# Add temporary phase/advection debugging only if required.

# 

# Do not leave intrusive debug behavior enabled by default.

# 

# \---

# 

# \# Long-Duration Validation

# 

# Run the water animation for an extended period.

# 

# Confirm:

# 

# \- no visible reset pop;

# \- no increasing distortion over time;

# \- no pattern collapse;

# \- direction remains correct along curves;

# \- calm regions remain calm;

# \- fast regions remain readable.

# 

# \---

# 

# \# External Assets

# 

# Reuse:

# 

# T\_FlowMap\_Test

# 

# and the pattern source selected previously.

# 

# No new visual texture should be required.

# 

# If a revised Flow Map is required for the curved-river test, create a technical revision only.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-009 is complete when:

# 

# \- the stylized pattern follows a spatially varying Flow Map;

# \- the pattern visually rotates/orients with flow;

# \- curved flow works;

# \- Flow Strength varies spatially;

# \- animation remains stable over time;

# \- a river-to-calm transition can be demonstrated in one technical setup;

# \- no foam or unrelated future features were added.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- foam;

# \- intersection foam;

# \- turbulence data in Flow Map A;

# \- Flow Map editor tools;

# \- waves;

# \- custom normals;

# \- reflection;

# \- refraction;

# \- waterfall VFX;

# \- interaction.

