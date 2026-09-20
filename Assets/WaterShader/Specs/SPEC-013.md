# \# SPEC-013 — Flow Strength Override Zones

# 

# \## Objective

# 

# Add lightweight artist-controlled Flow Strength overrides to the Flow Map Baker.

# 

# Automatic generation from SPEC-012 remains the default.

# 

# Override zones allow deliberate local art direction without manually painting the Flow Map.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated SPEC-012.

# 

# \---

# 

# \# Core Principle

# 

# Automatic generation produces the base Flow Strength.

# 

# Override zones modify that result.

# 

# Conceptually:

# 

# Automatic Strength

# \+

# Artist Override

# → Final Flow Strength B

# 

# Do not replace the automatic system.

# 

# \---

# 

# \# Zone Types

# 

# Support a small set of simple zone behaviors.

# 

# At minimum consider:

# 

# Strength Multiplier

# 

# Optional:

# 

# Force Calm

# Force Fast

# 

# Prefer one flexible multiplier system if it can cover the use cases cleanly.

# 

# \---

# 

# \# Zone Shape

# 

# Use simple scene volumes.

# 

# Examples:

# 

# Box

# Sphere

# 

# or another lightweight volume representation.

# 

# Avoid complex mesh painting.

# 

# \---

# 

# \# Blending

# 

# Zone influence must fade smoothly near its edges.

# 

# Expose:

# 

# Blend Distance

# 

# or equivalent.

# 

# Avoid abrupt B discontinuities.

# 

# \---

# 

# \# Multiple Zones

# 

# If multiple zones overlap, define deterministic behavior.

# 

# Prefer a simple combination model.

# 

# Examples:

# 

# multiplication

# or

# weighted blending.

# 

# Document the chosen behavior.

# 

# \---

# 

# \# Use Cases

# 

# The system should support scenarios such as:

# 

# \- artificially calm pool;

# \- intentionally fast rapid;

# \- reduced flow near lake entrance;

# \- stronger current near waterfall approach.

# 

# Do not implement foam or waterfall behavior yet.

# 

# \---

# 

# \# Debug Visualization

# 

# Display zones in Scene View.

# 

# Provide clear indication of:

# 

# \- bounds;

# \- influence;

# \- multiplier/value.

# 

# \---

# 

# \# Baking

# 

# Overrides are applied during Editor bake.

# 

# Do not evaluate zones every frame at runtime.

# 

# The final result remains encoded into Flow Map B.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-013 is complete when:

# 

# \- automatic SPEC-012 strength remains functional;

# \- an artist can locally increase or decrease Flow Strength;

# \- influence blends smoothly;

# \- rebaking updates B;

# \- RG direction remains unchanged;

# \- no manual texture painting is required.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- Flow Direction override;

# \- runtime volumes;

# \- foam;

# \- turbulence;

# \- waves;

# \- VFX.

