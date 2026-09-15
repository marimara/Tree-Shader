# \# Spec 007 — Canopy Depth and Interior Shading

# 

# \## Objective

# 

# Make the foliage canopy feel dense and volumetric.

# 

# Inner foliage should be visually darker than exposed outer foliage.

# 

# The effect should remain stylized and controllable.

# 

# \## Fake Density

# 

# Implement an approximate canopy-depth signal.

# 

# Possible inputs may include:

# 

# \- object-space position

# \- radial distance from canopy center

# \- vertical position

# \- other inexpensive object-local masks

# 

# Prefer simple stable calculations.

# 

# Do not implement expensive ray-based thickness.

# 

# \## Interior Darkening

# 

# Expose:

# 

# \_InteriorColor

# \_InteriorStrength

# \_InteriorRadius or equivalent

# 

# The foliage closer to the perceived canopy interior should receive a controlled darkening/tint.

# 

# The silhouette must remain relatively bright compared with dense interior regions.

# 

# \## Ambient Occlusion

# 

# Support ambient occlusion contribution when available.

# 

# Expose:

# 

# \_AOStrength

# 

# AO should influence color artistically.

# 

# Avoid multiplying the final result by extremely dark AO.

# 

# \## Bottom Bias

# 

# Optionally allow slightly darker foliage toward the bottom/interior of the canopy.

# 

# Expose:

# 

# \_HeightDarkening

# \_HeightGradientPosition

# 

# Keep this subtle.

# 

# \## Desired Result

# 

# The canopy should visually separate into:

# 

# bright exposed outer foliage

# medium-volume foliage

# dark interior pockets

# 

# without showing every card.

# 

# \## Acceptance Criteria

# 

# Turning InteriorStrength from 0 to a useful value should noticeably increase canopy depth.

# 

# The effect must not produce a dark circular blob in the exact geometric center.

# 

# The tree must retain readable foliage detail.

