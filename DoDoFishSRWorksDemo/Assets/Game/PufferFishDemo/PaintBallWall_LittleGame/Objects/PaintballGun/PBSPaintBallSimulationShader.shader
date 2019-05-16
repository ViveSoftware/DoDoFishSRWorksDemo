// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PaintVR/PBSPaintBallSimulationShader"
{
	Properties
	{
		_MainTex ("Original Texture", 2D) = "white" {}
		_PosMap ("Position Map", 2D) = "black" {}
		_NormalMap ("Normal Map", 2D) = "black" {}
		_DepthMap ("Depth Map", 2D) = "black" {}

		_CookieMap ("Cookie Map", 2D) = "white" {}

		_SprayColor ("Spray Color", Color) = (1, 1, 1, 1)
		_SpraySource ("Spray Source", Vector) = (0, 0, 0, 1)
		_SprayCenter ("Spray Center", Vector) = (0, 0, 0, 1)
		_SprayInnerAngle ("Spray Inner Angle", Float) = 0.5
		_SprayOuterAngle ("Spray Outer Angle", Float) = 0.5

		_SprayAlpha ("Spray Alpha", Range(0.0, 1.0)) = 1.0
	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Intepolator
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			Intepolator vert(appdata v)
			{
				Intepolator o;
				o.vertex = UnityObjectToClipPos(v.pos);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _PosMap;
			sampler2D _NormalMap;
			sampler2D _DepthMap;

			sampler2D _CookieMap;

			fixed4 _SprayColor;
			float4 _SpraySource;
			float4 _SprayCenter;
			float _SprayInnerAngle;
			float _SprayOuterAngle;
			float _SprayAlpha;

			uniform float4x4 _WorldMatrix;
			uniform float4x4 _NormalMatrix;
			uniform float4x4 _LightViewProjMatrix;
			uniform float4x4 _LightViewMatrix;

			float SquareDist(float3 a, float3 b)
			{
				float3 diff = a - b;
				return diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;
			}

			float SampleShadowMap(float2 lightUV, float depthLightViewSpace)
			{
				// _DepthMap store the depth in light view.
				float depthInDepthMap = tex2D(_DepthMap, lightUV).r;
				// Use depth bias 0.0001
				if (depthLightViewSpace > depthInDepthMap + 0.01)
        			return 0.0;
    			else
        			return 1.0;
			}

			float ShadowFactor(float3 posLightClipSpace, float3 worldPos)
			{
				float2 lightUV = 0.5 * posLightClipSpace.xy + 0.5;
				if (lightUV.x < 0.0 || lightUV.x > 1.0 || lightUV.y < 0.0 || lightUV.y > 1.0)
					return 0.0;

				float4 posLightViewSpace = mul(_LightViewMatrix, float4(worldPos, 1));
				float depthLightViewSpace = -posLightViewSpace.z / posLightViewSpace.w;

			#define PCF_SHADOW 1
			#if PCF_SHADOW
				float offset = 0.00097656;
				float shadowFactor = 0.0;
				[unroll]
				for (int h = -1 ; h <= 1 ; ++h)
				{
					for (int w = -1 ; w <= 1 ; ++w)
					{
						float2 offsetUV = offset * float2(w, h);
						shadowFactor += SampleShadowMap(lightUV + offsetUV, depthLightViewSpace);
					}
				}
				return shadowFactor / 9.0;
			#else
				return SampleShadowMap(lightUV, depthLightViewSpace);
			#endif
			}

			float CookieFactor(float3 posLightClipSpace)
			{
				float2 lightUV = 0.55 * posLightClipSpace.xy + 0.5;
				if (lightUV.x < 0.0 || lightUV.x > 1.0 || lightUV.y < 0.0 || lightUV.y > 1.0)
					return 0.0;

				return tex2D(_CookieMap, lightUV).a;
			}

			float3 ComputeLightClipSpacePosition(float3 worldPos)
			{
				float4 posLightClipSpace = mul(_LightViewProjMatrix, float4(worldPos, 1));
				return posLightClipSpace.xyz / posLightClipSpace.w;
			}

			fixed4 frag(Intepolator i) : SV_Target
			{
				// Retrieve position data.
				float3 posObject = tex2D(_PosMap, i.uv).rgb;
				float3 posWorld = mul(_WorldMatrix, float4(posObject, 1.0)).xyz;

				/*if (distance(posWorld, _SprayCenter.xyz) < 0.2)
				{
					return fixed4(1, 0, 0, 1);
				}
				else
				{
					return fixed4(0, 0, 0, 1);
				}*/

				// DEBUG ONLY.
				//float c = ShadowFactor(posWorld);
				//return fixed4(c, c, c, 1);

				float3 normalObject = tex2D(_NormalMap, i.uv).rgb;
				float3 normalWorld = mul(_NormalMatrix, float4(normalObject, 0.0)).xyz;
				normalWorld = normalize(normalWorld);

				// Spot-light diffuse term.
				float3 sprayDir = normalize(_SpraySource.xyz - _SprayCenter.xyz);
				float3 pointToSourceDir = normalize(_SpraySource.xyz - posWorld);

				float cosDir = dot(pointToSourceDir, sprayDir);
				float spotEffect = smoothstep(_SprayOuterAngle, _SprayInnerAngle, cosDir);
				float NdotL = max(dot(normalWorld, pointToSourceDir), 0.0);
				float atten = clamp(1.0 / SquareDist(_SpraySource.xyz, posWorld), 0.0, 1.0);
				spotEffect = spotEffect * NdotL * atten;

				// Integrate shadow and pattern mask.
				float3 posLightClipSpace = ComputeLightClipSpacePosition(posWorld);
				//spotEffect *= ShadowFactor(posLightClipSpace, posWorld);
				spotEffect *= CookieFactor(posLightClipSpace);
				spotEffect *= _SprayAlpha;

				fixed4 origColor = tex2D(_MainTex, i.uv);
				//return fixed4(spotEffect, spotEffect, spotEffect, 1.0);
				//return fixed4(_SprayColor.rgb * spotEffect, 1.0);
				return fixed4(lerp(origColor.rgb, _SprayColor.rgb, spotEffect), lerp(origColor.a, _SprayColor.a, spotEffect));
			}
			ENDCG
		}
	}
}
