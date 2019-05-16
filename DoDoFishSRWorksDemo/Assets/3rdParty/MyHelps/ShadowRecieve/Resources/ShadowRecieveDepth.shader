// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ShadowRecieveDepth"
{
	Properties
	{
		//_DepthMap ("_DepthMap", 2D) = "white" {}
		//_Bais ("Bais", Range(0,1)) = 0.01
        _ShadowFactor("_MyShadowFactor", Range(0,100)) = 0.01
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"
            #include "../../MyDeferred.cginc"
            #include "../../MyShadow.cginc"

            sampler2D _CameraDepthNormal;

			float2 frag (v2f_img i) : SV_Target
			{
                float4 depthNormal = tex2D(_CameraDepthNormal, float2(1 - i.uv.x, 1 - i.uv.y));
                float cameraDepth = depthNormal.r;
                float3 worldSpaceNormal = depthNormal.gba;
                float2 TexCoord = i.uv;
                float3 positionVS = CalcVS_PERSPECTIVE(TexCoord, cameraDepth);

                // Convert to worldSpace
                float4 position = mul(_ViewToWorld, float4(positionVS, 1));

                float shadow = shadowAtten(
                    position,
                    depthNormal.gba);

                return float2(shadow,cameraDepth);
			}
			ENDCG
		}
	}
}
