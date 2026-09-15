# Stylized Tree Shader — Project Goal

## Objective

Create a reusable stylized foliage shader for Unity URP.

The shader is intended primarily for tree canopies built using foliage cards:
flat or nearly-flat polygon planes containing leaf textures and alpha masks.

The target appearance is highly stylized rather than physically realistic.

## Visual References

Primary visual target:

References/target_style.png

Technical/reference implementation:

References/aversion_reference.png

External technical reference:
https://www.aversionofreality.com/blog/2022/8/7/stylized-tree-shader

The external reference should be used as conceptual guidance, not copied literally.

Its relevant techniques include:

- balancing directional/sun lighting
- interaction between world/ambient lighting and foliage shading
- radial/spherical normals
- fake sun angle
- fake Lambert shading
- artist-controlled color mixing
- leaf alpha textures
- leaf normal maps
- randomized UV behaviour
- variation per foliage island
- normal variation using noise
- combining custom normals and normal-map detail
- fake canopy/tree density
- ambient occlusion based darkening/brightening
- camera-distance masks

## Desired Appearance

The final tree should visually read as large rounded masses of foliage.

Individual foliage cards should not dominate the lighting.

The canopy should have:

- bright stylized colors
- large readable light and shadow regions
- soft transitions between lit and shaded foliage
- saturated colored shadows
- darker canopy interiors
- brighter exposed outer foliage
- subtle local variation
- strong silhouette readability

The result should resemble stylized hand-painted/game vegetation.

## Do Not Target

Do not attempt physically accurate vegetation shading.

Avoid:

- realistic PBR foliage appearance
- black shadows
- strong glossy highlights
- noisy per-leaf lighting
- obvious card-by-card shading
- overly complex subsurface scattering
- excessive shader properties without artistic purpose

## Technical Environment

- Unity URP
- foliage cards
- alpha-clipped leaf texture
- directional main light
- realtime shadows
- artist-adjustable material parameters

The shader must remain suitable for use on multiple tree models.

Do not hard-code behaviour specifically for one tree.

## Development Rule

Implement this project incrementally.

Never implement multiple major systems in one step unless explicitly required by the current spec.

After every spec:

1. Save the assets.
2. Allow Unity to compile.
3. Inspect the Console.
4. Verify the visual result in the test scene.
5. Report what changed.
6. Report any limitations or unresolved issues.

A specification is not complete merely because the shader compiles.

It must visibly work in Unity.