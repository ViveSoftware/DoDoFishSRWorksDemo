using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockFaceDirection : MonoBehaviour
{
    [SerializeField] private Transform ARWall;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.L))
        {
            transform.forward = ARWall.forward;
        }
	}
}
