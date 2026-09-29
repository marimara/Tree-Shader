# SPEC-011.7 — Procedural Water Pattern Generator

## Objective

Create an Editor-only procedural Water Pattern Generator for the Stylized Water System.

The tool should generate authored-looking, tileable grayscale pattern textures specifically designed for the validated WaterShader pipeline.

The generator exists because the current Noise source has become an artistic limitation.

The current source can support continuous translation and some broadening, but its underlying topology still tends to produce:

- fibrous branches;
- repeated connected structures;
- narrow tendrils;
- recognizable recurring clusters;
- insufficient variety in broad painterly masses.

The generator should provide better source material for the shader rather than forcing the shader to destructively reshape unsuitable noise.

The output should remain lightweight at runtime:

Generator
→ Editor-time texture creation
→ saved texture asset
→ normal shader texture sampling

Do not generate procedural patterns every frame at runtime.

---

# Dependencies

Requires:

- SPEC-011.6 — Stable Stylized Flow Pattern and Visual Fidelity
- existing WaterShader reference footage
- existing WaterShader validation scenes

The generator should be designed specifically around the visual requirements discovered during SPEC-011.6.

Do not redesign:

- Flow Map architecture;
- centerline flow;
- obstacle steering;
- channel coordinates;
- Flow Strength;
- Transform-aware baker behavior.

This Spec creates better visual source data.

It does not replace the flow system.

---

# Core Goal

Generate seamless water-pattern textures that already contain useful painterly structure before entering the shader.

The shader should need less destructive:

- thresholding;
- distortion;
- erosion;
- masking;

to obtain usable stylized marks.

The generator should produce a source closer to:

broad stylized water brush marks

than:

generic noise requiring heavy shaping.

---

# Visual Target

Use the existing water reference footage as the primary visual target.

Focus specifically on the source shapes needed for the base moving surface pattern.

Target characteristics:

- isolated broad masses;
- strong interior body;
- irregular stroke width;
- irregular stroke length;
- directional composition;
- organic broken edges;
- controlled tapering;
- occasional short tails;
- varied negative space;
- low repetition;
- strong readability at gameplay distance.

Avoid:

- dense connected vein networks;
- marble-like patterns;
- smoke;
- clouds;
- cellular blobs;
- uniform stripes;
- hair-like structures;
- many long filament branches;
- excessive tiny noise;
- evenly spaced marks.

---

# Source Philosophy

The generated source should do more of the artistic work itself.

Prefer:

good source shape
+
moderate shader shaping

over:

generic noise
+
aggressive shader reconstruction.

The texture should remain useful when viewed directly in grayscale.

A successful source should already suggest directional water highlights before shader processing.

---

# Pattern Categories

The generator should support at least two useful pattern roles.

## Primary Pattern

Designed for dominant painterly flow marks.

Visual target:

- broad;
- strong body;
- medium-to-long;
- sparse enough to preserve negative space;
- relatively low frequency;
- highly readable.

Primary marks should not consist mainly of thin branches.

---

## Secondary Pattern

Designed for supporting smaller streaks.

Visual target:

- shorter;
- somewhat thinner;
- more numerous;
- less visually dominant;
- varied enough not to appear as scaled-down copies of the Primary pattern.

Secondary output should complement the Primary pattern.

It should not simply be:

Primary texture at half scale.

---

# Optional Combined Output

A combined texture mode may be provided if useful.

However, Primary and Secondary generation should remain conceptually separable so the shader can art-direct them independently.

Do not force all pattern scales into one baked texture if doing so reduces control.

---

# Tileability

All generated textures must be perfectly seamless.

Required:

- seamless left ↔ right;
- seamless top ↔ bottom;
- no visible edge discontinuity;
- no border;
- no hidden padding requirement.

Tileability must survive:

- normal shader sampling;
- bilinear filtering;
- mipmaps.

The generated pattern should not only be mathematically tileable.

The tile repetition must also be visually difficult to notice.

---

# Directionality

The generator should support directional pattern generation.

At minimum provide a primary flow axis.

Conceptually:

Horizontal
Vertical
Custom Angle

Exact UI is flexible.

The pattern should generally align with the selected axis while retaining enough irregularity to avoid looking mechanically striped.

---

# Primary Inputs

Expose artist-facing controls rather than low-level algorithm parameters wherever possible.

Suggested inputs:

## Seed

Deterministic random seed.

Requirements:

- same settings + same seed → same output;
- changing seed → meaningfully different composition.

---

## Resolution

Selectable output resolution.

Suggested practical values:

- 256;
- 512;
- 1024.

Do not assume a fixed resolution.

Avoid generating larger textures without visual benefit.

