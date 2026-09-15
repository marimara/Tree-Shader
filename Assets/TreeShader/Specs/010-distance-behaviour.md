# \# Spec 010 — Distance Based Behaviour

# 

# \## Objective

# 

# Adjust foliage detail according to camera distance.

# 

# Do not dramatically change the tree's overall color.

# 

# \## Distance Mask

# 

# Expose:

# 

# \_DistanceStart

# \_DistanceEnd

# 

# Generate a smooth camera-distance mask.

# 

# \## Potential Uses

# 

# At greater distance:

# 

# reduce normal-map influence

# reduce high-frequency normal noise

# reduce fine color variation

# 

# Maintain:

# 

# large stylized lighting

# main color ramp

# canopy silhouette

# 

# \## Goal

# 

# Distant trees should appear cleaner rather than noisier.

# 

# Avoid visible popping.

# 

# All transitions must be smooth.

# 

# \## Acceptance Criteria

# 

# Moving the camera away does not cause sudden visual changes.

# 

# High-frequency detail gradually reduces.

# 

# The tree's main silhouette and stylized light/shadow structure remain stable.

