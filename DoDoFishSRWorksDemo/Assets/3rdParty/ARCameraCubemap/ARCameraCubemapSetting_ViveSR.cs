using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vive.Plugin.SR;

[RequireComponent(typeof(ARCameraCubemap))]
public class ARCameraCubemapSetting_ViveSR : MonoBehaviour
{
    bool isSetting;
    Texture SeethroughTexture;//For SRWorks 0.9.1.0
    public Renderer DebugCube;
    ARCameraCubemap cameraCubemap;

    void Start()
    {
        cameraCubemap = GetComponent<ARCameraCubemap>();
    }

    void Update()
    {
        if (SeethroughTexture == null && !isSetting && ViveSR.FrameworkStatus == FrameworkStatus.WORKING)
        {
            ViveSR_DualCameraRig rig = GameObject.FindObjectOfType<ViveSR_DualCameraRig>();
            SeethroughTexture = rig.DualCameraLeft.targetTexture;
            //ViveSR_TrackedCamera tc = rig.DualCameraLeft.GetComponentInParent<ViveSR_TrackedCamera>();
            //SeethroughTexture = tc.ImagePlane.GetComponent<Renderer>().sharedMaterial.mainTexture;
            isSetting = SeethroughTexture != null;
        }

        if (isSetting && SeethroughTexture != null)
        {
            cameraCubemap.SetTexture(SeethroughTexture);
            if (DebugCube != null)
                DebugCube.material.SetTexture("_Tex", cameraCubemap.cubeMap);
        }
    }


}
