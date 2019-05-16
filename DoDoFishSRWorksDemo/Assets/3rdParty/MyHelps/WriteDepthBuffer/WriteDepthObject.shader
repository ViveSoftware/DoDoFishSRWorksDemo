Shader "Custom/WriteDepthObject"
{
    SubShader{
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 200

        // extra pass that renders to depth buffer only
        Pass{
        ZWrite On
        ColorMask 0
    }

        // paste in forward rendering passes from Transparent/Diffuse
        UsePass "Transparent/Diffuse/FORWARD"
    }
        /*
	SubShader
	{
        ZTest Always
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
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
                float2 depth : TEXCOORD0;
			};

            struct fout
            {
                float4 color : SV_Target;
                float depth : SV_Depth;
            };
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.depth.x = o.vertex.z;
                o.depth.y = o.vertex.w;
				return o;
			}
			
            fout frag (v2f i)
			{
                fout o;
                o.color = float4(1, 0, 0, 1);
                o.depth = i.depth.x / i.depth.y;
                return  o;
			}
			ENDCG
		}
	}*/
}
