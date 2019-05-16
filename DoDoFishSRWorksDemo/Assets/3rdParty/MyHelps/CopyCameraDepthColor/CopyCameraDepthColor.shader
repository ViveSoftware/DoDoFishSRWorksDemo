//// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
//
//Shader "MyHelp/CopyCameraColorDepth"
//{
//    Properties
//    {
//        _MainTex("_MainTex", 2D) = "white" {}
//    }
//    SubShader{
//        Tags{ "RenderType" = "Opaque" }
//
//        Pass{
//        CGPROGRAM
//#pragma vertex vert
//#pragma fragment frag
//#include "UnityCG.cginc"
//
//        sampler2D _CameraDepthTexture;
//    sampler2D _MainTex;
//
//    struct v2f {
//        float4 pos : SV_POSITION;
//        float4 scrPos:TEXCOORD1;
//    };
//
//    //Vertex Shader
//    v2f vert(appdata_base v) {
//        v2f o;
//        o.pos = UnityObjectToClipPos(v.vertex);
//        o.scrPos = ComputeScreenPos(o.pos);
//        //for some reason, the y position of the depth texture comes out inverted
//        o.scrPos.y = 1 - o.scrPos.y;
//        return o;
//    }
//
//    //Fragment Shader
//    half4 frag(v2f i) : COLOR{
//        float4 uv = UNITY_PROJ_COORD(i.scrPos);
//        float depthValue = Linear01Depth(tex2Dproj(_CameraDepthTexture, uv).r);
//        half4 depth;
//        depth.rgb = tex2Dproj(_MainTex, uv).rgb;
//        depth.a = depthValue;
//        return depth;
//    }
//        ENDCG
//    }
//    }
//        FallBack "Diffuse"
//        
//
//    /*
//	Properties
//	{
//        _MainTex("_MainTex", 2D) = "white" {}
//	}
//
//    
//	SubShader
//	{
//		// No culling or depth
//		Cull Off ZWrite Off ZTest Always
//
//		Pass
//		{
//			CGPROGRAM
//			#pragma vertex vert_img
//			#pragma fragment frag
//			
//			#include "UnityCG.cginc"
//
//			//sampler2D _MainTex;
//        UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
//    sampler2D _MainTex;
//
//			float frag (v2f_img i) : SV_Target
//			{
//                float4 color;
//                color.rgb = tex2D(_MainTex, i.uv).rgb;
//                color.a = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.uv)).r);
//               // color.a = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv).r;
//               // color.a = Linear01Depth(color.a);
//                return color;
//			}
//			ENDCG
//		}
//	}*/
//}


Shader "MyHelp/CopyCameraDepthColor"
{
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" {}
    }

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
            sampler2D _MainTex;

            float4 frag(v2f_img i) : SV_Target
            {
                float4 color;
                color.r = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
                color.gba = tex2D(_MainTex, i.uv).rgb;
                
                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
