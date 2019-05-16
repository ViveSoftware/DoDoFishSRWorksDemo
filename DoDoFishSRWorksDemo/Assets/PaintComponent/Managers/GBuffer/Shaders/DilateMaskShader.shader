// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PaintVR/DilateMaskShader"
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
				float2 offset = float2(1.0 / 1024.0, 1.0 / 1024.0);
				float U = tex2D(_MainTex, i.uv + float2(0.0, -offset.y)).r;
				float B = tex2D(_MainTex, i.uv + float2(0.0, offset.y)).r;
				float L = tex2D(_MainTex, i.uv + float2(-offset.x, 0.0)).r;
				float R = tex2D(_MainTex, i.uv + float2(offset.x, 0.0)).r;
				float C = tex2D(_MainTex, i.uv).r;

				float F = max(C, max(max(U, B), max(L, R)));
				return fixed4(F, F, F, 1);
			}
			ENDCG
		}
	}
}
