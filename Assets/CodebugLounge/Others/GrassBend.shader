Shader "Custom/GrassBendShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BendStrength ("Bend Strength", Float) = 0.5
        _BendRadius ("Bend Radius", Float) = 2.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        //Cull Off
        CGPROGRAM
        // Use the Standard lighting model with a custom vertex modifier

        #pragma multi_compile _ LOD_FADE_CROSSFADE

        #pragma surface surf Standard vertex:VertMod

        sampler2D _MainTex;
        float _BendStrength;
        float _BendRadius;
        float3 _PlayerPositions[20]; // Must be set from a script (in world space)

        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
        };

        // Vertex modifier: offsets vertices away from the player when within _BendRadius
        void VertMod (inout appdata_full v) {
            // Convert vertex position to world space
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;


            for (int i = 0; i < 20; i++){
                 float distance = length(worldPos - _PlayerPositions[i]);
                
                if (distance < _BendRadius) {
                    // Calculate influence: 1.0 when at the player's position and 0.0 at _BendRadius
                    float influence = 1.0 - (distance / _BendRadius);
                    // Compute the direction from the player to the vertex (normalized)
                    float3 offsetDir = normalize(worldPos - _PlayerPositions[i]);
                    // Apply displacement based on influence and strength
                    worldPos += offsetDir * influence * _BendStrength;
                    
                }
            }

            // Transform back to object space
            v.vertex = mul(unity_WorldToObject, float4(worldPos, 1.0));
        }

        // Surface function: sample the main texture
        void surf (Input IN, inout SurfaceOutputStandard o) {

            #ifdef LOD_FADE_CROSSFADE
            float2 vpos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy;
            UnityApplyDitherCrossFade(vpos);
            #endif

            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = tex.rgb;
            o.Alpha = tex.a;
            clip(o.Alpha - 0.5);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
