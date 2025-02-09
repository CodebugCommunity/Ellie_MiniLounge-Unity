
Shader "Custom/Ok/PickerGuide"
{
	Properties
	{
	    _MainTex ("Color", 2D) = "white" {}
	}
	
	SubShader
	{
	    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    LOD 100
	    ZWrite Off
	    Blend SrcAlpha OneMinusSrcAlpha
	
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
	            sampler2D _AlphaTex;
	            float4 _AlphaTex_ST;
	
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
					fixed2 center = fixed2(0.5, 0.5);
	                fixed4 color = tex2D(_MainTex, center);
					float a = distance(i.texcoord.xy, center) * 16.0f;
					a = 8.0f - a;
					color.a = a;
	                return saturate(color);
	            }
	        ENDCG
	    }
	}
}
