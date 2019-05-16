using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionFlowController : MonoBehaviour
{
    public Vector2 Speed;
    private Renderer render;
    private float timer = 0;

    private void Start()
    {
        render = GetComponent<Renderer>();
    }
    // Update is called once per frame
    void Update ()
    {
        render.material.SetTextureOffset("_MainTex", Speed * timer);
        timer += Time.deltaTime;
	}
}
