
Shader "Custom/Ok/FullErase"
{
	Properties { }
	
	Subshader
	{
		Tags {"Queue"="Transparent"}
		Pass
		{
			BlendOp Add
			Blend Zero OneMinusSrcAlpha

			ZWrite Off
			ColorMask RGBA
			Offset -1, -1
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 pos : SV_POSITION;
			};
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(1,1,1,1);
			}
			ENDCG
		}
	}
}
