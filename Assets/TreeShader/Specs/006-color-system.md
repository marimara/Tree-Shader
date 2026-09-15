# \# Spec 006 — Artistic Foliage Color System

# 

# \## Objective

# 

# Give the artist direct control over foliage light, midtone and shadow colors.

# 

# The final appearance should be color-designed rather than generated only by physical light intensity.

# 

# The shader must be able to substantially recolor the foliage without requiring edits to the source texture.

# 

# The target is a stylized palette where the material colors define the visual identity of the tree while the source texture contributes local detail and variation.

# 

# \---

# 

# \## Required Colors

# 

# Expose:

# 

# \- `\_LightColor`

# \- `\_MidColor`

# \- `\_ShadowColor`

# \- `\_DeepShadowColor`

# 

# Optional:

# 

# \- `\_ColorIntensity`

# 

# Do not expose additional color controls unless they provide a clear artistic purpose.

# 

# \---

# 

# \## Artistic Color Ramp

# 

# Use the stylized lighting mask from Spec 005 to drive transitions between artistic foliage colors.

# 

# Conceptual ramp:

# 

# `Deep Shadow`

# → `Shadow`

# → `Midtone`

# → `Light`

# 

# The four colors represent regions along the stylized lighting response.

# 

# Conceptual starting distribution:

# 

# \- `0.00` → `\_DeepShadowColor`

# \- `\~0.33` → `\_ShadowColor`

# \- `\~0.66` → `\_MidColor`

# \- `1.00` → `\_LightColor`

# 

# These positions are conceptual guidance only.

# 

# Do not hard-code them unnecessarily if a cleaner implementation produces the same artistic result.

# 

# Transitions between color regions must remain smooth.

# 

# Do not implement four discrete toon bands.

# 

# The shader should preserve broad readable color masses while maintaining soft transitions between them.

# 

# \---

# 

# \## Color Character

# 

# The shader must allow the following artistic direction:

# 

# \### Light

# 

# Bright warm yellow-green.

# 

# The exposed foliage should feel fresh and luminous rather than white or desaturated.

# 

# \### Midtone

# 

# Saturated foliage green.

# 

# This should represent the primary visible body color of the tree.

# 

# \### Shadow

# 

# Emerald green or blue-green.

# 

# Shadowed foliage should retain strong chroma and should not simply become a darker version of the source texture.

# 

# \### Deep Shadow

# 

# Cool dark green / teal.

# 

# The deepest directional-light region should remain visibly colored rather than collapsing to black.

# 

# This is an artistic direction, not a fixed palette.

# 

# All colors must remain editable in the Material Inspector.

# 

# \---

# 

# \## Base Map Role

# 

# The source BaseMap should provide primarily:

# 

# \- local leaf detail

# \- subtle brightness variation

# \- subtle natural texture variation

# \- small-scale visual breakup

# 

# The BaseMap must NOT dictate the final foliage hue.

# 

# The artistic palette defined by:

# 

# \- `\_LightColor`

# \- `\_MidColor`

# \- `\_ShadowColor`

# \- `\_DeepShadowColor`

# 

# must be capable of substantially recoloring the source texture.

# 

# The current source BaseMap contains relatively dark olive-green coloration.

# 

# Do not simply multiply the artistic palette by the full RGB value of the BaseMap if that causes the original olive hue to dominate the final result.

# 

# Prefer treating the BaseMap primarily as detail/modulation information.

# 

# A valid approach may:

# 

# \- derive a luminance or value signal from the BaseMap;

# \- preserve local texture contrast;

# \- use that signal to modulate the artistic palette;

# \- allow the shader palette to control hue and overall color identity.

# 

# Preserve visible leaf texture detail.

# 

# Do not flatten the foliage into uniform solid colors.

# 

# \---

# 

# \## Base Map Preservation

# 

# At the end of this spec, the tree should still visibly contain texture information from the original foliage map.

# 

# The following should remain perceptible:

# 

# \- local differences between leaves;

# \- subtle value variation;

# \- small texture details.

# 

# However, the source texture should no longer prevent the artist from producing colors outside the original olive-green range.

# 

# The shader must be able to produce:

# 

# \- bright yellow-green foliage;

# \- saturated fresh green;

# \- emerald shadows;

# \- cool teal-green deep shadows;

# 

# without editing the BaseMap.

# 

# \---

# 

# \## Saturated Shadows

# 

# Avoid reducing foliage shadows to gray or black.

# 

# Dark areas should retain strong color information.

# 

# The shadow side of the canopy must remain visually readable.

# 

# Do not use black multiplication as the primary shadow-coloring mechanism.

# 

# The artistic shadow colors should remain clearly visible even when the directional-light response is low.

# 

# \---

# 

# \## Main Light Color

# 

# Allow the Unity Main Light color to contribute to the final appearance.

# 

