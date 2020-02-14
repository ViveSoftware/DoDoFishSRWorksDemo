using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Recons3DAssetMultiLoader : MonoBehaviour
    {
        public bool isAllColliderReady;
        public MeshRenderer[] meshRnds;
        public MeshRenderer[] cldRnds;
        public int loadedObjNumber = 0;
        public int totalObjNumber = 0;
        //System.Action done;

        private void LoadColliderDoneCallBack(GameObject go, string semanticFileName, bool updateIsReady)
        {
            if (ViveSR_RigidReconstructionColliderManager.ProcessDataAndGenColliderInfo(go) == true)
            {
                ViveSR_RigidReconstructionColliderManager cldPool = go.AddComponent<ViveSR_RigidReconstructionColliderManager>();
                Rigidbody rigid = go.AddComponent<Rigidbody>();
                rigid.isKinematic = true;
                rigid.useGravity = false;

                cldPool.OrganizeHierarchy();
                cldRnds = go.GetComponentsInChildren<MeshRenderer>(true);

                ViveSR_SceneUnderstanding.SetGameObjectByFileName(semanticFileName, go.name);
            }
            if (updateIsReady && ++loadedObjNumber == totalObjNumber) isAllColliderReady = true;
        }

        public GameObject[] LoadColliderObjs(string dirPath)
        {
            isAllColliderReady = false;

            List<GameObject> outputObjs = new List<GameObject>();
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
            {
                Debug.Log(dirPath + " does not exist.");
            }
            else
            {
                FileInfo[] files = dir.GetFiles("*_cld.obj");
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];
                    string filePath = dirPath + "/" + file.Name;
                    Debug.Log(filePath);

                    GameObject go = OBJLoader.LoadOBJFile(filePath, LoadColliderDoneCallBack);
                    go.SetActive(false);
                    outputObjs.Add(go);
                }
            }
            return outputObjs.ToArray();
        }

        public GameObject[] LoadSemanticColliderObjsByType(string dirPath, SceneUnderstandingObjectType type)
        {
            isAllColliderReady = false;

            ViveSR_SceneUnderstanding.ImportSceneObjects(dirPath); // read data from xml

            List<GameObject> outputObjs = new List<GameObject>();
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
            {
                Debug.Log(dirPath + " does not exist.");
            }
            else
            {
                string[] fileNames = ViveSR_SceneUnderstanding.GetColliderFileNamesByType(type);
                for (int i = 0; i < fileNames.Length; i++)
                {
                    FileInfo file = new FileInfo(dirPath + "/" + fileNames[i]);
                    //Debug.Log(file.FullName);
                    if (!file.Exists)
                    {
                        Debug.Log(file.FullName + " does not exist.");
                        if (i == fileNames.Length - 1)
                        {
                            isAllColliderReady = true;
                            return outputObjs.ToArray();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    GameObject go = OBJLoader.LoadOBJFile(file.FullName, LoadColliderDoneCallBack);
                    go.SetActive(false);
                    outputObjs.Add(go);
                }

            }
            return outputObjs.ToArray();
        }

        public GameObject[] LoadSemanticColliderObjs(string dirPath)
        {
            // reset
            isAllColliderReady = false;
            totalObjNumber = loadedObjNumber = 0;
            StopAllCoroutines();

            ViveSR_SceneUnderstanding.ImportSceneObjects(dirPath); // read data from xml

            List<GameObject> outputObjs = new List<GameObject>();
            DirectoryInfo dir = new DirectoryInfo(dirPath);
            if (!dir.Exists)
            {
                Debug.Log(dirPath + " does not exist.");
            }
            else
            {
                string[] fileNames = ViveSR_SceneUnderstanding.GetColliderFileNames();
                totalObjNumber = fileNames.Length;
                for (int i = 0; i < fileNames.Length; i++)
                {
                    FileInfo file = new FileInfo(dirPath + "/" + fileNames[i]);
                    //Debug.Log(file.FullName);
                    if (!file.Exists)
                    {
                        Debug.Log(file.FullName + " does not exist.");
                        if (i == fileNames.Length - 1)
                        {
                            isAllColliderReady = true;
                            return outputObjs.ToArray();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    GameObject go = OBJLoader.LoadOBJFile(file.FullName, LoadColliderDoneCallBack, file.Name);
                    go.SetActive(false);
                    outputObjs.Add(go);
                }
            }
            return outputObjs.ToArray();
        }
    }
}