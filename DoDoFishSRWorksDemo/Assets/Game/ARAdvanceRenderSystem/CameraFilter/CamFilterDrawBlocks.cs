
using UnityEngine;
using System.Collections;

public class CamFilterDrawBlocks : MonoBehaviour
{
    public Shader shader;
    [Range(0.001f, 0.8f)]
    public float Fade = 1f;

    Vector4 ScreenResolution;
    Material _material;

    Material material
    {
        get
        {
            if (_material == null)
            {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
            return _material;
        }
    }

    void Start()
    {
        shader = Shader.Find("Custom/CamFilterDrawBlocks");
    }

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        material.SetFloat("Fade", Fade);
        material.SetVector("_ScreenResolution", new Vector4(sourceTexture.width, sourceTexture.height, 0.0f, 0.0f));
        Graphics.Blit(sourceTexture, destTexture, material);
    }
}