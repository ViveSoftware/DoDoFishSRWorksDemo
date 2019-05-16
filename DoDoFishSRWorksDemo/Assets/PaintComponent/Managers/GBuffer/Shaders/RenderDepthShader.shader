Shader "PaintVR/RenderDepthShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		ZWrite On
		ZTest LEqual

        Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct VertexInput
			{
				float4 pos : POSITION;
			};

			struct Interpolator
			{
				float depth : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			Interpolator vert(VertexInput v)
			{
				Interpolator o;
				o.vertex = UnityObjectToClipPos(v.pos);
				o.depth = -UnityObjectToViewPos(v.pos).z;
				return o;
			}

			float4 frag(Interpolator i) : SV_Target
			{
				float c = i.depth;
				return float4(c, c, c, 1.0);
			}
			ENDCG
		}
	}
}
