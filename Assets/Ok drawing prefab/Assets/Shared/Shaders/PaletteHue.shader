
Shader "Custom/Ok/PaletteHue"
{
    Properties
    {
        _Hue ("Hue", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			float _Hue;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float h = _Hue;
				float s = i.uv.x;
				float l = i.uv.y;
				float3 rgb = clamp(abs(fmod(h*6+float3(0,4,2),6)-3)-1,0,1);
				rgb = l+s*(rgb-0.5)*(1-abs(2*l-1));
				return saturate(float4(rgb*rgb,1));
            }
            ENDCG
        }
    }
}