---

## Direction

Controls dominant mark orientation.

May be expressed as:

- angle;
- vector;
- horizontal/vertical preset.

---

## Mark Density

Controls how much of the texture is occupied by marks.

Higher Density:

- more marks;
- less negative space.

Lower Density:

- fewer marks;
- more open water.

Density should not simply scale all values toward white.

It should affect spatial distribution.

---

## Average Mark Width

Controls approximate primary body width.

Increasing it should create broader painterly forms.

It should not merely blur the texture.

---

## Width Variation

Controls diversity between narrow and broad marks.

Low:

marks have similar width.

High:

stronger width variation.

Avoid producing excessive thin tendrils as a side effect.

---

## Average Mark Length

Controls general longitudinal extent.

Long marks should retain body.

Do not achieve length primarily by extreme anisotropic stretching of thin structures.

---

## Length Variation

Controls diversity in mark length.

The system should support:

- short;
- medium;
- long;

marks in one texture.

Avoid making every shape share the same tail length.

---

## Tail Amount

Controls how much tapered continuation appears after a mark.

Low:

compact masses.

Higher:

more directional tails.

Even at high values, avoid producing many extremely long hair-like filaments.

---

## Edge Breakup

Controls painterly irregularity along mark boundaries.

Target:

organic broken brush edges.

Avoid:

uniform pixel noise around every edge.

Breakup should occur mostly at medium visual frequencies.

---

## Shape Irregularity

Controls how asymmetrical and non-uniform each mark is.

Should affect:

- width profile;
- contour;
- curvature;
- taper;

without turning marks into generic noise blobs.

---

## Negative Space

Provide direct artistic control over spacing/open areas.

This may overlap conceptually with Density but should result in predictable composition.

The goal is to avoid both:

continuous connected networks

and:

huge empty regions separated by isolated giant clusters.

---

# Optional Advanced Inputs

Advanced controls are allowed when useful.

Possible examples:

- curvature;
- branching suppression;
- cluster size;
- taper bias;
- edge roughness scale;
- large-scale distribution variation;
- secondary-detail amount.

These are suggestions, not mandatory API requirements.

Do not expose dozens of implementation-specific sliders by default.

---

# Branch Suppression

Because the previous Noise source produced excessive fibrous topology, the generator should explicitly avoid uncontrolled branching.

Primary marks should normally contain:

one dominant mass

rather than:

one central spine with many long branches.

Small local forks may occur where visually useful.

Dense connected filament networks should be strongly discouraged.

---

# Shape Independence

Generated Primary marks should have meaningful identity variation.

Avoid producing the same shape repeatedly with only:

- scale changes;
- rotation changes;
- intensity changes.

Variation should affect actual silhouette.

Examples:

- different width profiles;
- different tail structures;
- different lengths;
- different edge breakup;
- different asymmetry.

---

# Distribution

Marks should be distributed with deliberate irregularity.

Avoid:

- obvious grid placement;
- regular spacing;
- repeating horizontal bands;
- equally sized clusters.

Target:

visually balanced randomness.

The texture should contain:

- active regions;
- moderate gaps;
- smaller supporting groups;

without large predictable empty blocks.

---

# Multi-scale Composition

The generator may internally use multiple scales or procedural layers.

However, final output should remain visually coherent.

Do not expose obvious independent noise layers.

The final Primary texture should read as one authored composition.

---

# Generation Technique Freedom

The implementation method is intentionally open.

Possible approaches may include:

- procedural noise fields;
- signed distance fields;
- seeded brush-stroke placement;
- morphological operations;
- domain warping;
- cellular placement;
- layered masks;
- splat/brush generation;
- hybrid techniques.

These are suggestions only.

Do not select an approach just because it appears here.

Choose the simplest robust method that produces the required visual result.

Visual output matters more than matching a particular procedural algorithm.

---

# Determinism

Generation must be deterministic.

For identical:

- seed;
- resolution;
- settings;

the generated result must remain identical.

This is important for:

- version control;
- reproducibility;
- debugging;
- regeneration.

---

# Editor Tool

Implement as an Editor-only tool.

Possible forms include:

- custom EditorWindow;
- custom Inspector section;
- dedicated generator asset;
- another practical Editor workflow.

Suggested workflow:

1. Open Water Pattern Generator.
2. Choose preset or role.
3. Adjust controls.
4. Preview.
5. Change Seed if desired.
6. Generate.
7. Save texture asset.
8. Assign to WaterShader material.

The exact interface is not prescribed.

---

# Live Preview

Provide a useful preview before saving.

At minimum display:

- grayscale generated texture.

Ideally also provide optional preview modes such as:

