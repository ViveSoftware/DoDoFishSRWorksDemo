using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARCameraCubemap : MonoBehaviour
{
    public Cubemap cubeMap;//serialize for debug
    Camera _camera;
    ReflectionProbe _reflectProb;

    RenderTexture[] _BlendDest;
    [SerializeField] RenderTexture _BlendFinal;

    [SerializeField] float currentRatio;
    [SerializeField] Material blend2Tex;
    [SerializeField] float ratioSpeed = 0.1f;

    static int _layer = -1;
    public static int ARCameraCubemapLayer
    {
        get
        {
            if (_layer < 0)
                _layer = LayerMask.NameToLayer("ARCameraCubemap");
            if (_layer < 0)
                Debug.LogError("please add 'ARCameraCubemap' layer");
            return _layer;
        }
    }

    void Start()
    {
        _camera = GetComponentInChildren<Camera>();
        _reflectProb = GetComponentInChildren<ReflectionProbe>();
        //cubeMap = new Cubemap(2048, TextureFormat.RGBA32, false);

        //Renderer[] renderers = GetComponentsInChildren<Renderer>();
        //foreach (Renderer render in renderers)
        //    render.gameObject.layer = ARCameraCubemapLayer;
        MyHelpLayer.SetSceneLayer(transform, ARCameraCubemapLayer);

        _reflectProb.cullingMask =
        _camera.cullingMask = 1 << ARCameraCubemapLayer;
    }

    int currentFace = 0;
    void Update()
    {
        _reflectProb.enabled = !_camera.enabled;
        if (_camera.enabled)
        {
            _camera.RenderToCubemap(cubeMap, currentFace);
            currentFace++;
            if (currentFace == 6)
                currentFace = 0;
        }
    }

    public void SetTexture(Texture t)
    {
        if (_BlendDest == null)
        {
            _BlendDest = new RenderTexture[2];
            _BlendDest[0] = new RenderTexture(t.width, t.height, 0, RenderTextureFormat.ARGB32);
            _BlendDest[1] = new RenderTexture(t.width, t.height, 0, RenderTextureFormat.ARGB32);
            _BlendFinal = new RenderTexture(t.width, t.height, 0, RenderTextureFormat.ARGB32);

            currentRatio = 0;

            Graphics.Blit(t, _BlendDest[0]);
            Graphics.Blit(t, _BlendDest[1]);
            currentRatio = 0;
        }

        currentRatio += Time.unscaledDeltaTime * ratioSpeed;
        if (currentRatio < 1)
        {
            blend2Tex.SetFloat("ratio", currentRatio);
            blend2Tex.SetTexture("_TexSrc", _BlendDest[0]);
            blend2Tex.SetTexture("_TexSrc2", _BlendDest[1]);
            Graphics.Blit(null, _BlendFinal, blend2Tex);
        }
        else
        {
            Graphics.Blit(_BlendFinal, _BlendDest[0]);
            Graphics.Blit(t, _BlendDest[1]);
            currentRatio = 0;
        }

        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        renderer.sharedMaterial.mainTexture = _BlendFinal;
    }
}
