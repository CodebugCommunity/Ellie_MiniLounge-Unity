
Shader "Custom/Ok/Brush"
{
	Properties
	{
		_Alpha ("Alpha", Range(0.0, 1.0)) = 0.5
	    _RT ("RT", 2D) = "white" {}
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
		_SampleCenter ("SampleCenter", Range(0.0, 1.0)) = 0
		_Pointy ("Pointy", Range(0.0, 1.0)) = 0
	}
	
	Subshader
	{
		Tags {"Queue"="Transparent"}
		Pass
		{
			Blend OneMinusDstColor One
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
				float4 uvMain : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uvMain = mul(unity_Projector, vertex);
				o.uvFalloff = mul(unity_ProjectorClip, vertex);
				return o;
			}
			
			float _Alpha;
	        sampler2D _RT;
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
			float _SampleCenter;
			float _Pointy;

			fixed4 frag (v2f i) : SV_Target
			{
				if (i.uvFalloff.x < 0.0 || i.uvFalloff.x > 1.0)
				{
					discard;
				}

				float2 p = i.uvMain.xy;
				float x = pow(i.uvFalloff.x, 2.5);
				float xx = pow(1 - i.uvFalloff.x, -1);
				p = lerp(p, float2(0.5,0.5), -_Pointy * lerp(x, xx, i.uvFalloff.x));
				i.uvMain.xy = p;

				fixed4 c1 = tex2Dproj(_Blend1, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c2 = tex2Dproj(_Blend2, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c3 = tex2Dproj(_Blend3, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c4 = tex2Dproj(_Blend4, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c5 = tex2Dproj(_Blend5, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c6 = tex2Dproj(_Blend6, UNITY_PROJ_COORD(i.uvMain));
                c1 *= 1 - saturate(abs(0 - _Softness));
                c2 *= 1 - saturate(abs(1 - _Softness));
                c3 *= 1 - saturate(abs(2 - _Softness));
                c4 *= 1 - saturate(abs(3 - _Softness));
                c5 *= 1 - saturate(abs(4 - _Softness));
                c6 *= 1 - saturate(abs(5 - _Softness));
                fixed4 texS = c1+c2+c3+c4+c5+c6;
				float4 uv = lerp(i.uvMain, float4(0.5,0.5,0,0), float4(_SampleCenter,_SampleCenter,_SampleCenter,_SampleCenter));
				texS.rgb *= tex2Dproj(_RT, UNITY_PROJ_COORD(uv)).rgb;
				texS.a *= _Alpha;
				texS.rgba *= texS.rgba;
	
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

				fixed4 res = texS * fixed4(1, 1, 1, texF.a);
				return saturate(res);
			}
			ENDCG
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ColorMask RGB
			Offset -1, -1
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 uvMain : TEXCOORD0;
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
				o.uvMain = mul(unity_Projector, vertex);
				o.uvFalloff = mul(unity_ProjectorClip, vertex);
				return o;
			}
			
			float _Alpha;
	        sampler2D _RT;
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
			float _SampleCenter;
			float _Pointy;
			
			fixed4 frag (v2f i) : SV_Target
			{
				if (i.uvFalloff.x < 0.0 || i.uvFalloff.x > 1.0)
				{
					discard;
				}

				float2 p = i.uvMain.xy;
				float x = pow(i.uvFalloff.x, 2.5);
				float xx = pow(1 - i.uvFalloff.x, -1);
				p = lerp(p, float2(0.5,0.5), -_Pointy * lerp(x, xx, i.uvFalloff.x));
				i.uvMain.xy = p;

				float softness = _Softness + i.uvFalloff.x * 1.5;
				softness = clamp(softness, 0.0, 5.0);

				fixed4 c1 = tex2Dproj(_Blend1, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c2 = tex2Dproj(_Blend2, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c3 = tex2Dproj(_Blend3, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c4 = tex2Dproj(_Blend4, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c5 = tex2Dproj(_Blend5, UNITY_PROJ_COORD(i.uvMain));
	            fixed4 c6 = tex2Dproj(_Blend6, UNITY_PROJ_COORD(i.uvMain));
                c1 *= 1 - saturate(abs(0 - softness));
                c2 *= 1 - saturate(abs(1 - softness));
                c3 *= 1 - saturate(abs(2 - softness));
                c4 *= 1 - saturate(abs(3 - softness));
                c5 *= 1 - saturate(abs(4 - softness));
                c6 *= 1 - saturate(abs(5 - softness));
                fixed4 texS = c1+c2+c3+c4+c5+c6;
				float4 uv = lerp(i.uvMain, float4(0.5,0.5,0,0), float4(_SampleCenter,_SampleCenter,_SampleCenter,_SampleCenter));
				texS.rgb *= tex2Dproj(_RT, UNITY_PROJ_COORD(uv)).rgb;
				texS.a *= _Alpha;
				texS.a = saturate(texS.a * 1.3);
	
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

				fixed4 res = texS * fixed4(1, 1, 1, texF.a);
				res.r = clamp(res.r, 0.0001, 0.9999);
				res.g = clamp(res.g, 0.0001, 0.9999);
				res.b = clamp(res.b, 0.0001, 0.9999);
				return saturate(res);
			}
			ENDCG
		}
	}
}
