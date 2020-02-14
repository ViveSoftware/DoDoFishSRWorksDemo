//#define USE_MERGE_DEPTH

//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Demo
//{
//    public class AdvanceRender : MonoBehaviour
//    {
//        [Header("Camera filter pack----------------------------------------")]
//        public Shader dilateMaskShader, genBinaryMaskShader, refineGBufferShader;

//        [Header("Culling----------------------------------------")]
//        public Shader ScanLiveMeshShaderDepthNormal;
//        public Material CopyCameraDepthColorMaterial, RenderWithARDepthMaterial, MergeDepthMat, CopyCameraDepthMat;
//        CameraRenderRT _renderLivemeshDepthN;

//        [Header("Shadow----------------------------------------")]        
//        public Material screenShadowBlit,/* shadowBlur, */blurHMaterial, blurVMaterial;
//        public GameObject shadowCastCameraPrefab;
//        public LayerMask shadowCastMask = 1;

//        [Header("Light----------------------------------------")]
//        public Material BlitLightMapMaterial;
//        public Material DeferredPointLightMapMaterial, DeferredSpotLightMapMaterial;
//        public List<GameObject> PointLightObjList = new List<GameObject>();
//        public List<GameObject> SpotLightObjList = new List<GameObject>();
//        DeferredLightMap deferredLightMap;

//        [Header("Unity Render Object----------------------------------------")]
//        public GameObject writeDepthCameraPrefab;

//        Camera _writeDepthCamera;
//        Camera _UnityRenderWithDepthCamera;
//        WriteDepthPerspective _writeDepth;
//        const int _writeDepthCamLOrder = 4;
//        const int _UnityRenderWithDepthCamOrder = 6;

//        [Header("SoftEdgeWeight----------------------------------------")]
//        public Material softEdgeWeightMaterial;


//        void Update()
//        {
//            RenderSystemUpdate();
//        }

//        void RenderSystemUpdate()
//        {
//            if (deferredLightMap != null)
//            {
//                deferredLightMap.ClearAllLights();
//                foreach (GameObject lightObj in PointLightObjList)
//                {
//                    if (!lightObj.activeSelf)
//                        continue;

//                    {
//                        Light[] lights = lightObj.GetComponentsInChildren<Light>();
//                        foreach (Light l in lights)
//                        {
//                            deferredLightMap.AddPointLight(l);
//                        }
//                    }

//                    ParticleLights[] particleLights = lightObj.GetComponentsInChildren<ParticleLights>();
//                    foreach (ParticleLights pl in particleLights)
//                    {
//                        if (pl.particleLightNode == null || !pl.particleLightNode.activeSelf)
//                            continue;
//                        Light[] lights = pl.particleLightNode.GetComponentsInChildren<Light>();
//                        foreach (Light l in lights)
//                        {
//                            deferredLightMap.AddPointLight(l);
//                        }
//                    }

//                }
//                foreach (GameObject lightObj in SpotLightObjList)
//                {
//                    if (!lightObj.activeInHierarchy)
//                        continue;
//                    Light[] lights = lightObj.GetComponentsInChildren<Light>();
//                    foreach (Light l in lights)
//                    {
//                        deferredLightMap.AddSpotLight(l);
//                    }
//                }
//            }

//            //Alpha blend object render with depth
//            if (_UnityRenderWithDepthCamera == null)
//            {
//                GameObject camObj = new GameObject(ARRender.Instance.VRCamera.name + "_WithDetpth +" + _UnityRenderWithDepthCamOrder);
//                camObj.transform.parent = null;
//                Vive.Plugin.SR.ViveSR_HMDCameraShifter shifter = camObj.AddComponent<Vive.Plugin.SR.ViveSR_HMDCameraShifter>();

