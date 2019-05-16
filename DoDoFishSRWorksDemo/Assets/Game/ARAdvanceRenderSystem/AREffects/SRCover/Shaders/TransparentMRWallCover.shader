// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TransparentMRWallCover"
{
	Properties
	{
		_DiffuseTint("Diffuse Tint", Color) = (1, 1, 1, 1)
		_LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
		_RightEyeTexture("Right Eye Texture", 2D) = "white" {}
		_BlendTexture("Blend Texture", 2D) = "white" {}

		_Transparent("Transparent Volume", float) = 1.0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma vertex vertShadow
			#pragma fragment fragShadow
			#pragma multi_compile_fwdbase

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			float4 _DiffuseTint;
			sampler2D _LeftEyeTexture;
			sampler2D _RightEyeTexture;
			sampler2D _BlendTexture;
			float4 _BlendTexture_ST;

			float _Transparent;

			uniform float4x4 _WorldMatrix;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 lightDir : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float4 uvgrab : TEXCOORD2;
				LIGHTING_COORDS(3, 4)

				float3 wpos : TEXCOORD5;
			};

			v2f vertShadow(appdata_base v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				float4 coverPos = mul(UNITY_MATRIX_VP, mul(_WorldMatrix, float4(v.vertex.xyz, 1.0)));
				o.lightDir = normalize(ObjSpaceLightDir(v.vertex));
				o.normal = normalize(v.normal).xyz;
				o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;

				TRANSFER_VERTEX_TO_FRAGMENT(o);

				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif	
				o.uvgrab.xy = (float2(coverPos.x, coverPos.y * scale) + coverPos.w) * 0.5;
				o.uvgrab.zw = coverPos.zw;

				return o;
			}

			float4 fragShadow(v2f i) : COLOR
			{
				float3 L = normalize(i.lightDir);
				float3 N = normalize(i.normal);

				float attenuation = LIGHT_ATTENUATION(i);
				//float4 ambient = UNITY_LIGHTMODEL_AMBIENT;

				float NdotL = saturate(dot(N, L));
				float4 diffuseTerm = NdotL * _DiffuseTint * attenuation;

				half4 col = tex2Dproj(_LeftEyeTexture, UNITY_PROJ_COORD(i.uvgrab)) * (unity_StereoEyeIndex == 0) +
					tex2Dproj(_RightEyeTexture, UNITY_PROJ_COORD(i.uvgrab)) * (unity_StereoEyeIndex != 0);


				half2 newBlendUV = half2(0, clamp(i.wpos.y - _Transparent, 0, 1));
				half2 blendUV = TRANSFORM_TEX(newBlendUV, _BlendTexture);
				half blend = tex2D(_BlendTexture, blendUV).r;

                float4 finalColor = (attenuation)* col +(1 - attenuation) * (/*ambient * 0.5 + */col *0.5);
				finalColor.a = blend;

				return finalColor;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}