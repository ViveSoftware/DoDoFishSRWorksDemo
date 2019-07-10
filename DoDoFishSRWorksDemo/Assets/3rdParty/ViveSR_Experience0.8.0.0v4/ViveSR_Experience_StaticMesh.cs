using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Vive.Plugin.SR.Experience
{
    public enum ShowMode
    {
        All = 0,
        Horizon,
        LargestHorizon,
        NearestHorizon,
        FurthestHorizon,
        AllVertical,
        LargestVertical,
        NearestVertical,
        FurthestVertical,
        None,
        NumOfModes // always the last
    }

    public class ViveSR_Experience_StaticMesh : MonoBehaviour
    {
        public ViveSR_Experience_Recons3DAssetLoader ReconsLoader;
        public ViveSR_Experience_Recons3DAssetMultiLoader SemanticMeshLoader = null;

        private bool _modelIsLoading = false;
        private bool _semanticMeshIsLoading = false;
        private bool _semanticMeshIsLoaded = false;
        private bool _semanticMeshIsExporting = false;
        string MeshName = "Model";
        string mesh_path;
        string cld_path;
        string reconsRootDir = "Recons3DAsset/";
        string semanticObj_dir = "SceneUnderstanding/";
        string semanticObj_dirPath;

        [Header("LoadedMesh")]
        public GameObject texturedMesh;
        public GameObject collisionMesh;
        public ViveSR_StaticColliderPool cldPool { get; private set; }
        [SerializeField] MeshRenderer[] modelRenderers;

        public List<GameObject> semanticList = new List<GameObject>();
        public List<ViveSR_StaticColliderPool> semanticCldPools = new List<ViveSR_StaticColliderPool>();

        bool wasDynamicMeshOn;

        //public ViveSR_Experience_SwitchMode SwitchModeScript;
        public bool ModelMeshVisible { get; private set; }

        ShowMode _MeshShowMode = ShowMode.None;

        System.Action MeshReadyCallBack, SemanticMeshReadyCallback;
             
        [SerializeField] GameObject HintLocatorPrefab;
        List<GameObject> HintLocators = new List<GameObject>();

        public ShowMode MeshShowMode
        {
            get { return _MeshShowMode; }
        }

        public bool ModelIsLoading
        {
            get { return _modelIsLoading; }
        }

        public bool SemanticMeshIsLoading
        {
            get { return _semanticMeshIsLoading; }
        }

        public bool SemanticMeshIsLoaded
        {
            get { return _semanticMeshIsLoaded; }
        }

        public bool SemanticMeshIsExporting
        {
            get { return _semanticMeshIsExporting; }
        }

        private void Awake()
        {
            mesh_path = reconsRootDir  + MeshName + ".obj";
            cld_path = reconsRootDir  + MeshName + "_cld.obj";
            semanticObj_dirPath = reconsRootDir  + semanticObj_dir;
            ReconsLoader = GetComponent<ViveSR_Experience_Recons3DAssetLoader>();
            SemanticMeshLoader = GetComponent<ViveSR_Experience_Recons3DAssetMultiLoader>();
        }

        public void EnableDepthProcessingAndScanning(bool enable)
        {
            if (enable)
            {
                ViveSR_DualCameraImageCapture.EnableDepthProcess(true);
                ViveSR_RigidReconstruction.StartScanning();
            }
            else
            {
                ViveSR_RigidReconstruction.StopScanning();
                ViveSR_DualCameraImageCapture.EnableDepthProcess(false);
            }
        }

        public bool CheckModelFileExist()
        {
            return File.Exists(mesh_path) && File.Exists(cld_path);
        }

        public bool CheckModelLoaded()
        {
            return ( texturedMesh != null || collisionMesh != null);
        }

        public bool CheckChairExist()
        {
            return GetSegmentationInfo(SceneUnderstandingObjectType.CHAIR).Count > 0;
        }

        public bool CheckSemanticMeshDirExist()
        {
            bool ret = true;
            DirectoryInfo dir = new DirectoryInfo(semanticObj_dirPath);
            if (!dir.Exists)
            {
                Debug.Log(semanticObj_dirPath + " does not exist.");
                ret = false;
            }
            else if (dir.GetFiles("*.xml").Length == 0)
            {
                Debug.Log("There is no scene object in " + semanticObj_dirPath);
                ret = false;
            }
            return ret;
        }

        public void LoadMesh(bool isOn, bool showMesh = false, System.Action beforeLoad = null, System.Action done = null)
        {
            if (ModelIsLoading) return;

            RenderModelMesh(showMesh);

            if (isOn)
            {
                if (!CheckModelLoaded())
                {
                    if (CheckModelFileExist())
                    {
                        _modelIsLoading = true;

                        if (beforeLoad!= null) beforeLoad();

                        if (done != null) MeshReadyCallBack = done;

                        //Load here.
                        texturedMesh = ReconsLoader.LoadMeshObj(mesh_path);
                        collisionMesh = ReconsLoader.LoadColliderObj(cld_path);
                        StartCoroutine(waitForMeshLoad());
                    }
                    else
                    {
                        Debug.LogWarning("Scene files not found. Please rescan the scene!");
                        if (done != null) done();
                    }
                }
                else
                {
                    ActivateModelMesh(true);
                    if (done != null) done();
                }
            }
            else
            {
                ActivateModelMesh(false);
            }
        }

        public void ActivateSemanticMesh(bool active)
        {
            foreach (GameObject go in semanticList) go.SetActive(active);
        }

        public void LoadSemanticMesh(System.Action beforeLoad = null, System.Action done = null)
        {
            if (SemanticMeshIsLoading) return;

            if (SemanticMeshIsLoaded)
                UnloadSemanticMesh();

            _semanticMeshIsLoading = true;

            if (beforeLoad != null) beforeLoad();
            SemanticMeshReadyCallback = done;

            GameObject[] semanticObjs = SemanticMeshLoader.LoadSemanticColliderObjs(semanticObj_dirPath);
            foreach (GameObject go in semanticObjs)
            {
                go.SetActive(false);
                semanticList.Add(go);
            }
            StartCoroutine(waitForSemanticMeshLoad());
        }

        public void LoadSemanticMeshByType(SceneUnderstandingObjectType type, System.Action beforeLoad = null, System.Action done = null)
        {
            if (SemanticMeshIsLoading) return;

            if (SemanticMeshIsLoaded)
                UnloadSemanticMesh();

            _semanticMeshIsLoading = true;

            if (beforeLoad != null) beforeLoad();
            SemanticMeshReadyCallback = done;

            GameObject[] semanticObjs = SemanticMeshLoader.LoadSemanticColliderObjsByType(semanticObj_dirPath, type);
            foreach (GameObject go in semanticObjs)
            {
                go.SetActive(false);
                semanticList.Add(go);
            }
            StartCoroutine(waitForSemanticMeshLoad());
        }

        public void UnloadSemanticMesh(System.Action done = null)
        {
            if (!SemanticMeshIsLoaded) return;

            foreach (GameObject go in semanticList) Destroy(go);
            semanticList.Clear();
            semanticCldPools.Clear();
            _semanticMeshIsLoaded = false;
            if (done != null) done();
        }

        IEnumerator waitForSemanticMeshLoad()
        {
            while (!SemanticMeshLoader.isAllColliderReady)
            {
                yield return new WaitForEndOfFrame();
            }
            SemanticMeshReady();
        }

        private void SemanticMeshReady()
        {
            if (SemanticMeshReadyCallback != null) SemanticMeshReadyCallback();
            _semanticMeshIsLoaded = true;
            _semanticMeshIsLoading = false;
        }

        IEnumerator waitForMeshLoad()
        {
            while (!ReconsLoader.isMeshReady || !ReconsLoader.isColliderReady)
            {
                yield return new WaitForEndOfFrame();
            }
            MeshReady();
        }

        void MeshReady()
        {
            modelRenderers = ReconsLoader.meshRnds;

            int numRnds = ReconsLoader.meshRnds.Length;
            for (int id = 0; id < numRnds; ++id)
            {
                ReconsLoader.meshRnds[id].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            _modelIsLoading = false;

            RenderModelMesh(ModelMeshVisible);
            if (MeshReadyCallBack != null) MeshReadyCallBack();
        }

        public void RenderModelMesh(bool show)
        {
            ModelMeshVisible = show;
            foreach (MeshRenderer renderer in modelRenderers)
            {
                if (renderer != null)
                    renderer.material.shader = Shader.Find(show ? "ViveSR/Unlit, Textured, Shadowed, Stencil" : "ViveSR/MeshCuller, Shadowed, Stencil");
            }
        }

        void ActivateModelMesh(bool on)
        {
            if (CheckModelLoaded())
            {
                texturedMesh.SetActive(on);
                collisionMesh.SetActive(on);
            }
        }

        public void ExportSemanticMesh(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            _semanticMeshIsExporting = true;

            if (!ViveSR_DualCameraImageCapture.DepthProcessing)
                ViveSR_DualCameraImageCapture.EnableDepthProcess(true); // in case depth has been turned off by other process

            ViveSR_SceneUnderstanding.ExportSceneUnderstandingInfo(ViveSR_SceneUnderstanding.SemanticObjDir);
            StartCoroutine(_ExportSemanticMesh(UpdatePercentage, done));
        }

        private IEnumerator _ExportSemanticMesh(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            int percentage = 0;
            int lastPercentage = 0;

            while (percentage < 100)
            {
                percentage = ViveSR_SceneUnderstanding.GetSceneUnderstandingProgress();

                // wait until saving is really processing then we disable depth
                if (lastPercentage == 0 && percentage > 0)
                    ViveSR_DualCameraImageCapture.EnableDepthProcess(false);
                if (UpdatePercentage != null) UpdatePercentage(percentage);
                yield return new WaitForEndOfFrame();
            }

            _semanticMeshIsExporting = false;
            ViveSR_RigidReconstruction.StopScanning();
            if (done != null) done();
        }

        public void ExportModel(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            ViveSR_RigidReconstruction.StopScanning();

            if (!ViveSR_DualCameraImageCapture.DepthProcessing)
                ViveSR_DualCameraImageCapture.EnableDepthProcess(true); // in case depth has been turned off by other process

            StartCoroutine(_ExportModel(UpdatePercentage, done));
        }

        private IEnumerator _ExportModel(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            ViveSR_RigidReconstruction.ExportAdaptiveMesh = true;
            ViveSR_RigidReconstruction.ExportModel(MeshName);

            int percentage = 0;
            int lastPercentage = 0;

            if (CheckModelLoaded())
            {
                Destroy(collisionMesh);
                Destroy(texturedMesh);
                collisionMesh = null;
                texturedMesh = null;
            }

            while (percentage < 100)
            {
                ViveSR_RigidReconstruction.GetExportProgress(ref percentage);

                if(UpdatePercentage != null) UpdatePercentage(percentage);

                // wait until saving is really processing then we disable depth
                if (lastPercentage == 0 && percentage > 0)
                    ViveSR_DualCameraImageCapture.EnableDepthProcess(false);
                lastPercentage = percentage;
                yield return new WaitForEndOfFrame();
            }

            if (done != null) done();
        }

        public void FinishScanAndPreviewMesh(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            if (!ViveSR_DualCameraImageCapture.DepthProcessing)
                ViveSR_DualCameraImageCapture.EnableDepthProcess(true); // in case depth has been turned off by other process

            StartCoroutine(_ExtractPreviewModel(UpdatePercentage, done));
        }

        private IEnumerator _ExtractPreviewModel(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            ViveSR_RigidReconstruction.ExtractModelPreviewData();

            int percentage = 0;
            //int lastPercentage = 0;

            if (CheckModelLoaded())
            {
                Destroy(collisionMesh);
                Destroy(texturedMesh);
                collisionMesh = null;
                texturedMesh = null;
            }

            while (percentage < 100)
            {
                ViveSR_RigidReconstruction.GetExtractPreviewProgress(ref percentage);

                if (UpdatePercentage != null) UpdatePercentage(percentage);
                //lastPercentage = percentage;
                yield return new WaitForEndOfFrame();
            }

            if (done != null) done();
        }

        public void WaitForSemanticCldPool(GameObject go, System.Action done = null)
        {
            StartCoroutine(_CoroutineWaitForSemanticCldPool(go, done));
        }

        IEnumerator _CoroutineWaitForSemanticCldPool(GameObject go, System.Action done = null)
        {
            ViveSR_StaticColliderPool pool = go.GetComponent<ViveSR_StaticColliderPool>();
            while (pool == null)
            {
                pool = go.GetComponent<ViveSR_StaticColliderPool>();
                yield return new WaitForEndOfFrame();
            }
            done();
        }

        public void WaitForCldPool(System.Action done = null)
        {
            StartCoroutine(_WaitForCldPool(done));
        }

        IEnumerator _WaitForCldPool(System.Action done)
        {
            while(cldPool == null)
            {
                cldPool = collisionMesh.GetComponent<ViveSR_StaticColliderPool>();
                yield return new WaitForEndOfFrame();
            }
            if(done != null) done();
        }

        public GameObject[] GetSemanticObjects(SceneUnderstandingObjectType type)
        {
            List<GameObject> objList = new List<GameObject>();

            if (type != SceneUnderstandingObjectType.NONE)
            {
                foreach (GameObject go in semanticList)
                {
                    ViveSR_StaticColliderPool pool = go.GetComponent<ViveSR_StaticColliderPool>();
                    if (pool == null)
                    {
                        Debug.Log("[SemanticSegmentation] Semantic object is not loaded completely.");
                        break;
                    }
                    if (pool.GetSemanticType() == type)
                        objList.Add(go);
                }
            }
            return objList.ToArray();
        }

        public void ShowSemanticColliderByType(SceneUnderstandingObjectType type)
        {
            if (type == SceneUnderstandingObjectType.NONE) return;

            foreach (GameObject go in semanticList)
            {
                WaitForSemanticCldPool(go, () => {
                    ViveSR_StaticColliderPool pool = go.GetComponent<ViveSR_StaticColliderPool>();
                    {
                        if (pool.GetSemanticType() == type)
                            pool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE });
                    }
                });
            }
        }

        public void ShowAllSemanticCollider()
        {
            _SetSemanticColliderVisible(true);
        }

        public void HideAllSemanticCollider()
        {
            _SetSemanticColliderVisible(false);
        }

        private void _SetSemanticColliderVisible(bool isVisible)
        {
            foreach(GameObject go in semanticList)
            {
                if (go == null) continue;

                WaitForSemanticCldPool(go, () => {
                    ViveSR_StaticColliderPool pool = go.GetComponent<ViveSR_StaticColliderPool>();

                    if (isVisible)
                    {
                        pool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE });
                    }
                    else
                    {
                        pool.HideAllColliderRenderers();
                    }
                });
            }
        }

        public void SwitchShowCollider(ShowMode meshShowMode)
        {
            _MeshShowMode = meshShowMode;
            if (collisionMesh == null) return;
            cldPool = collisionMesh.GetComponent<ViveSR_StaticColliderPool>();
            if (cldPool == null) WaitForCldPool(() => _SwitchShowCollider(meshShowMode));
            else _SwitchShowCollider(meshShowMode);
        }

        void _SwitchShowCollider(ShowMode meshShowMode)
        {                           
            //ViveSR_StaticColliderInfo info;

            switch (meshShowMode)
            {
                case ShowMode.All:
                    cldPool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE });
                    break;
                case ShowMode.Horizon:
                    cldPool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.HORIZONTAL });
                    break;
                case ShowMode.LargestHorizon:
                    cldPool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.HORIZONTAL }, ColliderCondition.LARGEST);
                    break;
                case ShowMode.AllVertical:
                    cldPool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.VERTICAL });
                    break;
                case ShowMode.LargestVertical:
                    cldPool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.VERTICAL }, ColliderCondition.LARGEST);
                    break;
                case ShowMode.NearestHorizon:
                    cldPool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.HORIZONTAL }, ColliderCondition.CLOSEST);
                    break;
                case ShowMode.FurthestHorizon:
                    cldPool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.HORIZONTAL }, ColliderCondition.FURTHEST);
                    break;
                case ShowMode.NearestVertical:
                    cldPool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.VERTICAL }, ColliderCondition.CLOSEST);
                    break;
                case ShowMode.FurthestVertical:
                    cldPool.ShowAllColliderWithPropsAndCondition(new uint[] { (uint)ColliderShapeType.MESH_SHAPE, (uint)PlaneOrientation.VERTICAL }, ColliderCondition.FURTHEST);
                    break;
                case ShowMode.None:
                    cldPool.HideAllColliderRenderers();
                    break;
            }
        }
        //public void SetColliderForNavigation(List<ViveSR_Experience_Chair> Chairs, System.Action done)
        //{
        //    cldPool = collisionMesh.GetComponent<ViveSR_StaticColliderPool>();
        //    if (cldPool == null) WaitForCldPool(() => _SetColliderForNavigation(Chairs, done));
        //    else _SetColliderForNavigation(Chairs, done);
        //}
        //public void _SetColliderForNavigation(List<ViveSR_Experience_Chair> Chairs, System.Action done)
        //{
        //    Debug.Log("SetColliderForNavigation");
        //    ViveSR_StaticColliderInfo[] clds = cldPool.GetColliderByHeightRange(ColliderShapeType.MESH_SHAPE, 0.3f, 0.8f);
        //    foreach (ViveSR_StaticColliderInfo cld in clds)
        //    {
        //        if (cld.ApproxArea < 0.01f) continue;

        //        bool createObstacle = true;

        //        Vector3 cld_center = cld.transform.TransformPoint(cld.GetComponent<MeshFilter>().mesh.bounds.center);

        //        foreach (ViveSR_Experience_Chair chair in Chairs)
        //        {
        //            float dist = Vector3.Distance(cld_center, chair.transform.position);
        //            Vector3 dir = (cld_center - chair.transform.position) / dist;

        //            if (dist < 1f || (dist < 1.5f && Vector3.Angle(chair.transform.forward, dir) < 60f))
        //                createObstacle = false;
        //        }

        //        if (!createObstacle) continue;

        //        NavMeshObstacle obstacle;

        //        if (!cld.gameObject.GetComponent<NavMeshObstacle>())
        //        {
        //            obstacle = cld.gameObject.AddComponent<NavMeshObstacle>();
        //            obstacle.carving = true;
        //        }
         
        //        cld.GetComponent<MeshRenderer>().enabled = true;
        //    }
        //    if (done != null) done();
        //}

        public void SetSegmentation(bool isOn)
        {
            ViveSR_SceneUnderstanding.EnableSceneUnderstanding(isOn);
        }
        
        public void SetChairSegmentationConfig(bool isOn)
        {
            #region Set SceneUnderstandingConfig for Chair
            ViveSR_SceneUnderstanding.SetCustomSceneUnderstandingConfig(SceneUnderstandingObjectType.CHAIR, 5, isOn);
            #endregion
        }

    public List<SceneUnderstandingObjects.Element> GetSegmentationInfo(SceneUnderstandingObjectType type)
        {
            SceneUnderstandingObjects segObjs = new SceneUnderstandingObjects(semanticObj_dirPath);
            
            return new List<SceneUnderstandingObjects.Element>(segObjs.GetElements((int)type)) ;
        }

        public void TestSegmentationResult(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            StartCoroutine(_TestSegmentationResult(UpdatePercentage, done));
        }
        public IEnumerator _TestSegmentationResult(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            int percentage = 0;

            ViveSR_SceneUnderstanding.ExportSceneUnderstandingInfo(semanticObj_dir);

            while (percentage < 100)
            {
                percentage = ViveSR_SceneUnderstanding.GetSceneUnderstandingProgress();
                UpdatePercentage(percentage);

                yield return new WaitForEndOfFrame();
            }

            if (done != null) done();
        }

        private void OnDestroy()
        {
            SetSegmentation(false);
            EnableDepthProcessingAndScanning(false);
        }

        public void GenerateHintLocators(List<SceneUnderstandingObjects.Element> Elements)
        {
            ClearHintLocators();
            for (int i = 0; i < Elements.Count; i++)
            {                       
                HintLocators.Add(Instantiate(HintLocatorPrefab));
                HintLocators[i].transform.position = Elements[i].position[0];
                HintLocators[i].transform.forward = Elements[i].forward;
            }
        }

        public void ClearHintLocators()
        {
            foreach (GameObject hintLocator in HintLocators) Destroy(hintLocator);
            HintLocators.Clear();
        }
    }
}