
Shader "Custom/CamFilterDrawBlocks"
{
    Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _ScreenResolution ("_ScreenResolution", Vector) = (0.,0.,0.,0.)
    }

    SubShader 
    {
        Pass
        {
            ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 3.0
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;   
            uniform float Fade;
            uniform float4 _ScreenResolution;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord  : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
            };   

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            float isBlock(float2 p) 
            {
                p = floor(p*float2(4.0, -4.0) + 2.5);
                if (clamp(p.x, 0.0, 4.0) == p.x && clamp(p.y, 0.0, 4.0) == p.y)
                    return 1.0;
                return 0.0;
            }

            float4 frag (v2f i) : COLOR
            {
                float2 uv  = i.texcoord.xy * _ScreenResolution.xy;
                float3 col = tex2D(_MainTex, i.texcoord.xy).rgb;	

                float2 p = fmod(uv/4.0, 2.0) - 1.0;
                col = col + isBlock(p)*Fade;
                return float4(col, 1.0);	
            }

        ENDCG
        }

    }
}