# \# Spec 003 — Foliage Shader Foundation

# 

# \## Objective

# 

# Create the initial foliage shader foundation for foliage-card geometry.

# 

# This milestone should solve only the basic surface behaviour.

# 

# Do not implement radial normals, stylized shading or wind yet.

# 

# \## Shader

# 

# Create a reusable URP foliage shader.

# 

# Suggested name:

# 

# Meganeura/Stylized Foliage

# 

# or an equivalent project-consistent name.

# 

# \## Required Inputs

# 

# Expose:

# 

# \_BaseMap

# \_AlphaMap

# \_BaseColor

# \_AlphaClipThreshold

# 

# Optional but prepared for future use:

# 

# \_NormalMap

# 

# \## Base Color

# 

# The visible leaf color should initially use:

# 

# BaseMap \* BaseColor

# 

# The BaseColor property should allow tinting the foliage.

# 

# \## Alpha Clipping

# 

# Use the supplied leaf alpha mask.

# 

# Expected behaviour:

# 

# white = visible leaf

# black = discarded area

# 

# Expose:

# 

# \_AlphaClipThreshold

# 

# Suggested initial default:

# 

# 0.5

# 

# The threshold must remain artist adjustable.

# 

# \## Surface

# 

# Rendering must support foliage cards.

# 

# Required:

# 

# \- Alpha Clip

# \- Opaque/Cutout rendering

# \- ZWrite enabled

# \- appropriate shadow casting

# \- receive shadows

# 

# Do not use standard transparent blending.

# 

# \## Double-Sided Foliage

# 

# The foliage must be visible from both sides.

# 

# Backfaces must not disappear when viewing the tree from different directions.

# 

# Implement foliage-appropriate two-sided rendering.

# 

# Do not simply duplicate geometry.

# 

# \## Shadow Casting

# 

# The alpha mask must affect shadow casting.

# 

# The tree should cast leaf-shaped shadows rather than rectangular card shadows.

# 

# \## Material

# 

# Create:

# 

# MAT\_StylizedFoliage\_Test

# 

# Use:

# 

# leaf0\_diff

# leaf0\_alpha

# 

# Do not use leaf0\_n yet.

# 

# \## Acceptance Criteria

# 

# Tree foliage is correctly visible.

# 

# There are no visible opaque rectangles around leaf clusters.

# 

# Backfaces remain visible.

# 

# Leaf silhouette is preserved in cast shadows.

# 

# There are zero shader errors.

# 

# No stylized lighting is required yet.

