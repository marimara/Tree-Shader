#ifndef STYLIZED_FOLIAGE_LIGHTING_INCLUDED
#define STYLIZED_FOLIAGE_LIGHTING_INCLUDED

void GetMainLightData_float(
    out float3 Direction,
    out float3 Color)
{
    #if defined(SHADERGRAPH_PREVIEW)
    Direction = normalize(float3(0.5, 0.5, 0.5));
    Color = float3(1.0, 1.0, 1.0);
    #else
    Direction = normalize(_MainLightPosition.xyz);
    Color = _MainLightColor.rgb;
    #endif
}

#endif