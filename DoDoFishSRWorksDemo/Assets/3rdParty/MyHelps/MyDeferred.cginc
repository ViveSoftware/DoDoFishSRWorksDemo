
// https://forum.unity.com/threads/recreating-world-position-from-depth-algorithm.263435/
float4x4 _ViewToWorld;
float4 _FrustrumPoints;
float _CameraFar;
// 1. Take the screen space position uv and find where it lies on the rectangle on the far clip plane. This is a ray from the camera to the far clip plane.
// 2. Then we multiply the z position by the depth to find where that point lies on a ray. We have to multiple the x and y coordinates by depth if it's perspective.
// The _FrustrumPoints are calculated in the code
/*float3 CalcVS(float2 screenPos, float depth)
{
    float2 lerpVal = screenPos;
    float3 ray = float3(
        _FrustrumPoints.x * lerpVal.x + _FrustrumPoints.y * (1 - lerpVal.x),
        _FrustrumPoints.z * lerpVal.y + _FrustrumPoints.w * (1 - lerpVal.y),
        _CameraFar); // I think _ProjectionParams.z will also work

//#if defined (PERSPECTIVE)
//    float3 posVS = float3(ray.xy * -depth, ray.z * -depth);
//#else
    float3 posVS = float3(ray.xy, ray.z * -depth);
//#endif
    return posVS;
}*/

float3 CalcVS_PERSPECTIVE(float2 screenPos, float depth)
{
    float2 lerpVal = screenPos;
    float3 ray = float3(
        _FrustrumPoints.x * lerpVal.x + _FrustrumPoints.y * (1 - lerpVal.x),
        _FrustrumPoints.z * lerpVal.y + _FrustrumPoints.w * (1 - lerpVal.y),
        _CameraFar); // I think _ProjectionParams.z will also work

    float3 posVS = float3(ray.xy * -depth, ray.z * -depth);
    return posVS;
}

//https://answers.unity.com/questions/218333/shader-inversefloat4x4-function.html
                 /*	float4x4 inverse(float4x4 input)
                      {
                          #define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
                          //determinant(float3x3(input._22_23_23, input._32_33_34, input._42_43_44))

                          float4x4 cofactors = float4x4(
                               minor(_22_23_24, _32_33_34, _42_43_44),
                              -minor(_21_23_24, _31_33_34, _41_43_44),
                               minor(_21_22_24, _31_32_34, _41_42_44),
                              -minor(_21_22_23, _31_32_33, _41_42_43),

                              -minor(_12_13_14, _32_33_34, _42_43_44),
                               minor(_11_13_14, _31_33_34, _41_43_44),
                              -minor(_11_12_14, _31_32_34, _41_42_44),
                               minor(_11_12_13, _31_32_33, _41_42_43),

                               minor(_12_13_14, _22_23_24, _42_43_44),
                              -minor(_11_13_14, _21_23_24, _41_43_44),
                               minor(_11_12_14, _21_22_24, _41_42_44),
                              -minor(_11_12_13, _21_22_23, _41_42_43),

                              -minor(_12_13_14, _22_23_24, _32_33_34),
                               minor(_11_13_14, _21_23_24, _31_33_34),
                              -minor(_11_12_14, _21_22_24, _31_32_34),
                               minor(_11_12_13, _21_22_23, _31_32_33)
                          );
                          #undef minor
                          return transpose(cofactors) / determinant(input);
                      }
                      */

