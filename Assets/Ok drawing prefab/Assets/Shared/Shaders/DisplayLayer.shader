
Shader "Custom/Ok/DisplayLayer"
{
	Properties
	{
	    _MainTex ("RGB", 2D) = "white" {}
		_Opacity ("Opacity", Range(0.0, 1.0)) = 0
		_Invert ("Invert", Range(0.0, 1.0)) = 0
		_MySrcMode ("SrcMode", Float) = 0
		_MyDstMode ("DstMode", Float) = 0
	}
	
	SubShader
	{
	    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    LOD 100
	    ZWrite Off
		Blend [_MySrcMode] [_MyDstMode]

	    Pass
		{
	        CGPROGRAM
	            #pragma vertex vert
	            #pragma fragment frag
	            #pragma target 2.0
	            #pragma multi_compile_fog
	
	            #include "UnityCG.cginc"
	
	            struct appdata_t
				{
	                float4 vertex : POSITION;
	                float2 texcoord : TEXCOORD0;
	                UNITY_VERTEX_INPUT_INSTANCE_ID
	            };
	
	            struct v2f
				{
	                float4 vertex : SV_POSITION;
	                float2 texcoord : TEXCOORD0;
	                UNITY_FOG_COORDS(1)
	                UNITY_VERTEX_OUTPUT_STEREO
	            };
	
	            sampler2D _MainTex;
	            float4 _MainTex_ST;
				float _Opacity;
				float _MySrcMode;
				float _MyDstMode;
				float _Invert;
	
	            v2f vert (appdata_t v)
	            {
	                v2f o;
	                UNITY_SETUP_INSTANCE_ID(v);
	                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	                o.vertex = UnityObjectToClipPos(v.vertex);
	                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
	                return o;
	            }
	
	            fixed4 frag (v2f i) : SV_Target
	            {
	                fixed4 color = tex2D(_MainTex, i.texcoord);
					float opSquared = _Opacity * _Opacity;
					float opSqauredAlpha = opSquared * color.a;

					float lightenFactor = _MySrcMode == 1 ? opSqauredAlpha : 1.0;
					color.rgb *= saturate(lightenFactor);

					float darkenFactor = _MySrcMode == 2 ? opSqauredAlpha : 1.0;
					color.rgb = lerp(float3(1,1,1), color.rgb, saturate(darkenFactor));

					color.a *= opSquared;
					float3 inv = float3(1,1,1) - pow(color.rgb, 0.45);
					color.rgb = lerp(color.rgb, inv*inv, _Invert);
					
					color.r = clamp(color.r, 0.0001, 0.9999);
					color.g = clamp(color.g, 0.0001, 0.9999);
					color.b = clamp(color.b, 0.0001, 0.9999);
	                return saturate(color);
	            }
	        ENDCG
	    }
	}
}
