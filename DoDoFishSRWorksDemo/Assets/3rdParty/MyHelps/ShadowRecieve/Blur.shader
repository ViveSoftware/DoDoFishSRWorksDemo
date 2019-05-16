
//http://www.shaderslab.com/demo-39---blur-effect-with-grab-pass.html
Shader "Custom/Blur2"
{
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" {}
        _Factor ("Factor", Range(0, 5)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
       
        GrabPass { }
       
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float4 pos : SV_POSITION;
               // float4 uv : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };
 
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
               // o.uv = ComputeGrabScreenPos(o.pos);
                o.uv = v.uv;
                return o;
            }
           
            sampler2D _GrabTexture, _MainTex;
            float4 _GrabTexture_TexelSize;
            float _Factor;
 
            half2 frag (v2f i) : SV_Target
            {
 
                half pixelCol = 0;
 
                #define ADDPIXEL(weight,kernelX) tex2D(_GrabTexture, float2(i.uv.x + _GrabTexture_TexelSize.x * kernelX * _Factor, i.uv.y)).r * weight
               
                pixelCol += ADDPIXEL(0.05, 4.0);
                pixelCol += ADDPIXEL(0.09, 3.0);
                pixelCol += ADDPIXEL(0.12, 2.0);
                pixelCol += ADDPIXEL(0.15, 1.0);
                pixelCol += ADDPIXEL(0.18, 0.0);
                pixelCol += ADDPIXEL(0.15, -1.0);
                pixelCol += ADDPIXEL(0.12, -2.0);
                pixelCol += ADDPIXEL(0.09, -3.0);
                pixelCol += ADDPIXEL(0.05, -4.0);
                return float2(pixelCol, tex2D(_MainTex, i.uv).g);
            }
            ENDCG
        }
 
        GrabPass { }
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
 
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
           
            sampler2D _GrabTexture, _MainTex;
            float4 _GrabTexture_TexelSize;
            float _Factor;
 
            fixed2 frag (v2f i) : SV_Target
            {
 
                fixed pixelCol = 0;
 
                #define ADDPIXEL(weight,kernelY) tex2D(_GrabTexture, float2(i.uv.x, i.uv.y + _GrabTexture_TexelSize.y * kernelY * _Factor)).r * weight
               
                pixelCol += ADDPIXEL(0.05, 4.0);
                pixelCol += ADDPIXEL(0.09, 3.0);
                pixelCol += ADDPIXEL(0.12, 2.0);
                pixelCol += ADDPIXEL(0.15, 1.0);
                pixelCol += ADDPIXEL(0.18, 0.0);
                pixelCol += ADDPIXEL(0.15, -1.0);
                pixelCol += ADDPIXEL(0.12, -2.0);
                pixelCol += ADDPIXEL(0.09, -3.0);
                pixelCol += ADDPIXEL(0.05, -4.0);
                return float2(pixelCol, tex2D(_MainTex, i.uv).g);
            }
            ENDCG
        }
    }
}