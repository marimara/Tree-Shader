# \# SPEC-006 — Stylized Flow Pattern Shaping

# 

# \## Objective

# 

# Transform the hybrid pattern source selected in SPEC-005 into the project's actual stylized directional water language.

# 

# SPEC-005 established that the hybrid approach using the selected Noise 1 texture is the preferred pattern source.

# 

# SPEC-006 must now convert that raw moving noise into deliberate, clean, stylized water marks.

# 

# The result should no longer read primarily as:

# 

# "noise moving across a surface."

# 

# It should begin to read as:

# 

# "stylized directional water highlights moving with the current."

# 

# Do not implement foam, Flow Maps, waterfall particles, reflection, refraction, or other later systems.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated:

# 

# \- SPEC-004

# \- SPEC-005

# 

# Read the SPEC-005 completion report before implementation.

# 

# Use the pattern-source approach selected in SPEC-005:

# 

# hybrid texture + procedural shaping

# 

# using the selected Noise 1 texture as the primary authored noise source.

# 

# Do not restart the pattern-source comparison unless the validated implementation is technically unusable.

# 

# Preserve:

# 

# \- depth-based coloring;

# \- shallow/deep colors;

# \- opacity;

# \- uniform flow direction;

# \- flow speed;

# \- WaterShader\_TestScene.

# 

# \---

# 

# \# Primary Visual Problem

# 

# The current SPEC-005 result is technically functional but visually still too close to raw animated noise.

# 

# SPEC-006 must reduce that impression.

# 

# The pattern should become:

# 

# \- cleaner;

# \- more graphic;

# \- more selective;

# \- more directional;

# \- less continuously noisy;

# \- easier to read from a distance.

# 

# The shader should create intentional highlight marks rather than displaying most of the noise texture directly.

# 

# \---

# 

# \# Visual References

# 

# Use the visual references in:

# 

# Assets/WaterShader/References/

# 

# The references are visual targets only.

# 

# Do not attempt to reproduce or reverse-engineer a commercial shader.

# 

# Focus on these characteristics:

# 

# \- strong readable directional motion;

# \- isolated highlight-like streaks;

# \- elongated painterly shapes;

# \- large and medium forms rather than fine grain;

# \- visible negative space between marks;

# \- controlled irregularity;

# \- bright cyan / pale cyan / near-white highlights;

# \- clearly stylized game-art appearance.

# 

# \---

# 

# \# Important Visual Requirement

# 

# The final pattern must not cover the surface uniformly.

# 

# There must be visible regions where little or no pattern is present.

# 

# The pattern should behave more like:

# 

# "moving stylized highlights"

# 

# than:

# 

# "continuous animated texture."

# 

# Avoid a surface that looks completely filled with moving noise.

# 

# \---

# 

# \# Pattern Shaping Pipeline

# 

# Build a clear shaping pipeline from the selected hybrid source.

# 

# The implementation may use:

# 

# \- contrast adjustment;

# \- remapping;

# \- Smoothstep;

# \- thresholding;

# \- directional UV scaling;

# \- flow-relative stretching;

# \- procedural distortion;

# \- selective masking;

# \- multiplication between pattern layers.

# 

# Use only the operations that materially improve the result.

# 

# Do not add complexity for its own sake.

# 

# A conceptual pipeline may resemble:

# 

# raw pattern

# → directional UV transform

# → stretch

# → contrast / remap

# → threshold or Smoothstep

# → selective highlight mask

# → pattern color / intensity

# 

# The exact implementation may differ.

# 

# \---

# 

# \# Threshold and Coverage

# 

# The shader must provide a way to control how much of the surface receives visible pattern.

# 

# Expose a control equivalent to:

# 

# \_PatternThreshold

# 

# This control should allow the pattern to move between:

# 

# \- broad high-coverage noise;

# \- cleaner separated marks;

# \- sparse isolated highlights.

# 

# Expose a softness control if required:

# 

# \_PatternSoftness

# 

# Threshold and softness should work together without producing severe aliasing or harsh pixel noise.

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

