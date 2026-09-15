# \# SPEC-002 — Stylized Base Water Colors

# 

# \## Objective

# 

# Improve the flat water baseline into a controllable stylized water surface using simple layered water colors.

# 

# The water should begin to establish the project's cyan / turquoise visual identity without implementing motion yet.

# 

# \---

# 

# \# Dependencies

# 

# Requires completed SPEC-001.

# 

# Do not break:

# 

# \- WaterShader\_TestScene;

# \- MAT\_Water\_Test;

# \- existing basic shader rendering.

# 

# \---

# 

# \# Shader Properties

# 

# Add:

# 

# \_ShallowColor

# \_DeepColor

# 

# Optional:

# 

# \_ColorBlend

# 

# If \_BaseColor becomes redundant, migrate cleanly instead of leaving duplicate controls.

# 

# \---

# 

# \# Initial Visual Model

# 

# Create a simple controllable color blend suitable for later depth input.

# 

# For this Spec, the blend may use a temporary scalar property.

# 

# Example concept:

# 

# finalColor = lerp(ShallowColor, DeepColor, ColorBlend)

# 

# This temporary control will later be replaced or driven by water depth.

# 

# \---

# 

# \# Default Style

# 

# The default colors should resemble the intended project direction:

# 

# Shallow:

# bright cyan / turquoise

# 

# Deep:

# more saturated blue / cyan

# 

# Avoid:

# 

# \- dark realistic ocean blue;

# \- grey water;

# \- excessive transparency;

# \- realistic PBR appearance.

# 

# \---

# 

# \# Scene Validation Objects

# 

# Keep Water\_Test.

# 

# If useful, add a second test plane using the same material only if it helps compare different material settings.

# 

# Avoid unnecessary scene complexity.

# 

# \---

# 

# \# Validation

# 

# Confirm:

# 

# \- Shallow Color works;

# \- Deep Color works;

# \- Color Blend clearly transitions between them;

# \- existing opacity behavior remains correct;

# \- no animation exists yet;

# \- no foam exists yet;

# \- Unity compiles without new errors.

# 

# Capture a validation screenshot if practical.

# 

# Suggested path:

# 

# Assets/WaterShader/Validation/SPEC-002/

# 

# \---

# 

# \# Acceptance Criteria

# 

# The shader provides a clean two-color stylized water foundation that can later be driven by depth.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement:

# 

# \- Scene Depth;

# \- flow;

# \- waves;

# \- normals;

# \- foam;

# \- refraction;

# \- Flow Maps;

# \- reflections.

