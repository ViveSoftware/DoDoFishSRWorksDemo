using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ViveHandTracking.HandRenderer))]
public class ScaleHandOcclude : MonoBehaviour
{
    ViveHandTracking.HandRenderer handRenderer;
    public Vector3 scaleJoint = -Vector3.one, scaleBone = -Vector3.one;
    public float jointScale = 0.014f/* 0.025f*/, boneScale = 0.008f /*0.013f*/;
    void Start()
    {
        handRenderer = GetComponent<ViveHandTracking.HandRenderer>();
    }

    void LateUpdate()
    {
        for (int a = 0; a < transform.childCount; a++)
        {
            Transform child = transform.GetChild(a);
            if (child.name.Contains("point"))
            {
                Vector3 localS = child.localScale;
                if (scaleJoint.x < 0)
                    scaleJoint = localS;
                else
                    child.localScale = new Vector3(jointScale, jointScale, jointScale);
            }
            else if (child.name.Contains("link"))
            {
                Vector3 localS = child.localScale;
                if (scaleBone.x < 0)
                    scaleBone = localS;
                else
                    child.localScale = new Vector3(boneScale, boneScale, localS.z);
            }
        }
    }
}
