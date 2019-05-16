#define USE_MERGE_DEPTHxx
#define USE_CopyCameraDepthxx
#define USE_Normal
#define DEFERRED_USE_RAY
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DeferredLightMap : MonoBehaviour
{
    //static DeferredLightMap _lightMap;
    //public static DeferredLightMap Instance
    //{
    //    get { if (_lightMap == null) { _lightMap = FindObjectOfType<DeferredLightMap>(); } return _lightMap; }
    //}
    public float LightMapFactor = 1f;
    public float LightMapScale = 0.5f;
    public MeshRenderer debugRender, debugRender2;
    public Material DeferredPointLightMapMaterial, DeferredSpotLightMapMaterial, BlitLightMapMaterial;
    Camera mainCamera;
#if USE_CopyCameraDepth
#if USE_Normal
    CopyCameraDepthNormal copyCameraDepthNormal;
#else
    CopyCameraDepth copyCameraDepth;
#endif
#else
#if USE_MERGE_DEPTH
    public MergeDepth mergeDepth;
#else
#if USE_Normal
    public RenderTexture cameraRenderDepthNormal;
#else
    public RenderTexture cameraRenderDepth;
#endif
#endif
#endif
    RenderTexture lightMapRT, lightMapRT2;

    public bool UseDeferredLight = true;
    public bool TurnOffUnityLight = false;
    public List<Light> pointLightList = new List<Light>();
    public List<Light> spotLightList = new List<Light>();

    public void AddPointLight(Light light)
    {
        if (!light.enabled)
            return;
        if (light.type == LightType.Point)
        {
            pointLightList.Add(light);
            return;
        }
        Debug.LogError("[LightMap][AddPointLight] not point light : " + light.type.ToString());
    }

    public void RemovePointLight(Light light)
    {
        if (!pointLightList.Remove(light))
            Debug.LogError("[LightMap][RemovePointLight] light not in list : " + light.type.ToString());
    }

    public void AddSpotLight(Light light)
    {
        if (!light.enabled)
            return;
        if (light.type == LightType.Spot)
        {
            spotLightList.Add(light);
            return;
        }
        Debug.LogError("[LightMap][AddSpotLight] not point light : " + light.type.ToString());
    }

    public void RemoveSpotLight(Light light)
    {
        if (!spotLightList.Remove(light))
            Debug.LogError("[LightMap][RemoveSpotLight] light not in list : " + light.type.ToString());
    }

    public void ClearAllLights()
    {
        spotLightList.Clear();
        pointLightList.Clear();
    }

    private void Start()
    {
#if USE_CopyCameraDepth
#if USE_Normal
        copyCameraDepthNormal = GetComponent<CopyCameraDepthNormal>();
#else
        copyCameraDepth = GetComponent<CopyCameraDepth>();
#endif
#endif

        mainCamera = GetComponent<Camera>();
        lightMapRT = new RenderTexture(Mathf.CeilToInt(mainCamera.pixelWidth * LightMapScale), Mathf.CeilToInt(mainCamera.pixelHeight * LightMapScale), 0, RenderTextureFormat.ARGB32);
        lightMapRT2 = new RenderTexture(Mathf.CeilToInt(mainCamera.pixelWidth * LightMapScale), Mathf.CeilToInt(mainCamera.pixelHeight * LightMapScale), 0, RenderTextureFormat.ARGB32);

        //   Light l = gameObject.AddComponent<Light>();
        //     l.type = LightType.Point;
        //  AddPointLight(l);
        /*  AddPointLight(l);
          AddPointLight(l);
          AddPointLight(l);
          AddPointLight(l);
          AddPointLight(l);*/
    }

    void Update()
    {
        if (debugRender != null)
            debugRender.material.mainTexture = lightMapRT;
        if (debugRender2 != null)
            debugRender2.material.mainTexture = lightMapRT2;
    }

    static void ClearRT(RenderTexture myRenderTextureToClear)
    {
        RenderTexture rt = UnityEngine.RenderTexture.active;
        UnityEngine.RenderTexture.active = myRenderTextureToClear;
        GL.Clear(true, true, Color.clear);
        UnityEngine.RenderTexture.active = rt;
    }
    const int PLightPerPass = 64;
    Vector4[] PointLightColorList = new Vector4[PLightPerPass];
    Vector4[] PointLightPositionList = new Vector4[PLightPerPass];
    Vector4[] PointLightDecayRangeStrengthList = new Vector4[PLightPerPass];

    const int SLightPerPass = 64;
    Vector4[] SLightColorList = new Vector4[SLightPerPass];
    Vector4[] SLightPositionList = new Vector4[SLightPerPass];
    Vector4[] SLightDirectionList = new Vector4[SLightPerPass];
    Vector4[] SLightThetaPhiStrengthRangeList = new Vector4[SLightPerPass];
    //int oldLightAmount;
    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //    lightMapRT = RenderTexture.GetTemporary(mainCamera.pixelWidth, mainCamera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        //    lightMapRT2 = RenderTexture.GetTemporary(mainCamera.pixelWidth, mainCamera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        ClearRT(lightMapRT);
        //   ClearRT(lightMapRT2);
        /* Vector4 halfPixel;
         halfPixel.x = -1f * 0.5f / (float)lightMapRT.width;
         halfPixel.y = -1f * 0.5f / (float)lightMapRT.height;
         halfPixel.z = halfPixel.w = 0;
         DeferredLightMapMaterial.SetVector("halfPixel", halfPixel);*/

        //Point light=======================================================
#if USE_CopyCameraDepth
#if USE_Normal
        DeferredPointLightMapMaterial.SetTexture("_CameraDepthNormal", copyCameraDepthNormal.GetRT(mainCamera.stereoActiveEye));
#else
        DeferredPointLightMapMaterial.SetTexture("_CameraDepthNormal", copyCameraDepth.DepthRT);
#endif
#else
#if USE_MERGE_DEPTH
        DeferredPointLightMapMaterial.SetTexture("_CameraDepthNormal", mergeDepth.GetRT(mainCamera.stereoActiveEye));
#else
#if USE_Normal
        DeferredPointLightMapMaterial.SetTexture("_CameraDepthNormal", cameraRenderDepthNormal);
#else
        DeferredPointLightMapMaterial.SetTexture("_CameraDepthNormal", cameraRenderDepth);
#endif
#endif
#endif
        //Debug.LogError("[DeferredLightMap] there are no any depth input (copyCameraDepth, cameraRenderDepth)");


#if DEFERRED_USE_RAY
        Matrix4x4 camera2World = mainCamera.cameraToWorldMatrix;
        //DeferredLightMapMaterial.EnableKeyword("PERSPECTIVE");
        DeferredPointLightMapMaterial.SetMatrix("_ViewToWorld", camera2World);
        var lpoints = RecalculateFrustrumPoints(mainCamera);
        DeferredPointLightMapMaterial.SetVector("_FrustrumPoints", new Vector4(lpoints[2].x, lpoints[1].x, lpoints[1].y, lpoints[0].y));
        DeferredPointLightMapMaterial.SetFloat("_CameraFar", mainCamera.farClipPlane);
#else
        DeferredLightMapMaterial.SetMatrix("invVP", mainCameraDepth.GetInvVP());
#endif

        //https://www.alanzucconi.com/2016/01/27/arrays-shaders-heatmaps-in-unity3d/
        //https://stackoverflow.com/questions/45098671/how-to-define-an-array-of-floats-in-shader-properties
        int currentPack = 0;
        List<Light>.Enumerator pointItr = pointLightList.GetEnumerator();
        while (pointItr.MoveNext())
        {
            if (!UseDeferredLight)
            {
                pointItr.Current.enabled = true;
                continue;
            }
            if (TurnOffUnityLight)
                pointItr.Current.enabled = false;
            //else
            //    pointItr.Current.enabled = true;

            int id = currentPack % PLightPerPass;
            PointLightColorList[id].x = pointItr.Current.color.r;
            PointLightColorList[id].y = pointItr.Current.color.g;
            PointLightColorList[id].z = pointItr.Current.color.b;
            PointLightColorList[id].w = 1;
            PointLightPositionList[id].x = pointItr.Current.transform.position.x;
            PointLightPositionList[id].y = pointItr.Current.transform.position.y;
            PointLightPositionList[id].z = pointItr.Current.transform.position.z;
            PointLightDecayRangeStrengthList[id].x = pointItr.Current.intensity;
            PointLightDecayRangeStrengthList[id].y = pointItr.Current.range;
            PointLightDecayRangeStrengthList[id].z = 0;

            if (id == PLightPerPass - 1 || currentPack == pointLightList.Count - 1)
            {
                //drawing to map
                int PointLightAmount = id + 1;
                DeferredPointLightMapMaterial.SetInt("PointLightAmount", PointLightAmount);
                DeferredPointLightMapMaterial.SetVectorArray("PointLightColor", PointLightColorList);
                DeferredPointLightMapMaterial.SetVectorArray("PointLightPosition", PointLightPositionList);
                DeferredPointLightMapMaterial.SetVectorArray("PointLightDecayRangeStrength", PointLightDecayRangeStrengthList);

                //GL.Clear(false, false, Color.black);
                DeferredPointLightMapMaterial.SetTexture("_MainLightMap", lightMapRT);
                Graphics.Blit(null, lightMapRT2, DeferredPointLightMapMaterial);

                RenderTexture tmpRT = lightMapRT;
                lightMapRT = lightMapRT2;
                lightMapRT2 = tmpRT;

                //if (oldLightAmount != pointLightList.Count)
                //    Debug.LogWarning("[DeferredLightMap] render light map : light amount : " + PointLightAmount);
            }

            currentPack++;
        }

        //if (oldLightAmount != pointLightList.Count)
        //{
        //    oldLightAmount = pointLightList.Count;
        //    Debug.LogWarning("[DeferredLightMap] render light map : done");
        //}

        //Spot light=======================================================
#if USE_CopyCameraDepth
#if USE_Normal
        DeferredSpotLightMapMaterial.SetTexture("_CameraDepthNormal", copyCameraDepthNormal.GetRT(mainCamera.stereoActiveEye));
#else
        DeferredSpotLightMapMaterial.SetTexture("_CameraDepthNormal", copyCameraDepth.DepthRT);
#endif
#else
#if USE_MERGE_DEPTH
        DeferredSpotLightMapMaterial.SetTexture("_CameraDepth", mergeDepth.GetRT(mainCamera.stereoActiveEye));
#else
#if USE_Normal
        DeferredSpotLightMapMaterial.SetTexture("_CameraDepthNormal", cameraRenderDepthNormal);
#else
        DeferredSpotLightMapMaterial.SetTexture("_CameraDepthNormal", cameraRenderDepth);
#endif
#endif
#endif
        //Debug.LogError("[DeferredLightMap] there are no any depth input (copyCameraDepth, cameraRenderDepth)");

        DeferredSpotLightMapMaterial.SetMatrix("_ViewToWorld", camera2World);
        DeferredSpotLightMapMaterial.SetVector("_FrustrumPoints", new Vector4(lpoints[2].x, lpoints[1].x, lpoints[1].y, lpoints[0].y));
        DeferredSpotLightMapMaterial.SetFloat("_CameraFar", mainCamera.farClipPlane);

        currentPack = 0;
        List<Light>.Enumerator spotItr = spotLightList.GetEnumerator();
        while (spotItr.MoveNext())
        {
            if (!UseDeferredLight)
            {
                spotItr.Current.enabled = true;
                continue;
            }
            if (TurnOffUnityLight)
                spotItr.Current.enabled = false;
            //else
            //    spotItr.Current.enabled = true;

            int id = currentPack % SLightPerPass;

            SLightColorList[id].x = spotItr.Current.color.r;
            SLightColorList[id].y = spotItr.Current.color.g;
            SLightColorList[id].z = spotItr.Current.color.b;
            SLightColorList[id].w = 1;
            SLightPositionList[id].x = spotItr.Current.transform.position.x;
            SLightPositionList[id].y = spotItr.Current.transform.position.y;
            SLightPositionList[id].z = spotItr.Current.transform.position.z;
            SLightDirectionList[id].x = spotItr.Current.transform.forward.x;
            SLightDirectionList[id].y = spotItr.Current.transform.forward.y;
            SLightDirectionList[id].z = spotItr.Current.transform.forward.z;
            //SLightThetaPhiStrengthRangeList[id].x = anglePhi * 0.5f;
            SLightThetaPhiStrengthRangeList[id].y = spotItr.Current.spotAngle;
            SLightThetaPhiStrengthRangeList[id].z = spotItr.Current.intensity;
            SLightThetaPhiStrengthRangeList[id].w = spotItr.Current.range;

            if (id == SLightPerPass - 1 || currentPack == spotLightList.Count - 1)
            {
                //drawing to map
                int spotLightAmount = id + 1;
                DeferredSpotLightMapMaterial.SetInt("SpotLightAmount", spotLightAmount);
                DeferredSpotLightMapMaterial.SetVectorArray("SpotLightColor", SLightColorList);
                DeferredSpotLightMapMaterial.SetVectorArray("SpotLightPosition", SLightPositionList);
                DeferredSpotLightMapMaterial.SetVectorArray("SpotLightDirection", SLightDirectionList);
                DeferredSpotLightMapMaterial.SetVectorArray("SLightThetaPhiStrengthRange", SLightThetaPhiStrengthRangeList);

                DeferredSpotLightMapMaterial.SetTexture("_MainLightMap", lightMapRT);
                Graphics.Blit(null, lightMapRT2, DeferredSpotLightMapMaterial);

                RenderTexture tmpRT = lightMapRT;
                lightMapRT = lightMapRT2;
                lightMapRT2 = tmpRT;
            }

            currentPack++;
        }

        BlitLightMapMaterial.SetTexture("_ScreenLightMap", lightMapRT);
        BlitLightMapMaterial.SetFloat("_Factor", LightMapFactor);
        Graphics.Blit(source, destination, BlitLightMapMaterial);

        //   RenderTexture.ReleaseTemporary(lightMapRT);
        //    RenderTexture.ReleaseTemporary(lightMapRT2);
    }



