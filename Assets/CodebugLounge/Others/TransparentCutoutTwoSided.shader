Shader "Custom/TransparentCutoutDoubleSided" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest" // Render after opaque, before transparent, respects depth buffer
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout" // Helps Unity identify the shader type
        }
        LOD 100 // Legacy shaders often have lower LOD

        Cull Off // <<< THIS MAKES IT DOUBLE-SIDED

        CGPROGRAM
        // Use Lambert lighting model, enable alpha testing using _Cutoff property
        // addshadow directive generates a shadow caster pass that respects alpha test
        #pragma surface surf Lambert alphatest:_Cutoff addshadow

        #pragma target 2.0 // Suitable for legacy style

        sampler2D _MainTex;
        fixed4 _Color;
        // _Cutoff is automatically used by the "alphatest:_Cutoff" directive

        struct Input {
            float2 uv_MainTex;
            // float facing : VFACE; // VFACE is +1 for front, -1 for back (on D3D-like platforms)
                                  // Usually not needed as Lambert with Cull Off handles backface lighting
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);
            
            o.Albedo = texColor.rgb * _Color.rgb;
            o.Alpha = texColor.a * _Color.a; // The final alpha value used by alphatest

            // Optional: If backfaces are rendering black despite Cull Off
            // (This is rare with Unity's built-in Lambert lighting model, it usually handles it)
            // o.Normal = o.Normal * sign(IN.facing);
        }
        ENDCG
    }
    FallBack "Legacy Shaders/Transparent/Cutout"
}