Shader "Custom/Terrain"
{
    Properties
    {
       testTexture("Texture", 2D) = "white"{}
       testScale("Scale", Float) = 1
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

        const static int maxLayerCount = 16;
        const static float epsilon = 1E-4;

        int layerCount;
        float3 baseColors[maxLayerCount];
        float baseStartHeights[maxLayerCount];
        float baseBlends[maxLayerCount];
        float baseColorStrengths[maxLayerCount];
        float baseTextureScales[maxLayerCount];

        float minHeight;
        float maxHeight;

        sampler2D testTexture;
        float testScale;

        UNITY_DECLARE_TEX2DARRAY(baseTextures);

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float InverseLerp(float min, float max, float value)
        {
            return saturate((value - min) / (max - min));
        }

        float3 Triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex)
        {
            float3 scaledWorldPosition = worldPos / scale;
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPosition.y, scaledWorldPosition.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPosition.x, scaledWorldPosition.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPosition.x, scaledWorldPosition.y, textureIndex)) * blendAxes.z;

            return xProjection + yProjection + zProjection;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
            float heightPercent = InverseLerp(minHeight, maxHeight, IN.worldPos.y);
            
            for(int i = 0; i < layerCount; i++)
            {
                float drawStrength = InverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, heightPercent - baseStartHeights[i]);

                float3 baseColor = baseColors[i] * baseColorStrengths[i];
                float3 textureColor = Triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColorStrengths[i]);

                o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor + textureColor) * drawStrength;
            }

            //o.Albedo = xProjection + yProjection + zProjection;
        }
        ENDCG
    }
    FallBack "Diffuse"
}