# SPEC-009: diagnosis and channel-coordinate correction

## Diagnosis

The main failure was the representation of the pattern, not RG decoding.
The supplied working tree already contained the four-step streamline attempt.
Fresh inspection reproduced horizontal marks crossing the broad S-channel and
counter-bending near its exit (`Diagnosis_Before.png`). The previous README's
claim of visual approval was therefore rejected as evidence of the current result.

The current planar UV0 is `(worldXZ / (22,11)) + 0.5` on the untransformed fixture.
The technical map converts a nearest-centerline tangent to those same UV units
before normalization. This is coherent: interpreting RG directly as a world-space
direction would be wrong. However, using these anisotropic UVs as the pattern's
shape coordinates changes world-space aspect ratio and speed.

More fundamentally, backtracing a globally oriented anisotropic texture for a
short time does not establish a channel-aligned pattern everywhere. In a locally
constant oblique field it merely translates that texture; it does not rotate its
streaks. Each phase then restarts from the globally oriented source. More accurate
integration cannot supply the missing spatial parameterization. The direction
field and a globally consistent pattern coordinate field are different data.

## Hypotheses and experiments

| Hypothesis | Evidence / decision |
| --- | --- |
| Strength variation is the primary cause | Rejected for this defect: B was already constant, approximately 0.619608, while streaks crossed the channel. |
| Map tangent is unrelated to geometry | Rejected for the current map. Inspected mesh coordinates, decoded samples and debug view agree with the centerline. Regenerated map's maximum centerline angular error was 1.0314 degrees over 101 stations, after conversion back to world units. |
| Temporal motion alone causes the wrong curves | Zero-speed diagnostic retained the spatial problem. This test does not establish temporal quality. |
| Four-step bounded tracing establishes orientation | Rejected by baseline capture and the translation argument above. Retained only as compatibility behavior for meshes without channel coordinates. |
| More smoothing, strength remapping, world space alone, or more Euler steps | Not implemented as new trials: none supplies a consistent channel coordinate field. Smoothing is not the missing capability in this smooth source map. |
| Per-fragment local rotation | Reviewed mathematically and in existing code/history; not reimplemented. Its spatial derivative contains derivatives of the rotation multiplied by position, so local tangent alignment alone does not ensure coherent streak trajectories. |
| Continuous longitudinal/transverse coordinates | Tested successfully for constant strength: straight entrance, both broad bends and straight exit share the same streak family, with visible base water between marks. |
| Derivative-based conversion of RG into channel coordinates | Implemented and discarded. High-resolution capture exposed triangle-wise velocity discontinuities and small serrated marks (`ChannelChart_Play_Start.png`). |
| Smooth authored conversion frame | Chosen. Interpolated frame removes those conspicuous triangle-wise breaks (`ChannelChart_Smooth_HighRes.png`). |

## Chosen representation and shader changes

`_UseFlowCoordinates = 1` enables a channel chart only inside the Flow Map branch:

- UV0 / TEXCOORD0 still addresses the SPEC-008 RG/B map.
- Unity `mesh.uv2` / TEXCOORD1 contains accumulated centerline distance / 7.2
  and transverse distance / 2.2. These units match the straight 7.2 x 2.2 control.
- Unity UV channel index 2 / TEXCOORD2 contains a four-component smooth frame:
  the along and across basis vectors expressed in map UV units.
- The shader converts decoded RG through this frame and normalizes the resulting
  chart velocity. RG still controls motion; it is not ignored or repurposed.
- Pattern orientation comes from the continuous chart. There is no local rotation
  of absolute coordinates and no runtime backtrace in this new branch.
- Two bounded offsets, half a cycle apart, sample the existing shaped pattern;
  complementary sine-squared weights hide the individual resets.
- Existing SPEC-007 stretch/scale expressions were extracted into shared
  `GetPatternMetrics`; existing shaping, threshold, coverage and highlight functions
  are reused. B still feeds the existing remapped strength system.
- Depth calculation, shallow/deep colors, alpha and uniform animation are retained.
- `_UseFlowCoordinates` defaults off. `_UseFlowMap = 0` ignores it completely.
- `_FlowMapOrientationInfluence` affects only the old compatibility path; it does
  not change the channel chart. It is retained for existing materials.

The compatibility path is deliberately not advertised as corrected curved-flow
for arbitrary meshes. Enable the new mode only on a mesh with the documented
chart and compatible smooth frame. These additional mesh data are the explicit
tradeoff for reliable spatial orientation; this is not a covert change to RGBA.

## Technical data and scene

`Editor/Spec009ChannelFixture.cs` makes this fixed diagnostic fixture reproducible.
Run **Tools > WaterShader > SPEC-009 > Rebuild Diagnostic Coordinates** in the
WaterShader test scene, outside Play Mode. It updates only WaterShader assets.

The existing water, banks, bed and orange centerline were kept. The channel is
21.90575 units long, with straight entry/exit and two broad changes of curvature.
Its water mesh now has the additional chart and frame; topology is unchanged.
The top camera and existing straight comparison remain. The orange reference
can be toggled for an unobstructed view. Existing older diagnostic objects remain.

