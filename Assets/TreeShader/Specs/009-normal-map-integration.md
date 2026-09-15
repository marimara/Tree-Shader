# \# Spec 009 — Leaf Normal Map Integration

# 

# \## Objective

# 

# Add small leaf-surface normal detail without destroying the large-scale radial canopy shading.

# 

# \## Input

# 

# Use:

# 

# leaf0\_n

# 

# Expose:

# 

# \_NormalMap

# \_NormalStrength

# 

# \## Requirement

# 

# The radial/spherical normal remains responsible for large-scale canopy lighting.

# 

# The leaf normal map should add only smaller-scale surface detail.

# 

# Do not replace the stylized canopy normal with the normal map.

# 

# \## Combination

# 

# Use a mathematically appropriate method for combining normal-map detail with the custom stylized normal.

# 

# Avoid naive normal addition if it produces invalid or unstable results.

# 

# Surface-gradient-style composition or another robust method is acceptable.

# 

# \## Normal Strength

# 

# Default should be subtle.

# 

# Expected useful range:

# 

# approximately 0 to 0.5

# 

# depending on texture strength.

# 

# \## Acceptance Criteria

# 

# At NormalStrength = 0:

# 

# the result matches the previous milestone.

# 

# Increasing NormalStrength introduces visible leaf-surface lighting detail.

# 

# The tree must still read as a unified rounded canopy.

# 

# Individual foliage cards must not become strongly visible again.