# \_PatternThreshold

# 

# Add:

# 

# \_PatternSoftness

# 

# if it materially improves the threshold transition.

# 

# Exact internal names may differ if there is a clear technical reason.

# 

# Avoid duplicate controls affecting the same behavior.

# 

# \---

# 

# \# Pattern Scale

# 

# Pattern Scale should control the overall size of the visible water marks.

# 

# Lower-frequency / larger forms are preferred.

# 

# Avoid defaults dominated by tiny repeated details.

# 

# The default result should contain a readable mixture of:

# 

# \- a few larger forms;

# \- medium directional forms;

# \- limited small detail.

# 

# Do not make fine noise the dominant visual component.

# 

# \---

# 

# \# Directional Stretch

# 

# Pattern Stretch must operate relative to Flow Direction.

# 

# It must not simply stretch texture-space X or Y regardless of current direction.

# 

# Low Stretch:

# 

# \- broader forms;

# \- softer directional character;

# \- less obvious streaking.

# 

# Medium Stretch:

# 

# \- clear river-like directional highlights.

# 

# High Stretch:

# 

# \- long directional streaks;

# \- suitable as a base for fast water and future waterfall behavior.

# 

# Changing Flow Direction must also change the effective orientation of the stretched pattern.

# 

# \---

# 

# \# Pattern Color

# 

# The shaped pattern should be applied as a highlight/detail layer over the existing depth-based water color.

# 

# Use a light cyan / pale cyan / near-white default.

# 

# Pattern Color must not replace:

# 

# \- Shallow Color;

# \- Deep Color;

# \- depth-based blending.

# 

# The base water body should remain clearly visible beneath the pattern.

# 

# \---

# 

# \# Pattern Strength

# 

# Pattern Strength should control the contribution of the stylized highlights.

# 

# At low values:

# 

# \- pattern is subtle.

# 

# At medium values:

# 

# \- pattern is clearly readable but does not dominate the whole surface.

# 

# At high values:

# 

# \- pattern becomes strong enough for fast-flow / waterfall-like testing.

# 

# Avoid default values that make the water mostly white.

# 

# \---

# 

# \# Negative Space

# 

# Negative space is a required part of the visual target.

# 

# At the default River-like configuration:

# 

# \- large portions of the water base color should remain visible;

# \- highlight marks should be visually separated;

# \- the result should not resemble marble;

# \- the result should not resemble an evenly distributed noise field.

# 

# If the pattern looks too dense, prefer:

# 

# \- thresholding;

# \- scale adjustment;

# \- selective masking;

# 

# before adding more texture samples.

# 

# \---

# 

# \# Layering

# 

# A second sample from the same selected source is allowed only if it clearly improves:

# 

# \- breakup;

# \- repetition;

# \- shape variety.

# 

# If used, the second sample may vary in:

# 

# \- scale;

# \- speed;

# \- offset;

# \- threshold.

# 

# Do not automatically use two layers.

# 

# Start with the minimum required setup.

# 

# If one hybrid layer is sufficient, keep one.

# 

# \---

# 

# \# Procedural Distortion

# 

# The existing hybrid approach may retain inexpensive procedural distortion.

# 

# Distortion should:

# 

# \- break repetition;

# \- add slight organic variation;

# \- avoid making the pattern look wavy or chaotic.

# 

# Avoid distortion strong enough to create:

# 

# \- obvious S-shaped waves;

# \- marble patterns;

# \- turbulent noise unrelated to Flow Direction.

# 

# \---

# 

# \# Calm / River / Fast Flow Test Presets

# 

# Create or validate three manual visual states.

# 

# These are temporary validation states only.

# 

# Automatic control belongs to SPEC-007.

# 

# \---

# 

# \## Calm-like

# 

# Use:

# 

# \- low Flow Speed;

# \- low Pattern Stretch;

# \- low Pattern Strength;

# \- relatively broad pattern forms;

# \- sparse visible highlights.

# 

# Target:

# 

