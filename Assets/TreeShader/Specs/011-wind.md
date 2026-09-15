# \# Spec 011 — Stylized Foliage Wind

# 

# \## Objective

# 

# Add subtle stylized movement to foliage-card geometry.

# 

# Wind must not compromise the shader work already completed.

# 

# \## Vertex Animation

# 

# Implement simple vertex-position deformation.

# 

# Expose:

# 

# \_WindDirection

# \_WindStrength

# \_WindSpeed

# \_WindScale

# 

# \## Motion

# 

# Use smooth periodic motion.

# 

# Combine:

# 

# time

# world/object position

# 

# to avoid every leaf card moving identically.

# 

# \## Base Mask

# 

# Avoid moving the trunk.

# 

# Foliage motion may use:

# 

# vertex colors

# UV data

# object position

# or another stable foliage-only mask

# 

# depending on the imported model.

# 

# Do not modify the source model destructively unless explicitly necessary.

# 

# \## Artistic Direction

# 

# The movement should be:

# 

# slow

# soft

# slightly asynchronous

# 

# Avoid:

# 

# violent wobbling

# rubber-like stretching

# all cards moving together

# 

# \## Acceptance Criteria

# 

# Tree silhouette moves subtly.

# 

# The canopy lighting remains stable enough to read correctly.

# 

# No alpha/shadow artifacts appear.

# 

# Wind can be disabled by setting WindStrength to 0.

