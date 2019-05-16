#define USE_MERGE_DEPTH
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetColorDepthToQuad : MonoBehaviour
{
    public RenderTexture depthTex;
    public WriteDepth writeDepth;
    void Start()
    {

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
#if USE_MERGE_DEPTH
        Debug.LogError("Error Error Error");
#else
        writeDepth.cameraDepth = depthTex;
#endif
        //writeDepth.colorTex = 
        Graphics.Blit(source, destination);
    }
}
