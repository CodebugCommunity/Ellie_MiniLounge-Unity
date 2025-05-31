Shader "Custom/BlackTwoSidedCutout"
{
   Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
  }
  SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Geometry+1" "ForceNoShadowCasting"="True" }
    LOD 200
    Offset -1, -1
    Cull Off
    
    CGPROGRAM
    #pragma surface surf Lambert alphatest:_Cutoff
    
    sampler2D _MainTex;

    
    
    struct Input {
      float2 uv_MainTex;
    };
    
    void surf (Input IN, inout SurfaceOutput o) {
        half4 c = tex2D (_MainTex, IN.uv_MainTex);
        o.Albedo = c.rgb;
        const float treshold = 0.1f;
        if(c.r <= treshold && c.g <= treshold && c.b <= treshold)
        {
          //o.Albedo.rgb = 1; // Set Albedo to black
          o.Alpha = 0;
        }else o.Alpha = 1;

        clip(o.Alpha - 0.5);
          
      }
    ENDCG
    }
}