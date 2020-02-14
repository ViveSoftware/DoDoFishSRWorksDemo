using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCameraImage : MonoBehaviour
{
    public int delayFrame;
    public RenderTexture destination;

    public delegate void RTRefreshCallback(RenderTexture rt, RenderTexture OnRenderImageDest);
    public RTRefreshCallback rtRefreshCallback;

    public RenderTexture GetRT()
    {
        return destination;
    }

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        destination = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGB32);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, destination);

        if (rtRefreshCallback != null)
            rtRefreshCallback(destination, dest);
    }
}