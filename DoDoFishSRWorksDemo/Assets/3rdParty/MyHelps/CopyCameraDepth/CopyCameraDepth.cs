using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCameraDepth : MonoBehaviour
{
    Camera currentCamera;

    public RenderTexture DepthRT;
    public Material CopyCameraDepthMaterial;
    public MeshRenderer debugRender;

    public RenderTexture GetRT()
    {
        return DepthRT;
    }

    private void Awake()
    {
        currentCamera = GetComponent<Camera>();
        currentCamera.depthTextureMode = DepthTextureMode.Depth; ;

        DepthRT = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 0, RenderTextureFormat.RFloat);
        DepthRT.antiAliasing = 1;
        DepthRT.filterMode = FilterMode.Point;
        DepthRT.useMipMap = false;
        DepthRT.anisoLevel = 0;

        if (debugRender != null)
            debugRender.material.mainTexture = DepthRT;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //StereoTargetEyeMask eyeSide = CameraRenderRT.ConsiderStereoCamera(currentCamera);
        //if (eyeSide == StereoTargetEyeMask.Left)
        //    Debug.LogWarning("[CopyCameraDepth] L pass create RT");
        //else if (eyeSide == StereoTargetEyeMask.Right)
        //    Debug.LogWarning("[CopyCameraDepth] R pass create RT");

        Graphics.Blit(source, DepthRT, CopyCameraDepthMaterial);//get depth
    }

    public Matrix4x4 GetInvVP()
    {
        //https://forum.unity.com/threads/recreating-world-position-from-depth-algorithm.263435/
        Matrix4x4 viewMat = currentCamera.worldToCameraMatrix;
        //Matrix4x4 projMat = renderCamera.projectionMatrix;
        Matrix4x4 projMat = GL.GetGPUProjectionMatrix(currentCamera.projectionMatrix, false);
        return (viewMat * projMat).inverse;
        //return (projMat * viewMat).inverse;
    }
}
