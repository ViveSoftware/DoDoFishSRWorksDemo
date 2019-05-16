#define USE_BLURHV
#define USE_SCAN_DEPTH
using UnityEngine;
using System.Collections;

public class ShadowRecieve : MonoBehaviour
{
    //public bool UseBlur;
    public float ScreenMapScale = 0.5f;
    public Light shadowLight;
    ShadowCastCamera shadowLightCamera;    
    RenderTexture ScreenShadowMap, ScreenShadowMapBlur;


    public MeshRenderer debugRender;
    public Material screenShadowBlit, blurMaterial, blurHMaterial, blurVMaterial;
    [Range(0, 10), Tooltip("BlurFactor = 0 is turn off blur")]
    public float BlurFactor;

    Camera renderCamera;

    public CopyCameraDepthColor VRDepthColor;
    //public Texture VRColorDepth;

#if USE_SCAN_DEPTH
    public RenderTexture cameraDepthN;
    Material _shadowRecieveDepthMat;
#else
    GameObject _shaderReplacementCamera;
    public LayerMask recieveShadowLayer;
    Shader _ShadowRecieveShader;
#endif

    [Range(0.0f, 0.01f)]
    public float bais = 0.01f;

    [Range(0.0f, 0.1f)]
    public float shadowBlitBais = 0.0001f;

    [Range(0f, 100f)]
    public float ShadowFactor = 1;

    void Start()
    {
        shadowLightCamera = shadowLight.GetComponentInChildren<ShadowCastCamera>();
        renderCamera = GetComponent<Camera>();
        ScreenShadowMap = new RenderTexture(Mathf.CeilToInt(renderCamera.pixelWidth * ScreenMapScale), Mathf.CeilToInt(renderCamera.pixelHeight * ScreenMapScale), 24, RenderTextureFormat.RGFloat);
        ScreenShadowMapBlur = new RenderTexture(Mathf.CeilToInt(renderCamera.pixelWidth * ScreenMapScale), Mathf.CeilToInt(renderCamera.pixelHeight * ScreenMapScale), 24, RenderTextureFormat.RGFloat);
        ScreenShadowMap.useMipMap = ScreenShadowMapBlur.useMipMap = false;
        ScreenShadowMap.filterMode = ScreenShadowMapBlur.filterMode = FilterMode.Point;
        ScreenShadowMap.antiAliasing = ScreenShadowMapBlur.antiAliasing = 1;
        ScreenShadowMap.anisoLevel = ScreenShadowMapBlur.anisoLevel = 0;

#if USE_SCAN_DEPTH
        _shadowRecieveDepthMat = new Material(Shader.Find("Custom/ShadowRecieveDepth"));
        if (cameraDepthN == null)
            Debug.LogError("[ShadowRecieve] recieveShadowDepthMap must not null");
#else
        _ShadowRecieveShader = Shader.Find("Custom/ShadowRecieve");
        if (recieveShadowLayer == LayerMask.NameToLayer("Default"))
            Debug.LogError("[ShadowRecieve] recieveShadowLayer must not default");
#endif


    }

