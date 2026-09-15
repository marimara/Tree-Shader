# Spec 004 — Radial canopy normals

## Implemented
Object-space radial normals, transformed as normals into world space, interpolated and normalized for lighting. Only original mesh normals receive the backface sign. Existing Lambert, ambient, alpha, ShadowCaster and DepthOnly behavior is retained.
- _StylizedNormalStrength: 0 preserves mesh lighting; 1 uses canopy normals. Shader default 0; test material 1.
- _CanopyCenterOffset: mesh-local units. Test value (0.014050544, -0.0021115066, -0.0038637575), taken from leaves1 mesh bounds. Do not multiply this value by the approximately 85.815x renderer scale.
- Finite fallback for center singularity and opposing blend directions.

## Modified assets
- Assets/TreeShader/Shaders/StylizedFoliage.shader
- Assets/TreeShader/Materials/MAT_StylizedFoliage_Test.mat
- This report and Radial004 comparison images in this folder.
Imported source assets and the pre-existing untracked McpBootstrap004 tooling were preserved.

## Unity MCP validation
Unity 6000.6.0f1, URP PC_RPAsset, TreeShader_TestScene.
Inspected Tree_Test/tree1, child leaves1, Foliage_Test/leaves1, materials, textures, renderer bounds, transforms and initial Console.
Compared strength 0 and 1 in six sun orientations: original front lighting, two opposite lateral directions, rear lighting, 80-degree high sun and 10-degree low sun. Inspected rendered captures: broader continuous canopy light mass at strength 1, unchanged leaf outlines and alpha-shaped ground shadows. Back view retains foliage.
Temporary translation, rotation and uniform-scale reductions keep radial shading attached to foliage; restored original transforms, sun and camera after tests.
ShaderUtil reports no shader errors or messages. BaseMap and AlphaMap references are valid, alpha threshold remains 0.5. No new Console errors/warnings: existing five Unity AI NoSubscription errors, one historical MCP connection error and one transport warning remain.
The shader and material are saved. No scene changes are required for this milestone.

## Visual limitations
The lower canopy and unlit side are still very dark under the unchanged Spec 003 Lambert/ambient lighting. The broader mass is visible, but this is not the final palette or stylized lighting.
One center models one whole canopy, not individual branch clusters. Texture detail and real self-shadowing remain visible.
Non-uniform scale uses inverse-transpose normal transformation: an ellipsoidal response, not a world-space sphere. Avoid zero scale. Extreme opposing mesh/radial directions can change rapidly around strength 0.5.
During temporary tree transform tests the bark rendering did not follow the foliage consistently in captured frames; original transforms were restored. This is outside the modified foliage shader, and whole-tree transform behavior is not claimed as fixed.
Raw additional captures are in Temp/Spec004 (temporary evidence).

## Next spec
005-stylized-lighting.md, not started.

Final save: the MCP disconnected during evidence import and reconnected. A WebSocket-not-initialised warning and another Unity AI NoSubscription error appeared after reload; these are integration/service messages, not foliage shader errors. Scene save was retried.
