Shader "Meganeura/Stylized Foliage"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _AlphaMap ("Alpha Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _AlphaClipThreshold ("Alpha Clip Threshold", Range(0,1)) = 0.5
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
        };

        half SampleLeafAlpha(float2 uv)
        {
            return SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, uv).r;
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
                output.baseUV = TRANSFORM_TEX(input.uv, _BaseMap);
                output.alphaUV = TRANSFORM_TEX(input.uv, _AlphaMap);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE faceSign : FRONT_FACE_SEMANTIC) : SV_Target
            {
                clip(SampleLeafAlpha(input.alphaUV) - _AlphaClipThreshold);

                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV) * _BaseColor;
                half3 normalWS = normalize(input.normalWS);
                normalWS *= IS_FRONT_VFACE(faceSign, 1.0h, -1.0h);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 ambient = SampleSH(normalWS);
                half3 direct = mainLight.color * (NdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation);
                half3 color = baseSample.rgb * (ambient + direct);
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