# However, the Main Light must not overpower the artistic foliage palette.

# 

# The material colors should remain the primary artistic control.

# 

# The scene light should influence the foliage naturally enough to remain integrated with the environment while preserving the stylized palette.

# 

# Avoid situations where changing the Sun color completely destroys the intended foliage colors.

# 

# \---

# 

# \## Environment / Ambient Lighting

# 

# Preserve the ambient/environment lighting contribution established in Spec 005.

# 

# However, ambient lighting must not wash out or overpower the artistic foliage palette.

# 

# During validation, specifically check whether the current environment lighting:

# 

# \- desaturates the foliage;

# \- makes the four palette regions converge visually;

# \- makes shaded foliage excessively bright;

# \- shifts the palette away from the intended artistic hue;

# \- removes separation between light and shadow regions.

# 

# If necessary, adjust how ambient lighting contributes to the final foliage color.

# 

# Do not solve palette problems by changing:

# 

# \- scene exposure;

# \- aggressive post-processing;

# \- strong color grading;

# \- unrelated lighting settings.

# 

# The shader itself should remain capable of producing the intended palette under the neutral test-scene lighting.

# 

# \---

# 

# \## Relationship With Spec 005

# 

# Preserve the stylized lighting behaviour implemented in Spec 005.

# 

# The following controls must remain functional:

# 

# \- `\_ShadowThreshold`

# \- `\_ShadowSoftness`

# \- `\_ShadowStrength`

# \- `\_LightDirectionBias`

# 

# Spec 006 should use the existing stylized lighting mask as the driver for the artistic color ramp.

# 

# Do not replace the Spec 005 lighting model with an unrelated system.

# 

# The expected relationship is:

# 

# `Stylized Normal`

# → `Main Light`

# → `Stylized Lighting Mask`

# → `Artistic Color Ramp`

# → `BaseMap Detail Modulation`

# → `Ambient Contribution`

# → `Final Color`

# 

# \---

# 

# \## Scope Boundary

# 

# `\_DeepShadowColor` belongs to the artistic directional-light color ramp.

# 

# It does NOT replace the canopy-interior or fake-density system planned for Spec 007.

# 

# Do not implement during this specification:

# 

# \- canopy density masks;

# \- radial interior darkening;

# \- fake canopy AO;

# \- height darkening;

# \- interior-volume shading;

# \- noise variation;

# \- normal-map detail;

# \- distance behaviour;

# \- wind.

# 

# Spec 006 controls artistic foliage color response only.

# 

# Do not advance into Spec 007.

# 

# \---

# 

# \## Suggested Initial Test Palette

# 

# Use an initial palette inspired by the target visual reference.

# 

# These are starting points only and must remain editable.

# 

# Suggested character:

# 

# \### `\_LightColor`

# Bright warm yellow-green.

# 

# \### `\_MidColor`

# Saturated fresh green.

# 

# \### `\_ShadowColor`

# Deep emerald / blue-green.

# 

# \### `\_DeepShadowColor`

# Cool dark teal-green.

# 

# Do not optimize for exact numeric matching.

# 

# The purpose of the initial palette is to make the four regions visually distinct during validation and to move the current asset away from its original dark olive appearance.

# 

# \---

# 

# \## Reference Target

# 

# The target visual reference shows:

# 

# \- bright yellow-green exposed foliage;

# \- saturated green midtones;

# \- cool green / teal shadows;

# \- darker but still colorful shaded regions;

# \- soft transitions between light and shadow;

# \- large readable color masses;

# \- foliage that remains saturated even when not directly lit.

# 

# The target is not pixel-perfect reproduction.

# 

# Prioritize:

# 

# \- color separation;

# \- saturation;

# \- soft transitions;

# \- palette control;

# \- canopy readability.

# 

# \---

# 

# \## Implementation Guidance

# 

# Prefer a color system where the artistic palette controls hue and the source BaseMap contributes mainly detail.

# 

# Avoid implementations where:

# 

# `FinalColor = BaseMap.rgb \* ArtisticColor`

# 

# if the full RGB multiplication causes the original texture hue to dominate.

# 

# A more appropriate approach may:

# 

# 1\. sample the BaseMap;

# 2\. derive luminance or another detail/value signal;

# 3\. calculate the artistic color from the stylized lighting mask;

# 4\. modulate the artistic color using the BaseMap detail signal;

# 5\. combine with the existing ambient contribution.

# 

# The exact implementation may differ if another method produces the same artistic result more cleanly.

# 

# Keep the shader readable and maintainable.

# 

# Do not add unnecessary complexity.

# 

# \---

# 

# \## Validation

# 

# Validate in:

# 

# `Assets/TreeShader/Test/TreeShader\_TestScene.unity`

# 

# Use:

# 

# \- `Tree\_Test`

# \- `Foliage\_Test`

# 

# Keep the current neutral test-scene lighting.

