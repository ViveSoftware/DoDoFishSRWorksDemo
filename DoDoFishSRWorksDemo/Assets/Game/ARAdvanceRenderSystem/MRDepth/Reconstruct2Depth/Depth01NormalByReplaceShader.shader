Shader "Custom/Depth01NormalByReplaceShader"
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
        float3 worldNormal : TEXCOORD1;
    	};

	    v2f vert(appdata_base v) 
	    {
	        v2f o;
	        o.pos = UnityObjectToClipPos(v.vertex);
	        o.depth.x = o.pos.z;
	        o.depth.y = o.pos.w;

            o.worldNormal = UnityObjectToWorldNormal(v.normal);            
	        return o;
	    }

	    float4 frag(v2f i) : COLOR
	    {
            float4 output;
	        float d = i.depth.x / i.depth.y;
	        d = Linear01Depth(d);

            output.r = d;
            output.gba = normalize(i.worldNormal);
            return output;
	    }
    	ENDCG
    	}
	}
}