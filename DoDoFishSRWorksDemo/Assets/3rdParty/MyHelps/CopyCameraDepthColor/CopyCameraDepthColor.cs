#define USE_FLOAT32
#define USE_2RTxx
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCameraDepthColor : MonoBehaviour
{
    Camera currentCamera;
    public RenderTexture[] ColorDepthLRT = new RenderTexture[1];
#if USE_2RT
    public RenderTexture[] ColorDepthRRT = new RenderTexture[1];
#endif
    public Material CopyCameraDepthColorMaterial;//, CopyCameraDepthMaterial;
    public MeshRenderer debugRender;
    const int delayFrame = 0;

#if !USE_FLOAT32
    public RenderTexture DepthRT;
#endif

    public RenderTexture GetRT(Camera.MonoOrStereoscopicEye eye)
    {
#if USE_2RT
        return (eye == Camera.MonoOrStereoscopicEye.Left) ? ColorDepthLRT[delayFrame] : ColorDepthRRT[delayFrame];
#else
        return ColorDepthLRT[delayFrame];
#endif
    }

    public RenderTexture GetRT(StereoTargetEyeMask eye)
    {
#if USE_2RT
        return (eye == StereoTargetEyeMask.Left) ? ColorDepthLRT[delayFrame] : ColorDepthRRT[delayFrame];
#else
        return ColorDepthLRT[delayFrame];
#endif
    }

    private void Start()
    {
        currentCamera = GetComponent<Camera>();
        currentCamera.depthTextureMode = DepthTextureMode.Depth;

#if USE_FLOAT32
        for (int a = 0; a < ColorDepthLRT.Length; a++)
        {
            ColorDepthLRT[a] = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 0, RenderTextureFormat.ARGBFloat);
            ColorDepthLRT[a].antiAliasing = 1;
            ColorDepthLRT[a].filterMode = FilterMode.Point;
            ColorDepthLRT[a].useMipMap = false;
            ColorDepthLRT[a].anisoLevel = 0;
        }
#if USE_2RT
        for (int a = 0; a < ColorDepthRRT.Length; a++)
        {
            ColorDepthRRT[a] = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 0, RenderTextureFormat.ARGBFloat);
            ColorDepthRRT[a].antiAliasing = 1;
            ColorDepthRRT[a].filterMode = FilterMode.Point;
            ColorDepthRRT[a].useMipMap = false;
            ColorDepthRRT[a].anisoLevel = 0;
        }
#endif
#else
        ColorDepthRT = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        ColorDepthRT.antiAliasing = 1;
        ColorDepthRT.filterMode = FilterMode.Point;
        ColorDepthRT.useMipMap = false;
        ColorDepthRT.anisoLevel = 0;

        DepthRT = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 0, RenderTextureFormat.RFloat);
        DepthRT.antiAliasing = 1;
        DepthRT.filterMode = FilterMode.Point;
        DepthRT.useMipMap = false;
        DepthRT.anisoLevel = 0;
#endif
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture colorDepthRT = null;

#if USE_2RT
        if (currentCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
        {
            if (delayFrame == 0)
            {
                colorDepthRT = ColorDepthLRT[0];
            }
            else
            {
                RenderTexture recLast = ColorDepthLRT[ColorDepthLRT.Length - 1];
                Array.Copy(ColorDepthLRT, 0, ColorDepthLRT, 1, ColorDepthLRT.Length - 1);
                ColorDepthLRT[0] = recLast;
                colorDepthRT = ColorDepthLRT[0];
            }
            //if (current != 0)
            //    Debug.LogError("0");
            //current = 1;
            //Debug.LogWarning("[CopyCameraColorDepth] copy L");
        }
        else if (currentCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
        {
            if (delayFrame == 0)
            {
                colorDepthRT = ColorDepthRRT[0];
            }
            else
            {
                RenderTexture recLast = ColorDepthRRT[ColorDepthRRT.Length - 1];
                Array.Copy(ColorDepthRRT, 0, ColorDepthRRT, 1, ColorDepthRRT.Length - 1);
                ColorDepthRRT[0] = recLast;
                colorDepthRT = ColorDepthRRT[0];
            }

            //if (current != 1)
            //    Debug.LogError("1");
            //current = 0;
            //Debug.LogWarning("[CopyCameraColorDepth] copy R");
        }
        else
        {
            Debug.LogError("[CopyCameraDepthColor] copy Failed");
            return;
        }
#else
        colorDepthRT = ColorDepthLRT[delayFrame];
#endif

#if USE_FLOAT32
        Graphics.Blit(source, colorDepthRT, CopyCameraDepthColorMaterial);
#else
        Graphics.Blit(source, ColorDepthRT, CopyCameraColorDepthMaterial);
        Graphics.Blit(source, DepthRT, CopyCameraDepthMaterial);
#endif

        if (debugRender != null)
            debugRender.material.mainTexture = colorDepthRT;
    }
}
