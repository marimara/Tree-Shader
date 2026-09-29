"""Build an isolated experimental shader; never edits production assets."""
from pathlib import Path

source = (Path(__file__).parent / 'SourceBefore.shader.txt').read_text(encoding='utf-8-sig')
source = source.replace('Shader "Meganeura/Water/Stylized Water"', 'Shader "Hidden/Water/SPEC0116Candidate"')
source = source.replace('_Time.y', '_ValidationTime')
source = source.replace('        _ShallowColor (', '''        _ValidationTime ("Validation Time", Float) = 0
        _DiagnosticStage ("Diagnostic Stage", Float) = 7
        _PrimaryMarkWidth ("Primary Mark Width", Range(1, 8)) = 4
        _SecondaryMarkStrength ("Secondary Mark Strength", Range(0, 0.5)) = 0.24
        _ShallowColor (''')
source = source.replace('half4 _ShallowColor;', 'float _ValidationTime; float _DiagnosticStage; half _PrimaryMarkWidth; half _SecondaryMarkStrength; half4 _ShallowColor;')
a = source.index('            half GetShapedHybridPattern(')
b = source.index('            half GetPatternSource(', a)
source = source[:a] + '''            half ShapeAuthoredMark(half value)
            {
                // Lift the source's soft midtones instead of retaining only its
                // bright cores. No independent procedural gate or UV distortion.
                half authored = ApplyPatternContrast(sqrt(max(value, 0.0h)));
                half softness = max(_PatternSoftness, (half)fwidth(authored));
                return smoothstep(_PatternThreshold - softness,
                    _PatternThreshold + softness, authored);
            }

            half GetShapedHybridPattern(float2 patternUV, half flowStrength)
            {
                float2 uv = patternUV * float2(2.0, 1.0 / max(_PrimaryMarkWidth, 1.0h));
                half primary = ShapeAuthoredMark(SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv).r);
                // Integer scale shares the primary's exact repeat and velocity.
                half secondary = ShapeAuthoredMark(SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex,
                    uv * 2.0 + float2(0.37, 0.61)).r);
                return saturate(primary + (1.0h - primary) * secondary * _SecondaryMarkStrength);
            }

''' + source[b:]
a = source.index('            half GetChannelPattern(')
b = source.index('            Varyings Vert(', a)
source = source[:a] + '''            half GetChannelPattern(float2 channelUV, float4 frame, FlowData flow)
            {
                float2 along = frame.xy, across = frame.zw;
                float determinant = along.x * across.y - along.y * across.x;
                float safeDet = abs(determinant) > 1e-12 ? determinant : 1.0;
                float alongVelocity = (across.y * flow.direction.x - across.x * flow.direction.y) / safeDet;
                float directionSign = abs(determinant) > 1e-12 ? sign(alongVelocity) : 0.0;

                // The baked transverse chart already routes around obstacles.
                // Re-advecting it with tiny RG/frame interpolation residuals
                // double-deforms it and amplifies mesh/texel interpolation errors.
                half stretch, scale;
                GetPatternMetrics(flow.strength, stretch, scale);
                FlowData clockFlow = GetFlowData(float2(0.5, 0.5));
                half referenceStretch, referenceScale;
                GetPatternMetrics(clockFlow.strength, referenceStretch, referenceScale);
                half character = smoothstep(0.0h, 1.0h, clockFlow.strength);
                float clockRate = _FlowSpeed * lerp(0.06h, 1.0h, character)
                    * referenceScale / max(referenceStretch, 0.0001h);
                float2 uv = channelUV * float2(scale / max(stretch, 0.0001h), referenceScale);
                // One half-pattern-unit is an exact repeat after primary x2,
                // including the secondary x2 sample. No fading or phase reset.
                uv.x -= directionSign * frac(_ValidationTime * clockRate * 2.0) * 0.5;
                return GetPatternSource(uv, flow.strength);
            }

''' + source[b:]
(Path(__file__).parent / 'Candidate.shader').write_text(source, encoding='utf-8')