#if DEFERRED_USE_RAY    
    // Code to calculate all the corners near and far of a camera
    public static Vector3[] RecalculateFrustrumPoints(Camera cam)
    {
        Vector3[] frustumCorners = new Vector3[4];
        Camera.MonoOrStereoscopicEye eye = cam.stereoActiveEye;
        if (cam.stereoEnabled)
        {
            //lefr right switch (I don't know why)
            eye = Camera.MonoOrStereoscopicEye.Left;
            if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                eye = Camera.MonoOrStereoscopicEye.Right;
        }

        cam.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), cam.farClipPlane, eye, frustumCorners);
        return frustumCorners;

        //var frustrumPoints = new Vector3[8];
        //var far = cam.farClipPlane;
        //var near = cam.nearClipPlane;
        //var aspectRatio = cam.pixelWidth / cam.pixelHeight;

        //if (cam.orthographic)
        //{
        //    var orthoSize = cam.orthographicSize;

        //    frustrumPoints[0] = new Vector3(orthoSize, orthoSize, near);
        //    frustrumPoints[1] = new Vector3(-orthoSize, orthoSize, near);
        //    frustrumPoints[2] = new Vector3(-orthoSize, -orthoSize, near);
        //    frustrumPoints[3] = new Vector3(orthoSize, -orthoSize, near);
        //    frustrumPoints[4] = new Vector3(orthoSize, orthoSize, far);
        //    frustrumPoints[5] = new Vector3(-orthoSize, orthoSize, far);
        //    frustrumPoints[6] = new Vector3(-orthoSize, -orthoSize, far);
        //    frustrumPoints[7] = new Vector3(orthoSize, -orthoSize, far);
        //}
        //else
        //{
        //    var hNear = 2 * Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad) * near;
        //    var wNear = hNear * aspectRatio;

        //    var hFar = 2 * Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad) * far;
        //    var wFar = hFar * aspectRatio;

        //    var fc = Vector3.forward * far;
        //    var ftl = fc + (Vector3.up * hFar / 2) - (Vector3.right * wFar / 2);
        //    var ftr = fc + (Vector3.up * hFar / 2) + (Vector3.right * wFar / 2);
        //    var fbl = fc - (Vector3.up * hFar / 2) - (Vector3.right * wFar / 2);
        //    var fbr = fc - (Vector3.up * hFar / 2) + (Vector3.right * wFar / 2);

        //    var nc = Vector3.forward * near;
        //    var ntl = nc + (Vector3.up * hNear / 2) - (Vector3.right * wNear / 2);
        //    var ntr = nc + (Vector3.up * hNear / 2) + (Vector3.right * wNear / 2);
        //    var nbl = nc - (Vector3.up * hNear / 2) - (Vector3.right * wNear / 2);
        //    var nbr = nc - (Vector3.up * hNear / 2) + (Vector3.right * wNear / 2);

        //    frustrumPoints[0] = ntr;
        //    frustrumPoints[1] = ntl;
        //    frustrumPoints[2] = nbr;
        //    frustrumPoints[3] = nbl;
        //    frustrumPoints[4] = ftr;
        //    frustrumPoints[5] = ftl;
        //    frustrumPoints[6] = fbl;
        //    frustrumPoints[7] = fbr;
        //}

        //return frustrumPoints;
    }
#endif
}
