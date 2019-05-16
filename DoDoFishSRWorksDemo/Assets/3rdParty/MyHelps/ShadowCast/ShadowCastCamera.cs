using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ShadowCastCamera : MonoBehaviour
{
    public int shadowMapSize = 1024;
    public Shader RenderDepthShader;
    public RenderTexture DepthMap;
    GameObject _shaderReplacementCamera;
    //MyHelps.MyHelp.ShowMeshBounds showBound = new MyHelps.MyHelp.ShowMeshBounds();
    public MeshRenderer debugRenderer;
    //public int debug_cullMask;
    public LayerMask shadowRenderMask = 1;
    Camera lightCamera;

    void Start()
    {
        DepthMap = new RenderTexture(shadowMapSize, shadowMapSize, 24, RenderTextureFormat.RFloat);
        DepthMap.wrapMode = TextureWrapMode.Clamp;
        DepthMap.useMipMap = false;
        DepthMap.filterMode = FilterMode.Point;
        DepthMap.Create();

        lightCamera = GetComponent<Camera>();
        lightCamera.stereoTargetEye = StereoTargetEyeMask.None;//must set none for only 1 eye
        lightCamera.clearFlags = CameraClearFlags.Color;
        lightCamera.backgroundColor = Color.clear;
    }

    private void Update()
    {
        if (debugRenderer != null)
        {
            debugRenderer.material.mainTexture = DepthMap;
        }

        //debug_cullMask = _cullMask;
        //if (_cullMask == -1)
        //{
        //    _cullMask = lightCamera.cullingMask;
        lightCamera.cullingMask = 0;
        //}
        // lightCamera.cullingMask = lightCamera.cullingMask & ~cullMask; //remove my shadow layer from unity light cast
    }

    void LateUpdate()
    {
        if (_shaderReplacementCamera == null)
        {
            _shaderReplacementCamera = new GameObject("ShadowCameraClone");
            _shaderReplacementCamera.hideFlags = HideFlags.HideAndDontSave;
            Camera c = _shaderReplacementCamera.AddComponent<Camera>();
            c.enabled = false;
        }
        Camera cam = _shaderReplacementCamera.GetComponent<Camera>();
        cam.CopyFrom(lightCamera);
        //   cam.aspect = lightcamera.aspect;
        cam.backgroundColor = Color.black;//near plane is 0, far plane is 1
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.renderingPath = RenderingPath.Forward;
        cam.targetTexture = DepthMap;
        cam.depth = -100;
        cam.cullingMask = shadowRenderMask;//-1;//everythingCullingMask
        cam.stereoTargetEye = StereoTargetEyeMask.None;
        cam.RenderWithShader(RenderDepthShader, "");
    }

    void OnDisable()
    {
        if (_shaderReplacementCamera != null)
        {
            DestroyImmediate(_shaderReplacementCamera);
        }
    }

    void fixCameraPose()
    {
        lightCamera = GetComponent<Camera>();
        transform.position = transform.parent.position + transform.parent.forward * lightCamera.farClipPlane * -0.5f;
        transform.localRotation = Quaternion.identity;
    }

    void OnDrawGizmos()
    {
        Camera lightcamera = GetComponent<Camera>();
        float verticalHeightSeen = lightcamera.orthographicSize * 2.0f;

        fixCameraPose();
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position + transform.forward * lightcamera.farClipPlane * 0.5f,
            transform.rotation, Vector3.one);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(verticalHeightSeen/** lightcamera.aspect*/, verticalHeightSeen, lightcamera.farClipPlane));
    }

    public Matrix4x4 GetWorld2LightMatrix()
    {
        //https://github.com/Unity-Technologies/VolumetricLighting/blob/master/Assets/AreaLight/Scripts/AreaLight.cs
        Camera lightcamera = GetComponent<Camera>();
        Matrix4x4 ortho = Matrix4x4.Ortho(
            -lightcamera.orthographicSize, lightcamera.orthographicSize,
            -lightcamera.orthographicSize, lightcamera.orthographicSize,
            0, -lightcamera.farClipPlane);
        return ortho * transform.worldToLocalMatrix;
    }

    public Texture GetDepthMap()
    {
        return DepthMap;
    }
}