# \- subtle moving water;

# \- no impression of fast current;

# \- broad clean visual language.

# 

# Do not attempt final calm-water behavior yet.

# 

# \---

# 

# \## River-like

# 

# Use:

# 

# \- medium Flow Speed;

# \- medium Pattern Stretch;

# \- medium Pattern Strength;

# \- threshold producing separated directional highlights.

# 

# This is the most important visual target for SPEC-006.

# 

# The river state should clearly move closer to the visual references.

# 

# Target:

# 

# \- readable directional streaks;

# \- clean gaps between marks;

# \- large stylized forms;

# \- minimal fine noise;

# \- no raw-noise appearance.

# 

# \---

# 

# \## Fast / Waterfall-like

# 

# Use:

# 

# \- high Flow Speed;

# \- high Pattern Stretch;

# \- stronger Pattern Strength;

# \- elongated highlight forms.

# 

# Target:

# 

# \- long directional streaks;

# \- visually strong motion;

# \- useful base for future waterfall work.

# 

# Do not add:

# 

# \- foam;

# \- waterfall edge masks;

# \- particles;

# \- impact effects.

# 

# \---

# 

# \# Comparison Against SPEC-005

# 

# SPEC-006 validation should explicitly compare the shaped result against the raw SPEC-005 hybrid result.

# 

# The improvement should be visible.

# 

# SPEC-006 should reduce:

# 

# \- continuous noise appearance;

# \- visual clutter;

# \- even surface coverage.

# 

# SPEC-006 should increase:

# 

# \- shape intentionality;

# \- negative space;

# \- directional readability;

# \- clean stylized highlight forms.

# 

# \---

# 

# \# Validation

# 

# Confirm:

# 

# \- Flow Direction still controls movement direction;

# \- Flow Speed still works;

# \- pattern orientation follows Flow Direction;

# \- Pattern Stretch follows Flow Direction;

# \- Pattern Scale is controllable;

# \- Pattern Threshold changes visible coverage;

# \- Pattern Strength changes visual contribution;

# \- thresholding does not create severe aliasing;

# \- the default River-like state is cleaner than the SPEC-005 raw hybrid state;

# \- strong stretch produces useful long streaks;

# \- low stretch produces broad shapes without obvious artifacts;

# \- depth coloring remains fully functional;

# \- opacity remains functional.

# 

# Capture comparison validation.

# 

# Recommended captures:

# 

# \- raw SPEC-005 hybrid;

# \- SPEC-006 Calm-like;

# \- SPEC-006 River-like;

# \- SPEC-006 Fast-like.

# 

# \---

# 

# \# External Assets

# 

# Use the existing pattern assets selected during SPEC-005.

# 

# Do not create or import new:

# 

# \- noise textures;

# \- distortion textures;

# \- alpha textures;

# \- masks;

# \- foam textures;

# \- normal maps.

# 

# If the selected Noise 1 source demonstrably cannot produce the required result even after reasonable shaping, stop and report:

# 

# \- what failed visually;

# \- what additional asset would be required;

# \- why the existing source is insufficient.

# 

# Do not silently add another texture.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-006 is complete when:

# 

# \- the hybrid noise source has been transformed into deliberate stylized water highlights;

# \- the surface no longer primarily reads as raw moving noise;

# \- River-like behavior clearly shows separated directional streaks;

# \- visible negative space exists between highlight marks;

# \- Pattern Stretch behaves relative to flow direction;

# \- Pattern Threshold provides useful control over pattern coverage;

# \- Calm-like, River-like, and Fast-like manual states are visibly different;

# \- all three remain part of the same visual language;

# \- depth-based coloring remains functional;

# \- no foam or later-system features were introduced.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- automatic Flow Strength transition;

# \- Flow Maps;

# \- local curved flow;

# \- foam;

# \- intersection foam;

# \- waterfall foam;

# \- waterfall edge masks;

# \- waterfall particles;

# \- waves;

# \- custom normals;

# \- reflection;

# \- refraction;

# \- interaction.

