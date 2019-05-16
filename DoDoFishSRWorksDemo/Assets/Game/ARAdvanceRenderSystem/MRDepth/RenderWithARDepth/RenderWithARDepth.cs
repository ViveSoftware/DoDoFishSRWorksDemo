using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vive.Plugin.SR;

public class RenderWithARDepth : MonoBehaviour
{
    public Material RenderWithARDepthMaterial;
    //public CameraRenderRT MRNormalDepth; //currently without normal
    public RenderTexture MRDepthNormal;
    public CopyCameraDepthColor VRCamera;
    //public Texture seeThroughL, seeThroughR;
    public CopyCameraImage seeThroughL;
    public CopyCameraImage seeThroughR;
    public SoftEdgeWeight softEdgeWeight;

    [Range(0.001f, 0.00001f)]
    public float softCullLength = 0.001f;

    [Range(0.0f, 0.2f)]
    public float glowAmount = 0.039f;

    [Range(0.0f, 1.0f)]
    public float coefAmount = 0.04f;

    [Range(-0.001f, 0.001f)]
    public float CullingBaise = 0.0f;
    

    // [Range(0.01f, 0.0001f)]
    // public float softCullFactor = 0.6f;
    //public MeshRenderer CameraColorDebugPlane;
    //public MeshRenderer MRNormalDepthDebugPlane;
    //public MeshRenderer VirtualColorDepthDebugPlane;
    Camera currentCamera;

    private void Start()
    {
        currentCamera = GetComponent<Camera>();
    }

    void Update()
    {
        //CameraColorDebugPlane.material.mainTexture = MRNormalDepth.GetNormalDepthTexture();
        //MRNormalDepthDebugPlane.material.mainTexture = MRNormalDepth.GetNormalDepthTexture();
        //VirtualColorDepthDebugPlane.material.mainTexture = VRCamera.DepthRT;

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Camera.MonoOrStereoscopicEye eyeSide = currentCamera.stereoActiveEye;

        RenderWithARDepthMaterial.SetFloat("_CullingBaise", CullingBaise);
        
        RenderWithARDepthMaterial.SetTexture("_MRDepthNormal", MRDepthNormal);
        RenderWithARDepthMaterial.SetTexture("_SeeThroughColor", (eyeSide == Camera.MonoOrStereoscopicEye.Left) ? seeThroughL.GetRT() : seeThroughR.GetRT());
        RenderWithARDepthMaterial.SetTexture("_VRDepthColor", VRCamera.GetRT(eyeSide));
        //RenderWithMRDepthMaterial.SetTexture("_VRDepth", VRCamera.DepthRT);
        RenderWithARDepthMaterial.SetFloat("_softCullLength", softCullLength);
        //   RenderWithMRDepthMaterial.SetFloat("_softCullFactor", softCullFactor);       

        RenderWithARDepthMaterial.SetFloat("_GlowAmount", glowAmount);
        RenderWithARDepthMaterial.SetFloat("_CoefAmount", coefAmount);

        RenderWithARDepthMaterial.SetTexture("_SoftCullingMap", softEdgeWeight.SoftEdgeWeightRT);
        
        Graphics.Blit(source, destination, RenderWithARDepthMaterial);
    }




}
