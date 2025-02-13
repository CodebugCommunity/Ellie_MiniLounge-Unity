
Shader "Custom/Ok/ProjectorTarget"
{
	Properties { }
    
	SubShader
	{
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="False" "RenderType"="TransparentCutout" }
		LOD 100
 
		Lighting Off
 
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
		        UNITY_VERTEX_INPUT_INSTANCE_ID
		    };
 
		    struct v2f
			{
		        float4 vertex : SV_POSITION;
		        UNITY_VERTEX_OUTPUT_STEREO
		    };
 
		    v2f vert (appdata_t v)
		    {
		        v2f o;
		        UNITY_SETUP_INSTANCE_ID(v);
		        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		        o.vertex = UnityObjectToClipPos(v.vertex);
		        return o;
		    }
 
		    fixed4 frag (v2f i) : SV_Target
		    {
				clip(-1);
		        return fixed4(0,0,0,0);
		    }
		    ENDCG
		}
	}
}
