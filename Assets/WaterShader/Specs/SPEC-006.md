# \# SPEC-006 — Flow Strength and Flow-to-Calm Transition

# 

# \## Objective

# 

# Introduce Flow Strength as a central shader parameter.

# 

# The same shader must visually transition between:

# 

# \- calm water;

# \- slow flowing water;

# \- river water;

# \- strongly directional flowing water.

# 

# This is the first major proof-of-concept for the complete Stylized Water system.

# 

# \---

# 

# \# Dependencies

# 

# Requires SPEC-001 through SPEC-005.

# 

# \---

# 

# \# Core Property

# 

# Add:

# 

# \_FlowStrength

# 

# Range:

# 

# 0 to 1

# 

# Meaning:

# 

# 0 = calm / nearly still water

# 

# 1 = maximum directional flow

# 

# \---

# 

# \# Flow Strength Must Affect More Than Speed

# 

# Flow Strength must not simply multiply Flow Speed.

# 

# It should influence multiple visual characteristics.

# 

# At minimum:

# 

# 1\. effective flow speed;

# 2\. painterly pattern stretch;

# 3\. painterly pattern visibility / strength.

# 

# Optional if useful:

# 

# 4\. pattern distortion;

# 5\. secondary pattern contribution.

# 

# \---

# 

# \# Target Behavior

# 

# \## Flow Strength = 0

# 

# Water should appear calm.

# 

# Expected characteristics:

# 

# \- very slow or nearly static movement;

# \- broad pattern shapes;

# \- minimal directional streaking;

# \- no visual impression of rushing water.

# 

# The water should not look completely frozen.

# 

# A very subtle baseline movement is acceptable.

# 

# \---

# 

# \## Flow Strength \~ 0.3

# 

# Water should resemble slow-moving water.

# 

# Expected characteristics:

# 

# \- visible but gentle motion;

# \- some directional behavior;

# \- broader pattern shapes.

# 

# \---

# 

# \## Flow Strength \~ 0.6

# 

# Water should resemble a river.

# 

# Expected characteristics:

# 

# \- clear directional motion;

# \- more elongated patterns;

# \- stronger visual flow.

# 

# \---

# 

# \## Flow Strength = 1

# 

# Water should resemble fast-moving water suitable as a basis for rapids or waterfall surfaces.

# 

# Expected characteristics:

# 

# \- strong movement;

# \- high directional stretching;

# \- clearly readable streaks.

# 

# Do not implement waterfall foam or particles yet.

# 

# \---

# 

# \# Transition Test Surface

# 

# Create a dedicated test object demonstrating the concept.

# 

# Suggested name:

# 

# FlowTransition\_Test

# 

# The ideal test should show:

# 

# Fast Flow -> Medium Flow -> Slow Flow -> Calm

# 

# within one visual setup.

# 

# For this Spec, this may be accomplished with:

# 

# \- multiple adjacent meshes using materials with different Flow Strength values;

# 

# or

# 

# \- another simple controlled setup.

# 

# A true spatial mask / Flow Map is not required yet.

# 

# \---

# 

# \# Test Materials

# 

# Creating temporary comparison materials is allowed.

# 

# Examples:

# 

# MAT\_Water\_Flow\_00

# MAT\_Water\_Flow\_33

# MAT\_Water\_Flow\_66

# MAT\_Water\_Flow\_100

# 

# If these are only temporary validation materials, keep the number reasonable.

# 

# \---

# 

# \# Important Visual Requirement

# 

# The calm version must not look like:

# 

# "the river texture paused."

# 

# Instead, decreasing Flow Strength must visibly change the character of the pattern.

# 

# This is a critical acceptance requirement.

# 

# \---

# 

# \# Debug

# 

# A Flow Strength debug display may be added if useful.

# 

# \---

# 

# \# Validation

# 

# Capture a comparison showing multiple Flow Strength states.

# 

# Recommended values:

# 

# 0.0

# 0.33

# 0.66

# 1.0

# 

# Verify:

# 

# \- animation progressively accelerates;

# \- patterns progressively stretch;

# \- calm water remains alive but subtle;

# \- fast water feels directional;

# \- depth coloring remains functional;

# \- no existing feature regresses.

# 

# \---

# 

# \# Acceptance Criteria

# 

# The shader convincingly demonstrates a continuous visual family from calm water to fast flowing water.

# 

# This Spec is successful when a viewer can understand the difference in water velocity without needing UI labels.

# 

# \---

# 

# \# Out of Scope

# 

# Do not implement yet:

# 

# \- Flow Maps;

# \- curves in river direction;

# \- foam;

# \- intersection foam;

# \- waves;

# \- normals;

# \- reflection;

# \- refraction;

# \- waterfall impact VFX;

# \- interaction.

