using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRWallPaintingClear : MonoBehaviour
{
    public float decayCycle = 0.9f;
    public float decayValue = 0.005f;

    [SerializeField] private Shader alphaClearShader = null;
    private RenderTexture albedoTex = null;
    private RenderTexture temporaryTex = null;
    [SerializeField] private Material alphaClearMaterial = null;

    public RenderTexture srcTexture
    {
        set
        {
            albedoTex = value;
            CreateTemporaryTexture();
        }
    }

    // Use this for initialization
    void Start ()
    {
        alphaClearMaterial = new Material(alphaClearShader);
        alphaClearMaterial.SetFloat("_Decay", decayValue);
    }

    private void Update()
    {
        if (albedoTex != null)
        {
            Graphics.Blit(albedoTex, temporaryTex);
            Graphics.Blit(temporaryTex, albedoTex, alphaClearMaterial);
        }
    }

    // Update is called once per frame
    public void StartClearAlpha()
    {
        StartCoroutine(AlphaClearCoroutine());
    }

    private IEnumerator AlphaClearCoroutine()
    {
        while (albedoTex != null)
        {
            Graphics.Blit(albedoTex, temporaryTex);
            Graphics.Blit(temporaryTex, albedoTex, alphaClearMaterial);

            yield return new WaitForSeconds(decayCycle);
        }
    }

    private void CreateTemporaryTexture()
    {
        temporaryTex = new RenderTexture(albedoTex.width, albedoTex.height, albedoTex.depth, albedoTex.format);
    }
}
