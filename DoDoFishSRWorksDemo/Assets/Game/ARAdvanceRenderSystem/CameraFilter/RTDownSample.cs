using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTDownSample : MonoBehaviour
{
    [Range(1, 8)]
    public int DownScale = 2;

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        if (DownScale > 1)
        {
            int rtW = sourceTexture.width / DownScale;
            int rtH = sourceTexture.height / DownScale;
            RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);
            Graphics.Blit(sourceTexture, buffer);
            Graphics.Blit(buffer, destTexture);
            RenderTexture.ReleaseTemporary(buffer);
        }
        else
        {
            Graphics.Blit(sourceTexture, destTexture);
        }
    }
}
