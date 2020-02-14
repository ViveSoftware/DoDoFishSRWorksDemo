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

    [RequireComponent(typeof(ViveSR_Experience_Recons3DAssetLoader))]
    public class ViveSR_Experience_StaticMesh : MonoBehaviour
    {
        ViveSR_Experience_Recons3DAssetLoader ReconsLoader;

        private bool _modelIsLoading = false;
      
        string MeshName = "Model";
        string mesh_path;
        string cld_path;
        string reconsRootDir = Path.GetDirectoryName(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)) + "\\LocalLow\\HTC Corporation\\SR_Reconstruction_Output\\Recons3DAsset/";

        public GameObject texturedMesh;
        public GameObject collisionMesh;
        public ViveSR_RigidReconstructionColliderManager cldPool { get; private set; }
        [SerializeField] MeshRenderer[] modelRenderers;

        public bool ModelMeshVisible { get; private set; }

        ShowMode _MeshShowMode = ShowMode.None;

        System.Action MeshReadyCallBack;

        public bool isMeshReady 
        {
            get { return ReconsLoader.isMeshReady; }
        }

        public ShowMode MeshShowMode
        {
            get { return _MeshShowMode; }
        }

        public bool ModelIsLoading
        {
            get { return _modelIsLoading; }
        }

        private void Awake()
        {
            mesh_path = reconsRootDir  + MeshName + ".obj";
            cld_path = reconsRootDir  + MeshName + "_cld.obj";
            ReconsLoader = GetComponent<ViveSR_Experience_Recons3DAssetLoader>();
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

        public void ResetScannedData()
        {
            ViveSR_RigidReconstruction.ResetReconstructionModule();
        }

        public bool CheckModelFileExist()
        {
            return File.Exists(mesh_path) && File.Exists(cld_path);
        }

        public bool CheckModelLoaded()
        {
            return ( texturedMesh != null || collisionMesh != null);
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

        public void ExportModel(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            ViveSR_RigidReconstruction.StopScanning();

            if (!ViveSR_DualCameraImageCapture.IsDepthProcessing)
                ViveSR_DualCameraImageCapture.EnableDepthProcess(true); // in case depth has been turned off by other process

            StartCoroutine(_ExportModel(UpdatePercentage, done));
        }

        private IEnumerator _ExportModel(System.Action<int> UpdatePercentage = null, System.Action done = null)
        {
            ViveSR_RigidReconstruction.StartExporting(MeshName);

            int percentage = 0;

            if (CheckModelLoaded())
            {
                Destroy(collisionMesh);
                Destroy(texturedMesh);
                collisionMesh = null;
                texturedMesh = null;
            }
            ViveSR_RigidReconstruction.GetExportProgress(ref percentage);

            while (percentage < 100)
            {
                int error = ViveSR_RigidReconstruction.GetExportProgress(ref percentage);

                if (error != (int)Error.WORK)
                {
                    GPUMemoryFull();
                    break;
                }                         

                if (UpdatePercentage != null) UpdatePercentage(percentage);

                yield return new WaitForEndOfFrame();
            }

            ViveSR_DualCameraImageCapture.EnableDepthProcess(false);

            if (done != null) done();
        }
    

        public void WaitForCldPool(System.Action done = null)
        {
            StartCoroutine(_WaitForCldPool(done));
        }

        IEnumerator _WaitForCldPool(System.Action done)
        {
            while(cldPool == null)
            {
                cldPool = collisionMesh.GetComponent<ViveSR_RigidReconstructionColliderManager>();
                yield return new WaitForEndOfFrame();
            }
            if(done != null) done();
        }

     

        public void SwitchShowCollider(ShowMode meshShowMode)
        {
            _MeshShowMode = meshShowMode;
            if (collisionMesh == null) return;
            cldPool = collisionMesh.GetComponent<ViveSR_RigidReconstructionColliderManager>();
            if (cldPool == null) WaitForCldPool(() => _SwitchShowCollider(meshShowMode));
            else _SwitchShowCollider(meshShowMode);
        }

        void _SwitchShowCollider(ShowMode meshShowMode)
        {                           
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
        //    cldPool = collisionMesh.GetComponent<ViveSR_RigidReconstructionColliderManager>();
        //    if (cldPool == null) WaitForCldPool(() => _SetColliderForNavigation(Chairs, done));
        //    else _SetColliderForNavigation(Chairs, done);
        //}
        //public void _SetColliderForNavigation(List<ViveSR_Experience_Chair> Chairs, System.Action done)
        //{
        //    Debug.Log("SetColliderForNavigation");
        //    ViveSR_RigidReconstructionCollider[] clds = cldPool.GetColliderByHeightRange(ColliderShapeType.MESH_SHAPE, 0.3f, 0.8f);
        //    foreach (ViveSR_RigidReconstructionCollider cld in clds)
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

 

        private void OnDestroy()
        {
            EnableDepthProcessingAndScanning(false);
        }

        void GPUMemoryFull()
        {
            Debug.LogError("GPUMemoryFull()");

            //ViveSR_Experience.instance.ErrorHandlerScript.EnablePanel("GPU Memory Full. Quitting Game...");
            //this.Delay(QuitGame, 3f);
        }
        void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}