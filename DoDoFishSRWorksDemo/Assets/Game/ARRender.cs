#define ADVANCE_RENDER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
    public class ARRender : MySingleton<ARRender>
    {
#if ADVANCE_RENDER
        public const bool ADVANCE_RENDER = true;
#else
        public const bool ADVANCE_RENDER = false;
#endif

        public bool showFPS;
        public Camera VRCamera, DualCameraL, DualCameraR;
        public AdvanceRender advanceRender;
        public Light shadowCastDirLight;

        [SerializeField]
        private List<Transform> unityrenderWithDepthObject;
        [SerializeField]
        private List<Transform> unityrenderWithDepthNoShadowObject;

        CopyCameraImage _dualCameraLRT, _dualCameraRRT;
        Camera _UnityRenderOnTopCamera;
        const int _UnityRenderOnTopCamOrder = 3;

        public static int GetParticleLayer()
        {
#if ADVANCE_RENDER
            return ARRender.UnityRenderWithDepthLayer;
#else
            return LayerMask.NameToLayer("Default");
#endif
        }

        public static int UnityRenderWithDepthLayer
        {
            get
            {
#if ADVANCE_RENDER
                int layer = LayerMask.NameToLayer("UnityRenderWithDepth");
                if (layer < 0)
                { Debug.LogError("please add a layer => UnityRenderWithDepth"); return 0; }
                return layer;
#else
                return LayerMask.NameToLayer("Default");
#endif
            }
        }

        public static int UnityRenderWithDepthNoShadowLayer
        {
            get
            {
#if ADVANCE_RENDER
                int layer = LayerMask.NameToLayer("UnityRenderWithDepthNoShadow");
                if (layer < 0)
                { Debug.LogError("please add a layer => UnityRenderWithDepthNoShadow"); return 0; }
                return layer;
#else
                return LayerMask.NameToLayer("Default");
#endif
            }
        }

        public static int UnityRenderOnTopLayer//remove
        {
            get
            {
                int layer = LayerMask.NameToLayer("UnityRenderOnTop");
                if (layer < 0)
                { Debug.LogError("please add a layer => UnityRenderOnTop"); return 0; }
                return layer;
            }
        }

        public static int UnityRenderOnTopNoShadowLayer
        {
            get
            {
                int layer = LayerMask.NameToLayer("UnityRenderOnTopNoShadow");
                if (layer < 0)
                { Debug.LogError("please add a layer => UnityRenderOnTopNoShadow"); return 0; }
                return layer;
            }
        }

        public static int MRReconstructObjectLayer
        {
            get
            {
                int layer = LayerMask.NameToLayer("MRReconstructObject");
                if (layer < 0)
                { Debug.LogError("please add a layer => MRReconstructObject"); return 0; }
                return layer;
            }
        }

        public void AddUnityrenderWithDepth(Transform t)
        {
            if (unityrenderWithDepthObject.IndexOf(t) < 0)
            {
                unityrenderWithDepthObject.Add(t);
                MyHelpLayer.SetSceneLayer(t, UnityRenderWithDepthLayer);
            }
        }

        public void RemoveUnityrenderWithDepth(Transform t)
        {
            if (unityrenderWithDepthObject.IndexOf(t) >= 0)
                unityrenderWithDepthObject.Remove(t);
        }

        public void SetUnityrenderWithDepthToTopLayer()
        {
            foreach (Transform t in unityrenderWithDepthObject)
                MyHelpLayer.SetSceneLayer(t, UnityRenderOnTopLayer);
        }

        public void RecoverUnityrenderWithDepthLayer()
        {
            foreach (Transform t in unityrenderWithDepthObject)
                MyHelpLayer.SetSceneLayer(t, UnityRenderWithDepthLayer);
        }

        void Start()
        {
            VRCamera.gameObject.AddComponent<HTC.UnityPlugin.Vive3DSoundPerception.Vive3DSPAudioListener>();
            DualCameraR.depthTextureMode = DualCameraL.depthTextureMode = DepthTextureMode.None;

#if ADVANCE_RENDER
            advanceRender.gameObject.SetActive(true);
#endif
        }

        int frameCount, frameCountShow;
        float frameTimeCount;

        private void Update()
        {
            if (showFPS)
            {
                GameManager.Instance.eyeInfoText.text = "FPS : " + frameCountShow + Environment.NewLine +
                 // "cameraFPS : " + Vive.Plugin.SR.ViveSR_DualCameraImageCapature.DCameraFPSShow + Environment.NewLine +
                 "RealDistortedFPS : " + Vive.Plugin.SR.ViveSR_DualCameraImageRenderer.RealDistortedFPS + Environment.NewLine
                ;

                frameTimeCount -= Time.deltaTime;
                frameCount++;
                if (frameTimeCount < 0)
                {
                    frameCountShow = frameCount;
                    frameTimeCount = 1;
                    frameCount = 0;
                }
            }
        }

        public void SetRobotLightBeam(RobotController robotController)
        {
#if ADVANCE_RENDER
            advanceRender.SetDepthToRobotLightBeam(robotController);
#endif
        }

        public void SetDirectionalLight(Transform fishHidingWall)
        {
            //Set directional light        
            //Vector3 lightDir = fishHidingWall.forward;
            //Quaternion DLightRot = Quaternion.AngleAxis(80, Vector3.right) * Quaternion.LookRotation(lightDir);
            //Vector3 dir = DLightRot * Vector3.forward;
            //if (Vector3.Dot(Vector3.up, dir) > 0)
            //    DLightRot = Quaternion.AngleAxis(-80, Vector3.right) * Quaternion.LookRotation(lightDir);
            //shadowCastDirLight.transform.rotation = DLightRot;

            Vector3 lightDir = fishHidingWall.position - Vector3.up * 5f;
            shadowCastDirLight.transform.forward = lightDir.normalized;
        }

        public void InitRenderSystem()
        {
            //Get realworld camera image                 
            DualCameraL.clearFlags = CameraClearFlags.SolidColor;
            DualCameraL.backgroundColor = Color.clear;
            _dualCameraLRT = DualCameraL.gameObject.AddComponent<CopyCameraImage>();
            _dualCameraLRT.rtRefreshCallback = LDualCamRTRefreshCallback;

            DualCameraR.clearFlags = CameraClearFlags.SolidColor;
            DualCameraR.backgroundColor = Color.clear;
            _dualCameraRRT = DualCameraR.gameObject.AddComponent<CopyCameraImage>();
            _dualCameraRRT.rtRefreshCallback = RDualCamRTRefreshCallback;

            //Create None DepthTest Camera, which is clear the depth buffer ,and render mesh on screen top.
            _createRenderOnTopCamera();

#if ADVANCE_RENDER
            advanceRender.InitRenderSystem(_dualCameraLRT, _dualCameraRRT);
#endif

            foreach (Transform t in unityrenderWithDepthObject)
                MyHelpLayer.SetSceneLayer(t, ARRender.UnityRenderWithDepthLayer);
            foreach (Transform t in unityrenderWithDepthNoShadowObject)
                MyHelpLayer.SetSceneLayer(t, ARRender.UnityRenderWithDepthNoShadowLayer);
        }

        void LDualCamRTRefreshCallback(CopyCameraImage copyCameraImage, RenderTexture OnRenderImageDest)
        {
            //Update the real world camera image to shader
            GameManager.Instance.UpdateLDualCamRT(copyCameraImage.GetRT());

#if !ADVANCE_RENDER
            Graphics.Blit(copyCameraImage.GetRT(), OnRenderImageDest);
#endif
        }

        void RDualCamRTRefreshCallback(CopyCameraImage copyCameraImage, RenderTexture OnRenderImageDest)
        {
            //Update the real world camera image to shader
            GameManager.Instance.UpdateRDualCamRT(copyCameraImage.GetRT());

#if !ADVANCE_RENDER
            Graphics.Blit(copyCameraImage.GetRT(), OnRenderImageDest);
#endif
        }

        void _createRenderOnTopCamera()
        {
            //Create None DepthTest Camera, which is clear the depth buffer ,and render mesh on screen top.
            GameObject camObj = new GameObject(VRCamera.name + "_RenderOnTop +" + _UnityRenderOnTopCamOrder);
            camObj.transform.parent = null;
            Vive.Plugin.SR.ViveSR_HMDCameraShifter shifter = camObj.AddComponent<Vive.Plugin.SR.ViveSR_HMDCameraShifter>();

            GameObject eyeObj = new GameObject("eye");
            eyeObj.transform.parent = camObj.transform;
            eyeObj.transform.localPosition = Vector3.zero;
            eyeObj.transform.localRotation = Quaternion.identity;
            _UnityRenderOnTopCamera = eyeObj.AddComponent<Camera>();
            _UnityRenderOnTopCamera.CopyFrom(VRCamera);
            _UnityRenderOnTopCamera.cullingMask = (1 << UnityRenderOnTopLayer | 1 << UnityRenderOnTopNoShadowLayer);
            _UnityRenderOnTopCamera.clearFlags = CameraClearFlags.Depth;
            _UnityRenderOnTopCamera.depth = VRCamera.depth + _UnityRenderOnTopCamOrder;
            //shifter.TargetCamera = _UnityRenderOnTopCamera;
            bool setTargetCam = MyReflection.SetMemberVariable(shifter, "TargetCamera", _UnityRenderOnTopCamera);
            if (!setTargetCam)
                Debug.LogError("shifter.TargetCamera set fail..._UnityRenderOnTopCamera");

#if ADVANCE_RENDER
            VRCamera.cullingMask = MyHelpLayer.RemoveMaskLayer(VRCamera.cullingMask, UnityRenderOnTopLayer);
            VRCamera.cullingMask = MyHelpLayer.RemoveMaskLayer(VRCamera.cullingMask, UnityRenderOnTopNoShadowLayer);
#else
            //We render AR top model on both camera(VRCamera&UnityRenderOnTopCamera), to produce shadow on VRCamera,
            //and render on top layer to prevent culled by scanned mesh.
            //So, we don't remove VRCamera's UnityRenderOnTop Layer
#endif
        }
    }

}