//                GameObject eyeObj = new GameObject("eye");
//                eyeObj.transform.parent = camObj.transform;
//                eyeObj.transform.localPosition = Vector3.zero;
//                eyeObj.transform.localRotation = Quaternion.identity;
//                _UnityRenderWithDepthCamera = eyeObj.AddComponent<Camera>();
//                _UnityRenderWithDepthCamera.CopyFrom(ARRender.Instance.VRCamera);
//                _UnityRenderWithDepthCamera.cullingMask = 1 << ARRender.UnityRenderWithDepthLayer | 1 << ARRender.UnityRenderWithDepthNoShadowLayer;
//                _UnityRenderWithDepthCamera.clearFlags = CameraClearFlags.Nothing;
//                _UnityRenderWithDepthCamera.depth = ARRender.Instance.VRCamera.depth + _UnityRenderWithDepthCamOrder;
//                ARRender.Instance.VRCamera.cullingMask =
//                    MyHelpLayer.RemoveMaskLayer(ARRender.Instance.VRCamera.cullingMask, ARRender.UnityRenderWithDepthLayer);
//                ARRender.Instance.VRCamera.cullingMask =
//                    MyHelpLayer.RemoveMaskLayer(ARRender.Instance.VRCamera.cullingMask, ARRender.UnityRenderWithDepthNoShadowLayer);
//                shifter.TargetCamera = _UnityRenderWithDepthCamera;
//            }
//        }

//        MonoBehaviour _createCameraFilter<T>()
//        {
//            MonoBehaviour cf = ARRender.Instance.VRCamera.gameObject.AddComponent(typeof(T)) as MonoBehaviour;
//            cf.enabled = true;
//            return cf;
//        }

//        public void InitRenderSystem(CopyCameraImage _dualCameraLRT, CopyCameraImage _dualCameraRRT)
//        {
//            //Set reconstruct mesh to ScanLiveMeshLayer if exist
//            Material reconMeshMat = new Material(Shader.Find("Unlit/Color"));
//            reconMeshMat.color = Color.white * 0.5f;
//            MeshRenderer[] reconMeshes = SRWorkControl.Instance.GetReconstructStaticMeshes();
//            if (reconMeshes != null)
//            {
//                foreach (MeshRenderer r in reconMeshes)
//                {
//                    r.gameObject.layer = ScanLiveMeshLayer;
//                    r.gameObject.GetComponent<MeshRenderer>().sharedMaterial = reconMeshMat;
//                    r.GetComponent<MeshFilter>().mesh.RecalculateNormals();//must RecalculateNormals since original normal is wrong
//                }
//            }

//            //-------------------------------------------------------------------------------------------------
//            //Add culling processor
//            //-------------------------------------------------------------------------------------------------
//            _renderLivemeshDepthN = ARRender.Instance.VRCamera.gameObject.AddComponent<CameraRenderRT>();
//            _renderLivemeshDepthN.RTType = CameraRenderRT.RTTYPE.FLOAT4;
//            _renderLivemeshDepthN.RenderShader = ScanLiveMeshShaderDepthNormal;
//            _renderLivemeshDepthN.cullMask = 1 << ScanLiveMeshLayer;
//            _renderLivemeshDepthN.clearColor = Color.white;
//            _renderLivemeshDepthN.blurHMaterial = new Material(blurHMaterial);
//            _renderLivemeshDepthN.blurVMaterial = new Material(blurVMaterial);
//            _renderLivemeshDepthN.BlurFactor = 3f;// 2.7f;//0 turn off blur
//            _renderLivemeshDepthN.Init();

//            //VRCamera.cullingMask = VRCamera.cullingMask & ~(1 << ScanLiveMeshLayer);//Not render 'LiveMesh' in vr camera
//            ARRender.Instance.VRCamera.cullingMask = MyHelpLayer.RemoveMaskLayer(ARRender.Instance.VRCamera.cullingMask, ScanLiveMeshLayer);
//            ARRender.Instance.VRCamera.clearFlags = CameraClearFlags.Color;
//            ARRender.Instance.VRCamera.backgroundColor = new Color(0, 0, 0, 0);

//            //Add dilation for later pass 'DownSample' not sample outter edge's black color.
//            Dilation dilation = ARRender.Instance.VRCamera.gameObject.AddComponent<Dilation>();
//            dilation.dilateCount = 2;
//            dilation.dilateMaskShader = dilateMaskShader;
//            dilation.genBinaryMaskShader = genBinaryMaskShader;
//            dilation.refineGBufferShader = refineGBufferShader;

