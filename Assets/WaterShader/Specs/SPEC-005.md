# \# SPEC-005 — Painterly Flow Pattern

# 

# \## Objective

# 

# Replace the temporary flow visualization with a stylized painterly surface pattern inspired by the project's primary visual reference.

# 

# This is the first Spec where the water should begin to resemble the intended final visual language.

# 

# \---

# 

# \# Dependencies

# 

# Requires SPEC-001 through SPEC-004.

# 

# \---

# 

# \# Visual Target

# 

# Use the project's references in:

# 

# Assets/WaterShader/References/

# 

# Primary moving-water characteristics:

# 

# \- elongated directional streaks;

# \- irregular brush-like shapes;

# \- multiple sizes of marks;

# \- graphic cyan / light-cyan highlights;

# \- readable movement;

# \- limited fine noise.

# 

# Do not reproduce the reference shader exactly.

# 

# Use it as visual inspiration only.

# 

# \---

# 

# \# Pattern Source

# 

# The system may use:

# 

# \- one authored grayscale texture;

# \- multiple authored textures;

# \- procedural pattern generation;

# \- procedural distortion combined with a texture.

# 

# Choose the simplest solution that produces a convincing stylized result.

# 

# Any new texture belongs in:

# 

# Assets/WaterShader/Textures/

# 

# \---

# 

# \# Shader Properties

# 

# Expose useful controls such as:

# 

# \_PatternScale

# \_PatternSpeed

# \_PatternStrength

# \_PatternColor

# \_PatternStretch

# 

# Exact names may vary if the implementation is cleaner.

# 

# Avoid exposing redundant controls.

# 

# \---

# 

# \# Pattern Movement

# 

# The painterly pattern must follow the Flow Direction created in SPEC-004.

# 

# Changing Flow Direction must change both:

# 

# \- movement direction;

# \- visual orientation where appropriate.

# 

# \---

# 

# \# Pattern Stretch

# 

# Introduce directional stretching.

# 

# Low Stretch:

# broader / more organic shapes.

# 

# High Stretch:

# long directional streaks.

# 

# This system will later be driven by Flow Strength.

# 

# For this Spec, manual control is acceptable.

# 

# \---

# 

# \# Layering

# 

# If needed, use two pattern samples with different:

# 

# \- scales;

# \- offsets;

# \- speeds.

# 

# Avoid excessive texture sampling.

# 

# The pattern should remain readable rather than noisy.

# 

# \---

# 

# \# Validation

# 

# Test at least three material configurations:

# 

# Calm-like:

# low speed

# low stretch

# 

# River-like:

# medium speed

# medium stretch

# 

# Waterfall-like:

# high speed

# high stretch

# 

# These are only visual tests.

# 

# Do not yet automate the transition between them.

# 

# \---

# 

# \# Acceptance Criteria

# 

# The shader clearly produces stylized flowing brush-like patterns that follow Flow Direction and respond to Flow Speed and Pattern Stretch.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- Flow Maps;

# \- automatic Flow Strength transition;

# \- foam;

# \- waterfall particles;

# \- refraction;

# \- interaction.