Both 256 x 256 maps are regenerated by nearest projection onto 320 centerline
segments, with the analytic tangent evaluated at the projected path parameter.
Sampling uses texel centers. The frame and texture use the same path definition.
Import remains linear, uncompressed, bilinear, clamp, with no mipmaps.

- Constant B: 0.62 (quantized to 0.619608); effective strength approximately 0.7202.
- Strength map: B smoothly changes 0.82 to 0.28 after t=0.55; effective strength
  approximately 0.8044 to 0.5776 using the same 0.46–0.88 remap.
- A remains reserved, set to 1.

This is a hard-coded technical fixture, not a baker, collision solver or automatic
river-to-lake generator. No commercial shader implementation was inspected.
Local concept imagery and SPEC-007 captures guided silhouette and negative space.

Curved material pattern properties are copied from the uniform comparison for
a meaningful comparison. Both use Flow Speed 0.25. The comparison's direction is
set to -X because the preserved legacy uniform shader uses a positive UV offset
(visible travel opposite its vector); the map branch uses negative advection
offsets (visible travel along RG). This changes the comparison material only.

## Validation evidence

1. Inspected the before capture and constant-strength source data.
2. Tested the continuous chart, then replaced the derivative frame after inspecting
   its high-resolution artifacts.
3. Observed constant-strength Play Mode through 130 seconds. A 12-frame contact
   sheet spans actual runtime 42.33511–43.70675 seconds, crossing multiple half-cycle
   handoffs. Timestamps are in `ChannelChart_TemporalTimes.txt`.
4. At 129.8 seconds shader message count was zero; the later capture retained
   alignment and negative space. No growing deformation was seen in these samples.
5. Only at 130.34 seconds switched to the strength-transition material. The exit
   became less elongated and less prominent while retaining the same route.
6. Direction and strength debug captures remain coherent with the data.
7. The session reached 289.06 seconds, including the later strength test. A
   temporary cloned material verified shallow/deep response (depth distances 10
   and 0.05) and opacity 0.15. `ChannelChart_DepthOpacity.png` shows those three
   cases left-to-right; the original material was restored afterward.
8. Saved the scene outside Play Mode with constant strength and debug off. The
   centerline object remains available, disabled for the clean final comparison.

An initial Play attempt had a frozen clock and is not counted as runtime evidence.
Focusing Game View and restarting Play produced advancing timestamps. An MCP
disconnect during transition recovered without infrastructure changes. Unity AI
reported unrelated `NoSubscription` console errors; do not describe the entire
console as error-free. The water shader itself reported zero compiler messages,
and the fixture C# compiled and executed successfully.

Key captures: `Diagnosis_Before.png`, `Diagnosis_ZeroSpeed.png`,
`ChannelChart_Smooth_HighRes.png`, `ChannelChart_TemporalContact.png`,
`ChannelChart_Play_T120Plus.png`, `ChannelChart_Strength.png`,
`ChannelChart_DebugDirection.png`, `ChannelChart_DebugStrength.png`,
and `ChannelChart_Final.png`.

`ChannelChart_Play_Start.png` belongs to the discarded derivative-frame trial;
`ChannelChart_SmoothFrame.png` was captured before the rebuilt frame reached the
mesh. Neither is evidence for the final implementation. Use the explicitly rebuilt
and high-resolution captures, the temporal contact sheet and final comparison.

## Cost and remaining limits

For Shaped Hybrid, the new branch uses **1 Flow Map + 2 Noise + 1 depth** texture
reads per fragment, versus **7 Flow Map + 2 Noise + 1 depth** for the supplied
four-step implementation. Uniform uses 1 Noise + 1 depth. The chart adds six
interpolated floats and approximately 24 bytes per vertex (4.8 KB for 202 vertices),
plus a small basis conversion. These are source-level counts, not GPU timings;
compiler branch decisions and hardware affect actual performance.

- The chart must be authored/generated with the channel, and RG must broadly
  agree with it. Arbitrary rotated currents, junctions and recirculation are not
  solved by this representation. No automatic chart generation tool is provided.
- The smooth frame models centerline directions. Inner/outer-bank metric changes
  are approximate, and tight curves can stretch a chart or overlap it.
- There are still fine aliasing and polygonal traces at small image sizes; this
  work does not add AA, change URP settings or claim pixel-perfect smoothness.
- Strength-dependent pattern scale/stretch can change spatial frequency. Only the
  smooth moderate transition was checked, not abrupt strength boundaries.
- Dual-phase crossfading can soften/double marks. The sampled sequence showed no
  obvious reset pop, but a contact sheet is not continuous frame-by-frame proof.
- Scene appearance is now coherent with the channel in the inspected captures.
  This is visual evidence for this fixture, not blanket approval of SPEC-009 on
  every flow field or a substitute for the owner's artistic assessment.
