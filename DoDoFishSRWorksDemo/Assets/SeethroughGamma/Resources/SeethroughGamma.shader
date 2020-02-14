Shader "MyHelp/SeethroughGamma"
{
    Properties
    {
        _TexSrc ("Texture", 2D) = "white" {}
        _Gamma("Gamma", Float) = 1.8
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

            sampler2D _TexSrc;
            float _Gamma;

            fixed4 frag (v2f_img i) : SV_Target
            {
                float4 color = tex2D(_TexSrc, i.uv);
                color.rgb = pow(color.rgb, _Gamma);
                return color;
            }
            ENDCG
        }
    }
}
