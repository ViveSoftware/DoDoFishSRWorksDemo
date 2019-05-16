Shader "Custom/WriteDepthScreenShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MainCamNear("_MainCamNear", Range(0, 10000)) = 1.0
        _MainCamFar("_MainCamFar", Range(0, 10000)) = 1.0
        _TestFactor("TestFactor", Range(0, 1)) = 1.0
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


    sampler2D _MainTex;
    float4 _MainTex_ST;
    float _TestFactor, _MainCamNear, _MainCamFar, _stereoRender;

    //http://blog.csdn.net/zhao_92221/article/details/46844267
    inline float UnLinear01Depth(float z01, float near, float far)
    {
        float y = far / near;
#if defined(UNITY_REVERSED_Z)
        return ((1 / z01) - 1) / (y - 1);
#else
        return ((1 / z01) - y) / (1 - y);
#endif
    }

    float frag(v2f_img i) : SV_Depth
    {
        // sample the texture
        float2 uv = float2(i.uv.x, i.uv.y);
        float col = tex2D(_MainTex, uv).r;
        col = UnLinear01Depth(col, _MainCamNear, _MainCamFar);
        //col = _TestFactor;
        return col;
    }
        ENDCG
    }
    }
}