    void OnPostRender()
    {
        //https://github.com/Unity-Technologies/VolumetricLighting/blob/master/Assets/AreaLight/Scripts/AreaLight.cs

#if USE_SCAN_DEPTH
        //deferred        
        _shadowRecieveDepthMat.SetTexture("_CameraDepthNormal", cameraDepthN);
        Matrix4x4 camera2World = renderCamera.cameraToWorldMatrix;
        //DeferredLightMapMaterial.EnableKeyword("PERSPECTIVE");
        _shadowRecieveDepthMat.SetMatrix("_ViewToWorld", camera2World);
        var lpoints = DeferredLightMap.RecalculateFrustrumPoints(renderCamera);
        _shadowRecieveDepthMat.SetVector("_FrustrumPoints", new Vector4(lpoints[2].x, lpoints[1].x, lpoints[1].y, lpoints[0].y));
        _shadowRecieveDepthMat.SetFloat("_CameraFar", renderCamera.farClipPlane);

        //shadow
        _shadowRecieveDepthMat.SetMatrix("_MyShadowVP", shadowLightCamera.GetWorld2LightMatrix());
        _shadowRecieveDepthMat.SetTexture("_MyShadowDepth", shadowLightCamera.GetDepthMap());
        _shadowRecieveDepthMat.SetFloat("_MyShadowBais", bais);
        _shadowRecieveDepthMat.SetVector("_MyShadowLightDir", shadowLight.transform.forward);
        _shadowRecieveDepthMat.SetFloat("_MyShadowFactor", ShadowFactor);

        Graphics.Blit(null, ScreenShadowMap, _shadowRecieveDepthMat);
#else
        if (_shaderReplacementCamera == null)
        {
            _shaderReplacementCamera = new GameObject("RecieveShadowCameraClone");
            _shaderReplacementCamera.hideFlags = HideFlags.HideAndDontSave;
            Camera c = _shaderReplacementCamera.AddComponent<Camera>();
            c.enabled = false;
        }

        Camera cam = _shaderReplacementCamera.GetComponent<Camera>();
        cam.CopyFrom(renderCamera);
        //cam.CopyFrom(shadowLightCamera.GetComponent<Camera>());

        //   cam.aspect = lightcamera.aspect;
        cam.backgroundColor = Color.white;//new Color(0, 0, 0, 0);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.renderingPath = RenderingPath.Forward;
        cam.useOcclusionCulling = false;
        cam.targetTexture = ScreenShadowMap;
        cam.depthTextureMode = DepthTextureMode.None;

        cam.depth = 100;
        cam.cullingMask = recieveShadowLayer;//-1;//everythingCullingMask

        {
            Shader.SetGlobalMatrix("_MyShadowVP", shadowLightCamera.GetWorld2LightMatrix());
            Shader.SetGlobalTexture("_MyShadowDepth", shadowLightCamera.GetDepthMap());
            Shader.SetGlobalFloat("_MyShadowBais", bais);
            Shader.SetGlobalVector("_MyShadowLightDir", shadowLight.transform.forward);
            Shader.SetGlobalFloat("_MyShadowFactor", ShadowFactor);
            cam.stereoTargetEye = StereoTargetEyeMask.None;
            cam.RenderWithShader(_ShadowRecieveShader, "");
        }
#endif

        if (BlurFactor > 0)
        {
#if USE_BLURHV
            blurHMaterial.SetFloat("_Factor", BlurFactor);
            blurVMaterial.SetFloat("_Factor", BlurFactor);
            Vector4 texelSize = new Vector4(ScreenShadowMap.texelSize.x, ScreenShadowMap.texelSize.y, 0, 0);
            blurHMaterial.SetVector("_MainTex_TexelSize", texelSize);
            blurVMaterial.SetVector("_MainTex_TexelSize", texelSize);

            Graphics.Blit(ScreenShadowMap, ScreenShadowMapBlur, blurHMaterial);
            Graphics.Blit(ScreenShadowMapBlur, ScreenShadowMap, blurVMaterial);
#else
            blurMaterial.SetFloat("_Factor", BlurFactor);

            //http://www.shaderslab.com/demo-39---blur-effect-with-grab-pass.html
            //blur shadow map
            Graphics.Blit(ScreenShadowMap, ScreenShadowMapBlur, blurMaterial);
            RenderTexture tmp = ScreenShadowMap;
            ScreenShadowMap = ScreenShadowMapBlur;
            ScreenShadowMapBlur = tmp;
#endif
        }

        if (debugRender != null)
            debugRender.material.mainTexture = ScreenShadowMap;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
#if USE_SCAN_DEPTH
        screenShadowBlit.SetInt("needFlip", 1);
#else
        screenShadowBlit.SetInt("needFlip", 0);
#endif

        screenShadowBlit.SetTexture("_ScreenShadowMap", ScreenShadowMap);

        if (VRDepthColor == null)
            VRDepthColor = GetComponent<CopyCameraDepthColor>();
        screenShadowBlit.SetTexture("_VRDepthColor", VRDepthColor.GetRT(renderCamera.stereoActiveEye));
        screenShadowBlit.SetFloat("_shadowBlitBais", shadowBlitBais);
        Graphics.Blit(src, dest, screenShadowBlit);
    }

    void OnDisable()
    {
#if !USE_SCAN_DEPTH
        if (_shaderReplacementCamera != null)
        {
            DestroyImmediate(_shaderReplacementCamera);
        }
#endif
    }
}
