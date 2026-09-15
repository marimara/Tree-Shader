# \# Spec 006 — Artistic Foliage Color System

# 

# \## Objective

# 

# Give the artist direct control over foliage light, midtone and shadow colors.

# 

# The final appearance should be color-designed rather than generated only by physical light intensity.

# 

# \## Required Colors

# 

# Expose:

# 

# \_LightColor

# \_MidColor

# \_ShadowColor

# \_DeepShadowColor

# 

# Optional:

# 

# \_ColorIntensity

# 

# \## Color Ramp

# 

# Use the stylized lighting mask from Spec 005 to drive transitions between artistic foliage colors.

# 

# Conceptual ramp:

# 

# Deep Shadow

# → Shadow

# → Midtone

# → Light

# 

# Do not interpret this as a hard four-band toon shader.

# 

# Transitions should remain smooth unless deliberately configured otherwise.

# 

# \## Color Character

# 

# The shader must allow:

# 

# Light:

# yellow-green / warm green

# 

# Mid:

# saturated foliage green

# 

# Shadow:

# emerald / blue-green

# 

# Deep shadow:

# cool dark green / teal

# 

# This is an artistic direction, not a fixed palette.

# 

# All colors must remain editable.

# 

# \## Saturated Shadows

# 

# Avoid reducing foliage shadows to gray or black.

# 

# Dark areas should retain strong color information.

# 

# \## Main Light Color

# 

# Allow the Unity Main Light color to contribute without overpowering the artistic palette.

# 

# The material colors should remain the primary artistic control.

# 

# \## Reference Target

# 

# The target reference shows:

# 

# bright yellow-green exposed foliage

# saturated medium green

# cool darker interior foliage

# 

# The canopy remains colorful even in shadow.

# 

# \## Acceptance Criteria

# 

# The tree can be recolored substantially using material properties alone.

# 

# Changing ShadowColor changes shaded foliage without affecting only brightness.

# 

# There should be no dependency on editing the source BaseMap to achieve the desired palette.

