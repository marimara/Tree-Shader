# \# Spec 005 — Stylized Directional Lighting

# 

# \## Objective

# 

# Create a soft stylized lighting model driven by Unity's Main Directional Light.

# 

# Do not use realistic PBR foliage lighting as the primary appearance.

# 

# \## Main Light

# 

# Read URP Main Light direction and shadow information.

# 

# The foliage lighting should react to the actual Directional Light used in the scene.

# 

# \## Lambert Base

# 

# Use a Lambert-like directional relationship as the starting signal:

# 

# N dot L

# 

# Use the stylized/radial normal produced by Spec 004.

# 

# Do NOT directly multiply the final foliage color by raw NdotL.

# 

# The lighting value is a mask used to drive artistic color transitions.

# 

# \## Remapping

# 

# Expose:

# 

# \_ShadowThreshold

# \_ShadowSoftness

# 

# Conceptual result:

# 

# lightingMask =

# smooth transition around the configured threshold

# 

# Low softness:

# more toon-like

# 

# High softness:

# more painterly / soft

# 

# \## Recommended Behaviour

# 

# The transition should produce:

# 

# large light masses

# large shadow masses

# soft border between them

# 

# Avoid:

# 

# small high-frequency lighting changes.

# 

# \## Fake Sun Bias

# 

# Add optional control:

# 

# \_LightDirectionBias

# 

# or an equivalent artist control.

# 

# The purpose is to slightly bias the apparent foliage-lighting direction without rotating the actual Directional Light.

# 

# Keep this subtle.

# 

# Default behaviour should follow the real Main Light.

# 

# \## Realtime Shadows

# 

# URP shadow attenuation must influence stylized shading.

# 

# Do not multiply the final foliage RGB directly by dark shadow values.

# 

# Realtime shadowing should influence the stylized light mask.

# 

# The foliage should remain colored when shadowed.

# 

# \## Properties

# 

# Expose:

# 

# \_ShadowThreshold

# \_ShadowSoftness

# \_ShadowStrength

# \_LightDirectionBias or equivalent

# 

# \## Acceptance Criteria

# 

# Rotating the Directional Light causes the stylized light region to move around the canopy.

# 

# The transition is soft.

# 

# Individual foliage cards do not become visually dominant.

# 

# Realtime shadows affect the tree.

# 

# Shadowed foliage does not become black.