- tiled 2x2 view;
- high-contrast silhouette view.

A tiled preview is strongly useful for identifying repetition and seams.

Do not require saving a texture after every parameter change just to inspect it.

---

# Optional Water Preview

If practical, provide a lightweight preview using the current WaterShader or validation material.

This is optional.

Do not make the generator dependent on a full scene or runtime setup.

The grayscale output itself remains the primary generator artifact.

---

# Presets

Provide useful starting presets.

At minimum:

Primary Painterly Flow

Secondary Directional Streaks

Optional future-friendly presets may include:

Broad Calm Marks

Fast River Marks

Foam Source

However:

do not implement future foam behavior in this Spec.

A Foam preset may only be included if trivial and clearly separated.

Primary and Secondary water patterns are the priority.

---

# Output

Generate persistent texture assets.

Recommended location:

Assets/WaterShader/Generated/Patterns/

Suggested naming:

T_WaterPattern_Primary_<Seed>

T_WaterPattern_Secondary_<Seed>

or equivalent predictable names.

Avoid uncontrolled duplicate files.

---

# Texture Format

Output should be grayscale source data.

Recommended conceptual format:

R / grayscale

No transparency required unless a future demonstrated use case requires it.

Generated textures should be configured appropriately for data/pattern use.

Consider:

- sRGB state;
- compression;
- filtering;
- mipmaps;
- wrap mode.

Exact import settings should be chosen based on visual validation.

---

# Required Runtime Properties

Generated output must be compatible with normal runtime texture sampling.

The generator should not require:

- compute generation at runtime;
- procedural regeneration at runtime;
- special runtime buffer setup.

Once generated, runtime cost should remain comparable to sampling any ordinary pattern texture.

---

# Source Preservation

Do not overwrite the current Noise source.

Keep it available for:

- regression comparison;
- fallback;
- validation.

Generated patterns are additional assets.

---

# Comparison Against Noise 1

Validation must compare generated output against the existing Noise 1 source.

Compare both:

raw texture

and

WaterShader result.

The new generator is only justified if it materially improves the artistic result.

---

# Raw Texture Validation

Before using the shader, inspect generated textures directly.

Primary pattern must show:

- broad isolated masses;
- usable interiors;
- varied width;
- varied length;
- controlled tails;
- broken edges;
- meaningful negative space;
- limited repetition.

Secondary pattern must show:

- smaller distinct shapes;
- stronger frequency variation;
- less visual weight;
- no simple scaled duplication of Primary.

---

# Tiling Validation

Display each output tiled at least:

2x2

Preferably:

4x4.

Inspect for:

- visible seams;
- obvious repeated landmarks;
- horizontal/vertical band repetition;
- repeated large silhouettes.

A mathematically seamless but visually obvious tile does not pass.

---

# Mipmap Validation

Inspect at multiple viewing distances / mip levels.

The silhouette should degrade gracefully.

Avoid textures where:

- all detail disappears immediately;
- mipmaps turn broad marks into uniform gray;
- thin branches dominate at one distance then vanish abruptly.

Primary masses should remain readable at practical gameplay distance.

---

# Shader Validation Order

Use the generated texture in the existing SPEC-011.6 shader pipeline.

First validate:

## Straight Channel

- constant Flow Strength;
- no obstacles;
- stable camera.

This remains the main visual checkpoint.

Expected:

- broad painterly bodies;
- controlled aspect ratio;
- continuous downstream movement;
- no obvious blinking;
- no extreme longitudinal smearing;
- less repetition than Noise 1.

Do not advance until the straight-channel result improves materially over the previous source.

---

# Revised SPEC-011.6 Integration

The generator should not immediately trigger large shader rewrites.

First test the new source using the existing stable SPEC-011.6 architecture.

Only adjust shaping parameters that are genuinely required by the new source.

Prefer:

better texture source
→ simpler shaping

over:

better texture source
→ equally aggressive old shaping.

Document which old shaping stages can be reduced or removed.

---

# Curved Channel Validation

After straight-flow approval:

test the S-shaped channel.

Requirements:

- Primary bodies remain broad;
- no excessive inside compression;
- no excessive outside smear;
- no new faceting;
- visual language remains consistent with straight flow.

---

# Obstacle Validation

Then test the validated obstacle setup.

Requirements:

- marks flow around the collider;
- Primary body remains readable;
- no exaggerated circular halo;
- no fragmentation into thin tendrils;
- pattern recombines downstream.

Do not redesign obstacle steering solely because a generated source exposes another visual issue.

---

# Flow Strength Validation

Test at least:

Calm

River

Fast

The generated source should remain usable across all three.

Fast must not become:

thin stretched fibers.

Calm must not become:

