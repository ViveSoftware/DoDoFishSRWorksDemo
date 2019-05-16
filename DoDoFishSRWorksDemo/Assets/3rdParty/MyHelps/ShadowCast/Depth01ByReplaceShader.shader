// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Depth01ByReplaceShader"
{
    //http://disenone.github.io/2014/03/27/unity-depth-minimap
    SubShader
    {
        Tags{ "RenderType" = "Opaque" }
        Pass
        {
        Fog{ Mode Off }
        CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

        struct v2f 
        {
        float4 pos : SV_POSITION;
        float2 depth : TEXCOORD0;
    	};

	    v2f vert(appdata_base v) 
	    {
	        v2f o;
	        o.pos = UnityObjectToClipPos(v.vertex);
	        o.depth.x = o.pos.z;
	        o.depth.y = o.pos.w;
	        return o;
	    }

	    float frag(v2f i) : COLOR
	    {
	        float d = i.depth.x / i.depth.y;
	        d = Linear01Depth(d);
	    	//return float4(d,d,d,1);
            return d;
	    }
    	ENDCG
    	}
	}
}