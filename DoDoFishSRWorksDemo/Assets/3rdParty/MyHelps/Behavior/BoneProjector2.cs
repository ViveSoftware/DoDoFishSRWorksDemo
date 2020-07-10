using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveHandTracking;

public class BoneProjector2 : MonoBehaviour
{
    public BoneProjector.RelateBone[] mirrorBones;
    Quaternion[] offsetRots;

    public bool start = false;
    bool alreadyStart = false;
    private void Update()
    {
        if (start && !alreadyStart)
        {
            start = false;
            alreadyStart = true;
            _initStart();
        }

        if (!alreadyStart)
            return;

        //mirrorBones[0].dest.position = mirrorBones[0].orig.position;
        for (int a = 1; a < mirrorBones.Length; a++)
        {
            mirrorBones[a].dest.localRotation =  Quaternion.Inverse(mirrorBones[a].orig.localRotation) * offsetRots[a];
        }
    }

    void _initStart()
    {
        offsetRots = new Quaternion[mirrorBones.Length];
        for (int a = 0; a < mirrorBones.Length; a++)
        {
            offsetRots[a] = mirrorBones[a].dest.localRotation * Quaternion.Inverse(mirrorBones[a].orig.localRotation);
        }
    }
}
