# \# Spec 002 — Shader Test Scene

# 

# \## Goal

# 

# Create a controlled Unity scene for developing and evaluating the stylized foliage shader.

# 

# The scene must make lighting behaviour easy to evaluate.

# 

# Do not attempt to create a beautiful game environment.

# 

# The purpose is shader validation.

# 

# \## Scene

# 

# Create:

# 

# Assets/TreeShader/Test/TreeShader\_TestScene.unity

# 

# \## Scene contents

# 

# TreeShader\_TestScene

# ├── Camera

# ├── Lighting

# │   └── Sun

# ├── Environment

# │   └── Ground

# ├── ShaderTests

# │   ├── Tree\_Test

# │   ├── Foliage\_Test

# │   └── Sphere\_Test

# └── Helpers

# 

# \## Tree Test

# 

# Use one of the imported tree assets as:

# 

# Tree\_Test

# 

# Prefer tree1 initially.

# 

# The trunk and leaves must remain independently assignable to materials.

# 

# Do not modify the source model destructively.

# 

# Use an instance/prefab copy for testing.

# 

# \## Foliage Test

# 

# Also place a foliage-only mesh such as:

# 

# leaves1

# 

# This object exists to make alpha, normals and lighting issues easier to inspect without the trunk.

# 

# \## Sphere Test

# 

# Create one regular sphere.

# 

# The sphere is used only to compare the intended light gradient with the foliage lighting.

# 

# \## Ground

# 

# Create a large neutral ground plane.

# 

# Material:

# 

# Base Color:

# neutral middle gray

# 

# Metallic:

# 0

# 

# Smoothness:

# low

# 

# The ground must receive the tree's shadow.

# 

# \## Camera

# 

# Perspective camera.

# 

# Frame the complete test tree.

# 

# Use a neutral background.

# 

# Avoid strong post-processing during initial shader development.

# 

# \## Directional Light

# 

# Create one Directional Light named:

# 

# Sun

# 

# Use this as the URP Main Light.

# 

# Initial configuration should produce a clear but soft readable light direction.

# 

# Suggested starting configuration:

# 

# Rotation X:

# approximately 45 degrees

# 

# Rotation Y:

# approximately -35 degrees

# 

# Intensity:

# approximately 1.0

# 

# Color:

# slightly warm neutral white

# 

# Do not use strongly yellow sunlight.

# 

# Shadows:

# enabled

# 

# Soft Shadows:

# enabled

# 

# Shadow Strength:

# approximately 0.7–0.85

# 

# The exact values may be adjusted if required by the project's current URP configuration.

# 

# \## Environment Lighting

# 

# Use environment/ambient lighting sufficient to prevent completely black shaded foliage.

# 

# Prefer a neutral or slightly cool ambient contribution.

# 

# Avoid an HDRI with strong directional coloration during development.

# 

# The environment must remain visually simple.

# 

# \## Exposure

# 

# Keep exposure consistent during shader development.

# 

# Do not compensate for a shader problem by changing scene exposure.

# 

# \## Validation Views

# 

# The scene should support testing the tree with sunlight from:

# 

# Front-left

# Front-right

# Side

# Back

# 

# Rotate the Directional Light rather than changing the material when evaluating lighting.

# 

# \## Reference Comparison

# 

# Keep the provided target reference accessible during development.

# 

# Target characteristics:

# 

# \- strong rounded canopy volume

# \- bright top/exposed areas

# \- green/teal colored shadow areas

# \- soft transitions

# \- readable foliage silhouette

# 

# Do not attempt pixel-perfect matching.

# 

# Match the lighting principles and overall visual language.

