Shader "Custom/DeferredPointLightMap"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma multi_compile PERSPECTIVE

            #include "UnityCG.cginc"
            #include "..\MyDeferred.cginc"

            //float4 halfPixel;
            //float4x4 invVP;
            static const int PLightPerPass = 64;
            int PointLightAmount;
            float4 PointLightColor[PLightPerPass];
            float4 PointLightPosition[PLightPerPass];
            float4 PointLightDecayRangeStrength[PLightPerPass];

			sampler2D _MainLightMap, _CameraDepthNormal;

            float4 frag(v2f_img i) : SV_Target
            {
                float4 depthNormal = tex2D(_CameraDepthNormal, float2(1 - i.uv.x, 1 - i.uv.y));
                float cameraDepth = depthNormal.r;
                float3 worldSpaceNormal = depthNormal.gba;
                float2 TexCoord = i.uv;
                float3 positionVS = CalcVS_PERSPECTIVE(TexCoord, cameraDepth);

                // Convert to worldSpace
                float4 position = mul(_ViewToWorld, float4(positionVS, 1));

                float3 final = 0;
                for (int a = 0; a < PointLightAmount; a++)
                {
                    float3 PLightDirection = position.xyz - PointLightPosition[a].xyz;
                    //return float4(PointLightPosition[a].xyz, 1);
                    float lightDist = length(PLightDirection);
                    //return float4(lightDist, lightDist, lightDist, 1) / 200;

                    PLightDirection = PLightDirection / lightDist;                  

                    //calculate the intensity of the light with exponential falloff
                    //float decay = PointLightDecayRangeStrength[a].x;
                    float range = PointLightDecayRangeStrength[a].y;
                    float origIntensity = PointLightDecayRangeStrength[a].x;
                    float decay = clamp(30 - origIntensity, 0, 30);
                    float decay01 = decay / 30;
                    float baseIntensity = saturate((range - lightDist) / range);
                    float atten = baseIntensity* (1 - decay01) * 4 +
                        //   pow(baseIntensity, decay01*8) *
                        origIntensity*pow(baseIntensity, 4);
                                    
                    /*   float3 toLight = PointLightPosition[a].xyz - position;
                    float lengthSq = dot(toLight, toLight);
                    lengthSq = max(lengthSq, 0.000001);
                    toLight *= rsqrt(lengthSq);
                    float quadratic_attenuation = 1.0 / range*range;
                    atten = 1.0 / (1.0 + lengthSq * 1);
                    */

                    float diffuseIntensity = saturate(dot(worldSpaceNormal, -PLightDirection));
                    final += atten*PointLightColor[a].xyz * diffuseIntensity;
                }

                final.xyz += tex2D(_MainLightMap, float2(i.uv)).xyz;
                return float4(final, 1);
            }
            ENDCG
        }
    }
}
