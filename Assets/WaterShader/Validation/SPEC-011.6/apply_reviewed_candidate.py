"""One-time, guarded promotion of the inspected pattern into the production shader."""
from pathlib import Path

folder = Path(__file__).resolve().parent
path = folder.parents[1] / 'Shaders/StylizedWater.shader'
old = path.read_text(encoding='utf-8-sig')
if 'GetStableChannelPattern' in old:
    raise SystemExit('Already promoted; do not apply twice.')
candidate = (folder / 'CandidateGate.shader').read_text(encoding='utf-8-sig')
start = candidate.index('            half ShapeAuthoredMark(')
end = candidate.index('            half GetPatternSource(', start)
shape = candidate[start:end]
start = candidate.index('            half GetChannelPattern(')
end = candidate.index('            Varyings Vert(', start)
transport = candidate[start:end].replace('GetChannelPattern(', 'GetStableChannelPattern(').replace('_ValidationTime', '_Time.y')
transport = transport.replace('One half-pattern-unit is an exact repeat after primary x2,\n                // including the secondary x2 sample.',
    'One pattern-unit is an exact repeat for primary x2,\n                // secondary x4 and the low-frequency authored gate x1.')
transport = transport.replace('                FlowData clockFlow', '''                // A fixed map-space reference keeps one clock across the chart.
                // B still controls local spatial frequency, elongation and opacity;
                // a smaller longitudinal frequency produces faster local travel.
                // Constant-B surfaces retain the full SPEC-007 speed response.
                // This avoids time * spatially-varying velocity, whose derivative
                // grows forever even when the sampling coordinates are wrapped.
                FlowData clockFlow''')
a = old.index('            half GetShapedHybridPattern(')
b = old.index('            half GetPatternSource(', a)
new = old[:a] + shape + old[b:]
new = new.replace('            Varyings Vert(', transport + '            Varyings Vert(')
new = new.replace('? GetChannelPattern(input.flowUV, input.flowFrame, flow)',
    '? (_PatternSourceMode > 2.5h\n                            ? GetStableChannelPattern(input.flowUV, input.flowFrame, flow)\n                            : GetChannelPattern(input.flowUV, input.flowFrame, flow))')
new = new.replace('Shaped Hybrid, 3', 'Painterly Flow, 3')
new = new.replace('        _PatternScale (', '''        _PrimaryMarkWidth ("Primary Mark Width", Range(1.0, 8.0)) = 4.0
        _SecondaryMarkStrength ("Secondary Mark Strength", Range(0.0, 0.5)) = 0.24
        _PatternScale (''')
new = new.replace('                half _PatternScale;', '''                half _PrimaryMarkWidth;
                half _SecondaryMarkStrength;
                half _PatternScale;''')
new = new.replace('Range(0.1, 10.0)) = 2.2', 'Range(0.1, 10.0)) = 1.65')
new = new.replace('Range(1.0, 12.0)) = 4.0', 'Range(1.0, 12.0)) = 7.0')
new = new.replace('Range(0.0, 1.0)) = 0.62', 'Range(0.0, 1.0)) = 0.5')
path.write_text(new, encoding='utf-8')
