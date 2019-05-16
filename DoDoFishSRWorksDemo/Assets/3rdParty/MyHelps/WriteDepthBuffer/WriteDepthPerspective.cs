#define USE_MERGE_DEPTH
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Write depth map back to zbuffer with perspective camera setting
/// </summary>
public class WriteDepthPerspective : MonoBehaviour
{
    //public bool? IsRenderRightEye;
    // public float addScale = 1;
    [Range(0.01f, 0.001f)]
    public float addShiftR = 0.00145f;

    [Range(-0.001f, 0.001f)]
    public float addShiftUP = -0.00005f;

    //public float addShiftR = 0.01f;
    public Camera mainCam;
#if USE_CopyCameraDepth
    public CopyCameraDepth copyCameraDepth;
#else
#if USE_MERGE_DEPTH
    public MergeDepth mergeDepth;
#else
    public RenderTexture cameraDepth;
#endif
#endif

    Transform fullScreenQuadL, fullScreenQuadR;

    //public Texture2D colorTex;
    public static int WriteDepthLayerL
    {
        get
        {
            int layer = LayerMask.NameToLayer("WriteDepthL");
            if (layer < 0)
            { Debug.LogError("please add a layer => WriteDepthL"); return 0; }
            return layer;
        }
    }

    //public static int WriteDepthLayerR
    //{
    //    get
    //    {
    //        int layer = LayerMask.NameToLayer("WriteDepthR");
    //        if (layer < 0)
    //        { Debug.LogError("please add a layer => WriteDepthR"); return 0; }
    //        return layer;
    //    }
    //}

    public void Init(LayerMask renderLayer)
    {
        fullScreenQuadL = transform.Find("FullScreenQuadL");
        fullScreenQuadR = transform.Find("FullScreenQuadR");
        Camera cam = GetComponent<Camera>();
        float recDetpth = cam.depth;
        cam.CopyFrom(mainCam);
        cam.depth = recDetpth;

        MeshRenderer quadL = fullScreenQuadL.GetComponent<MeshRenderer>();
        quadL.material = new Material(quadL.sharedMaterial);

        MeshRenderer quadR = fullScreenQuadR.GetComponent<MeshRenderer>();
        quadR.material = new Material(quadR.sharedMaterial);

        //if (IsRenderRightEye == null)
        {
            cam.cullingMask = 1 << WriteDepthLayerL | renderLayer;
            cam.clearFlags = CameraClearFlags.Depth;
            fullScreenQuadR.gameObject.layer = fullScreenQuadL.gameObject.layer = WriteDepthLayerL;
        }
        //else
        //{
        //    if (IsRenderRightEye.Value)
        //    {
        //        cam.cullingMask = 1 << WriteDepthLayerR;
        //        cam.clearFlags = CameraClearFlags.Nothing;
        //        fullScreenQuadL.gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        cam.cullingMask = 1 << WriteDepthLayerL;
        //        cam.clearFlags = CameraClearFlags.Depth;
        //        fullScreenQuadR.gameObject.SetActive(false);
        //    }
        //    fullScreenQuadR.gameObject.layer = WriteDepthLayerR;
        //    fullScreenQuadL.gameObject.layer = WriteDepthLayerL;
        //}

        //RemoveMaskLayer
        LayerMask mainCamLayer = (LayerMask)mainCam.cullingMask;
        mainCam.cullingMask = mainCamLayer & ~(1 << WriteDepthLayerL);// & ~(1 << WriteDepthLayerR);
    }

    private void LateUpdate()
    {
        Camera cam = GetComponent<Camera>();
        Vector3 camDir = cam.transform.forward;//camMat.rotation * Vector3.forward;

        //  float aspectRatio = 1f / cam.aspect;
        // fullScreenQuad.localScale = new Vector3(1, aspectRatio, 1);

        //float currentShift = (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left) ? addShiftL : addShiftR;


        Matrix4x4 camMat = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
        camMat = camMat.inverse;
        Vector3 camPosL = camMat.GetColumn(3);

        camMat = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
        camMat = camMat.inverse;
        Vector3 camPosR = camMat.GetColumn(3);

        //https://answers.unity.com/questions/314049/how-to-make-a-plane-fill-the-field-of-view.html
        float pos = (cam.nearClipPlane + 0.01f);
        fullScreenQuadL.position = camPosL + camDir * pos + cam.transform.right * -addShiftR + cam.transform.up * addShiftUP;
        float h = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f;
        fullScreenQuadL.localScale = new Vector3(h * cam.aspect, h, 0f);

        pos = (cam.nearClipPlane + 0.01f);
        fullScreenQuadR.position = camPosR + camDir * pos + cam.transform.right * addShiftR + cam.transform.up * addShiftUP;
        h = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f;
        fullScreenQuadR.localScale = new Vector3(h * cam.aspect, h, 0f);

        //Set not culling
        //https://forum.unity.com/threads/can-i-disable-culling.43916/
        Bounds notCullingbounds = new Bounds(transform.position, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
        fullScreenQuadL.GetComponent<MeshFilter>().mesh.bounds = fullScreenQuadR.GetComponent<MeshFilter>().mesh.bounds = notCullingbounds;

        Material depthMat = fullScreenQuadL.GetComponent<MeshRenderer>().material;
#if USE_MERGE_DEPTH
        depthMat.mainTexture = mergeDepth.GetRT(Camera.MonoOrStereoscopicEye.Left);
#else
        depthMat.mainTexture = cameraDepth;
#endif
        depthMat.SetFloat("_MainCamNear", mainCam.nearClipPlane);
        depthMat.SetFloat("_MainCamFar", mainCam.farClipPlane);
        depthMat.SetFloat("_RenderOnRight", 0);


        depthMat = fullScreenQuadR.GetComponent<MeshRenderer>().material;
#if USE_MERGE_DEPTH
        depthMat.mainTexture = mergeDepth.GetRT(Camera.MonoOrStereoscopicEye.Right);
#else
        depthMat.mainTexture = cameraDepth;
#endif
        depthMat.SetFloat("_MainCamNear", mainCam.nearClipPlane);
        depthMat.SetFloat("_MainCamFar", mainCam.farClipPlane);
        depthMat.SetFloat("_RenderOnRight", 1);

        //fullScreenQuadL.gameObject.layer = fullScreenQuadR.gameObject.layer = WriteDepthLayerL;
        //fullScreenQuadL.GetComponent<Renderer>().enabled = true;
        //fullScreenQuadR.GetComponent<Renderer>().enabled = true;
    }

}
