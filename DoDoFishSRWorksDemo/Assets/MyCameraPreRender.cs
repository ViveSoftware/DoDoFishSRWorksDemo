using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCameraPreRender : MonoBehaviour
{
    public RenderTexture rt;
    public Material[] mats;
    public string matName;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnPreRender()
    {
        if (mats != null)
            foreach (Material mat in mats)
                mat.SetTexture(matName, rt);
    }
}