//            //Add down sample for matching the low resolution see through camera.
//            RTDownSample downsample = _createCameraFilter<RTDownSample>() as RTDownSample;
//            downsample.DownScale = 3;

//            //Render block pattern for simulate noise for matching low resolution see through camera.
//            CamFilterDrawBlocks drawblocks = _createCameraFilter<CamFilterDrawBlocks>() as CamFilterDrawBlocks;
//            drawblocks.Fade = 0.003f;

//            CopyCameraDepthColor vrdepth = ARRender.Instance.VRCamera.gameObject.AddComponent<CopyCameraDepthColor>();
//            vrdepth.CopyCameraDepthColorMaterial = CopyCameraDepthColorMaterial;

//#if USE_MERGE_DEPTH
//            MergeDepth mergeDepth = ARRender.Instance.VRCamera.gameObject.AddComponent<MergeDepth>();
//            mergeDepth.cameraRenderRT = _renderLivemeshDepthN.RT;
//            mergeDepth.mergeDepthMat = MergeDepthMat;
//            mergeDepth.copyCameraColorDepth = vrdepth;
//#endif

//            SoftEdgeWeight softEdgeWeight = ARRender.Instance.VRCamera.gameObject.AddComponent<SoftEdgeWeight>();
//            softEdgeWeight.renderDepth = _renderLivemeshDepthN;
//            softEdgeWeight.SoftEdgeWeightMaterial = softEdgeWeightMaterial;
//            softEdgeWeight.factor = 2f;

//            RenderWithARDepth renderWithARDepth = ARRender.Instance.VRCamera.gameObject.AddComponent<RenderWithARDepth>();
//            renderWithARDepth.RenderWithARDepthMaterial = RenderWithARDepthMaterial;
//            renderWithARDepth.MRDepthNormal = _renderLivemeshDepthN.RT;
//            renderWithARDepth.VRCamera = vrdepth;
//            renderWithARDepth.softCullLength = 0.0001f;
//            renderWithARDepth.glowAmount = 0.018f;
//            renderWithARDepth.coefAmount = 0.062f;
//            renderWithARDepth.seeThroughL = _dualCameraLRT;
//            renderWithARDepth.seeThroughR = _dualCameraRRT;
//            renderWithARDepth.softEdgeWeight = softEdgeWeight;
//            renderWithARDepth.CullingBaise = 0.000017f;//1.7e-05

//            //-------------------------------------------------------------------------------------------------
//            //Add shadow processor
//            //-------------------------------------------------------------------------------------------------
//            ARRender.Instance.shadowCastDirLight.transform.position = ARRender.Instance.VRCamera.transform.position;

//            ShadowRecieve shadowRecieve = ARRender.Instance.VRCamera.gameObject.AddComponent<ShadowRecieve>();
//            shadowRecieve.ScreenMapScale = 0.5f;
//            shadowRecieve.shadowLight = ARRender.Instance.shadowCastDirLight;
//            shadowRecieve.screenShadowBlit = screenShadowBlit;
//            // shadowRecieve.blurMaterial = shadowBlur;
//            shadowRecieve.blurHMaterial = new Material(blurHMaterial);
//            shadowRecieve.blurVMaterial = new Material(blurVMaterial);
//            shadowRecieve.BlurFactor = 3;
//            shadowRecieve.ShadowFactor = 10;            
//            shadowRecieve.VRDepthColor = vrdepth;//set VR scene depth for screen shadow culling (no need recieve mesh's depth)

//            shadowRecieve.cameraDepthN = _renderLivemeshDepthN.RT;           
//            //shadowRecieve.recieveShadowLayer = (1 << ScanLiveMeshLayer);

//            GameObject shadowCastCameraObj = Instantiate(shadowCastCameraPrefab);
//            shadowCastCameraObj.transform.parent = ARRender.Instance.shadowCastDirLight.transform;
//            ShadowCastCamera shadowCastCamera = shadowCastCameraObj.GetComponent<ShadowCastCamera>();
//            shadowCastCamera.GetComponent<Camera>().orthographicSize = 4; //4 is enough
//            shadowCastCamera.shadowMapSize = 400;

