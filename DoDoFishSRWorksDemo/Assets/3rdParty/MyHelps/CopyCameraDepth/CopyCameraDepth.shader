// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyHelp/CopyCameraDepth"
{
        SubShader
        {
            Tags{ "RenderType" = "Opaque" }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _CameraDepthTexture;
                float frag(v2f_img i) : SV_Target
                {
                    float depthValue = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
                    return depthValue;
                }
                ENDCG
            }
        }
        FallBack "Diffuse"
}
