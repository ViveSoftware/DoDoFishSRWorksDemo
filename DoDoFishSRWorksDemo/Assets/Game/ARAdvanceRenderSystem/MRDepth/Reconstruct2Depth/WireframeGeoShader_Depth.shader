Shader "Custom/Wireframe_Depth"
{
	Properties
    {
        _Color ("Wire Color", Color) = (0.0, 1.0, 0.0, 1.0)
    }

    SubShader
    {
        //Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		//Blend SrcAlpha OneMinusSrcAlpha
		//
		//Pass
        //{
		//	ZWrite On
		//	ColorMask 0
		//}

		Tags { "Queue" = "Geometry" "RenderType"="Opaque" }

        Pass
        {
			// http://developer.download.nvidia.com/SDK/10/direct3d/Source/SolidWireframe/Doc/SolidWireframe.pdf
            CGPROGRAM
			#pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform float4 _Color; 

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 cPos : SV_POSITION;
            };

            struct g2f
            {
                float4 cPos : SV_POSITION;
                float2 depth : TEXCOORD0;
            };
            
            v2g vert (appdata v)
            {
                v2g o;
                o.cPos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
            {
                g2f o;

                o.cPos = i[0].cPos;
                o.depth.x = o.cPos.z;
                o.depth.y = o.cPos.w;
                triangleStream.Append(o);

                o.cPos = i[1].cPos;
                o.depth.x = o.cPos.z;
                o.depth.y = o.cPos.w;
                triangleStream.Append(o);

                o.cPos = i[2].cPos;
                o.depth.x = o.cPos.z;
                o.depth.y = o.cPos.w;
                triangleStream.Append(o);
            }

            float frag (g2f i) : SV_Target
            {
                float finalColor = Linear01Depth( i.depth.x / i.depth.y);
                return finalColor;
            }
			ENDCG
        }
    }
}
