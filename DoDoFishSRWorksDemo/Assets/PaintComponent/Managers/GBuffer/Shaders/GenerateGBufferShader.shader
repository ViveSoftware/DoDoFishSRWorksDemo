// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PaintVR/GenerateGBufferShader"
{
	Properties
	{
	}
	SubShader
	{
		Cull off
        ZWrite Off
        ZTest Always

        Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct VertexInput
			{
				float4 pos : POSITION;
				fixed4 color : COLOR;
			};

			struct Interpolator
			{
				float4 color : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			Interpolator vert(VertexInput v)
			{
				Interpolator o;
				o.vertex = UnityObjectToClipPos(v.pos);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag(Interpolator i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
