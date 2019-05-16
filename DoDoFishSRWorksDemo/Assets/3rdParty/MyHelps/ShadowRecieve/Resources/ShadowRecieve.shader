Shader "Custom/ShadowRecieve"
{
    Properties
    {
        //_DepthMap ("_DepthMap", 2D) = "white" {}
        _ShadowFactor ("_MyShadowFactor", Range(0,100)) = 0.01
    }
    SubShader
    {
        Tags{ "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../../MyShadow.cginc"


            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD0;
                float2 depth : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.depth.x = o.pos.z;
                o.depth.y = o.pos.w;

                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float2 frag(v2f i) : SV_Target
            {
                float cameraDepth = Linear01Depth(i.depth.x / i.depth.y);
            float shadow = shadowAtten(
                i.worldPos.xyz,
                i.worldNormal);
        
                return float2(shadow, cameraDepth);
            }
                    ENDCG
        }
    }
}
