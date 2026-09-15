# \# SPEC-002 — Stylized Base Water Colors

# 

# \## Objective

# 

# Improve the flat water baseline created in SPEC-001 into a controllable stylized water surface using two layered water colors.

# 

# The purpose of this Spec is to establish the cyan / turquoise color language of the WaterShader before introducing real scene-depth calculations.

# 

# This Spec does not implement actual shallow/deep detection yet.

# 

# The transition between shallow and deep colors will temporarily be controlled manually.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed and validated SPEC-001.

# 

# Preserve:

# 

# \- Assets/WaterShader/Scenes/WaterShader\_TestScene.unity

# \- Assets/WaterShader/Shaders/StylizedWater.shader

# \- Assets/WaterShader/Materials/MAT\_Water\_Test.mat

# \- existing opacity behavior;

# \- existing basic URP rendering.

# 

# Do not regress any validated SPEC-001 behavior.

# 

# \---

# 

# \# Required Shader Properties

# 

# Add:

# 

# \_ShallowColor

# \_DeepColor

# \_ColorBlend

# 

# Use readable Inspector names:

# 

# Shallow Color

# Deep Color

# Color Blend

# 

# \_ColorBlend must use a 0–1 range.

# 

# Meaning:

# 

# 0 = fully Shallow Color

# 

# 1 = fully Deep Color

# 

# \---

# 

# \# Base Color Migration

# 

# SPEC-001 introduced:

# 

# \_BaseColor

# 

# If the final surface color is now fully controlled by:

# 

# \_ShallowColor

# \_DeepColor

# \_ColorBlend

# 

# then \_BaseColor should be removed.

# 

# Do not retain multiple redundant properties controlling the same final color.

# 

# If \_BaseColor is required for a clear technical reason, document that reason in the completion report.

# 

# \---

# 

# \# Initial Visual Model

# 

# Create a simple manual color blend.

# 

# Conceptually:

# 

# finalColor = lerp(ShallowColor, DeepColor, ColorBlend)

# 

# For this Spec, \_ColorBlend is intentionally manual.

# 

# It exists only to validate the two-color system.

# 

# A future Spec will replace or drive this value using actual water depth.

# 

# \---

# 

# \# Default Visual Direction

# 

# Use colors compatible with the project's visual references.

# 

# \## Shallow Color

# 

# Bright cyan / turquoise.

# 

# It should feel:

# 

# \- luminous;

# \- saturated;

# \- stylized;

# \- suitable for shallow water.

# 

# \## Deep Color

# 

# A somewhat deeper and more saturated blue / cyan.

# 

# It should remain colorful.

# 

# Avoid turning the deep water into dark realistic ocean blue.

# 

# \---

# 

# \# Suggested Initial Values

# 

# Choose visually appropriate defaults close to:

# 

# Shallow Color:

# bright turquoise / cyan

# 

# Deep Color:

# stronger blue / cyan

# 

# Color Blend:

# approximately 0.35

# 

# Opacity:

# preserve the validated SPEC-001 default unless a minor adjustment is necessary.

# 

# Exact final colors are not locked by this Spec.

# 

# The goal is establishing a useful starting palette.

# 

# \---

# 

# \# Visual Style Requirements

# 

# The result should begin moving toward the intended stylized water direction.

# 

# Prefer:

# 

# \- saturated colors;

# \- clean readable color shapes;

# \- bright cyan/turquoise identity.

# 

# Avoid:

# 

# \- dark realistic ocean colors;

# \- grey or desaturated water;

# \- excessive transparency;

# \- realistic PBR water appearance;

# \- metallic appearance;

# \- physically realistic water simulation.

# 

# \---

# 

# \# Test Scene

# 

# Continue using:

# 

# Assets/WaterShader/Scenes/WaterShader\_TestScene.unity

# 

# Keep:

# 

# Water\_Test

# 

# Do not build underwater terrain for depth testing yet.

# 

# Actual shallow/deep scene geometry belongs to the future depth-based Spec.

# 

# \---

# 

# \# Optional Comparison Surface

# 

# If useful for validation, a second simple water plane may temporarily be added.

# 

# For example:

# 

# Water\_Shallow\_Test

# 

# Water\_Deep\_Test

# 

# Both should use the same shader.

# 

# They may use separate material instances with different \_ColorBlend values.

# 

# Example:

# 

# Shallow test:

# Color Blend = 0

# 

# Deep test:

# Color Blend = 1

# 

# This is optional.

# 

# Do not complicate the scene if changing the Color Blend slider on MAT\_Water\_Test is already sufficient for validation.

# 

# \---

# 

# \# Textures and External Assets

# 

# SPEC-002 requires no external textures.

# 

# Do not create or import:

# 

# \- noise textures;

# \- distortion textures;

# \- foam textures;

# \- alpha textures;

# \- masks;

# \- normal maps;

# \- gradient textures;

# \- Flow Maps.

# 

# Do not create custom terrain or water meshes for this Spec.

# 

# \---

# 

# \# Validation

# 

# Open:

# 

# Assets/WaterShader/Scenes/WaterShader\_TestScene.unity

# 

# Validate the following.

# 

# \## Shallow Color

# 

# Set:

# 

# Color Blend = 0

# 

# Confirm that the water displays the Shallow Color.

# 

# Change Shallow Color.

# 

# Confirm that the surface updates correctly.

# 

# \---

# 

# \## Deep Color

# 

# Set:

# 

# Color Blend = 1

# 

# Confirm that the water displays the Deep Color.

# 

# Change Deep Color.

# 

# Confirm that the surface updates correctly.

# 

# \---

# 

# \## Blend

# 

# Test multiple Color Blend values.

# 

# Suggested:

# 

# 0.0

# 0.25

# 0.5

# 0.75

# 1.0

# 

# Confirm that the transition between Shallow Color and Deep Color is smooth and predictable.

# 

# \---

# 

# \## Existing Features

# 

# Confirm that:

# 

# \- Opacity still functions correctly;

# \- the water remains visible;

# \- the shader still renders correctly in URP;

# \- no animation exists;

# \- no foam exists;

# \- no depth calculation exists;

# \- no future water systems were introduced.

# 

# \---

# 

# \# Validation Output

# 

# If practical, create:

# 

# Assets/WaterShader/Validation/SPEC-002/

# 

# Save a useful comparison screenshot demonstrating the color range.

# 

# Prefer a comparison that clearly shows:

# 

# Shallow Color

# Intermediate Blend

# Deep Color

# 

# Do not create unnecessary validation assets.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-002 is complete when:

# 

# \- \_ShallowColor exists and works;

# \- \_DeepColor exists and works;

# \- \_ColorBlend exists as a 0–1 control;

# \- the shader smoothly blends between the two colors;

# \- the default palette follows the intended cyan / turquoise visual direction;

# \- existing opacity behavior still works;

# \- redundant \_BaseColor behavior has been removed or justified;

# \- Unity reports no new shader compilation errors;

# \- no future-spec features were implemented.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- Scene Depth;

# \- depth-based color calculation;

# \- depth gradients;

# \- underwater terrain systems;

# \- Flow Maps;

# \- flow direction;

# \- animated flow;

# \- waves;

# \- vertex displacement;

# \- normals;

# \- normal maps;

# \- foam;

# \- intersection foam;

# \- refraction;

# \- reflection;

# \- water interaction;

# \- waterfall effects;

# \- waterfall VFX.

