# SPEC-006 Stylized Flow Pattern Shaping

## Result

PASS. The selected Noise 1 hybrid source is now shaped into selective cyan
highlight marks instead of being displayed as continuous moving noise.

## Implementation

- Added the `Shaped Hybrid` source mode while retaining `Raw Hybrid` for the
  SPEC-005 comparison.
- Added artist controls for pattern scale, strength, flow-relative stretch,
  highlight color, threshold, and edge softness.
- Noise 1 provides the authored silhouette. A low-frequency procedural field is
  used only for mild distortion and selective breakup.
- Smooth thresholding and a selective mask create clean gaps between marks.
- Highlights are layered over the existing depth-colored water rather than
  replacing shallow/deep color or opacity.

## Validation

- `SPEC-005_PreSPEC006_Baseline.png` preserves the raw SPEC-005 reference.
- `SPEC-006_RiverLike_FinalCandidate.png` shows the final River-like target.
- `SPEC-006_Calm_River_Fast_Comparison.png` compares the three manual states.
- `SPEC-006_Direction_PositiveY.png` confirms that orientation follows Flow
  Direction.
- `SPEC-006_LowStretch.png` and `SPEC-006_HighStretch.png` confirm broad-to-long
  directional shaping.
- `SPEC-006_HighThreshold_Sparse.png` and `SPEC-006_LowThreshold_Broad.png`
  confirm useful coverage control with soft edges.

Depth coloring, shallow/deep colors, opacity, Flow Direction, and Flow Speed code
paths remain intact. No new texture or future water feature was added.

## Tool limitation

The automated Play Mode transition remained at `playmode_transition` with
`Time.time = 0`, so a new A/B animation capture could not be produced. The
unchanged Flow Speed path had already passed SPEC-005 validation; shader import
and all static functional comparisons completed without shader errors.