# 

# Test multiple Sun orientations.

# 

# At minimum validate:

# 

# \- front lighting;

# \- side lighting;

# \- opposite-side lighting.

# 

# Do not modify the scene exposure to improve the shader result.

# 

# \---

# 

# \## Property Validation

# 

# Individually modify:

# 

# \- `\_LightColor`

# \- `\_MidColor`

# \- `\_ShadowColor`

# \- `\_DeepShadowColor`

# 

# Each property must produce a clear and understandable visual change.

# 

# The controls should affect the intended lighting regions.

# 

# Changing `\_ShadowColor` should visibly affect the shadow region, not merely the overall brightness.

# 

# Changing `\_LightColor` should visibly affect exposed foliage.

# 

# Changing `\_DeepShadowColor` should affect the deepest directional-light region without implementing canopy-interior shading.

# 

# \---

# 

# \## Base Map Validation

# 

# Compare the result against the original foliage appearance.

# 

# Confirm that:

# 

# \- the original olive-green hue no longer dominates the shader;

# \- leaf texture detail remains visible;

# \- the foliage can be recolored substantially without editing the texture;

# \- the palette can become brighter and more saturated than the source texture;

# \- no large foliage region becomes a flat solid color unless intentionally configured.

# 

# \---

# 

# \## Ambient Validation

# 

# Verify that the current environment contribution does not:

# 

# \- wash out the artistic palette;

# \- remove separation between the four color regions;

# \- make all shadows appear gray;

# \- over-brighten the canopy.

# 

# If ambient contribution requires adjustment, keep the change local to the foliage shader and document the approach.

# 

# Do not change unrelated scene lighting simply to make the test pass.

# 

# \---

# 

# \## Regression Validation

# 

# The following previously validated systems must remain functional:

# 

# \- `\_BaseMap`

# \- `\_AlphaMap`

# \- `\_BaseColor`

# \- `\_AlphaClipThreshold`

# \- alpha clipping

# \- two-sided foliage

# \- alpha-clipped shadow casting

# \- `\_StylizedNormalStrength`

# \- `\_CanopyCenterOffset`

# \- radial/spherical normals

# \- `\_ShadowThreshold`

# \- `\_ShadowSoftness`

# \- `\_ShadowStrength`

# \- `\_LightDirectionBias`

# \- separate trunk material

# 

# The foliage silhouette must remain unchanged.

# 

# The source tree asset must remain unmodified.

# 

# \---

# 

# \## Acceptance Criteria

# 

# The Spec 006 is complete only if all of the following are true:

# 

# \- `\_LightColor` is implemented and editable;

# \- `\_MidColor` is implemented and editable;

# \- `\_ShadowColor` is implemented and editable;

# \- `\_DeepShadowColor` is implemented and editable;

# \- the tree can be substantially recolored using material properties alone;

# \- the original olive-green BaseMap does not dominate the final hue;

# \- visible leaf texture detail is preserved;

# \- color transitions remain smooth;

# \- the shader does not behave like a hard four-band toon shader;

# \- light regions can become bright yellow-green;

# \- midtones can become saturated green;

# \- shadows can become emerald / blue-green;

# \- deep shadows can become cool teal-green;

# \- dark foliage remains saturated;

# \- no region unexpectedly collapses to neutral gray or black;

# \- the Spec 005 lighting controls remain functional;

# \- radial normals remain functional;

# \- alpha clipping remains correct;

# \- two-sided rendering remains correct;

# \- shadow casting remains alpha-clipped;

# \- trunk material remains separate;

# \- ambient/environment lighting does not overpower the palette;

# \- there are no new shader compilation errors;

# \- there are no new material/property errors;

# \- the result is visually validated in the test scene;

# \- no Spec 007 feature has been implemented.

# 

# \---

# 

# \## Completion Report

# 

# At the end of this spec, report:

# 

# \### Implemented

# 

# Describe the artistic color system and how the four-color ramp works.

# 

# \### Base Map Treatment

# 

# Explain how the source BaseMap contributes to the final result.

# 

# Specifically state how the implementation prevents the original olive hue from dominating the artistic palette.

# 

# \### Assets Modified

# 

# List all modified assets.

# 

# \### Material Properties Added

# 

# List all new exposed material properties.

# 

# \### Validation

# 

# Report:

# 

# \- shader compilation status;

# \- Console status;

# \- Sun orientations tested;

# \- result of changing each artistic color;

# \- whether the original BaseMap hue still dominates;

# \- whether texture detail remains visible;

# \- ambient-lighting behaviour;

# \- regression status for Specs 003, 004 and 005.

# 

# \### Limitations

# 

# Document any remaining limitation.

# 

# \### Next Spec

# 

# State that the next step is:

# 

# `Spec 007 — Canopy Depth and Interior Shading`

# 

# Do not implement Spec 007 in this task.

