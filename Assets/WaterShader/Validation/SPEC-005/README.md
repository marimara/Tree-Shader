# SPEC-005 Pattern Source Evaluation

## Test setup

The technical scene contains three adjacent surfaces using matched flow, scale,
stretch, contrast, depth, and opacity settings:

- `Pattern_Procedural` / `MAT_Water_Pattern_Procedural`
- `Pattern_Texture` / `MAT_Water_Pattern_Texture`
- `Pattern_Hybrid` / `MAT_Water_Pattern_Hybrid`

All modes use the reusable animated UV path introduced by SPEC-004. Directional
coordinates are constructed from the normalized flow direction, and `_NoiseStretch`
elongates the pattern along that direction.

## Source comparison

### Procedural

- Zero texture samples.
- A small, stable combination of warped sine fields produces broad forms.
- Cheapest in texture bandwidth and useful at calm speed, but the shapes are very
  smooth and offer less authored character for rivers and waterfalls.

### Texture

- One grayscale texture sample plus contrast remapping.
- Preserves authored character efficiently and responds correctly to flow and
  directional stretch.
- Repetition and baked-in directional character remain more obvious.

### Hybrid

- One grayscale texture sample plus one simple procedural field used for UV
  distortion, blending, and threshold shaping.
- Retains useful authored shapes while reducing direct texture repetition.
- Produces the best balance across calm water, river motion, and strong stretch at
  modest additional arithmetic cost.

## Existing texture candidates

All supplied candidates were tested on both the texture and hybrid surfaces:

1. `Assets/WaterShader/Textures/Noise 1.png` — best candidate; broad abstract forms,
   readable after stretch, and sufficiently raw for shader shaping.
2. `Assets/WaterShader/Textures/Noise 2.png` — dense and strongly streaked; becomes
   busy under directional transformation.
3. `Assets/WaterShader/Textures/Noise 3.png` — very sparse, with a dominant band;
   loses useful coverage and reveals repetition.
4. `Assets/WaterShader/Textures/Noise 4.png` — excessive fine-frequency grain for
   the target broad painterly language.
5. `Assets/WaterShader/Textures/Noise 5.png` — attractive brush marks, but already
   too close to final authored streaks and too sparse as general raw noise data.
6. `Assets/WaterShader/Textures/Noise 6.png` — recognizable finished water/foam
   structure; less flexible as a neutral base pattern.

## Speed and direction validation

- Low speed: `_FlowSpeed = 0.12`; `LowSpeed_A/B` show slow movement in all modes.
- Medium speed: `_FlowSpeed = 0.55`; `MediumSpeed_A/B` show readable river motion.
- High speed: `_FlowSpeed = 1.6`; `HighSpeed_A/B` show stronger displacement over a
  shorter interval without jitter.
- `AlternateDirection_AllModes` uses `(-0.5, 1.0)` and confirms that procedural,
  texture, and hybrid patterns all reorient and animate through the shared flow path.

Every A/B speed pair produced distinct image hashes. The final play-mode capture
confirms that pattern modulation remains layered on top of the existing depth color
gradient and does not replace the existing opacity calculation.

## Decision for SPEC-006

Select the **hybrid** pattern source using:

`Assets/WaterShader/Textures/Noise 1.png`

This gives SPEC-006 the strongest combination of broad authored structure,
directional stretch, thresholdability, stable animation, and acceptable cost. The
existing `MAT_Water_Test` is configured to this basis; the three comparison materials
remain available for technical inspection.

## Limitations

- The pattern mode switch is a development comparison control and may be removed or
  converted to a production variant in a later Spec.
- These captures evaluate uniform flow only. Flow Maps and Flow Strength were not
  implemented.
- The candidate textures were evaluated with their existing import settings; no new
  textures were created.

## Result

PASS. Procedural, sampled texture, and hybrid sources were compared at all required
speeds, Noise 1 was selected, and hybrid was chosen as the SPEC-006 basis.
