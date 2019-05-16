using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftEdgeWeight : MonoBehaviour
{
    public Material SoftEdgeWeightMaterial;
    public CameraRenderRT renderDepth;
    public RenderTexture SoftEdgeWeightRT;
    Camera currentCamera;
    [Range(0, 20)]
    public float factor = 1;

    public MeshRenderer debugRenderDepth;
    public MeshRenderer debugRenderBlurDepth;
    public MeshRenderer debugRenderFinal;

    void Start()
    {
        currentCamera = GetComponent<Camera>();
        SoftEdgeWeightRT = new RenderTexture(currentCamera.pixelWidth, currentCamera.pixelHeight, 0, RenderTextureFormat.RFloat);
        SoftEdgeWeightRT.antiAliasing = 1;
        SoftEdgeWeightRT.filterMode = FilterMode.Point;
        SoftEdgeWeightRT.useMipMap = false;
        SoftEdgeWeightRT.anisoLevel = 0;
    }

    void Update()
    {
        if (debugRenderDepth != null)
            debugRenderDepth.material.mainTexture = renderDepth.RT;
        if (debugRenderBlurDepth != null)
            debugRenderBlurDepth.material.mainTexture = renderDepth.RTBlur;
        if (debugRenderFinal != null)
            debugRenderFinal.material.mainTexture = SoftEdgeWeightRT;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SoftEdgeWeightMaterial.SetTexture("_DepthTex", renderDepth.RT);
        SoftEdgeWeightMaterial.SetTexture("_DepthBlurTex", renderDepth.RTBlur);
        SoftEdgeWeightMaterial.SetFloat("factor", factor);
        
        Graphics.Blit(null, SoftEdgeWeightRT, SoftEdgeWeightMaterial);

        //Graphics.Blit(source, destination);
    }
}
