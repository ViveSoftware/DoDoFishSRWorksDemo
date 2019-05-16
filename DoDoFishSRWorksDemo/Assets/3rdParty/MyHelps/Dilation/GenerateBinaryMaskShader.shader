// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PaintVR/GenerateBinaryMaskShader"
{
	Properties
	{
		_MainTex ("Original Texture", 2D) = "white" {}
	}
	SubShader
	{
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
				float2 uv : TEXCOORD0;
			};

			struct Interpolator
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			Interpolator vert(VertexInput v)
			{
				Interpolator o;
				o.vertex = UnityObjectToClipPos(v.pos);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;

			fixed4 frag(Interpolator i) : SV_Target
			{
				// If the alpha channel has been written, output white color.
				float4 col = tex2D(_MainTex, i.uv);
				return fixed4(col.a, col.a, col.a, 1.0);
			}
			ENDCG
		}
	}
}
