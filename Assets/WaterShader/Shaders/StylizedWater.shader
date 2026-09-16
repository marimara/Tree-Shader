Shader "Meganeura/Water/Stylized Water"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.05, 0.90, 0.95, 1.0)
        _DeepColor ("Deep Color", Color) = (0.0, 0.38, 0.72, 1.0)
        _DepthDistance ("Depth Distance", Range(0.01, 10.0)) = 1.6
        _DepthFalloff ("Depth Falloff", Range(0.1, 4.0)) = 1.0
        _Opacity ("Opacity", Range(0.0, 1.0)) = 0.72
        _FlowDirection ("Flow Direction", Vector) = (1.0, 0.0, 0.0, 0.0)
        _FlowSpeed ("Flow Speed", Range(0.0, 5.0)) = 0.35
        _FlowStrength ("Flow Strength", Range(0.0, 1.0)) = 0.66
        [NoScaleOffset] _FlowMap ("Flow Map (RG Direction, B Strength)", 2D) = "gray" {}
        [Toggle] _UseFlowMap ("Use Flow Map", Float) = 0.0
        [Toggle] _UseFlowCoordinates ("Use Channel Coordinates (UV2 + UV3 Frame)", Float) = 0.0
        _FlowMapMinStrength ("Flow Map Min Strength", Range(0.0, 1.0)) = 0.35
        _FlowMapMaxStrength ("Flow Map Max Strength", Range(0.0, 1.0)) = 0.82
        _FlowMapOrientationInfluence ("Flow Map Orientation Influence", Range(0.0, 1.0)) = 0.72
        [Enum(Off, 0, Direction, 1, Strength, 2)] _FlowDebugMode ("Flow Debug Mode", Float) = 0.0
        [Enum(Procedural, 0, Texture, 1, Raw Hybrid, 2, Shaped Hybrid, 3)] _PatternSourceMode ("Pattern Source Mode", Float) = 3.0
        [NoScaleOffset] _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _PatternScale ("Pattern Scale", Range(0.1, 10.0)) = 2.2
        _PatternStrength ("Pattern Strength", Range(0.0, 1.0)) = 0.55
        _PatternStretch ("Pattern Stretch", Range(1.0, 12.0)) = 4.0
        [HDR] _PatternColor ("Pattern Color", Color) = (0.62, 1.15, 1.30, 0.85)
        _PatternThreshold ("Pattern Threshold", Range(0.0, 1.0)) = 0.62
        _PatternSoftness ("Pattern Softness", Range(0.01, 0.25)) = 0.075
        [HideInInspector] _NoiseScale ("Legacy Noise Scale", Range(0.1, 10.0)) = 2.0
        _NoiseContrast ("Noise Contrast", Range(0.1, 4.0)) = 1.35
        [HideInInspector] _NoiseStretch ("Legacy Noise Stretch", Range(1.0, 8.0)) = 2.5
        [HideInInspector] _PatternIntensity ("Legacy Pattern Intensity", Range(0.0, 1.0)) = 0.45
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half _DepthDistance;
                half _DepthFalloff;
                half _Opacity;
                float4 _FlowDirection;
                half _FlowSpeed;
                half _FlowStrength;
                half _UseFlowMap;
                half _UseFlowCoordinates;
                half _FlowMapMinStrength;
                half _FlowMapMaxStrength;
                half _FlowMapOrientationInfluence;
                half _FlowDebugMode;
                half _PatternSourceMode;
                half _PatternScale;
                half _PatternStrength;
                half _PatternStretch;
                half4 _PatternColor;
                half _PatternThreshold;
                half _PatternSoftness;
                half _NoiseScale;
                half _NoiseContrast;
                half _NoiseStretch;
                half _PatternIntensity;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_FlowMap);
            SAMPLER(sampler_FlowMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 flowUV : TEXCOORD1;
                float4 flowFrame : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float2 flowUV : TEXCOORD3;
                float4 flowFrame : TEXCOORD4;
            };

            float2 GetUniformFlowDirection(float2 flowDirection)
            {
                float lengthSquared = dot(flowDirection, flowDirection);
                return lengthSquared > 0.000001 ? flowDirection * rsqrt(lengthSquared) : float2(0.0, 0.0);
            }

            struct FlowData
            {
                float2 direction;
                half strength;
                half encodedStrength;
            };

            FlowData GetFlowData(float2 baseUV)
            {
                FlowData flow;
                flow.direction = GetUniformFlowDirection(_FlowDirection.xy);
                flow.strength = saturate(_FlowStrength);
                flow.encodedStrength = flow.strength;

                if (_UseFlowMap > 0.5h)
                {
                    half4 flowSample = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, baseUV);
                    float2 decodedDirection = flowSample.rg * 2.0 - 1.0;
                    float directionLengthSquared = dot(decodedDirection, decodedDirection);
                    flow.direction = directionLengthSquared > 0.0001
                        ? decodedDirection * rsqrt(directionLengthSquared)
                        : float2(0.0, 0.0);
                    flow.encodedStrength = saturate(flowSample.b);
                    half minStrength = min(_FlowMapMinStrength, _FlowMapMaxStrength);
                    half maxStrength = max(_FlowMapMinStrength, _FlowMapMaxStrength);
                    flow.strength = lerp(minStrength, maxStrength, flow.encodedStrength);
                }

                return flow;
            }

            float2 GetAnimatedFlowUV(float2 baseUV, FlowData flow)
            {
                half flowCharacter = smoothstep(0.0h, 1.0h, flow.strength);
                half effectiveSpeedFactor = _PatternSourceMode < 2.5h ? 1.0h : lerp(0.06h, 1.0h, flowCharacter);
                return baseUV + flow.direction * _FlowSpeed * effectiveSpeedFactor * _Time.y;
            }

            float2 GetPatternDirection(FlowData flow)
            {
                float hasDirection = step(0.000001, dot(flow.direction, flow.direction));
                float2 localDirection = lerp(float2(1.0, 0.0), flow.direction, hasDirection);
                float2 referenceDirection = GetUniformFlowDirection(_FlowDirection.xy);
                float hasReference = step(0.000001, dot(referenceDirection, referenceDirection));
                referenceDirection = lerp(localDirection, referenceDirection, hasReference);

                // Flow-map curvature is produced by integrated advection below,
                // not by rotating absolute coordinates independently per fragment.
                // A stable reference basis keeps the SPEC-006/007 mark silhouette
                // clean while the backtrace supplies the spatial trajectory.
                return _UseFlowMap > 0.5h ? referenceDirection : localDirection;
            }

            void GetPatternMetrics(half strength, out half effectiveStretch, out half effectiveScale)
            {
                half flowCharacter = smoothstep(0.0h, 1.0h, strength);
                half shapedStretch = lerp(1.15h, _PatternStretch, flowCharacter);
                half shapedScale = _PatternScale * lerp(0.58h, 1.0h, flowCharacter);
                effectiveStretch = _PatternSourceMode < 2.5h ? _NoiseStretch : shapedStretch;
                effectiveScale = _PatternSourceMode < 2.5h ? _NoiseScale : shapedScale;
            }

            float2 GetDirectionalPatternUV(float2 animatedUV, FlowData flow)
            {
                float2 patternDirection = _UseFlowMap > 0.5h ? GetPatternDirection(flow) : flow.direction;
                float hasDirection = step(0.000001, dot(patternDirection, patternDirection));
                patternDirection = lerp(float2(1.0, 0.0), patternDirection, hasDirection);
                float2 patternPerpendicular = float2(-patternDirection.y, patternDirection.x);
                half effectiveStretch, effectiveScale;
                GetPatternMetrics(flow.strength, effectiveStretch, effectiveScale);
                // Rotate around the UV centre instead of the texture origin. This
                // bounds the displacement introduced by a varying local basis and
                // prevents large-scale bowing across the surface.
                float2 centredUV = _UseFlowMap > 0.5h ? animatedUV - 0.5 : animatedUV;
                float alongFlow = dot(centredUV, patternDirection) / max(effectiveStretch, 0.0001h);
                float acrossFlow = dot(centredUV, patternPerpendicular);
                return float2(alongFlow, acrossFlow) * effectiveScale;
            }

            half ProceduralNoise(float2 uv)
            {
                half broadWave = sin(uv.x * 6.283h + sin(uv.y * 3.7h) * 1.4h);
                half crossWave = sin(uv.y * 5.11h - uv.x * 2.37h);
                return saturate(0.5h + broadWave * 0.32h + crossWave * 0.18h);
            }

            half ApplyPatternContrast(half value)
            {
                return saturate((value - 0.5h) * _NoiseContrast + 0.5h);
            }

            half GetProceduralPattern(float2 patternUV)
            {
                return ApplyPatternContrast(ProceduralNoise(patternUV));
            }

            half GetTexturePattern(float2 patternUV)
            {
                half sampledNoise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, patternUV).r;
                return ApplyPatternContrast(sampledNoise);
            }

            half GetHybridPattern(float2 patternUV)
            {
                half broadNoise = ProceduralNoise(patternUV * 0.55 + 13.7);
                float2 distortedUV = patternUV + float2(broadNoise - 0.5h, 0.5h - broadNoise) * 0.16h;
                half sampledNoise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, distortedUV).r;
                half combinedNoise = lerp(sampledNoise, broadNoise, 0.22h);
                return smoothstep(0.22h, 0.78h, ApplyPatternContrast(combinedNoise));
            }

            half GetShapedHybridPattern(float2 patternUV, half flowStrength)
            {
                // Noise 1 supplies the authored silhouette. A low-frequency analytic
                // field only bends it slightly and gates it into separated marks.
                half distortionField = ProceduralNoise(patternUV * 0.38h + float2(4.7h, 11.3h));
                float2 distortedUV = patternUV + float2(distortionField - 0.5h, 0.5h - distortionField) * 0.12h;
                half authoredNoise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, distortedUV).r;
                half shapedSource = saturate((authoredNoise - 0.5h) * _NoiseContrast + 0.5h);

                half flowCharacter = smoothstep(0.0h, 1.0h, flowStrength);
                half effectiveThreshold = saturate(_PatternThreshold + lerp(0.16h, -0.10h, flowCharacter));
                half edgeSoftness = max(_PatternSoftness * lerp(1.35h, 1.0h, flowCharacter), 0.005h);
                half thresholdMask = smoothstep(
                    effectiveThreshold - edgeSoftness,
                    effectiveThreshold + edgeSoftness,
                    shapedSource);

                half selectiveField = ProceduralNoise(patternUV * float2(0.42h, 0.58h) + float2(19.1h, 3.4h));
                half selectiveLow = lerp(0.54h, 0.22h, flowCharacter);
                half selectiveHigh = lerp(0.78h, 0.56h, flowCharacter);
                half selectiveMask = smoothstep(selectiveLow, selectiveHigh, selectiveField);
                return thresholdMask * selectiveMask;
            }

            half GetPatternSource(float2 patternUV, half flowStrength)
            {
                if (_PatternSourceMode < 0.5h)
                    return GetProceduralPattern(patternUV);

                if (_PatternSourceMode < 1.5h)
                    return GetTexturePattern(patternUV);

                if (_PatternSourceMode < 2.5h)
                    return GetHybridPattern(patternUV);

                return GetShapedHybridPattern(patternUV, flowStrength);
            }

            float2 SampleFlowMapDirection(float2 uv)
            {
                half2 encodedDirection = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, saturate(uv)).rg;
                float2 decodedDirection = encodedDirection * 2.0 - 1.0;
                float directionLengthSquared = dot(decodedDirection, decodedDirection);
                return directionLengthSquared > 0.0001
                    ? decodedDirection * rsqrt(directionLengthSquared)
                    : float2(0.0, 0.0);
            }

            float2 TraceFlowMapBackward(float2 baseUV, float2 initialDirection, half distance)
            {
                // Compatibility path for meshes without a channel chart. Bounded
                // tracing improves motion, but cannot establish global alignment
                // of an anisotropic source pattern around arbitrary bends.
                const half integrationSteps = 4.0h;
                half stepDistance = distance / integrationSteps;
                float2 tracedUV = baseUV;
                float2 direction = initialDirection;

                [unroll]
                for (int stepIndex = 0; stepIndex < 4; stepIndex++)
                {
                    tracedUV -= direction * stepDistance;
                    if (stepIndex < 3)
                        direction = SampleFlowMapDirection(tracedUV);
                }

                return tracedUV;
            }

            half GetDualPhaseFlowMapPattern(float2 baseUV, FlowData flow)
            {
                half flowCharacter = smoothstep(0.0h, 1.0h, flow.strength);
                half hasDirection = step(0.000001h, dot(flow.direction, flow.direction));

                // A shared bounded clock keeps differently strong regions from
                // accumulating spatial phase shear over long runs.
                const half loopDistance = 0.24h;
                float cycleTime = _Time.y * _FlowSpeed / loopDistance;
                float phaseA = frac(cycleTime);
                float phaseB = frac(cycleTime + 0.5);
                half travelDistance = loopDistance * lerp(0.06h, 1.0h, flowCharacter) * hasDirection;

                // Orientation influence is now a small integrated pre-roll only.
                // It cannot blend away the Flow Map direction and does not rotate
                // coordinates locally; streamline integration remains authoritative.
                half orientationPreRoll = loopDistance * 0.18h * saturate(_FlowMapOrientationInfluence) * hasDirection;
                float2 sourceUVA = TraceFlowMapBackward(
                    baseUV,
                    flow.direction,
                    orientationPreRoll + phaseA * travelDistance);
                float2 sourceUVB = TraceFlowMapBackward(
                    baseUV,
                    flow.direction,
                    orientationPreRoll + phaseB * travelDistance);

                float2 patternUVA = GetDirectionalPatternUV(sourceUVA, flow);
                float2 patternUVB = GetDirectionalPatternUV(sourceUVB, flow);
                half patternA = GetPatternSource(patternUVA, flow.strength);
                half patternB = GetPatternSource(patternUVB, flow.strength);

                half sineA = sin(phaseA * 3.14159265h);
                half sineB = sin(phaseB * 3.14159265h);
                half weightA = sineA * sineA;
                half weightB = sineB * sineB;
                half weightSum = max(weightA + weightB, 0.0001h);
                return (patternA * weightA + patternB * weightB) / weightSum;
            }

            // UV0 addresses the SPEC-008 vector field. UV2 is a continuous chart:
            // X = distance along the channel, Y = distance across it. Geometry,
            // rather than a rotating per-pixel basis, establishes the silhouette.
            half GetChannelPattern(float2 channelUV, float4 frame, FlowData flow)
            {
                // UV3 holds d(UV0)/d(channel U,V). Interpolating a smooth authored
                // frame avoids triangle-wise ddx/ddy velocity seams on curved strips.
                float2 along = frame.xy, across = frame.zw;
                float determinant = along.x * across.y - along.y * across.x;
                float safeDet = abs(determinant) > 1e-12 ? determinant : 1.0;
                float2 chartVector = float2(
                    across.y * flow.direction.x - across.x * flow.direction.y,
                    along.x * flow.direction.y - along.y * flow.direction.x) / safeDet;
                chartVector = abs(determinant) > 1e-12 ? GetUniformFlowDirection(chartVector) : float2(0, 0);

                half character = smoothstep(0.0h, 1.0h, flow.strength);
                half speedFactor = _PatternSourceMode < 2.5h ? 1.0h : lerp(0.06h, 1.0h, character);
                const float loopDistance = 0.24;
                float phaseA = frac(_Time.y * _FlowSpeed / loopDistance);
                float phaseB = frac(phaseA + 0.5);
                float2 travel = chartVector * (loopDistance * speedFactor);

                // Reuse SPEC-006/007 shaping unchanged. A fixed chart basis avoids
                // reintroducing derivatives of a locally rotated coordinate frame.
                half stretch, scale;
                GetPatternMetrics(flow.strength, stretch, scale);
                float2 patternScale = float2(scale / max(stretch, 0.0001h), scale);
                half a = GetPatternSource((channelUV - phaseA * travel) * patternScale, flow.strength);
                half b = GetPatternSource((channelUV - phaseB * travel) * patternScale, flow.strength);
                half weight = sin(phaseA * 3.14159265);
                weight *= weight;
                return lerp(b, a, weight);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.flowUV = input.flowUV;
                output.flowFrame = input.flowFrame;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float sceneDepth = SampleSceneDepth(screenUV);

                #if !UNITY_REVERSED_Z
                    sceneDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1.0, sceneDepth);
                #endif

                float3 scenePositionWS = ComputeWorldSpacePosition(screenUV, sceneDepth, UNITY_MATRIX_I_VP);
                float3 depthDeltaWS = scenePositionWS - input.positionWS;
                float waterDepth = max(0.0, -dot(depthDeltaWS, normalize(input.normalWS)));
                half depthMask = pow(saturate(waterDepth / max(_DepthDistance, 0.0001h)), max(_DepthFalloff, 0.0001h));
                half4 waterColor = lerp(_ShallowColor, _DeepColor, depthMask);

                FlowData flow = GetFlowData(input.uv);
                if (_FlowDebugMode > 0.5h && _FlowDebugMode < 1.5h)
                    return half4(flow.direction * 0.5 + 0.5, 0.5h, 1.0h);
                if (_FlowDebugMode > 1.5h)
                    return half4(flow.encodedStrength, flow.encodedStrength, flow.encodedStrength, 1.0h);

                half pattern;
                if (_UseFlowMap > 0.5h)
                {
                    pattern = _UseFlowCoordinates > 0.5h
                        ? GetChannelPattern(input.flowUV, input.flowFrame, flow)
                        : GetDualPhaseFlowMapPattern(input.uv, flow);
                }
                else
                {
                    // Preserve the validated uniform-flow path as the fallback.
                    float2 animatedFlowUV = GetAnimatedFlowUV(input.uv, flow);
                    float2 patternUV = GetDirectionalPatternUV(animatedFlowUV, flow);
                    pattern = GetPatternSource(patternUV, flow.strength);
                }
                if (_PatternSourceMode < 2.5h)
                {
                    half patternBrightness = lerp(0.76h, 1.22h, pattern);
                    waterColor.rgb *= lerp(1.0h, patternBrightness, saturate(_PatternIntensity));
                }
                else
                {
                    half flowCharacter = smoothstep(0.0h, 1.0h, flow.strength);
                    half effectivePatternStrength = _PatternStrength * lerp(0.22h, 1.0h, flowCharacter);
                    half highlightContribution = saturate(pattern * effectivePatternStrength * _PatternColor.a);
                    waterColor.rgb = lerp(waterColor.rgb, _PatternColor.rgb, highlightContribution);
                }

                return half4(waterColor.rgb, saturate(waterColor.a * _Opacity));
            }
            ENDHLSL
        }
    }

    FallBack Off
}
