Shader "Meganeura/Stylized Foliage"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _AlphaMap ("Alpha Map", 2D) = "white" {}
        [Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _AlphaClipThreshold ("Alpha Clip Threshold", Range(0,1)) = 0.5
        _StylizedNormalStrength ("Stylized Normal Strength", Range(0,1)) = 0
        _NormalStrength ("Normal Strength", Range(0,1)) = 0.35
        _CanopyCenterOffset ("Canopy Center (Object Space)", Vector) = (0,0,0,0)
        _NormalNoiseStrength ("Normal Noise Strength", Range(0,1)) = 0.18
        _NormalNoiseScale ("Normal Noise Scale", Range(0.1,4)) = 0.75
        _ShadowThreshold ("Shadow Threshold", Range(0,1)) = 0.5
        _ShadowSoftness ("Shadow Softness", Range(0,1)) = 0.5
        _ShadowStrength ("Shadow Strength (Stops)", Range(0,4)) = 0.7
        _LightDirectionBias ("Light Direction Bias (World Space)", Vector) = (0,0,0,0)
        _LightColor ("Light Color", Color) = (0.36,0.68,0.18,1)
        _MidColor ("Mid Color", Color) = (0.18,0.55,0.20,1)
        _ShadowColor ("Shadow Color", Color) = (0.08,0.42,0.20,1)
        _DeepShadowColor ("Deep Shadow Color", Color) = (0.055,0.34,0.19,1)
        _InteriorColor ("Interior Color", Color) = (0.035,0.24,0.13,1)
        _InteriorStrength ("Interior Strength", Range(0,1)) = 0.75
        _InteriorRadius ("Interior Radius (Object Space)", Range(0.001,0.1)) = 0.026
        _AOStrength ("AO Strength", Range(0,1)) = 0.35
        _HeightDarkening ("Height Darkening", Range(0,1)) = 0.15
        _HeightGradientPosition ("Height Gradient Position", Range(-1,1)) = -0.05
        _ColorVariationStrength ("Color Variation Strength", Range(0,1)) = 0.18
        _ColorVariationScale ("Color Variation Scale", Range(0.1,4)) = 0.9
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
        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _AlphaMap_ST;
            float4 _NormalMap_ST;
            half4 _BaseColor;
            half _AlphaClipThreshold;
            float4 _CanopyCenterOffset;
            half _StylizedNormalStrength;
            half _NormalStrength;
            half _NormalNoiseStrength;
            half _NormalNoiseScale;
            half _ShadowThreshold;
            half _ShadowSoftness;
            half _ShadowStrength;
            float4 _LightDirectionBias;
            half4 _LightColor;
            half4 _MidColor;
            half4 _ShadowColor;
            half4 _DeepShadowColor;
            half4 _InteriorColor;
            half _InteriorStrength;
            half _InteriorRadius;
            half _AOStrength;
            half _HeightDarkening;
            half _HeightGradientPosition;
            half _ColorVariationStrength;
            half _ColorVariationScale;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
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
            float3 positionOS : TEXCOORD6;
            half4 tangentWSAndSign : TEXCOORD7;
            float2 normalUV : TEXCOORD8;
        };

        half SampleLeafAlpha(float2 uv)
        {
            return SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, uv).r;
        }

        // Smooth, static object-space value noise. It is intentionally sampled at
        // low frequency and never uses screen position, time, or foliage UVs.
        float HashVariation(float3 cell)
        {
            cell = frac(cell * 0.1031);
            cell += dot(cell, cell.yzx + 33.33);
            return frac((cell.x + cell.y) * cell.z);
        }

        float ValueNoise3D(float3 position)
        {
            float3 cell = floor(position);
            float3 local = frac(position);
            local = local * local * (3.0 - 2.0 * local);

            float x00 = lerp(HashVariation(cell + float3(0, 0, 0)),
                HashVariation(cell + float3(1, 0, 0)), local.x);
            float x10 = lerp(HashVariation(cell + float3(0, 1, 0)),
                HashVariation(cell + float3(1, 1, 0)), local.x);
            float x01 = lerp(HashVariation(cell + float3(0, 0, 1)),
                HashVariation(cell + float3(1, 0, 1)), local.x);
            float x11 = lerp(HashVariation(cell + float3(0, 1, 1)),
                HashVariation(cell + float3(1, 1, 1)), local.x);
            return lerp(lerp(x00, x10, local.y), lerp(x01, x11, local.y), local.z);
        }

        float3 GetCanopyVariationCoordinates(float3 positionOS, half scale)
        {
            float3 canopyLocal = positionOS - _CanopyCenterOffset.xyz;
            return canopyLocal * (scale / max(_InteriorRadius, 0.0001h));
        }

        float3 ApplyNormalNoise(float3 radialOS, float3 positionOS)
        {
            float3 radialDirectionOS = SafeNormalize(radialOS);
            float3 noisePosition = GetCanopyVariationCoordinates(positionOS,
                _NormalNoiseScale);
            float3 noiseVector = float3(
                ValueNoise3D(noisePosition + float3(11.7, 3.1, 7.9)),
                ValueNoise3D(noisePosition + float3(2.3, 17.1, 5.4)),
                ValueNoise3D(noisePosition + float3(6.2, 9.8, 19.3))) - 0.5;
            // Tangential noise deforms the spherical direction without pulling
            // the canopy lighting inward or exposing card orientation. The full
            // slider is deliberately strong for validation; production defaults
            // keep this broad deformation secondary to canopy volume.
            noiseVector -= radialDirectionOS * dot(noiseVector, radialDirectionOS);
            return SafeNormalize(radialDirectionOS
                + noiseVector * (_NormalNoiseStrength * 1.10h));
        }

        float3 BuildStableTangent(float3 baseNormalWS, float3 tangentWS)
        {
            float3 projectedTangent = tangentWS
                - baseNormalWS * dot(tangentWS, baseNormalWS);
            if (dot(projectedTangent, projectedTangent) > 1e-8)
                return normalize(projectedTangent);

            // Imported tangents can become parallel to a strongly radial normal.
            // Build a deterministic fallback axis instead of allowing a zero basis.
            float3 fallbackAxis = abs(baseNormalWS.y) < 0.999
                ? float3(0.0, 1.0, 0.0)
                : float3(1.0, 0.0, 0.0);
            return SafeNormalize(cross(fallbackAxis, baseNormalWS));
        }

        float3 ApplyLeafNormalDetail(float3 baseNormalWS, float3 tangentWS,
            half tangentSign, float2 normalUV)
        {
            baseNormalWS = SafeNormalize(baseNormalWS);
            float3 tangent = BuildStableTangent(baseNormalWS, tangentWS);
            float3 bitangent = SafeNormalize(cross(baseNormalWS, tangent)) * tangentSign;

            // Reconstructing Z after scaling XY gives a valid unit tangent-space
            // normal. At strength zero it is exactly (0,0,1), so the Spec 008
            // canopy normal passes through unchanged.
            half3 detailTS = UnpackNormal(SAMPLE_TEXTURE2D(
                _NormalMap, sampler_NormalMap, normalUV));
            detailTS.xy *= _NormalStrength;
            detailTS.z = sqrt(saturate(1.0h - dot(detailTS.xy, detailTS.xy)));

            // Rotate the tangent-space detail into an orthonormal frame whose Z
            // axis is the stylized canopy normal. This adds a surface gradient
            // without replacing or naively adding to the large-scale normal.
            return SafeNormalize(tangent * detailTS.x
                + bitangent * detailTS.y
                + baseNormalWS * detailTS.z);
        }

        half3 SampleArtisticColorRamp(half lightingMask)
        {
            // Compress the deepest directional region so the canopy is led by
            // mid and light greens while Deep Shadow remains an editable endpoint.
            half rampPosition = saturate(lightingMask * 0.85h + 0.15h) * 3.0h;
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
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/AmbientOcclusion.hlsl"

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(
                    input.normalOS, input.tangentOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                // Keep the offset in mesh units, independent of translation and uniform scale.
                float3 radialOS = input.positionOS.xyz - _CanopyCenterOffset.xyz;
                // Only the exact center is undefined; do not clamp small imported meshes.
                radialOS = dot(radialOS, radialOS) > 1e-20 ? radialOS : input.normalOS;
                radialOS = ApplyNormalNoise(radialOS, input.positionOS.xyz);
                output.radialNormalWS = TransformObjectToWorldNormal(radialOS);
                output.positionOS = input.positionOS.xyz;
                output.tangentWSAndSign = half4(normalInputs.tangentWS,
                    input.tangentOS.w * GetOddNegativeScale());
                output.baseUV = TRANSFORM_TEX(input.uv, _BaseMap);
                output.alphaUV = TRANSFORM_TEX(input.uv, _AlphaMap);
                output.normalUV = TRANSFORM_TEX(input.uv, _NormalMap);
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
                normalWS = ApplyLeafNormalDetail(normalWS,
                    input.tangentWSAndSign.xyz, input.tangentWSAndSign.w,
                    input.normalUV);

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
                half detailModulation = lerp(0.85h, 1.15h, saturate(baseLuminance));
                half3 paletteColor = SampleArtisticColorRamp(lightingMask) * _BaseColor.rgb;
                half3 detailedColor = paletteColor * detailModulation;

                // Approximate canopy density from a soft object-local ellipsoid. The
                // broad transition leaves the outer shell untouched and avoids a hard
                // dark disk at the exact center of the mesh.
                float3 canopyLocal = input.positionOS - _CanopyCenterOffset.xyz;
                float3 densityShape = canopyLocal * float3(1.00, 1.20, 1.10);
                half normalizedRadius = length(densityShape) / max(_InteriorRadius, 0.0001h);
                half radialInterior = 1.0h - smoothstep(0.38h, 1.0h, normalizedRadius);

                // This source asset is Z-up before its scene transform. Keeping the
                // gradient object-local makes the control stable under scene rotation.
                half normalizedHeight = canopyLocal.z / max(_InteriorRadius, 0.0001h);
                half heightMask = 1.0h - smoothstep(_HeightGradientPosition - 0.25h,
                    _HeightGradientPosition + 0.25h, normalizedHeight);
                // Restrict height darkening toward the canopy body so the lower
                // silhouette does not become a uniformly dark ring.
                half heightInterior = heightMask * (1.0h - smoothstep(0.65h, 1.15h,
                    normalizedRadius));

                AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(
                    GetNormalizedScreenSpaceUV(input.positionCS));
                half aoOcclusion = 1.0h - min(aoFactor.directAmbientOcclusion,
                    aoFactor.indirectAmbientOcclusion);
                half baseInterior = saturate(radialInterior * _InteriorStrength
                    + heightInterior * _HeightDarkening);
                // Remap the renderer's soft SSAO signal into a readable artistic cue.
                // Compositing into the remaining headroom prevents early saturation,
                // while the density weighting keeps AO subordinate to fake density.
                half aoDepthCue = smoothstep(0.02h, 0.45h, aoOcclusion);
                half aoWeight = aoDepthCue * _AOStrength *
                    lerp(0.12h, 0.38h, smoothstep(0.05h, 0.75h, baseInterior));
                half interiorAmount = baseInterior + (1.0h - baseInterior) * aoWeight;
                detailedColor = lerp(detailedColor,
                    _InteriorColor.rgb * _BaseColor.rgb * detailModulation,
                    interiorAmount);

                // A single broad field adds coherent brightness and warm/cool drift.
                // Strength 1 is intentionally exaggerated for visual validation,
                // while lower values preserve the established palette hierarchy.
                half colorVariation = ValueNoise3D(GetCanopyVariationCoordinates(
                    input.positionOS, _ColorVariationScale) + float3(4.7, 13.2, 8.1));
                // Expand the useful middle of value noise so the top half of the
                // slider is readable without introducing hard bands.
                colorVariation = (smoothstep(0.15h, 0.85h, colorVariation) * 2.0h - 1.0h)
                    * _ColorVariationStrength;
                half brightnessVariation = 1.0h + colorVariation * 0.48h;
                half3 warmCoolTint = half3(1.0h + colorVariation * 0.20h,
                    1.0h + colorVariation * 0.05h,
                    1.0h - colorVariation * 0.22h);
                detailedColor *= brightnessVariation * warmCoolTint;

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

        Pass
        {
            Name "DepthNormalsOnly"
            Tags { "LightMode"="DepthNormalsOnly" }

            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            struct DepthNormalsVaryings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                float2 alphaUV : TEXCOORD1;
                float2 normalUV : TEXCOORD2;
                half4 tangentWSAndSign : TEXCOORD3;
            };

            DepthNormalsVaryings DepthNormalsVert(Attributes input)
            {
                DepthNormalsVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

                VertexNormalInputs normalInputs = GetVertexNormalInputs(
                    input.normalOS, input.tangentOS);
                half3 meshNormalWS = normalInputs.normalWS;
                float3 radialOS = input.positionOS.xyz - _CanopyCenterOffset.xyz;
                radialOS = dot(radialOS, radialOS) > 1e-20 ? radialOS : input.normalOS;
                radialOS = ApplyNormalNoise(radialOS, input.positionOS.xyz);
                half3 radialWS = TransformObjectToWorldNormal(radialOS);
                half3 blendedWS = lerp(meshNormalWS, radialWS, _StylizedNormalStrength);
                output.normalWS = dot(blendedWS, blendedWS) > 1e-8
                    ? normalize(blendedWS) : normalize(radialWS);
                output.alphaUV = TRANSFORM_TEX(input.uv, _AlphaMap);
                output.normalUV = TRANSFORM_TEX(input.uv, _NormalMap);
                output.tangentWSAndSign = half4(normalInputs.tangentWS,
                    input.tangentOS.w * GetOddNegativeScale());
                return output;
            }

            half4 DepthNormalsFrag(DepthNormalsVaryings input) : SV_Target
            {
                clip(SampleLeafAlpha(input.alphaUV) - _AlphaClipThreshold);
                input.normalWS = ApplyLeafNormalDetail(input.normalWS,
                    input.tangentWSAndSign.xyz, input.tangentWSAndSign.w,
                    input.normalUV);

                #if defined(_GBUFFER_NORMALS_OCT)
                    float2 octNormalWS = PackNormalOctQuadEncode(normalize(input.normalWS));
                    float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
                    half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
                    return half4(packedNormalWS, 0.0h);
                #else
                    return half4(NormalizeNormalPerPixel(input.normalWS), 0.0h);
                #endif
            }
            ENDHLSL
        }
    }

    FallBack Off
}
