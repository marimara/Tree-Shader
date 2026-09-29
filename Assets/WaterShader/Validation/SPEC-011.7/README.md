# SPEC-011.7 - Periodic Water Ridge Field Revision

Status: technically implemented and validated in Unity 6000.6.0f1. The raw
sources and minimal straight-channel integration are ready for human review.
Artistic approval is explicitly not claimed.

## Reference reading

The target is organized as a coherent directional field, not a population of
small marks. Its dominant structures cross large parts of the image, widen and
narrow gradually, bend smoothly, occasionally meet or fork, and leave deliberate
black negative space. Bright cores, broad midtones and soft falloff make the raw
grayscale useful before shader thresholding. Fine detail is subordinate to the
macro ridges.

## Why the previous representation failed

The previous generator created 111 Primary or 144 Secondary analytic SDF stamps
and max-composited them. Mark placement, length, width and taper controls could
improve each capsule, but could never create shared long-range topology. More
marks only produced more dashes; fewer or longer marks exposed repetition and
still read as separate brush stamps. The old output is preserved in
`GeneratedSources/` for comparison.

## New mathematical representation

The revision generates a scalar phase field directly on a 2D torus.

1. An integer lattice normal defines the periodic directional phase. Integer
   wave vectors make every contributing function exactly periodic on both axes.
2. A small seeded Fourier basis warps that phase at low frequency, producing
   smooth curvature without Perlin, Worley, cellular or marble noise.
3. Distance to the repeated phase ridge produces the main bands.
4. A separate periodic width field expands and contracts the bands over long
   distances. Width can collapse almost to zero, creating natural tapered gaps.
5. A low-frequency breakup field is applied to width, after the continuous
   structure exists. It opens space rather than multiplying the result into a
   collection of dark capsules.
6. A second locally gated phase creates occasional forks and meetings. It is
   full-bodied only inside broad branch regions and remains subordinate to the
   main ridges.
7. Smooth distance falloff, ridge profile power and low-amplitude intensity
   modulation preserve white cores, midtones and soft painterly edges.

The Seed changes every harmonic phase, amplitude and selected lattice vector for
warp, width, breakup, intensity and branching. It therefore changes placement,
curvature, width, encounters, gaps and composition while retaining one family.

## Investigated alternatives

- Single warped phase: excellent continuity, but too parallel and regular.
- Local branch union: selected. It adds meetings and silhouette diversity while
  retaining broad dominant bands.
- Coherent multiscale detail: rejected for Primary because the extra ridges read
  as a fine secondary network.
- Primary band counts 3, 4 and 5: three was retained. Four and five reduced the
  size of the obvious motif but moved the result toward dense uniform stripes.

See `FieldExperiments/ApproachComparison.png` and
`FieldExperiments/BandCountComparison.png`.

## Controls

Removed because they described independent stamps:

- Mark Density
- Average Mark Width
- Average Mark Length
- Length Variation
- Tail Amount
- Edge Breakup
- Shape Irregularity
- Negative Space

New artist-facing field controls:

- Band Count
- Band Width
- Width Variation
- Direction
- Warp Strength
- Warp Scale
- Curvature
- Continuity
- Breakup Amount
- Breakup Scale
- Branching / Meets
- Ridge Sharpness
- Edge Softness

Primary defaults to three broad, highly variable and mostly continuous ridges.
Secondary uses six narrower ridges, higher breakup frequency, lower continuity,
lower intensity and less branching. Secondary therefore shares the coherent
field logic without being a scaled Primary copy.

## Preview and output workflow

The existing interaction pass is preserved:

- 128x128 source preview;
- 250 ms trailing debounce after slider release;
- 1x, 2x2 and 4x4 presentation without source regeneration;
- silhouette toggle without source regeneration;
- explicit final generation only on Save;
- saved linear, uncompressed, Repeat/Bilinear Texture2D with mipmaps.

The selected angle is converted to the closest integer lattice normal at the
requested band frequency. This quantizes some low-frequency diagonal angles, a
necessary tradeoff for exact mathematical tiling without seam repair.

## Raw validation

The Unity-generated review set contains Primary and Secondary Seeds 1107, 2309,
4513 and 7823 at 512x512. Each has raw, 2x2, 4x4 and mip-strip views in
`FieldSources/`.

Primary coverage above 0.5 ranges from 20.6% to 32.6%. Midtone coverage ranges
from 28.8% to 41.1%. Secondary coverage ranges from 8.3% to 9.8%, with midtones
from 17.1% to 18.2%. Exact hashes and per-seed measurements are in
`FieldSourceMetrics.json`.

Key review images:

- `Checkpoint_Reference_Old_Field.png`
- `Old_vs_Field_Comparison.png`
- `Field_Primary_Seeds_Contact.png`
- `Field_Secondary_Seeds_Contact.png`
- `Field_Primary_Seed2309_1x_2x2_4x4.png`

The new output is structurally closer to the reference than the previous stamp
generator: it replaces hundreds of disconnected capsules with a few continuous
macro ridges, stronger width evolution, broad soft edges, occasional encounters
and substantially larger negative-space regions.

## Determinism, seamlessness and performance

Two independent Unity generations of Primary Seed 2309 differed at 0 of 262,144
pixels. All phase, warp, width, breakup and branch terms use integer wave vectors;
periodicity is intrinsic and no border blend or seam post-process exists.

Measured in Unity on this machine:

- Primary 128 preview: 34.3 ms
- Secondary 128 preview: 27.7 ms
- Primary 512 final: 449.8-469.4 ms
- Secondary 512 final: 439.0 ms

The previous stamp implementation measured approximately 518 ms for Primary
preview and 7,957-8,096 ms for Primary 512 final. The field representation is
therefore roughly 15x faster in preview and 17x faster at 512 on this machine.

Unity confirmed the saved candidate at 512x512 with 10 mip levels. Script
validation and Editor compilation completed with no new errors.

## Minimal shader integration

No shader rewrite was made. The existing generated-pattern path and stable
SPEC-011.6 flow pipeline were reused, and the newly generated Primary 1107 asset
was captured for 540 frames at 30 fps on the straight channel. The 18-second
contact is `Straight_FieldPrimary_Contact.png`; the deterministic source frames
and capture metadata are under `.frames/Straight_Primary_1107/`.

The band translates continuously through the straight fixture with no phase pop.
The test deliberately keeps the prior moderate mode-4 shaping, Flow Map path,
UV2/UV3 coordinates, transform-aware infrastructure and obstacle systems intact.

## Remaining limitations

- Human artistic approval is required.
- Exact tiling is seamless, but a finite macro tile remains learnable in a
  deliberate 4x4 inspection. Three bands best match the broad reference language;
  simply increasing count made the source stripe-like. Anti-tiling composition
  would be a separate shader/system feature and was not added here.
- Low-frequency custom directions are quantized to an integer lattice normal.
- Some seeds create sharper pinches or hook-like branch ends than others.
- Seed 1107 is visually calmer and more regular than Seeds 2309, 4513 and 7823.
- Secondary can become very faint in distant mips, consistent with its supporting
  role but still subject to human review.

No SPEC-012, foam, reflection, refraction, waves, runtime generation, Flow Map,
UV2/UV3, obstacle, transform-aware, render-pipeline or project-setting changes
were made.
