// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PaintVR/RefineGBufferShader"
{
	Properties
	{
		_MainTex ("Original Texture", 2D) = "white" {}
		_OrigMask ("Original Mask", 2D) = "black" {}
		_DilatedMask ("Dilated Mask", 2D) = "black" {}
        _ScaleW("_ScaleW", Float) = 0.0009765625
        _ScaleH("_ScaleH", Float) = 0.0009765625
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
			sampler2D _OrigMask;
			sampler2D _DilatedMask;
            float _ScaleW, _ScaleH;

			fixed4 frag(Interpolator i) : SV_Target
			{
				float diff = tex2D(_DilatedMask, i.uv).r - tex2D(_OrigMask, i.uv).r;
				if (diff == 0.0)
				{
					return tex2D(_MainTex, i.uv);
				}
				else
				{
					#define SAMPLE_COUNT 8
					float2 uvOffset[SAMPLE_COUNT] = 
					{
						float2(-_ScaleW, -_ScaleH),
						float2(0.0, -_ScaleH),
						float2(_ScaleW, -_ScaleH),
						float2(-_ScaleW, 0.0),
						float2(_ScaleW, 0.0),
						float2(-_ScaleW, _ScaleH),
						float2(0.0, _ScaleH),
						float2(_ScaleW, _ScaleH),
					};

					float3 refineColor = float3(0.0, 0.0, 0.0);
					float count = 0.0;
					[unroll]
					for (int s = 0 ; s < SAMPLE_COUNT ; ++s)
					{
						float2 uv = i.uv + uvOffset[s];
						float maskValue = tex2D(_OrigMask, uv).r;
						if (maskValue == 1.0)
						{
							refineColor += tex2D(_MainTex, uv).rgb;
							count++;
						}
					}
					if (count != 0.0)
						return fixed4(refineColor/count, 1.0);
					else
						return tex2D(_MainTex, i.uv);
				}
			}
			ENDCG
		}
	}
}
