Shader "Custom/DeferredSpotLightMap"
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
            static const int SLightPerPass = 64;
            int SpotLightAmount;
            float4 SpotLightColor[SLightPerPass];
            float4 SpotLightPosition[SLightPerPass];
            float4 SpotLightDirection[SLightPerPass];
            float4 SLightThetaPhiStrengthRange[SLightPerPass];

			sampler2D _MainLightMap, _CameraDepthNormal;

            float CalcDegree(float3 v1, float3 v2)
            {
                //The degree with 2 vector
                //A dot B = | A| |B | cos(θ)
                //θ = Acos((A dot B) / (| A| |B | ))
                float radian = acos(dot(v2,v1 ));//|A| |B| is 1
                    return degrees(radian)*2;
            }

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
                for (int a = 0; a<SpotLightAmount; a++)
                {
                    float3 directionToLight = SpotLightPosition[a] - position;
                    float lightDist = length(directionToLight);
                    directionToLight = directionToLight / lightDist;

                    //float coneDot = dot(-directionToLight, SpotLightDirection[a]);
                    float coneDegree = CalcDegree(-directionToLight, SpotLightDirection[a]);

                    float anglePhi = SLightThetaPhiStrengthRange[a].y;
                //    float angleTheta = SLightThetaPhiStrengthRange[a].x;
                    float range = SLightThetaPhiStrengthRange[a].w;
                    float origIntensity = SLightThetaPhiStrengthRange[a].z;                   
                    float coneAttenuation = 0;

                    //The more inner circle will dot==1 with light direction,
                    //So, the inner circle's dot value is larger then outer circle.
                    //if (coneDot > anglePhi)//the inner circle must larger then outer circle, so, we can remove the if().
                    {   
                       // float baseIntensity = saturate((anglePhi - coneDegree) / anglePhi);
                       // coneAttenuation = pow(baseIntensity, 1);
                        
                        float decay = clamp(30 - origIntensity, 0, 30);
                        float decay01 = decay / 30;
                        float baseIntensity = saturate((anglePhi- coneDegree) / anglePhi);
                        coneAttenuation =
                            baseIntensity* (1 - decay01) * 1 +
                            //   pow(baseIntensity, decay01*8) *
                            origIntensity*pow(baseIntensity, 1.5);
                            
                        coneAttenuation *= pow
                        (
                            saturate
                            (
                            (range - lightDist) / range
                            )
                            , 2
                        );
                        /*
                        float3 toLight = SpotLightPosition[a] - position;
                        float lengthSq = dot(toLight, toLight);
                        lengthSq = max(lengthSq, 0.000001);
                        toLight *= rsqrt(lengthSq);
                        //1.0 / (1.0 + 25.0*r*r)
                        float quadratic_attenuation = 1.0 / (1.0 + 25.0*r*r);
                        float atten = 1.0 / (1.0 + lengthSq * quadratic_attenuation);
                        float rho = max(0, dot(toLight, SpotLightDirection[a]));
                        float unity_LightAtten_x = cos(radians(anglePhi) / 2);
                        float unity_LightAtten_y = 1 / cos(radians(anglePhi) / 4);
                        float spotAtt = (rho - unity_LightAtten_x) * unity_LightAtten_y;
                        atten *= saturate(spotAtt);
                        coneAttenuation = atten;
                        */
                        /*
                        baseIntensity = saturate((range - lightDist) / range);
                        coneAttenuation *= (
                            baseIntensity* (1 - decay01) * 4 +
                            //   pow(baseIntensity, decay01*8) *
                            origIntensity*pow(baseIntensity, 32)
                            );
                            */

                        float diffuseIntensity = saturate(dot(worldSpaceNormal, directionToLight));
                        final += coneAttenuation * SpotLightColor[a].xyz *diffuseIntensity;
                    }
                }

                final.xyz += tex2D(_MainLightMap, float2(i.uv)).xyz;
                return float4(final, 1);
            }
            ENDCG
        }
    }
}
