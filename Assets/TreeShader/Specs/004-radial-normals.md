# \# Spec 004 — Radial / Spherical Canopy Normals

# 

# \## Objective

# 

# Prevent individual foliage cards from looking independently lit.

# 

# The tree canopy should respond to directional lighting as a larger rounded volume.

# 

# \## Problem

# 

# Using the original foliage-card normals causes each plane to produce an independent lighting response.

# 

# This reveals the card structure.

# 

# The desired stylized tree should instead read as one or more rounded foliage volumes.

# 

# \## Stylized Normal

# 

# Implement a radial/spherical normal representation for foliage lighting.

# 

# Conceptually:

# 

# StylizedNormal =

# normalize(VertexPosition - CanopyCenter)

# 

# The exact implementation may use object-space or world-space data as appropriate.

# 

# \## Normal Blend

# 

# Expose:

# 

# \_StylizedNormalStrength

# 

# Range:

# 

# 0 to 1

# 

# Behaviour:

# 

# 0 =

# use original mesh normals

# 

# 1 =

# use full radial/spherical canopy normals

# 

# Intermediate values =

# blend between the two.

# 

# \## Transform Safety

# 

# The custom normal calculation must behave correctly when the object is:

# 

# \- translated

# \- rotated

# \- uniformly scaled

# 

# Avoid calculations that only work at world origin.

# 

# If non-uniform object scale produces limitations, document them.

# 

# \## Canopy Center

# 

# Provide a practical way to define the radial-normal origin.

# 

# Preferred solutions, in order:

# 

# 1\. derive from appropriate object-space origin if the foliage mesh is authored correctly;

# 2\. expose an adjustable canopy-center offset;

# 3\. use another robust per-object method.

# 

# Expose if required:

# 

# \_CanopyCenterOffset

# 

# \## Desired Visual Behaviour

# 

# When sunlight moves around the tree:

# 

# the whole canopy should show a broad light-facing region and broad shaded region.

# 

# Avoid:

# 

# \- alternating bright/dark foliage cards

# \- sudden lighting discontinuities based on card orientation

# \- obvious flat planes

# 

# \## Test

# 

# Compare:

# 

# \_StylizedNormalStrength = 0

# 

# against:

# 

# \_StylizedNormalStrength = 1

# 

# The difference must be visually obvious on Tree\_Test.

# 

# \## Acceptance Criteria

# 

# At high StylizedNormalStrength:

# 

# the tree reads more like a rounded canopy than a pile of intersecting planes.

# 

# The foliage silhouette remains unchanged.

# 

# Alpha clipping remains correct.

# 

# Realtime shadows remain correct.

# 

# There are no shader compilation errors.

