uniform float4 _MyShadowLightDir;
uniform sampler2D _MyShadowDepth;
uniform float4x4 _MyShadowVP;
uniform float _MyShadowBais;
uniform float _MyShadowFactor;

float shadowAtten(
    float3 worldPos,
    float3 worldNor)
{
    float4 frustumPos = mul(_MyShadowVP, float4(worldPos, 1));

    //Outside shadow frustum method 1:
    //float2 shadowUV = 0.5 * frustumPos.xy / frustumPos.w + float2(0.5,0.5);
    //float2 frustumUV = shadowUV;
    //if(!(shadowUV.x>0 &&shadowUV.x<1 && shadowUV.y>0 && shadowUV.y<1))

    //Outside shadow frustum method 2: https://github.com/Unity-Technologies/VolumetricLighting/blob/master/Assets/AreaLight/Shaders/AreaLightShadows.cginc
    float2 shadowUV = frustumPos.xy / frustumPos.w;
    float2 frustumUV = shadowUV * 0.5 + 0.5;
    if (any(step(1.0, abs(shadowUV))))
    {
        //shadowColor.rgb = float3(1,1,1);//outside shadow frustum
        return 1;
    }

    float shadowDepth = 1 - tex2D(_MyShadowDepth, frustumUV).r;
    
    //return float4(shadowDepth,shadowDepth,shadowDepth,1);//Debug
    float pixelDepth = frustumPos.z / frustumPos.w;
    pixelDepth = 1 + pixelDepth;
    pixelDepth = pixelDepth / 2;

    //if(pixelDepth >0.9)//Debug
    //if(shadowDepth < 0.1)//Debug
    float shadowOffset = (pixelDepth - _MyShadowBais) - shadowDepth;
    if (shadowOffset > 0)//near plane is 0, far plane is 1
    {
        float face = dot(_MyShadowLightDir, worldNor);
        if (face < 0)
        {
            //shadowColor.rgb = _ShadowColor.rgb;
            return shadowOffset * _MyShadowFactor;//make the shadow not close with model will not so black
        }
    }
    //return float4(shadowColor.r, shadowColor.g, shadowColor.b, cameraDepth);//Debug
    //return EncodeDepthNormal(cameraDepth, float3(shadowColor.r, shadowColor.g, shadowColor.b));
    return 1;
}