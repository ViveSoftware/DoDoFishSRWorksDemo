Shader "Unlit/LightBeam2D"
{
	Properties
	{
        _Color("Main Color (A=Opacity)", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
    _MyDepth01Texture("Texture", 2D) = "white" {}
        _SoftEdge("Soft Edge", Range(0.0001, 0.01)) = 0.004
	}
	SubShader
	{
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
                float2 depth : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
			};

            float _SoftEdge;
			sampler2D _MainTex;
            sampler2D _MyDepth01Texture;
			float4 _MainTex_ST;
            uniform float4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.depth = float2(o.vertex.z, o.vertex.w);
                o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float2 screenPosition = (i.screenPos.xy / i.screenPos.w);
                //screenPosition.y = 1-screenPosition.y;
                //screenPosition.x = 1 - screenPosition.x;
                float ARdepth = tex2D(_MyDepth01Texture, screenPosition).r;
                float lightBeamDepth = i.depth.x / i.depth.y;
                lightBeamDepth = Linear01Depth(lightBeamDepth);

                // Soft Edges              
                //float diff = saturate((ARdepth - lightBeamDepth) * _SoftEdge);
                float diff = saturate((ARdepth - lightBeamDepth) / _SoftEdge);
                col.a *= diff;
               // col= diff;
               // col = float4(lightBeamDepth *10, 0, 0, 1);
                //col = float4(ARdepth * 10, 0, 0, 1);
				return col;
			}
			ENDCG
		}
	}
}
