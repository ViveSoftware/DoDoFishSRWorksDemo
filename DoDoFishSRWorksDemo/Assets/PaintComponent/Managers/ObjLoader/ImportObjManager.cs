//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using UnityEngine;

//namespace PaintVR
//{
//    public class ImportObjManager : MonoBehaviour
//    {
//        public string defaultObjPath = "";
//        public GBufferResolution defaultResolution = GBufferResolution.High;
//        public int ImportProcessPercent { get { return importProcessPercent; } }

//        [SerializeField] private ObjGBufferGenerator generator;
//        [SerializeField] private Camera PositionMapCamera,NormalMapCamera;

//        private GameObject ImportObj = null;
//        private List<PBSPaintableObjectRoot> ActPaintableObjs;
//        private int importProcessPercent = 0;
//        private bool isImporting = false;
//        private bool meshReady;
//        private OBJLoader.ObjData objdata = null;

//        private static ImportObjManager mgr = null;
//        public static ImportObjManager instance
//        {
//            get
//            {
//                if (mgr == null)
//                {
//                    mgr = FindObjectOfType(typeof(ImportObjManager)) as ImportObjManager;
//                    if (mgr == null)
//                    {
//                        Debug.LogError("[ImportObjManager::Singleton] There should be one active gameobject attached ObjectManager in scene");
//                    }
//                }
//                return mgr;
//            }
//        }

//        // Use this for initialization
//        void Start()
//        {
//            //LoadObjAndCreate("/Recons3DAsset/Model.obj");
//        }

//        public void LoadObjAndCreate(string path)
//        {
//            if (isImporting)
//            {
//                Debug.LogError("[ImportObjManager::LoadObj] There another mesh is loading.");
//            }

//            DestroyLoadedObj();
//            StartCoroutine(LoadObjAndCreateCoroutine(Directory.GetParent(Application.dataPath) + path));
//            //StartCoroutine(LoadObjAndCreateCoroutine(path));
//        }

//        public void DestroyLoadedObj()
//        {
//            if (ImportObj != null)
//            {
//                foreach (var paintComponent in ActPaintableObjs)
//                {
//                    paintComponent.UnregisterUndoMgr();
//                }
                
//                Destroy(ImportObj);
//            }
//        }

//        private void LoadMeshDoneCallBack(GameObject go, OBJLoader.ObjData data)
//        {
//            meshReady = true;
//            objdata = data;
//            ImportObj = go;
//        }

//        private IEnumerator LoadObjAndCreateCoroutine(string path)
//        {
//            meshReady = false;
//            isImporting = true;
//            if (!File.Exists(path))
//            {
//                Debug.LogError("[ImportObjManager::GenerateGBufferMeshes] " + path + " Cannot find .obj file");
//            }
//            else
//            {
//                importProcessPercent = 0;
//                OBJLoader.LoadOBJFile(path, LoadMeshDoneCallBack);
//                while (!meshReady)
//                {
//                    yield return new WaitForEndOfFrame();
//                }
//                importProcessPercent = 50;

//                AddMeshColliderOnEachObj(ImportObj);

//                importProcessPercent = 60;

//                List<Material> matList = SeparateObjRootByMaterial(ImportObj);

//                OBJLoader.OBJFace[] originfaces = objdata.faceList.ToArray();
//                ActPaintableObjs = new List<PBSPaintableObjectRoot>();

//                for (int i = 0; i < ImportObj.transform.childCount; i++)
//                {
//                    OBJLoader.OBJFace[] faces = originfaces.Where(x => matList[i].name.Contains(x.materialName)).ToArray();

//                    generator.gBufferResolution = defaultResolution;
//                    generator.GBufferGeneratorInit(ref faces, ref objdata.uuvs, ref objdata.uvertices, ref objdata.unormals);

//                    yield return new WaitForSeconds(3.0f);

//                    ActPaintableObjs.Add(SetRootPBSPaintableComponent(ImportObj.transform.GetChild(i).gameObject, matList[i]));
//                }

//                importProcessPercent = 90;

//                for (int i = 0; i < ImportObj.transform.childCount; i++)
//                {
//                    SetSubPBSPaintableComponent(ImportObj.transform.GetChild(i).gameObject, matList[i]);
//                }
//                yield return null;

//                importProcessPercent = 100;
//            }
//            isImporting = false;
//        }

//        private List<Material> SeparateObjRootByMaterial(GameObject root)
//        {
//            Dictionary<string, GameObject> rootList = new Dictionary<string, GameObject>();
//            List<Material> materials = new List<Material>();
//            GameObject tempRoot = new GameObject("temp");
//            while(root.transform.childCount != 0)
//            {
//                Transform child = root.transform.GetChild(0);
//                string matName = child.GetComponent<MeshRenderer>().material.name;
//                if (rootList.ContainsKey(matName))
//                {
//                    GameObject rt = rootList[matName];

//                    child.name = matName;
//                    child.transform.parent = rt.transform;
//                }
//                else
//                {
//                    GameObject rt = new GameObject(matName);
//                    rt.transform.parent = tempRoot.transform;
//                    rootList.Add(matName, rt);

//                    child.name = matName;
//                    child.transform.parent = rt.transform;

//                    materials.Add(child.GetComponent<MeshRenderer>().material);
//                }
//            }

//            foreach (KeyValuePair<string, GameObject> item in rootList)
//            {
//                item.Value.transform.parent = root.transform;
//            }
//            Destroy(tempRoot);

//            return materials;
//        }

//        private void AddMeshColliderOnEachObj(GameObject root)
//        {
//            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
//            foreach (var mesh in meshFilters)
//            {
//                MeshCollider collider = mesh.gameObject.AddComponent<MeshCollider>();
//                collider.sharedMesh = mesh.mesh;
//            }
//        }

//        private PBSPaintableObjectRoot SetRootPBSPaintableComponent(GameObject root, Material mat)
//        {
//            MeshRenderer render = root.AddComponent<MeshRenderer>();
//            render.material = mat;

//            PBSPaintableObjectRoot component = root.AddComponent<PBSPaintableObjectRoot>();
//            component.ImportPBSMap(PositionMapCamera.targetTexture, NormalMapCamera.targetTexture);

//            root.layer = PaintComponentDefine.PaintObjectLayer;
//            root.tag = PaintComponentDefine.PaintMeshTag;

//            return component;
//        }

//        private void SetSubPBSPaintableComponent(GameObject root, Material mat)
//        {
//            for (int i = 0;i<root.transform.childCount;i++)
//            {
//                GameObject tp = root.transform.GetChild(i).gameObject;

//                tp.AddComponent<PBSPaintableMiniature>();
//                tp.layer = PaintComponentDefine.PaintObjectLayer;
//                tp.tag = PaintComponentDefine.PaintMeshTag;
//            }
//        }
//    }
//}