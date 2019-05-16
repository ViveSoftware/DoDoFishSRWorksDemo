// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/RenderWithARDepth"
{
    Properties
    {
        _SoftCullingMap("_SoftCullingMap", 2D) = "white" {}
        _VRDepthColor("_VRDepthColor", 2D) = "white" {}
        _MRNormalDepth("_MRDepthNormal", 2D) = "white" {}
        _SeeThroughColor("_SeeThroughColor", 2D) = "white" {}
        _CullingBaise("_CullingBaise", Range(-0.01,0.01)) = 0.001
        _GlowAmount("Glow Amount", Range(0,0.2)) = 0.039
        _CoefAmount("Coef", Range(0,1)) = 0.04
        _softCullLength("Soft Cull Length", Range(0.001,0.00001)) = 0.001
            //_softCullFactor("Soft Cull Factor", Range(0.01,0.0001)) = 0.001
    }

        SubShader
        {
            Cull Off
            ZWrite Off
            ZTest Always

            Pass
            {
            Tags{ "LightMode" = "ForwardBase" }

                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "AutoLight.cginc"

            sampler2D _MRDepthNormal, _SeeThroughColor, _VRDepthColor, _SoftCullingMap;
            float _GlowAmount, _CoefAmount, _CullingBaise;
            float _softCullLength;//, _softCullFactor;

float4 frag(v2f_img i) : COLOR
{
       float4 VRDepthColor = tex2D(_VRDepthColor, i.uv);
       float VRdepth = VRDepthColor.r;
       float3 VRColor = VRDepthColor.gba;

       float3 SeeThroughColor = tex2D(_SeeThroughColor, i.uv).rgb;

        float ARdepth = tex2D(_MRDepthNormal, i.uv).r;

       float4 finalColor = 0;
       finalColor.a = 1;
       float ratio = VRdepth - ARdepth;// MRdepth;
       // ratio = ratio / _softCullLength;

       if (VRdepth > 0.99)//doesn't has VR color
       {
           finalColor.rgb = SeeThroughColor;
       }
       else
       {
           if (ratio < _CullingBaise)
           {
               finalColor.rgb = VRColor;
           }
           else
           {
               //finalColor.rgb = SeeThroughColor;
               float cullingWeight = tex2D(_SoftCullingMap, i.uv).r;
               finalColor.rgb = lerp(VRColor, SeeThroughColor, cullingWeight);
           }
       }

        return finalColor;
}
                ENDCG
            }
        }
}
