using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedWorldPosCover : MonoBehaviour
{
    private Renderer render;
	// Use this for initialization
	void Start ()
    {
        render = GetComponent<Renderer>();
        render.material.SetMatrix("_WorldMatrix", transform.localToWorldMatrix);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.U))
        {
            UpdateWorldMatrix();
        }
    }

    public void UpdateWorldMatrix()
    {
        render.material.SetMatrix("_WorldMatrix", transform.localToWorldMatrix);
    }
}
