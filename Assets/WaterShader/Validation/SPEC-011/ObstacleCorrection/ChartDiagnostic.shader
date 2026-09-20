Shader "Hidden/WaterShader/ChartDiagnostic"
{
 Properties { _FlowMap("Flow",2D)="gray"{} }
 SubShader { Tags { "RenderPipeline"="UniversalPipeline" } Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_FlowMap); SAMPLER(sampler_FlowMap);
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;float2 chart:TEXCOORD1;float4 frame:TEXCOORD2;};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float2 chart:TEXCOORD1;float4 frame:TEXCOORD2;};
 V vert(A a){V v;v.p=TransformObjectToHClip(a.p.xyz);v.uv=a.uv;v.chart=a.chart;v.frame=a.frame;return v;}
 half4 frag(V v):SV_Target {float2 d=SAMPLE_TEXTURE2D(_FlowMap,sampler_FlowMap,v.uv).rg*2-1;float2 a=v.frame.xy,b=v.frame.zw;float det=a.x*b.y-a.y*b.x;float2 vel=float2(b.y*d.x-b.x*d.y,a.x*d.y-a.y*d.x)/det;return half4(saturate(abs(vel.y)*5),frac(v.chart.y*10)>.5?.4:.1,0,1);}
 ENDHLSL
 } }
}
