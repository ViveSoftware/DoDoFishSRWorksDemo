// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BlitLightMap"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ScreenLightMap("Texture", 2D) = "white" {}
        _Factor("Factor", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags{ "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex, _ScreenLightMap;
            float _Factor;

            float4 frag(v2f_img i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                float4 light = tex2D(_ScreenLightMap, float2(1-i.uv.x, 1-i.uv.y));
                return float4(col.rgb + _Factor*light.rgb , 1);
            }
            ENDCG
        }
    }
}
