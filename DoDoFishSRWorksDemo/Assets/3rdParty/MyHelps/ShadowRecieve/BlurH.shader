
//http://www.shaderslab.com/demo-39---blur-effect-with-grab-pass.html
Shader "Custom/BlurH"
{
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" {}
        _Factor ("Factor", Range(0, 5)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
       
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
 
            #include "UnityCG.cginc"
           
            sampler2D _MainTex;
			float4 _MainTex_TexelSize;
            float _Factor;
 
            float4 frag (v2f_img i) : SV_Target
            {
 
                half pixelCol = 0;
 
                #define ADDPIXEL(weight,kernelX) tex2D(_MainTex, float2(i.uv.x + _MainTex_TexelSize.x * kernelX * _Factor, i.uv.y)).r * weight
               
                pixelCol += ADDPIXEL(0.05, 4.0);
                pixelCol += ADDPIXEL(0.09, 3.0);
                pixelCol += ADDPIXEL(0.12, 2.0);
                pixelCol += ADDPIXEL(0.15, 1.0);
                pixelCol += ADDPIXEL(0.18, 0.0);
                pixelCol += ADDPIXEL(0.15, -1.0);
                pixelCol += ADDPIXEL(0.12, -2.0);
                pixelCol += ADDPIXEL(0.09, -3.0);
                pixelCol += ADDPIXEL(0.05, -4.0);
                return float4(pixelCol, tex2D(_MainTex, i.uv).gba); //preserve GBA
            }
            ENDCG
        }
    }
}