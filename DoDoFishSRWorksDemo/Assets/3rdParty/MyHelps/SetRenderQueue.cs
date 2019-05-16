using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRenderQueue : MonoBehaviour
{
    public int renderQueue;

    // Use this for initialization
    void Start()
    {
        Material newMat = new Material(GetComponent<MeshRenderer>().sharedMaterial);
        newMat.renderQueue = renderQueue;
        GetComponent<MeshRenderer>().material = newMat;
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    public void SetNewQueue(int value)
    {
        GetComponent<MeshRenderer>().material.renderQueue = value;
    }

    public void RevertQueue()
    {
        GetComponent<MeshRenderer>().material.renderQueue = renderQueue;
    }
}
