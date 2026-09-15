Shader "Meganeura/Stylized Foliage"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _AlphaMap ("Alpha Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _AlphaClipThreshold ("Alpha Clip Threshold", Range(0,1)) = 0.5
        _StylizedNormalStrength ("Stylized Normal Strength", Range(0,1)) = 0
        _CanopyCenterOffset ("Canopy Center (Object Space)", Vector) = (0,0,0,0)
        _ShadowThreshold ("Shadow Threshold", Range(0,1)) = 0.5
        _ShadowSoftness ("Shadow Softness", Range(0,1)) = 0.5
        _ShadowStrength ("Shadow Strength (Stops)", Range(0,4)) = 1.5
        _LightDirectionBias ("Light Direction Bias (World Space)", Vector) = (0,0,0,0)
        _LightColor ("Light Color", Color) = (0.75,1.0,0.22,1)
        _MidColor ("Mid Color", Color) = (0.12,0.72,0.20,1)
        _ShadowColor ("Shadow Color", Color) = (0.025,0.46,0.30,1)
        _DeepShadowColor ("Deep Shadow Color", Color) = (0.02,0.32,0.30,1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
        }

        Cull Off
        ZWrite On

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_AlphaMap);
        SAMPLER(sampler_AlphaMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _AlphaMap_ST;
            half4 _BaseColor;
            half _AlphaClipThreshold;
            float4 _CanopyCenterOffset;
            half _StylizedNormalStrength;
            half _ShadowThreshold;
            half _ShadowSoftness;
            half _ShadowStrength;
            float4 _LightDirectionBias;
            half4 _LightColor;
            half4 _MidColor;
            half4 _ShadowColor;
            half4 _DeepShadowColor;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            float2 baseUV : TEXCOORD2;
            float2 alphaUV : TEXCOORD3;
            half fogFactor : TEXCOORD4;
            float3 radialNormalWS : TEXCOORD5;
        };

        half SampleLeafAlpha(float2 uv)
        {
            return SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, uv).r;
        }

        half3 SampleArtisticColorRamp(half lightingMask)
        {
            half rampPosition = saturate(lightingMask) * 3.0h;
            half3 color = lerp(_DeepShadowColor.rgb, _ShadowColor.rgb,
                smoothstep(0.0h, 1.0h, rampPosition));
            color = lerp(color, _MidColor.rgb,
                smoothstep(0.0h, 1.0h, rampPosition - 1.0h));
            color = lerp(color, _LightColor.rgb,
                smoothstep(0.0h, 1.0h, rampPosition - 2.0h));
            return color;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                // Keep the offset in mesh units, independent of translation and uniform scale.
                float3 radialOS = input.positionOS.xyz - _CanopyCenterOffset.xyz;
                // Only the exact center is undefined; do not clamp small imported meshes.
                radialOS = dot(radialOS, radialOS) > 1e-20 ? radialOS : input.normalOS;
                output.radialNormalWS = TransformObjectToWorldNormal(radialOS);
                output.baseUV = TRANSFORM_TEX(input.uv, _BaseMap);
                output.alphaUV = TRANSFORM_TEX(input.uv, _AlphaMap);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE faceSign : FRONT_FACE_SEMANTIC) : SV_Target
            {
                clip(SampleLeafAlpha(input.alphaUV) - _AlphaClipThreshold);

                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
                half3 normalWS = normalize(input.normalWS);
                normalWS *= IS_FRONT_VFACE(faceSign, 1.0h, -1.0h);
                // Radial normals describe the canopy, so never flip them on card backfaces.
                float3 radialWS = SafeNormalize(input.radialNormalWS);
                float3 blendedWS = lerp(normalWS, radialWS, _StylizedNormalStrength);
                // Opposing normals can cancel at the midpoint; keep a finite direction.
                normalWS = dot(blendedWS, blendedWS) > 1e-8 ? normalize(blendedWS) : radialWS;

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                // Bias is world-space and bounded to a subtle angular offset.
                float3 bias = _LightDirectionBias.xyz;
                bias *= min(1.0, 0.25 / max(length(bias), 1e-5));
                float3 lightDirection = SafeNormalize(mainLight.direction + bias);
                half lambertMask = dot(normalWS, lightDirection) * 0.5h + 0.5h;
                half halfWidth = max(_ShadowSoftness * 0.5h, 0.0001h);
                half lightingMask = smoothstep(_ShadowThreshold - halfWidth,
                    _ShadowThreshold + halfWidth, lambertMask);
                // Realtime occlusion changes the mask, never multiplies final RGB.
                lightingMask *= mainLight.shadowAttenuation;
                half3 ambient = max(SampleSH(normalWS), 0.0h);

                // The palette owns hue. BaseMap contributes only bounded luminance detail,
                // so the source olive color cannot steer the final color identity.
                const half3 luminanceWeights = half3(0.2126h, 0.7152h, 0.0722h);
                half baseLuminance = dot(baseSample.rgb, luminanceWeights);
                half detailModulation = lerp(0.65h, 1.35h, saturate(baseLuminance));
                half3 paletteColor = SampleArtisticColorRamp(lightingMask) * _BaseColor.rgb;
                half3 detailedColor = paletteColor * detailModulation;

                // Preserve the Spec 005 stop-based shadow control without black multiplication.
                // Ambient remains palette-tinted and deliberately subordinate to avoid washout.
                half shadowExposure = lerp(exp2(-_ShadowStrength), 1.0h, lightingMask);
                half3 mainLightTint = lerp(1.0h, mainLight.color * mainLight.distanceAttenuation,
                    lightingMask * 0.35h);
                half3 ambientContribution = detailedColor * ambient * 0.35h;
                half3 color = detailedColor * mainLightTint * shadowExposure + ambientContribution;
                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 alphaUV : TEXCOORD0;
            };

            ShadowVaryings ShadowVert(Attributes input)
            {
                ShadowVaryings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                output.alphaUV = TRANSFORM_TEX(input.uv, _AlphaMap);
                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_Target
            {
                clip(SampleLeafAlpha(input.alphaUV) - _AlphaClipThreshold);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 alphaUV : TEXCOORD0;
            };

            DepthVaryings DepthVert(Attributes input)
            {
                DepthVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.alphaUV = TRANSFORM_TEX(input.uv, _AlphaMap);
                return output;
            }

            half4 DepthFrag(DepthVaryings input) : SV_Target
            {
                clip(SampleLeafAlpha(input.alphaUV) - _AlphaClipThreshold);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack Off
}