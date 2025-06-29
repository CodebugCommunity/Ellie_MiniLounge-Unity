// Unity Shader that behaves like the Standard PBR shader but is
// transparent and renders on both sides.
Shader "Custom/StandardTransparentTwoSided"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB) Alpha (A)", 2D) = "white" {}
        
        [Header(PBR Properties)]
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0

        [Header(Transparency)]
        _Opacity("Opacity", Range(0, 1)) = 0.5
    }

    SubShader
    {
        // Tags are crucial for transparency.
        // "Queue"="Transparent" ensures it draws after opaque objects.
        // "RenderType"="Transparent" is for shader replacement features.
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        // --- Key Changes for Transparency and Two-Sidedness ---

        // 1. Render both sides
        Cull Off

        // 2. Set the blending mode for transparency
        Blend SrcAlpha OneMinusSrcAlpha

        // 3. Don't write to the depth buffer (common for transparent objects)
        ZWrite Off

        CGPROGRAM
        // 4. Update pragma for alpha blending and shadow casting
        #pragma surface surf Standard alpha:fade fullforwardshadows addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        half _Opacity; // Our new opacity property
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Get the color and alpha from the texture
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            // Set the PBR properties
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            // 5. Calculate final alpha by multiplying texture's alpha with the global opacity
            o.Alpha = c.a * 5.0f;
        }
        ENDCG
    }
    FallBack "Transparent/VertexLit"
}