using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dilation : MonoBehaviour
{
    public int dilateCount = 5;
    public Shader dilateMaskShader, genBinaryMaskShader, refineGBufferShader;

    Material dilateMaskMaterial, genBinaryMaskMaterial, refineGBufferMaterial;
    RenderTexture dilatedMaskRT, binaryMaskRT, origMapTP;
    void DilateRT(int DilateCount, RenderTexture origRT)
    {
        if (dilateMaskMaterial == null)
        {
            binaryMaskRT = new RenderTexture(origRT.width, origRT.height, 0, RenderTextureFormat.RHalf);
            binaryMaskRT.antiAliasing = 1;
            binaryMaskRT.filterMode = FilterMode.Point;

            dilatedMaskRT = new RenderTexture(origRT.width, origRT.height, 0, RenderTextureFormat.RHalf);
            dilatedMaskRT.antiAliasing = 1;
            dilatedMaskRT.filterMode = FilterMode.Point;

            dilateMaskMaterial = new Material(dilateMaskShader);
            genBinaryMaskMaterial = new Material(genBinaryMaskShader);

            // Create temporary  map.
            origMapTP = new RenderTexture(origRT);
            refineGBufferMaterial = new Material(refineGBufferShader);
        }

        for (int i = 0; i < DilateCount; i++)
        {
            // Generate binary mask.
            Graphics.Blit(origRT, binaryMaskRT, genBinaryMaskMaterial);

            // Generate dilated binary mask.
            dilateMaskMaterial.SetFloat("_TextureW", binaryMaskRT.width);
            dilateMaskMaterial.SetFloat("_TextureH", binaryMaskRT.height);
            Graphics.Blit(binaryMaskRT, dilatedMaskRT, dilateMaskMaterial);

            // Copy texture data to temporary texture buffer
            Graphics.Blit(origRT, origMapTP);

            // Refine pos map.
            refineGBufferMaterial.SetTexture("_OrigMask", binaryMaskRT);
            refineGBufferMaterial.SetTexture("_DilatedMask", dilatedMaskRT);
            refineGBufferMaterial.SetFloat("_ScaleW", 1f / (float)binaryMaskRT.width);
            refineGBufferMaterial.SetFloat("_ScaleH", 1f / (float)binaryMaskRT.height);
            Graphics.Blit(origMapTP, origRT, refineGBufferMaterial);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        DilateRT(dilateCount, source);
        Graphics.Blit(source, destination);
    }
}
