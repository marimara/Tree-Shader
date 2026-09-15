# \# SPEC-003 — Depth-Based Water Coloring

# 

# \## Objective

# 

# Replace the temporary manual color blend from SPEC-002 with depth-based water coloring.

# 

# Shallow water should appear brighter.

# 

# Deeper water should gradually transition toward Deep Color.

# 

# \---

# 

# \# Dependencies

# 

# Requires:

# 

# SPEC-001

# SPEC-002

# 

# \---

# 

# \# Depth System

# 

# Use URP scene depth information to estimate the distance between:

# 

# \- the water surface;

# \- visible geometry below the water.

# 

# Use that value to create a normalized Water Depth mask.

# 

# \---

# 

# \# Required Properties

# 

# Expose:

# 

# \_DepthDistance

# 

# Optional:

# 

# \_DepthFalloff

# 

# Use readable names:

# 

# Depth Distance

# Depth Falloff

# 

# \---

# 

# \# Depth Mask

# 

# Expected behavior:

# 

# 0 = shallow / intersection region

# 

# 1 = sufficiently deep water

# 

# Use this mask to drive:

# 

# lerp(ShallowColor, DeepColor, depthMask)

# 

# \---

# 

# \# Stability

# 

# Prefer a depth calculation that remains visually stable across normal camera angles.

# 

# Avoid obviously camera-dependent depth coloration if practical within URP.

# 

# \---

# 

# \# Test Scene Changes

# 

# Modify WaterShader\_TestScene to make depth easy to inspect.

# 

# Add simple underwater geometry such as:

# 

# \- sloped ground;

# \- stepped boxes;

# \- shallow platform;

# \- deeper section.

# 

# Do not create a decorative environment.

# 

# The scene should clearly contain:

# 

# shallow area -> medium depth -> deep area.

# 

# \---

# 

# \# Debug

# 

# A temporary debug mode for displaying the depth mask is allowed.

# 

# If added, expose a property such as:

# 

# \_DebugDepth

# 

# or another simple debug mechanism.

# 

# Do not make debug output the default material state.

# 

# \---

# 

# \# Validation

# 

# Verify:

# 

# 1\. shallow regions use Shallow Color;

# 2\. deeper regions transition to Deep Color;

# 3\. transition distance responds to Depth Distance;

# 4\. camera movement does not cause severe visual instability;

# 5\. SPEC-002 color controls still work;

# 6\. shader compiles correctly.

# 

# \---

# 

# \# Acceptance Criteria

# 

# The test scene clearly demonstrates:

# 

# shallow cyan water -> deeper blue water

# 

# based on actual scene depth.

# 

# \---

# 

# \# Out of Scope

# 

# Do not add:

# 

# \- flow;

# \- foam;

# \- refraction;

# \- waves;

# \- Flow Maps;

# \- normals;

# \- reflections.

