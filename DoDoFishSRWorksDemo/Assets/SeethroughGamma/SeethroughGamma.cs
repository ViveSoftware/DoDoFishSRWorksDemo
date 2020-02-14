using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeethroughGamma : MonoBehaviour
{
    Material mat;
    [Range(0, 3)]
    public float gamma = 1.8f;

    void Start()
    {
        mat = new Material(Shader.Find("MyHelp/SeethroughGamma"));
    }

    void Update()
    {

    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Camera cam = GetComponent<Camera>();
        //RenderTexture rt = cam.targetTexture;
        RenderTexture rt = RenderTexture.GetTemporary(cam.targetTexture.descriptor);
        //Graphics.Blit(cam.targetTexture, rt);

        mat.SetTexture("_TexSrc", src);
        mat.SetFloat("_Gamma", gamma);
        Graphics.Blit(null, rt, mat);
        Graphics.Blit(rt, dest);
        RenderTexture.ReleaseTemporary(rt);

        
    }
}
