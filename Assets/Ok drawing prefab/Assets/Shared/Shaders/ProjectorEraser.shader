
Shader "Custom/Ok/ProjectorEraser"
{
	Properties
	{
		_Alpha ("Alpha", Range(0.0, 1.0)) = 0.5
		_Softness("Softness", Range(0.0, 5.0)) = 0.0
	    _Blend1 ("Blend 1", 2D) = "white" {}
	    _Blend2 ("Blend 2", 2D) = "white" {}
	    _Blend3 ("Blend 3", 2D) = "white" {}
	    _Blend4 ("Blend 4", 2D) = "white" {}
	    _Blend5 ("Blend 5", 2D) = "white" {}
	    _Blend6 ("Blend 6", 2D) = "white" {}
		_FalloffTex1 ("FallOff 1", 2D) = "" {}
		_FalloffTex2 ("FallOff 2", 2D) = "" {}
		_FalloffTex3 ("FallOff 3", 2D) = "" {}
		_FalloffTex4 ("FallOff 4", 2D) = "" {}
	}
	
	Subshader
	{
		Tags {"Queue"="Transparent"}
		Pass
		{
			BlendOp Add
			Blend Zero OneMinusSrcAlpha

			ZWrite Off
			ColorMask A
			Offset -1, -1
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uvShadow = mul (unity_Projector, vertex);
				o.uvFalloff = mul (unity_ProjectorClip, vertex);
				return o;
			}
			float _Alpha;
	        float _Softness;
	        sampler2D _Blend1;
	        sampler2D _Blend2;
	        sampler2D _Blend3;
	        sampler2D _Blend4;
	        sampler2D _Blend5;
	        sampler2D _Blend6;
	        float4 _Blend1_ST;
	        float4 _Blend2_ST;
	        float4 _Blend3_ST;
	        float4 _Blend4_ST;
	        float4 _Blend5_ST;
	        float4 _Blend6_ST;
			sampler2D _FalloffTex1;
			sampler2D _FalloffTex2;
			sampler2D _FalloffTex3;
			sampler2D _FalloffTex4;
			
			fixed4 frag (v2f i) : SV_Target
			{
				if (i.uvFalloff.x < 0.0 || i.uvFalloff.x > 1.0)
				{
					discard;
				}

				fixed4 c1 = tex2Dproj(_Blend1, UNITY_PROJ_COORD(i.uvShadow));
	            fixed4 c2 = tex2Dproj(_Blend2, UNITY_PROJ_COORD(i.uvShadow));
	            fixed4 c3 = tex2Dproj(_Blend3, UNITY_PROJ_COORD(i.uvShadow));
	            fixed4 c4 = tex2Dproj(_Blend4, UNITY_PROJ_COORD(i.uvShadow));
	            fixed4 c5 = tex2Dproj(_Blend5, UNITY_PROJ_COORD(i.uvShadow));
	            fixed4 c6 = tex2Dproj(_Blend6, UNITY_PROJ_COORD(i.uvShadow));
                c1 *= 1 - saturate(abs(0 - _Softness));
                c2 *= 1 - saturate(abs(1 - _Softness));
                c3 *= 1 - saturate(abs(2 - _Softness));
                c4 *= 1 - saturate(abs(3 - _Softness));
                c5 *= 1 - saturate(abs(4 - _Softness));
                c6 *= 1 - saturate(abs(5 - _Softness));
                fixed4 texS = c1+c2+c3+c4+c5+c6;
	
				fixed4 f1 = tex2Dproj(_FalloffTex1, UNITY_PROJ_COORD(i.uvFalloff));
	            fixed4 f2 = tex2Dproj(_FalloffTex2, UNITY_PROJ_COORD(i.uvFalloff));
	            fixed4 f3 = tex2Dproj(_FalloffTex3, UNITY_PROJ_COORD(i.uvFalloff));
	            fixed4 f4 = tex2Dproj(_FalloffTex4, UNITY_PROJ_COORD(i.uvFalloff));
				float fSoftness = _Softness * 0.6f;
                f1 *= 1 - saturate(abs(0 - fSoftness));
                f2 *= 1 - saturate(abs(1 - fSoftness));
                f3 *= 1 - saturate(abs(2 - fSoftness));
                f4 *= 1 - saturate(abs(3 - fSoftness));
                fixed4 texF = f1+f2+f3+f4;

				fixed4 res = texS * fixed4(1,1,1,texF.a);
				return fixed4(1,1,1, res.a * _Alpha);
			}
			ENDCG
		}
	}
}
