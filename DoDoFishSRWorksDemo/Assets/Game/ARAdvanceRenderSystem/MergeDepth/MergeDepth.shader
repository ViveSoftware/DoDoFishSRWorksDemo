Shader "Custom/MergeDepth"
{
	Properties
	{
        _CameraRenderDepth("Texture", 2D) = "white" {}
        _VRDepthColor("Texture", 2D) = "white" {}
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

            sampler2D _CameraRenderDepth, _VRDepthColor;
			float frag (v2f_img i) : SV_Target
			{
                float4 VRDepthColor = tex2D(_VRDepthColor, i.uv);
                float VRdepth = VRDepthColor.r;
                float MRdepth = tex2D(_CameraRenderDepth, i.uv).r;
                return min(VRdepth, MRdepth);
                //return MRdepth;
			}
			ENDCG
		}
	}
}
