#define ADVANCE_RENDERxx

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
#if ADVANCE_RENDER
        public AdvanceRender advanceRender;
#endif
        public Light shadowCastDirLight;

        [SerializeField]
        private List<Transform> unityrenderWithDepthObject;
        [SerializeField]
        private List<Transform> unityrenderWithDepthNoShadowObject;

        CopyCameraImage _dualCameraLRT, _dualCameraRRT;
        [HideInInspector] public Camera _UnityRenderOnTopCameraLL, _UnityRenderOnTopCameraRR;
        const int _UnityRenderOnTopCamOrder = 1;

        public static int ScanLiveMeshLayer
        {
            get
            {
                if (ARRender.ADVANCE_RENDER)
                {
                    int layer = LayerMask.NameToLayer("ScanLiveMesh");
                    if (layer < 0)
                    { Debug.LogError("please add a layer => ScanLiveMesh"); return 0; }
                    return layer;
                }
                else
                {
                    return LayerMask.NameToLayer("Default");
                }
            }
        }

        public static int MRCollisionFloorLayer
        {
            get
            {
                if (ARRender.ADVANCE_RENDER)
                {
                    int layer = LayerMask.NameToLayer("MRCollisionFloor");
                    if (layer < 0)
                    { Debug.LogError("please add a layer => MRCollisionFloor"); return 0; }
                    return layer;
                }
                else
                {
                    return LayerMask.NameToLayer("Default");
                }
            }
        }

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

        Transform _VRCameraRTran;
        bool _fixVRCamera;
        void _checkVRCamera()
        {
            if (_fixVRCamera)
                return;
            Vive.Plugin.SR.ViveSR_VirtualCameraRig rig = GameObject.FindObjectOfType<Vive.Plugin.SR.ViveSR_VirtualCameraRig>();
            if (rig != null)
            {
                _fixVRCamera = true;

                MyHelpNode.FindTransform(rig.transform, "right", out _VRCameraRTran, true, true);
                if (_VRCameraRTran != null)
                {
                    if (_VRCameraRTran.GetComponent<BackGroundSound>() != null)
                        Destroy(_VRCameraRTran.GetComponent<BackGroundSound>());//.enabled = false;
                    if (_VRCameraRTran.GetComponent<AudioListener>() != null)
                        Destroy(_VRCameraRTran.GetComponent<AudioListener>());//.enabled = false;
                    if (_VRCameraRTran.GetComponent<HTC.UnityPlugin.Vive3DSoundPerception.Vive3DSPAudioListener>() != null)
                        Destroy(_VRCameraRTran.GetComponent<HTC.UnityPlugin.Vive3DSoundPerception.Vive3DSPAudioListener>());//.enabled = false;
                    AudioSource[] ass = _VRCameraRTran.GetComponents<AudioSource>();
                    for (int a = 0; a < ass.Length; a++)
                        Destroy(ass[a]);
                }

                Debug.LogWarning("[ARRender] [_checkVRCamera] : already fix right VRCamera");
            }
        }

        public void VRCameraRemoveLayer(int layer)
        {
            VRCamera.cullingMask = MyHelpLayer.RemoveMaskLayer(VRCamera.cullingMask, layer);
            Transform _VRCameraRTran;
            MyHelpNode.FindTransform(VRCamera.transform.parent, "right", out _VRCameraRTran, true, true);
            Camera cameraRight = _VRCameraRTran.GetComponent<Camera>();
            cameraRight.cullingMask = MyHelpLayer.RemoveMaskLayer(cameraRight.cullingMask, layer);
        }

        private void OnDestroy()
        {
            Camera.onPreRender -= PreRender;
        }

        void Start()
        {
            Camera.onPreRender += PreRender;
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

            //if (renderOnTopCameraShifter != null)
            //{
            //    Vive.Plugin.SR.ViveSR_HMDCameraShifter mainShifter = VRCamera.transform.parent.GetComponent<Vive.Plugin.SR.ViveSR_HMDCameraShifter>();
            //    renderOnTopCameraShifter.CameraShift = mainShifter.CameraShift;
            //}

            _checkVRCamera();
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

            //Vector3 lightDir = fishHidingWall.position - VRCamera.transform.position + Vector3.up * 5f;
            //shadowCastDirLight.transform.forward = -lightDir.normalized;

            Vector3 lightDir = fishHidingWall.position - VRCamera.transform.position;
            lightDir.y = 0;
            lightDir.Normalize();
            Vector3 oppoPos = fishHidingWall.position + lightDir * -4f + Vector3.up * 5f;
            shadowCastDirLight.transform.forward = fishHidingWall.position - oppoPos;
        }

        public void SetDirectionalLightBack(Transform fishHidingWall)
        {
            Vector3 lightDir = fishHidingWall.position - VRCamera.transform.position;
            lightDir.y = 0;
            lightDir.Normalize();
            Vector3 oppoPos = fishHidingWall.position + lightDir * 10f + Vector3.up * 5f;
            shadowCastDirLight.transform.forward = fishHidingWall.position - oppoPos;
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
                if (t != null)
                    MyHelpLayer.SetSceneLayer(t, ARRender.UnityRenderWithDepthLayer);
            foreach (Transform t in unityrenderWithDepthNoShadowObject)
                if (t != null)
                    MyHelpLayer.SetSceneLayer(t, ARRender.UnityRenderWithDepthNoShadowLayer);
        }

        void LDualCamRTRefreshCallback(RenderTexture copyCameraImage, RenderTexture OnRenderImageDest)
        {
            //Update the real world camera image to shader
            GameManager.Instance.UpdateLDualCamRT(copyCameraImage);

#if !ADVANCE_RENDER
            Graphics.Blit(copyCameraImage, OnRenderImageDest);
#endif
        }

        void RDualCamRTRefreshCallback(RenderTexture copyCameraImage, RenderTexture OnRenderImageDest)
        {
            //Update the real world camera image to shader
            GameManager.Instance.UpdateRDualCamRT(copyCameraImage);

#if !ADVANCE_RENDER
            Graphics.Blit(copyCameraImage, OnRenderImageDest);
#endif
        }

        //Vive.Plugin.SR.ViveSR_HMDCameraShifter renderOnTopCameraShifter;
        void _createRenderOnTopCamera()
        {
            Vive.Plugin.SR.ViveSR_VirtualCameraRig rig = GameObject.FindObjectOfType<Vive.Plugin.SR.ViveSR_VirtualCameraRig>();

            if (rig == null)
                Debug.LogError("There is no ViveSR_VirtualCameraRig in scene");

            //Create None DepthTest Camera, which is clear the depth buffer ,and render mesh on screen top.
            GameObject camObj = new GameObject(VRCamera.name + "_RenderOnTop +" + _UnityRenderOnTopCamOrder);
            camObj.transform.parent = null;
            //renderOnTopCameraShifter = camObj.AddComponent<Vive.Plugin.SR.ViveSR_HMDCameraShifter>();

            GameObject eyeObjLL = new GameObject("eyeL");
            eyeObjLL.transform.parent = camObj.transform;
            eyeObjLL.transform.localPosition = Vector3.zero;
            eyeObjLL.transform.localRotation = Quaternion.identity;
            _UnityRenderOnTopCameraLL = eyeObjLL.AddComponent<Camera>();
            _UnityRenderOnTopCameraLL.CopyFrom(rig.CameraLeft);
            _UnityRenderOnTopCameraLL.cullingMask = (1 << UnityRenderOnTopLayer | 1 << UnityRenderOnTopNoShadowLayer);
            _UnityRenderOnTopCameraLL.clearFlags = CameraClearFlags.Depth;
            _UnityRenderOnTopCameraLL.depth = VRCamera.depth + _UnityRenderOnTopCamOrder;
            _UnityRenderOnTopCameraLL.targetTexture = rig.CameraLeft.targetTexture;//_RenderOnTopCameraRTL;//assign render texture to control FOV
            _UnityRenderOnTopCameraLL.stereoTargetEye = StereoTargetEyeMask.Left;//Use 'Both' for unity_StereoEyeIndex in shader is worked.
            //renderOnTopCameraShifter.TargetCamera = _UnityRenderOnTopCamera;
            _UnityRenderOnTopCameraLL.fieldOfView = rig.CameraLeft.fieldOfView;

            GameObject eyeObjRR = new GameObject("eyeR");
            eyeObjRR.transform.parent = camObj.transform;
            eyeObjRR.transform.localPosition = Vector3.zero;
            eyeObjRR.transform.localRotation = Quaternion.identity;
            _UnityRenderOnTopCameraRR = eyeObjRR.AddComponent<Camera>();
            _UnityRenderOnTopCameraRR.CopyFrom(rig.CameraRight);
            _UnityRenderOnTopCameraRR.cullingMask = (1 << UnityRenderOnTopLayer | 1 << UnityRenderOnTopNoShadowLayer);
            _UnityRenderOnTopCameraRR.clearFlags = CameraClearFlags.Depth;
            _UnityRenderOnTopCameraRR.depth = VRCamera.depth + _UnityRenderOnTopCamOrder;
            _UnityRenderOnTopCameraRR.targetTexture = rig.CameraRight.targetTexture; //_RenderOnTopCameraRTR;//assign render texture to control FOV
            _UnityRenderOnTopCameraRR.stereoTargetEye = StereoTargetEyeMask.Right;//Use 'Both' for unity_StereoEyeIndex in shader is worked.
            _UnityRenderOnTopCameraRR.fieldOfView = rig.CameraRight.fieldOfView;

#if ADVANCE_RENDER
            VRCamera.cullingMask = MyHelpLayer.RemoveMaskLayer(VRCamera.cullingMask, UnityRenderOnTopLayer);
            VRCamera.cullingMask = MyHelpLayer.RemoveMaskLayer(VRCamera.cullingMask, UnityRenderOnTopNoShadowLayer);
#else
            //We render AR top model on both camera(VRCamera&UnityRenderOnTopCamera), to produce shadow on VRCamera,
            //and render on top layer to prevent culled by scanned mesh.
            //So, we don't remove VRCamera's UnityRenderOnTop Layer
#endif
        }

        private void PreRender(Camera eye)
        {
            UpdateRenderTopCamera();
        }

        public void UpdateRenderTopCamera()
        {
            if (_UnityRenderOnTopCameraLL == null)
                return;
            Vive.Plugin.SR.ViveSR_VirtualCameraRig rig = VRCamera.transform.parent.GetComponent<Vive.Plugin.SR.ViveSR_VirtualCameraRig>();

            Vector3 positionLeft = rig.CameraLeft.transform.localPosition;
            Quaternion rotationLeft = rig.CameraLeft.transform.localRotation;
            Vector3 positionRight = rig.CameraRight.transform.localPosition;
            Quaternion rotationRight = rig.CameraRight.transform.localRotation;

            _UnityRenderOnTopCameraLL.transform.localPosition = positionLeft;
            _UnityRenderOnTopCameraLL.transform.localRotation = rotationLeft;

            if (_VRCameraRTran != null)
            {
                _UnityRenderOnTopCameraRR.transform.localPosition = positionRight;
                _UnityRenderOnTopCameraRR.transform.localRotation = rotationRight;
            }
        }
    }


}