using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockXZRotate : MonoBehaviour
{
    public Transform track;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.P))
        {
            Vector3 direct = Vector3.ProjectOnPlane(track.forward, Vector3.up);
            transform.rotation = Quaternion.LookRotation(direct, Vector3.up);
        }
    }
}
