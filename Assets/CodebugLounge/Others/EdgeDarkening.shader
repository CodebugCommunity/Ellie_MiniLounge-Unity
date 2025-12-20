Shader "Custom/Legacy/HardAndSmoothEdgeDarken"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        
        [Header(Edge Settings)]
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)
        
        [Header(Smooth Edge Detection)]
        _SmoothThreshold ("Smooth Threshold", Range(0.0, 1.0)) = 0.8
        _SmoothPower ("Smooth Hardness", Range(0.1, 10.0)) = 4.0

        [Header(Hard Edge Detection)]
        _HardThreshold ("Hard Normal Diff", Range(0.0, 1.0)) = 0.2
        _HardThickness ("Hard Thickness (Pixels)", Range(0.5, 4.0)) = 1.0
        
        [Toggle] _ShowDebug ("Debug Edges", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _CameraDepthNormalsTexture; // Automatically set by Unity
        float4 _CameraDepthNormalsTexture_TexelSize; // Automatically set (1/width, 1/height, width, height)

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
            float4 screenPos; // Built-in screen position
            INTERNAL_DATA
        };

        fixed4 _Color;
        fixed4 _EdgeColor;
        
        // Smooth settings
        half _SmoothThreshold;
        half _SmoothPower;
        
        // Hard settings
        half _HardThreshold;
        half _HardThickness;
        float _ShowDebug;

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            // We don't need to do manual calculation here, 
            // Input.screenPos is handled by surface shader generation
        }

        // Helper to get normal from screen buffer
        float3 GetScreenNormal(float2 uv)
        {
            float4 cdn = tex2D(_CameraDepthNormalsTexture, uv);
            float3 normal;
            float depth;
            DecodeDepthNormal(cdn, depth, normal);
            return normal;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            // ---------------------------------------------------------
            // 1. SMOOTH EDGE DETECTION (Geometry based)
            // ---------------------------------------------------------
            float3 flatNormal = normalize(cross(ddy(IN.worldPos), ddx(IN.worldPos)));
            float3 smoothNormal = normalize(IN.worldNormal);
            float dotN = saturate(abs(dot(smoothNormal, flatNormal)));
            
            float smoothEdgeFactor = smoothstep(_SmoothThreshold, 1.0, dotN);
            smoothEdgeFactor = pow(smoothEdgeFactor, _SmoothPower);


            // ---------------------------------------------------------
            // 2. HARD EDGE DETECTION (Screen-Space based)
            // ---------------------------------------------------------
            // Calculate UV coordinates for screen sampling
            float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

            // Get the normal of the current pixel from the buffer
            float3 centerNormal = GetScreenNormal(screenUV);

            // Sample neighbor pixels (Right and Up)
            float2 offset = _CameraDepthNormalsTexture_TexelSize.xy * _HardThickness;
            float3 rightNormal = GetScreenNormal(screenUV + float2(offset.x, 0));
            float3 upNormal    = GetScreenNormal(screenUV + float2(0, offset.y));

            // Calculate difference between neighbors
            // If the dot product is low, the angle is sharp
            float diffX = dot(centerNormal, rightNormal);
            float diffY = dot(centerNormal, upNormal);

            // A dot product of 1 means normals are same. 
            // A dot product of 0 means 90 degrees.
            // We verify if the difference is significant.
            float hardEdgeFactor = 1.0;
            
            // If difference is high (dot product low), we darken
            if(diffX < (1.0 - _HardThreshold) || diffY < (1.0 - _HardThreshold))
            {
                hardEdgeFactor = 0.0; // It's an edge
            }

            // ---------------------------------------------------------
            // 3. COMBINE
            // ---------------------------------------------------------
            
            // smoothEdgeFactor: 1 = white, 0 = black edge
            // hardEdgeFactor:   1 = white, 0 = black edge
            // We want to apply the edge color if EITHER is detecting an edge.
            
            float combinedFactor = min(smoothEdgeFactor, hardEdgeFactor);

            float3 finalColor = lerp(_EdgeColor.rgb, c.rgb, combinedFactor);

            if (_ShowDebug > 0.5)
            {
                // Visualization: White = Surface, Black = Edge
                o.Albedo = combinedFactor; 
            }
            else
            {
                o.Albedo = finalColor;
            }

            o.Metallic = 0;
            o.Smoothness = 0.5;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}