//            //cast shadow for default layer
//            shadowCastCamera.shadowRenderMask = shadowCastMask;

//            //shadowRecieve.shadowColor = Color.white * 50f / 255f;
//            shadowRecieve.bais = 0;
//            shadowRecieve.shadowBlitBais = 0;

//            //-------------------------------------------------------------------------------------------------
//            //Add point light processor
//            //-------------------------------------------------------------------------------------------------
//            deferredLightMap = ARRender.Instance.VRCamera.gameObject.AddComponent<DeferredLightMap>();
//            deferredLightMap.DeferredPointLightMapMaterial = DeferredPointLightMapMaterial;
//            deferredLightMap.DeferredSpotLightMapMaterial = DeferredSpotLightMapMaterial;
//            deferredLightMap.BlitLightMapMaterial = BlitLightMapMaterial;
//            deferredLightMap.LightMapFactor = 0.2f;

//            //deferredLightMap.mergeDepth = mergeDepth;
//            deferredLightMap.cameraRenderDepthNormal = _renderLivemeshDepthN.RT;//not use merge depth, since game objects use unity lighting.

//            deferredLightMap.TurnOffUnityLight = false;//Dont' turn off unity light, because current time my deferred lighting is not concern normal, and I need unity lighting to render object.


//            //-------------------------------------------------------------------------------------------------
//            //write Depth Camera and render alpha object
//            //-------------------------------------------------------------------------------------------------
//            GameObject writeDepthCameraRoot = new GameObject(ARRender.Instance.VRCamera.name + "_writeDepth +" + _writeDepthCamLOrder);
//            writeDepthCameraRoot.transform.parent = null;
//            Vive.Plugin.SR.ViveSR_HMDCameraShifter shifter = writeDepthCameraRoot.AddComponent<Vive.Plugin.SR.ViveSR_HMDCameraShifter>();
//            writeDepthCameraRoot.transform.position = Vector3.zero;
//            writeDepthCameraRoot.transform.rotation = Quaternion.identity;

//            GameObject writeDepthCameraObj = Instantiate(writeDepthCameraPrefab);
//            writeDepthCameraObj.transform.parent = writeDepthCameraRoot.transform;
//            writeDepthCameraObj.transform.localPosition = Vector3.zero;
//            writeDepthCameraObj.transform.localRotation = Quaternion.identity;
//            _writeDepthCamera = writeDepthCameraObj.GetComponent<Camera>();
//            _writeDepthCamera.depth = ARRender.Instance.VRCamera.depth + _writeDepthCamLOrder;
//            _writeDepthCamera.name = _writeDepthCamera.name + " +" + _writeDepthCamLOrder;
//            _writeDepth = _writeDepthCamera.GetComponent<WriteDepthPerspective>();
//            _writeDepth.mainCam = ARRender.Instance.VRCamera;
//            _writeDepth.addShiftR = 0.00145f;//0.00807f;
//            _writeDepth.addShiftUP = -0.00005f; ;// -0.0001f;
//            _writeDepth.Init(0);
//            //_writeDepth.IsRenderRightEye = null;
//            shifter.TargetCamera = _writeDepthCamera;
//#if USE_MERGE_DEPTH
//            _writeDepth.mergeDepth = mergeDepth;
//#else
//            _writeDepth.cameraDepth = renderLiveMeshDepthN.RT;
//#endif

//            ARRender.Instance.shadowCastDirLight.shadowBias = 0.02f;
//        }


//        public void SetDepthToRobotLightBeam(RobotController robotController)
//        {
//            //Lightbeam lightBeam = robotController.GetComponentInChildren<Lightbeam>();
//            Transform lightBeam;
//            MyHelpNode.FindTransform(robotController.transform, "Lightbeam2D", out lightBeam);
//            Renderer lightBeamRender = lightBeam.GetComponent<Renderer>();

//            lightBeamRender.material.SetTexture("_MyDepth01Texture", _renderLivemeshDepthN.RT);
//        }


//        public CopyCameraDepthColor GetVRCameraDepthColor()
//        {
//            return ARRender.Instance.VRCamera.gameObject.GetComponent<CopyCameraDepthColor>();
//        }
//    }
//}