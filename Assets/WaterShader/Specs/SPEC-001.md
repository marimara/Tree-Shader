# \# SPEC-001 — Water Shader Foundation and Technical Test Scene

# 

# \## Objective

# 

# Create the isolated technical foundation for development of the Stylized Water Shader.

# 

# This Spec does not attempt to create the final water appearance.

# 

# The goal is to establish:

# 

# \- a dedicated WaterShader test scene;

# \- a minimal handwritten URP water shader;

# \- a test material;

# \- a simple water surface;

# \- a stable baseline for future Specs.

# 

# \---

# 

# \# Required Files

# 

# Create:

# 

# Assets/WaterShader/Scenes/WaterShader\_TestScene.unity

# 

# Assets/WaterShader/Shaders/StylizedWater.shader

# 

# Assets/WaterShader/Materials/MAT\_Water\_Test.mat

# 

# Create the Includes folder if it does not exist:

# 

# Assets/WaterShader/Shaders/Includes/

# 

# Do not create unnecessary include files yet.

# 

# \---

# 

# \# Test Scene

# 

# Create a new independent scene:

# 

# WaterShader\_TestScene

# 

# The scene should contain at minimum:

# 

# \- Main Camera;

# \- Directional Light;

# \- Ground;

# \- Water\_Test.

# 

# Optional simple rocks may be added if useful for visual scale.

# 

# Do not use vegetation or other environment systems yet.

# 

# \---

# 

# \# Ground

# 

# Create a simple large ground plane below the water.

# 

# Use a neutral material.

# 

# The ground exists mainly to:

# 

# \- provide visual context;

# \- make water opacity/transparency readable later;

# \- make depth features easier to test.

# 

# \---

# 

# \# Water Test Surface

# 

# Create a simple horizontal water surface.

# 

# A Unity Plane is acceptable for this Spec.

# 

# Name:

# 

# Water\_Test

# 

# Assign:

# 

# MAT\_Water\_Test

# 

# \---

# 

# \# Shader

# 

# Create a handwritten URP shader:

# 

# StylizedWater.shader

# 

# For this Spec, the shader only needs to provide:

# 

# \- Base Color property;

# \- basic URP rendering;

# \- configurable opacity if required by the chosen surface setup.

# 

# Do not implement:

# 

# \- depth coloring;

# \- foam;

# \- flow;

# \- waves;

# \- refraction;

# \- normals;

# \- reflections;

# \- Flow Maps.

# 

# \---

# 

# \# Initial Properties

# 

# Expose at minimum:

# 

# \_BaseColor

# \_Opacity

# 

# Use readable Inspector display names.

# 

# Example:

# 

# Base Color

# Opacity

# 

# \---

# 

# \# Default Appearance

# 

# Use a saturated cyan / turquoise default color appropriate for the project references.

# 

# The exact final water color is not important yet.

# 

# The important goal is a clean stable water surface.

# 

# \---

# 

# \# Render Setup

# 

# Choose a render configuration suitable as a future foundation for stylized water.

# 

# If transparency is enabled, ensure:

# 

# \- correct blending;

# \- sensible depth behavior;

# \- no obvious sorting issue in the simple test scene.

# 

# Do not implement advanced transparent rendering workarounds yet.

# 

# \---

# 

# \# Validation

# 

# Open:

# 

# WaterShader\_TestScene.unity

# 

# Confirm:

# 

# 1\. scene loads correctly;

# 2\. water surface is visible;

# 3\. MAT\_Water\_Test uses StylizedWater.shader;

# 4\. Base Color changes are visible;

# 5\. Opacity changes are visible if transparency is used;

# 6\. shader compiles without errors;

# 7\. no unrelated project files were changed.

# 

# \---

# 

# \# Acceptance Criteria

# 

# SPEC-001 is complete when:

# 

# \- WaterShader\_TestScene exists;

# \- StylizedWater.shader exists;

# \- MAT\_Water\_Test exists;

# \- a visible test surface uses the material;

# \- the shader is handwritten and does not use Shader Graph;

# \- Unity has no new shader compilation errors.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement future water systems.

# 

# Specifically exclude:

# 

# \- depth gradients;

# \- Flow Maps;

# \- animated flow;

# \- foam;

# \- painterly streaks;

# \- waves;

# \- refraction;

# \- reflection;

# \- interaction;

# \- waterfall VFX.

