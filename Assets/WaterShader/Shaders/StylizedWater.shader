Shader "Meganeura/Water/Stylized Water"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.05, 0.90, 0.95, 1.0)
        _DeepColor ("Deep Color", Color) = (0.0, 0.38, 0.72, 1.0)
        _DepthDistance ("Depth Distance", Range(0.01, 10.0)) = 1.6
        _DepthFalloff ("Depth Falloff", Range(0.1, 4.0)) = 1.0
        _Opacity ("Opacity", Range(0.0, 1.0)) = 0.72
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
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
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
                return half4(waterColor.rgb, saturate(waterColor.a * _Opacity));
            }
            ENDHLSL
        }
    }

    FallBack Off
}