large static blobs.

The shader may still adjust presentation by Flow Strength.

The source must provide enough structure to support those states.

---

# Transform Validation

Test:

Scale 1

and

strong non-uniform X/Z Transform.

The generated pattern should preserve comparable:

- world-space width;
- world-space density;
- visual identity.

SPEC-011.5 Transform-aware behavior must remain intact.

---

# Temporal Validation

The generated pattern must work with the temporally stable translation established in SPEC-011.6.

Run multiple complete animation cycles.

Check for:

- blinking;
- popping;
- tile reset visibility;
- recognizable repeating landmarks;
- density pulses;
- visible pattern replacement.

A visually good static source is insufficient if repetition becomes obvious during motion.

---

# Seed Validation

Generate multiple Seeds using identical settings.

At minimum compare several seeds.

All should satisfy the same visual language.

A preset does not pass if:

only one lucky Seed looks good.

The generator should produce consistently useful results.

---

# Parameter Robustness

Test meaningful changes to major controls.

For example:

- Width;
- Length;
- Density;
- Edge Breakup;
- Seed.

Controls should behave predictably.

Avoid parameter ranges where small slider changes completely destroy the texture.

---

# Performance — Editor

Generation occurs in Editor.

Moderate generation cost is acceptable.

Prioritize:

1. visual quality;
2. deterministic output;
3. practical iteration time.

Avoid extremely long generation times when a simpler technique produces equivalent results.

---

# Performance — Runtime

Runtime cost should come primarily from texture sampling.

The generator itself introduces no runtime update cost.

If generated textures allow the shader to reduce existing texture samples or shaping operations, document that benefit.

---

# Diagnostics

Provide useful generation information.

Suggested summary:

Preset
Seed
Resolution
Direction
Mark Count / approximate density
Primary Width Range
Length Range
Output Path

Exact diagnostics may vary.

Do not expose internal implementation details unnecessarily.

---

# External Assets

No external texture asset is required.

The point of this Spec is to generate project-owned pattern textures procedurally.

Do not download or import internet noise textures.

---

# Acceptance Criteria

SPEC-011.7 is complete when:

- a Water Pattern Generator exists as an Editor-only workflow;
- generation is deterministic by Seed;
- generated outputs are perfectly seamless;
- Primary and Secondary pattern roles are supported;
- Primary output contains broad isolated painterly masses;
- Primary shapes retain useful interior body;
- excessive fibrous branching is significantly reduced compared with Noise 1;
- width and length vary independently enough to produce diverse silhouettes;
- edges contain controlled painterly breakup;
- negative-space distribution is visually balanced;
- obvious repetition is reduced;
- generated output remains readable through mipmaps;
- several different Seeds produce consistently usable patterns;
- generated patterns can be saved as normal texture assets;
- runtime does not require procedural regeneration;
- straight-channel WaterShader result is materially better than Noise 1;
- excessive straight-channel elongation is reduced;
- primary marks do not collapse into thin stretched fibers;
- curved flow preserves broad mark identity;
- obstacle flow remains coherent;
- Calm / River / Fast remain usable;
- Transform-aware behavior remains valid;
- temporal stability from SPEC-011.6 is preserved;
- no future foam/reflection/refraction systems are implemented.

---

# Approval Criteria

Do not consider this Spec complete merely because:

- a procedural texture is generated;
- it is mathematically seamless;
- the Editor tool works;
- one Seed looks acceptable.

Approval requires visual quality.

At least several generated Seeds should produce sources that are visibly better suited to the target than Noise 1.

The generated Primary pattern should read as:

"authored painterly water strokes"

rather than:

"procedural noise."

When used in the WaterShader, the result should read as:

"broad stylized water marks travelling with the current"

rather than:

"thresholded noise stretched downstream."

Final artistic approval remains manual.

Do not let automated image metrics substitute for visual review.

---

# Relationship to SPEC-011.6

SPEC-011.7 does not replace SPEC-011.6.

After a generated source is approved:

return to SPEC-011.6

and perform a final shader integration / art-direction pass.

The expected sequence is:

SPEC-011.6 diagnosis
→ source limitation identified
→ SPEC-011.7 generates better source
→ return to SPEC-011.6
→ simplify/refine shaping
→ final visual approval
→ continue to SPEC-012.

---

# Out of Scope

Do not implement:

- runtime procedural texture generation;
- runtime fluid simulation;
- SPEC-012;
- automatic river-to-lake strength;
- strength override zones;
- foam system;
- shoreline foam;
- waterfall foam;
- waves;
- custom normals;
- reflection;
- refraction;
- splash VFX;
- mist;
- water interaction;
- multi-branch river logic.