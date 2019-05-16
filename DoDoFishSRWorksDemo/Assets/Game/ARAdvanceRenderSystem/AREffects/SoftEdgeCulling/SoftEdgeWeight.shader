Shader "Custom/SoftEdgeWeight"
{
	Properties
	{
		_DepthTex ("DepthTex", 2D) = "white" {}
        _DepthBlurTex ("DepthBlurTex", 2D) = "white" {}
        
        factor("factor", Range(0,10)) = 1
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

			sampler2D _DepthTex, _DepthBlurTex;
            float factor;
			
			float4 frag (v2f_img i) : SV_Target
			{
                float depth = tex2D(_DepthTex, i.uv).r;
                float depthBlur = tex2D(_DepthBlurTex, i.uv).r;
                float depthOffset = depthBlur - depth;
                depthOffset *= factor;
              //  depth = pow(depth, factor);
                depthOffset = 1 - depthOffset;
               // if (depth > 0.99)
               //     return 0;
                return float4(depthOffset, depthOffset, depthOffset, 1);
			}
			ENDCG
		}
	}
}
