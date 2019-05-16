using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionBreatheController : MonoBehaviour
{
    public Renderer render;
    public Color emissionColor = Color.white;
    public float halfCycle = 2f;
    private float timer = 0, sign = 1;
    
	// Update is called once per frame
	void Update ()
    {
        if (timer < 0 || timer > halfCycle)
        {
            sign = (timer < 0) ? 1 : -1;
        }

        render.material.SetColor("_EmissionColor", emissionColor * timer / halfCycle);
        timer += Time.deltaTime * sign;
	}
}
