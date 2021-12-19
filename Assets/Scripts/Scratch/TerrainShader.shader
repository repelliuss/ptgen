Shader "Custom/ProceduralTerrain"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static int maxLayerCount = 10;
        const static float epsilon = 1e-5;

        int layerCount;
        float3 tints[maxLayerCount];
        float tintStrengths[maxLayerCount];
        float startHeights[maxLayerCount];
        float blendStrengths[maxLayerCount];
        float scales[maxLayerCount];

        UNITY_DECLARE_TEX2DARRAY(textures);

        float minHeight;
        float maxHeight;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float inverseLerp(float a, float b, float value)
        {
            return saturate((value - a)/(b-a));
        }

        float3 triplanar(float3 pos, float scale, float3 blend, int index)
        {
            float3 scaledPos = pos / scale;

            float3 x = UNITY_SAMPLE_TEX2DARRAY(textures,
                float3(scaledPos.y, scaledPos.z, index)) * blend.x;

            float3 y = UNITY_SAMPLE_TEX2DARRAY(textures,
                float3(scaledPos.x, scaledPos.z, index)) * blend.y;

            float3 z = UNITY_SAMPLE_TEX2DARRAY(textures,
                float3(scaledPos.x, scaledPos.y, index)) * blend.z;

            return x + y + z;
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float normalizedHeight = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

            for(int i = 0; i < layerCount; ++i)
            {
                float strength = inverseLerp(-blendStrengths[i]/2,
                    blendStrengths[i]/2 + epsilon, normalizedHeight - startHeights[i]);
                float3 tint = tints[i] * tintStrengths[i];
                float3 tex = triplanar(IN.worldPos, scales[i], blendAxes, i);
                tex *= (1 - tintStrengths[i]);

                o.Albedo = o.Albedo * (1 - strength) + (tint+tex) * strength;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
