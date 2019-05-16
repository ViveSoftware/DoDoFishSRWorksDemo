Shader "PaintBall/SmoothAlphaClear"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Decay ("Decay", float) = 0.001
	}
	SubShader
	{
		ZWrite Off
		ZTest Always

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
			};

			sampler2D _MainTex;
			float _Decay;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				half fadeout = clamp(0, 1, col.a - _Decay);
				return fixed4(col.rgb, fadeout);
			}
			ENDCG
		}
	}
}
