// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ScreenShadowBlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ScreenShadowMap ("Texture", 2D) = "white" {}
        _VRDepthColor("Texture", 2D) = "white" {}
        _shadowBlitBais("shadowBlitBais", Range(0,1)) = 0.01
		//_ShadowColor ("shadow color", Color) = (0.3,0.3, 0.4)
	}
	SubShader
	{
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
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
                float2 shadowMapUV : TEXCOORD1;
			};

			sampler2D _MainTex, _ScreenShadowMap, _VRDepthColor;
			float4 _MainTex_ST;
            float _shadowBlitBais;
            int needFlip;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);                
                o.shadowMapUV = (needFlip==1) ? (1-v.uv) : v.uv;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
                float4 screenVRDepthColor = tex2D(_VRDepthColor, float2(i.uv.x, i.uv.y));
                float screenVRDepth = screenVRDepthColor.r;
				float2 shadow = tex2D(_ScreenShadowMap, i.shadowMapUV);
                float screenShadowDepth = shadow.g;
                float3 shadowColor = float3(shadow.r, shadow.r, shadow.r*1.0f);//shadow blue need modify
            
                if(screenShadowDepth < 0.99 && screenVRDepth > screenShadowDepth - _shadowBlitBais)
				    col.xyz = col.xyz * shadowColor.xyz;
				return col;
			}
			ENDCG
		}
	}
}
