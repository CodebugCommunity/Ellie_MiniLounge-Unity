Shader "Custom/PostApocBuildings_EnhancedV3" { // Renamed for clarity
    Properties {
        _Color ("Color Tint", Color) = (0.6, 0.7, 1.0, 1.0)

        // --- Building Walls ---
        [Header(Building Walls)]
        [NoScaleOffset] _MainTex ("Wall Albedo (XYZ)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap ("Wall Normal (XYZ)", 2D) = "bump" {}
        _Glossiness ("Wall Smoothness", Range(0.0, 1.0)) = 0.1
        _Metallic ("Wall Metallic", Range(0.0, 1.0)) = 0.1

        // --- Building Roofs ---
        [Header(Building Roofs)]
        [NoScaleOffset] _RoofTex ("Roof Albedo (XYZ)", 2D) = "gray" {}
        [NoScaleOffset] _RoofNormalMap ("Roof Normal (XYZ)", 2D) = "bump" {}
        _RoofGlossiness ("Roof Smoothness", Range(0.0, 1.0)) = 0.2
        _RoofMetallic ("Roof Metallic", Range(0.0, 1.0)) = 0.1
        _RoofBlendSharpness ("Roof Blend Sharpness", Range(1, 50)) = 15.0

        // --- Windows ---
        [Header(Windows)]
        [NoScaleOffset] _WindowTex ("Window Albedo (XYZ, A=Visibility)", 2D) = "black" {}
        _WindowGloss ("Window Smoothness", Range(0.0, 1.0)) = 0.8
        _WindowMetallic("Window Metallic", Range(0.0, 1.0)) = 0.5
        _WindowAlphaThreshold ("Window Visibility Threshold", Range(0.01, 1)) = 0.5

        // --- Vegetation ---
        [Header(Vegetation)]
        _MossTex ("Moss Albedo (XYZ, A=Alpha)", 2D) = "white" {}
        [NoScaleOffset] _MossNormal ("Moss Normal (XYZ)", 2D) = "bump" {}
        _MossColor ("Moss Color Tint", Color) = (0.3, 0.6, 0.2, 1.0)
        _MossGloss ("Moss Smoothness", Range(0.0, 1.0)) = 0.05
        _MossMetallic("Moss Metallic", Range(0.0, 1.0)) = 0.0

        [Header(Moss Growth)]
        _GrowHeight ("Max Grow World Height", Float) = 10.0
        _GrowFalloff ("Grow Height Falloff", Float) = 5.0
        _MossNoiseTex ("Moss Noise (XYZ, R=Patch/Hang)", 2D) = "gray" {}
        _PatchThreshold ("Patch Noise Threshold", Range(0, 1)) = 0.6
        _PatchStrength ("Patch Strength", Range(0, 1)) = 0.7
        _HangingStrength ("Hanging Moss Strength", Range(0, 1)) = 0.8
        _HangingNormalThreshold ("Hanging Normal Y Threshold", Range(-1, 0)) = -0.2
        _HangingNormalFalloff ("Hanging Normal Falloff", Range(0.01, 1.0)) = 0.3

        // --- General Settings ---
        [Header(General Settings)]
        _TriplanarScale ("Global Texture Scale", Float) = 10.0
        _TriplanarSharpness ("Triplanar Blend Sharpness", Float) = 5.0

    }
    SubShader {
        Tags { "RenderType"="Opaque" } // ** CORRECTED: Back to Opaque **
        LOD 300

        CGPROGRAM
        // ** CORRECTED: Removed alpha:fade **
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma optimization_level 3

        #include "UnityCG.cginc"

        // Triplanar Sampler (Handles individual Tiling/Offset via ST)
        float4 TriplanarSample(sampler2D tex, float3 worldPos, float3 worldNormal, float scale, float sharpness, float4 st) {
            float3 weights = pow(abs(worldNormal), sharpness);
            weights = weights / max(0.0001, (weights.x + weights.y + weights.z));

            float2 uvX = worldPos.yz / scale;
            float2 uvY = worldPos.xz / scale;
            float2 uvZ = worldPos.xy / scale;

            uvX = uvX * st.xy + st.zw;
            uvY = uvY * st.xy + st.zw;
            uvZ = uvZ * st.xy + st.zw;

            float4 sampleX = tex2D(tex, uvX);
            float4 sampleY = tex2D(tex, uvY);
            float4 sampleZ = tex2D(tex, uvZ);

            return sampleX * weights.x + sampleY * weights.y + sampleZ * weights.z;
        }

        // Triplanar Normal Sampler (Uses Global Scale Only)
        float3 TriplanarNormal(sampler2D bumpMap, float3 worldPos, float3 worldNormal, float scale, float sharpness) {
             float3 weights = pow(abs(worldNormal), sharpness);
             weights = weights / max(0.0001, (weights.x + weights.y + weights.z));

             float2 uvX = worldPos.yz / scale;
             float2 uvY = worldPos.xz / scale;
             float2 uvZ = worldPos.xy / scale;

             float3 normalX = UnpackNormal(tex2D(bumpMap, uvX));
             float3 normalY = UnpackNormal(tex2D(bumpMap, uvY));
             float3 normalZ = UnpackNormal(tex2D(bumpMap, uvZ));

             float3 blendedNormal = normalize(normalX * weights.x + normalY * weights.y + normalZ * weights.z);
             return blendedNormal;
        }

        struct Input {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        // Samplers
        sampler2D _MainTex, _BumpMap, _RoofTex, _RoofNormalMap, _WindowTex;
        sampler2D _MossTex, _MossNormal, _MossNoiseTex;

        // ST uniforms
        float4 _MossTex_ST;
        float4 _MossNoiseTex_ST;

        // Properties
        half _Glossiness, _Metallic, _RoofGlossiness, _RoofMetallic;
        half _WindowGloss, _WindowMetallic, _WindowAlphaThreshold;
        half _MossGloss, _MossMetallic;
        float4 _Color, _MossColor;
        float _GrowHeight, _GrowFalloff, _PatchThreshold, _PatchStrength;
        float _HangingStrength, _HangingNormalThreshold, _HangingNormalFalloff;
        float _TriplanarScale, _TriplanarSharpness, _RoofBlendSharpness;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            float3 worldPos = IN.worldPos;
            float3 geomWorldNormal = WorldNormalVector(IN, float3(0,0,1));

            float4 neutralST = float4(1.0, 1.0, 0.0, 0.0);

            // --- 1. Sample Base Wall & Roof Textures ---
            float4 wallAlbedo = TriplanarSample(_MainTex, worldPos, geomWorldNormal, _TriplanarScale, _TriplanarSharpness, neutralST);
            float3 wallNormal = TriplanarNormal(_BumpMap, worldPos, geomWorldNormal, _TriplanarScale, _TriplanarSharpness);
            float4 roofAlbedo = TriplanarSample(_RoofTex, worldPos, geomWorldNormal, _TriplanarScale, _TriplanarSharpness, neutralST);
            float3 roofNormal = TriplanarNormal(_RoofNormalMap, worldPos, geomWorldNormal, _TriplanarScale, _TriplanarSharpness);

            // --- 2. Sample Window Texture (RGBA) ---
            float4 windowSample = TriplanarSample(_WindowTex, worldPos, geomWorldNormal, _TriplanarScale, _TriplanarSharpness, neutralST);

            // --- 3. Sample Moss & Noise Textures ---
            float4 mossAlbedoSample = TriplanarSample(_MossTex, worldPos, geomWorldNormal, _TriplanarScale, _TriplanarSharpness, _MossTex_ST);
            float3 mossNormalSample = TriplanarNormal(_MossNormal, worldPos, geomWorldNormal, _TriplanarScale, _TriplanarSharpness);
            float4 noiseSample = TriplanarSample(_MossNoiseTex, worldPos, geomWorldNormal, _TriplanarScale, _TriplanarSharpness, _MossNoiseTex_ST);

            // --- 4. Determine Base Surface (Wall vs Roof) ---
            float roofBlend = pow(saturate(geomWorldNormal.y), _RoofBlendSharpness);
            float3 baseAlbedo = lerp(wallAlbedo.rgb, roofAlbedo.rgb, roofBlend);
            float3 baseNormal = lerp(wallNormal, roofNormal, roofBlend);
            float baseGloss = lerp(_Glossiness, _RoofGlossiness, roofBlend);
            float baseMetallic = lerp(_Metallic, _RoofMetallic, roofBlend);
            // No need to track baseAlpha for opaque shader

            // --- 5. Calculate Window Mask & Apply Window Layer ---
            float windowVisibility = step(_WindowAlphaThreshold, windowSample.a);
            float windowMask = windowVisibility * (1.0 - roofBlend); // Still prevent windows on roof

            // Apply window properties using the mask (overwriting base properties)
            baseAlbedo = lerp(baseAlbedo, windowSample.rgb, windowMask);
            baseGloss = lerp(baseGloss, _WindowGloss, windowMask);
            baseMetallic = lerp(baseMetallic, _WindowMetallic, windowMask);
            // Don't need to modify baseNormal unless window has specific normals

            // --- 6. Calculate Moss Coverage ---
            float mossBaseAlpha = mossAlbedoSample.a;
            float heightFactor = 1.0 - smoothstep(_GrowHeight - _GrowFalloff, _GrowHeight + _GrowFalloff, worldPos.y);
            float heightMoss = heightFactor * mossBaseAlpha;
            float patchNoiseFactor = step(_PatchThreshold, noiseSample.r);
            float patchMoss = patchNoiseFactor * _PatchStrength * mossBaseAlpha;
            float downwardFactor = smoothstep(_HangingNormalThreshold + _HangingNormalFalloff, _HangingNormalThreshold - _HangingNormalFalloff, geomWorldNormal.y);
            float hangingNoiseFactor = noiseSample.r;
            float hangingMoss = downwardFactor * hangingNoiseFactor * _HangingStrength * mossBaseAlpha;
            float finalMossMask = saturate(heightMoss + patchMoss + hangingMoss);

            // Reduce moss mask where windows are visible
            finalMossMask *= saturate(1.0 - windowMask * 0.2); // Use saturate just in case


            // --- 7. Apply Moss Layer ---
            float3 finalAlbedo = lerp(baseAlbedo, mossAlbedoSample.rgb * _MossColor.rgb, finalMossMask);
            float3 finalNormal = normalize(lerp(baseNormal, mossNormalSample, finalMossMask));
            float finalGloss = lerp(baseGloss, _MossGloss, finalMossMask);
            float finalMetallic = lerp(baseMetallic, _MossMetallic, finalMossMask);

            // --- 8. Final Output ---
            o.Albedo = finalAlbedo * _Color.rgb;
            o.Normal = finalNormal;
            o.Metallic = finalMetallic;
            o.Smoothness = finalGloss;
            o.Alpha = 1.0; // ** CORRECTED: Explicitly set Alpha to 1.0 for opaque output **
        }
        ENDCG
    }
    FallBack "Diffuse"
}