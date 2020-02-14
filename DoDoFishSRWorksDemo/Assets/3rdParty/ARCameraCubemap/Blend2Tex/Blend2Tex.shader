Shader "MyHelp/Blend2Tex"
{
    Properties
    {
        _TexSrc ("Texture", 2D) = "white" {}
        _TexSrc2("Texture", 2D) = "white" {}
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

            sampler2D _TexSrc, _TexSrc2;
            float ratio;

            fixed4 frag (v2f_img i) : SV_Target
            {
                // sample the texture
                float4 src = tex2D(_TexSrc, i.uv);
                float4 src2 = tex2D(_TexSrc2, i.uv);
                float3 final = lerp(src.rgb, src2.rgb, ratio);
                return float4(final, 1);
            }
            ENDCG
        }
    }
}
