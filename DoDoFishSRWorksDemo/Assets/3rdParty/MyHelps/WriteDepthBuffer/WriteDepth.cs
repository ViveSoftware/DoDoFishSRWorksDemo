#define USE_PERSPECTIVE
#define USE_MERGE_DEPTH
#define USE_CopyCameraDepthxx
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class WriteDepth : MonoBehaviour
{
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

    Material depthMat;

    //public Texture2D colorTex;
    public static int WriteDepthLayer
    {
        get
        {
            int layer = LayerMask.NameToLayer("WriteDepth");
            if (layer < 0)
            { Debug.LogError("please add a layer => WriteDepth"); return 0; }
            return layer;
        }
    }

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        cam.cullingMask = 1 << WriteDepthLayer;
        transform.GetChild(0).gameObject.layer = WriteDepthLayer;

        //RemoveMaskLayer
        LayerMask mainCamLayer = (LayerMask)mainCam.cullingMask;
        mainCam.cullingMask = mainCamLayer & ~(1 << WriteDepthLayer);

        depthMat = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        //depthMat.SetTexture("_ColorTex", colorTex);

    }

    private void OnPreRender()
    {
        Camera cam = GetComponent<Camera>();

#if USE_CopyCameraDepth
        if (copyCameraDepth != null)
            depthMat.mainTexture = copyCameraDepth.DepthRT;
#else
#if USE_MERGE_DEPTH
        depthMat.mainTexture = mergeDepth.GetRT(cam.stereoActiveEye);
#else
        depthMat.mainTexture = cameraDepth;
#endif
#endif


        //int screenW = cam.pixelWidth;// Screen.width;
        //int screenH = cam.pixelHeight;// Screen.height;
        float aspectRatio = 1f / cam.aspect;//(float)screenH / (float)screenW
        transform.GetChild(0).localScale = new Vector3(1, aspectRatio, 1);
        cam.orthographicSize = aspectRatio * 0.5f;

        depthMat.SetFloat("_MainCamNear", mainCam.nearClipPlane);
        depthMat.SetFloat("_MainCamFar", mainCam.farClipPlane);
    }
}
