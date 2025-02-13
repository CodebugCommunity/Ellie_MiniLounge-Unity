
Shader "Custom/Ok/UnlitColor"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
    
        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag exclude_path:deferred exclude_path:prepass noshadow nolightmap nodynlightmap nodirlightmap nofog nometa noforwardadd nolppv noshadowmask
                #pragma target 2.0
    
                #include "UnityCG.cginc"
    
                struct appdata_t
                {
                    float4 vertex : POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
    
                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    UNITY_FOG_COORDS(1)
                    UNITY_VERTEX_OUTPUT_STEREO
                };
    
                float4 _Color;
    
                v2f vert (appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }
    
                fixed4 frag (v2f i) : SV_Target
                {
                    return _Color;
                }
            ENDCG
        }
    }

}
