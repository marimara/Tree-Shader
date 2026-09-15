# SPEC-004 Validation

Validated before SPEC-005 implementation began.

## Configuration

- Shader: `Meganeura/Water/Stylized Water`
- Material: `Assets/WaterShader/Materials/MAT_Water_Test.mat`
- Test scene: `Assets/WaterShader/Scenes/WaterShader_TestScene.unity`
- Temporary visualization: procedural two-axis bands (`_FlowVisualization = 1`)

## Direction checks

The material was tested with normalized shader-side flow directions for positive X,
negative X, positive Y, negative Y, and two diagonal directions. Captures in this
folder record each configuration. The same reusable animated UV function drives all
tests; no authored texture is sampled by the SPEC-004 visualization.

## Speed checks

- Zero: `_FlowSpeed = 0`; `ZeroSpeed_A` and `ZeroSpeed_B` are byte-identical after
  separate captures, confirming that time produces no movement at zero speed.
- Low: `_FlowSpeed = 0.12`; the `PositiveX_Low_Runtime` pair records slow movement.
- High: `_FlowSpeed = 1.6`; the `PositiveX_High_Runtime` pair records clearly faster
  movement over a shorter capture interval.

Unity was configured to continue updating while unfocused for the runtime capture
pairs. Earlier non-runtime pairs are retained as configuration evidence.

## Compatibility checks

- The shallow-to-deep gradient remains visible in all captures.
- `_Opacity` was temporarily changed from `0.72` to `0.35`, captured, and restored.
- The procedural visualization only modulates RGB brightness after depth coloring;
  it does not alter the existing alpha calculation.
- No WaterShader shader or script errors were reported after import and play-mode
  validation.

## Result

PASS. Uniform direction and independent speed behave as required, including a
stationary zero-speed state, without replacing the existing depth/opacity system.
