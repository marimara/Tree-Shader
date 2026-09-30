Shader "Meganeura/Water/StylizedWater"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.1, 0.5, 0.8, 1)

        [Enum(None, 0, UV0, 1, FlowDirection, 2, FlowMagnitude, 3)]
        _DebugMode ("Debug Mode", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Forward"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 flow : TEXCOORD3;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 flow : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _DebugMode;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;
                output.flow = input.flow;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // -------------------------
                // Base Color
                // -------------------------

                half4 finalColor = _BaseColor;


                // -------------------------
                // Debug UV0
                // -------------------------

                half4 uvDebug = half4(
                    input.uv.x,
                    input.uv.y,
                    0.0,
                    1.0
                );


                // -------------------------
                // Flow data
                // -------------------------

                float2 rawFlow = input.flow;

                float flowMagnitude = length(rawFlow);

                float2 normalizedFlow =
                    flowMagnitude > 0.0001
                        ? rawFlow / flowMagnitude
                        : float2(0.0, 0.0);


                // -------------------------
                // Debug Flow Direction
                // -------------------------

                half4 flowDirectionDebug = half4(
                    normalizedFlow.x * 0.5 + 0.5,
                    normalizedFlow.y * 0.5 + 0.5,
                    0.0,
                    1.0
                );


                // -------------------------
                // Debug Flow Magnitude
                // -------------------------

                half magnitudeDebug = saturate(flowMagnitude);

                half4 flowMagnitudeDebug = half4(
                    magnitudeDebug,
                    magnitudeDebug,
                    magnitudeDebug,
                    1.0
                );


                // -------------------------
                // Debug Mode
                // -------------------------

                if (_DebugMode < 0.5)
                {
                    return finalColor;
                }

                if (_DebugMode < 1.5)
                {
                    return uvDebug;
                }

                if (_DebugMode < 2.5)
                {
                    return flowDirectionDebug;
                }

                return flowMagnitudeDebug;
            }
            ENDHLSL
        }
    }
}