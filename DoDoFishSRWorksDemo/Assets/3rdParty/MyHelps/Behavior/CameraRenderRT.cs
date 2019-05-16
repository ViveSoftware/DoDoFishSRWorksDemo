using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraRenderRT : MonoBehaviour
{
    public enum RTTYPE
    {
        ARGB32, FLOAT, FLOAT2, FLOAT4,
    }
    public RTTYPE RTType = RTTYPE.ARGB32;
    public RenderTexture /*leftEyeRT, rightEyeRT,*/ RT;
    public Shader RenderShader;
    public LayerMask cullMask;
    public Color clearColor = Color.clear;
    Camera currentCamera;
    GameObject _shaderReplacementCamera;

    [Header("Blur R-----------------------------------")]
    public Material blurHMaterial, blurVMaterial;
    [Range(0, 10), Tooltip("BlurFactor = 0 is turn off blur")]
    public float BlurFactor;
    public RenderTexture RTBlur;
    RenderTexture RTBlur2;

    public MeshRenderer debugRender;

    public void Init()
    {
        currentCamera = GetComponent<Camera>();
        RT = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 24, getFormat());
        RT.antiAliasing = 1;
        RT.filterMode = FilterMode.Point;
        RT.useMipMap = false;
        RT.anisoLevel = 0;
    }

    public RenderTextureFormat getFormat()
    {
        switch (RTType)
        {
            case RTTYPE.FLOAT:
                return RenderTextureFormat.RFloat;
            case RTTYPE.FLOAT2:
                return RenderTextureFormat.RGFloat;
            case RTTYPE.FLOAT4:
                return RenderTextureFormat.ARGBFloat;
            default:
                return RenderTextureFormat.ARGB32;
        }
    }

    public static StereoTargetEyeMask ConsiderStereoCamera(Camera camera)
    {
        if (camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left) return StereoTargetEyeMask.Left;
        else if (camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right) return StereoTargetEyeMask.Right;
        else return StereoTargetEyeMask.None;

        //Matrix4x4 viewMat = camera.worldToCameraMatrix;
        //Matrix4x4 viewMatL = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
        //Matrix4x4 viewMatR = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

        //if (viewMat == viewMatL)
        //{
        //    //Debug.LogWarning("[CameraRenderRT] L pass create RT");
        //    return StereoTargetEyeMask.Left;
        //}
        //else if (viewMat == viewMatR)
        //{
        //    //Debug.LogWarning("[CameraRenderRT] R pass create RT");
        //    return StereoTargetEyeMask.Right;
        //}
        //else
        //{
        //    //Debug.LogWarning("[CameraRenderRT] one main pass");
        //    return StereoTargetEyeMask.None;
        //}
    }

    private void OnPreRender()
    {
        //StereoTargetEyeMask cameraSide = ConsiderStereoCamera(currentCamera);

        ////  RenderTexture currentRT = null;
        //if (cameraSide == StereoTargetEyeMask.Left)
        //{
        //    Debug.LogWarning("[CameraRenderRT][OnPreRender] L pass");
        //    //if (leftEyeRT == null)
        //    //{
        //    //    Debug.LogWarning("[StereoCameraRT] L pass create RT");
        //    //    leftEyeRT = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, getFormat());
        //    //}
        //    //// renderEye = 1;
        //    //currentRT = leftEyeRT;
        //}
        //else if (cameraSide == StereoTargetEyeMask.Right)
        //{
        //    Debug.LogWarning("[CameraRenderRT][OnPreRender] R pass");
        //    //if (rightEyeRT == null)
        //    //{
        //    //    Debug.LogWarning("[StereoCameraRT] R pass create RT");
        //    //    rightEyeRT = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, getFormat());
        //    //}
        //    //// renderEye = 2;
        //    //currentRT = rightEyeRT;
        //}
        //else
        //{
        //    Debug.LogWarning("[CameraRenderRT][OnPreRender] one main pass");
        //    //need to do...
        //    //renderEye = 0;
        //    //currentRT = main1RT;
        //}

        if (_shaderReplacementCamera == null)
        {
            _shaderReplacementCamera = new GameObject("Camera render RT");
            _shaderReplacementCamera.hideFlags = HideFlags.HideAndDontSave;
            Camera c = _shaderReplacementCamera.AddComponent<Camera>();
            c.enabled = false;
        }
        Camera cam = _shaderReplacementCamera.GetComponent<Camera>();
        cam.CopyFrom(currentCamera);
        cam.backgroundColor = clearColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.renderingPath = RenderingPath.Forward;
        cam.targetTexture = RT;
        cam.depth = -100;
        cam.cullingMask = cullMask;//-1;//everythingCullingMask
        cam.stereoTargetEye = StereoTargetEyeMask.None;
        cam.RenderWithShader(RenderShader, "");

        if (BlurFactor > 0)
        {
            if (RTBlur == null)
                RTBlur = new RenderTexture(RT.descriptor);
            if (RTBlur2 == null)
                RTBlur2 = new RenderTexture(RT.descriptor);

            blurHMaterial.SetFloat("_Factor", BlurFactor);
            blurVMaterial.SetFloat("_Factor", BlurFactor);
            Vector4 texelSize = new Vector4(RT.texelSize.x, RT.texelSize.y, 0, 0);
            blurHMaterial.SetVector("_MainTex_TexelSize", texelSize);
            blurVMaterial.SetVector("_MainTex_TexelSize", texelSize);

            Graphics.Blit(RT, RTBlur2, blurHMaterial);
            Graphics.Blit(RTBlur2, RTBlur, blurVMaterial);
        }

        if (debugRender != null)
            debugRender.material.mainTexture = RT;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDisable()
    {
        if (_shaderReplacementCamera != null)
        {
            DestroyImmediate(_shaderReplacementCamera);
        }
    }
}
