# \# Spec 008 — Foliage Variation

# 

# \## Objective

# 

# Break visual repetition without destroying the large-scale canopy lighting.

# 

# Variation must remain secondary to the main stylized shading.

# 

# \## Normal Noise

# 

# Introduce low-frequency noise into the stylized normal direction.

# 

# Expose:

# 

# \_NormalNoiseStrength

# \_NormalNoiseScale

# 

# The purpose is to prevent the canopy from looking like a mathematically perfect sphere.

# 

# Noise must remain subtle.

# 

# \## Color Variation

# 

# Expose:

# 

# \_ColorVariationStrength

# \_ColorVariationScale

# 

# Use object/world-space variation to introduce slight foliage color differences.

# 

# Avoid visible procedural-noise patterns.

# 

# \## Per-Island / Per-Card Variation

# 

# If the mesh data provides a robust way to identify foliage cards or UV islands, support subtle per-island variation.

# 

# Possible effects:

# 

# small hue offset

# small brightness offset

# 

# Do not require mesh restructuring unless necessary.

# 

# \## UV Variation

# 

# If appropriate for the imported foliage atlas, support controlled UV variation to reduce obvious texture repetition.

# 

# Do not break atlas boundaries.

# 

# Do not implement UV randomization if it causes sampling from incorrect leaf regions.

# 

# \## Priority

# 

# Large-scale canopy lighting must remain dominant.

# 

# The user should perceive:

# 

# tree volume first

# foliage variation second

# 

# \## Acceptance Criteria

# 

# Variation reduces repetitive appearance.

# 

# No obvious noise crawling or shimmering.

# 

# No foliage cards receive dramatically different colors.

# 

# No UV seams or incorrect atlas sampling.

