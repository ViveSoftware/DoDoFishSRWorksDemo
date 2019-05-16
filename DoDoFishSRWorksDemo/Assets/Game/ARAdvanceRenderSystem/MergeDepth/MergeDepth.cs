#define USE_2RT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeDepth : MonoBehaviour
{
    // public CameraRenderRT cameraRenderRT;
    public RenderTexture cameraRenderRT;
    public CopyCameraDepthColor copyCameraColorDepth;
    public Material mergeDepthMat;
    public RenderTexture RTL, RTR;
    Camera currentCamera;

    public MeshRenderer debugRender;
    public void Awake()
    {
        currentCamera = GetComponent<Camera>();
        RTL = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 0, RenderTextureFormat.RFloat);
        RTL.antiAliasing = 1;
        RTL.filterMode = FilterMode.Point;
        RTL.useMipMap = false;
        RTL.anisoLevel = 0;
#if USE_2RT
        RTR = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 0, RenderTextureFormat.RFloat);
        RTR.antiAliasing = 1;
        RTR.filterMode = FilterMode.Point;
        RTR.useMipMap = false;
        RTR.anisoLevel = 0;
#endif
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        mergeDepthMat.SetTexture("_CameraRenderDepth", cameraRenderRT);
        mergeDepthMat.SetTexture("_VRDepthColor", copyCameraColorDepth.GetRT(currentCamera.stereoActiveEye));

#if USE_2RT
        if (currentCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
            Graphics.Blit(null, RTL, mergeDepthMat);
        else
            Graphics.Blit(null, RTR, mergeDepthMat);
#else
        Graphics.Blit(null, RT, mergeDepthMat);
#endif

        if (debugRender != null)
            debugRender.material.mainTexture = RTL;
    }

    public RenderTexture GetRT(Camera.MonoOrStereoscopicEye eye)
    {
        return (eye == Camera.MonoOrStereoscopicEye.Left) ? RTL : RTR;
        //return (eye == Camera.MonoOrStereoscopicEye.Left) ? RTR : RTL;
    }
}
