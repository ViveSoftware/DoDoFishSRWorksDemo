using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Vive.Plugin.SR;
using Vive.Plugin.SR.Experience;

public class SRWorkControl : MySingleton<SRWorkControl>
{
    public ViveSR viveSR;
    public Transform eye;
    public float Near = 0f, Far = 0.63f;
    public bool DebugShow;
    public ViveSR_Experience_StaticMesh viveSR_Experience_StaticMesh;

    public void SaveScanning(Action<int> UpdatePercentage, Action Done)
    {
        if (ViveSR_RigidReconstruction.IsScanning)
        {
            viveSR_Experience_StaticMesh.ExportModel(UpdatePercentage, Done);
        }
    }

    public void SetScanning(bool isOn)
    {
        if (isOn)
        {
            if (!ViveSR_RigidReconstruction.IsScanning && !viveSR_Experience_StaticMesh.ModelIsLoading)
            {
                ViveSR_RigidReconstructionRenderer.LiveMeshDisplayMode = ReconstructionDisplayMode.ADAPTIVE_MESH;
                ViveSR_DualCameraImageCapture.EnableDepthProcess(true);
                ViveSR_RigidReconstruction.StartScanning();

                if (viveSR_Experience_StaticMesh.CheckModelLoaded())
                {
                    viveSR_Experience_StaticMesh.LoadMesh(false);
                }
            }
        }
        else
        {
            if (ViveSR_RigidReconstruction.IsScanning)
            {
                ViveSR_RigidReconstruction.StopScanning();
                ViveSR_DualCameraImageCapture.EnableDepthProcess(false);
            }
        }
    }

    public MeshRenderer GetReconstructLiveMesh()
    {
        if (GameObject.FindObjectOfType<ViveSR_RigidReconstructionRenderer>() == null || ViveSR_RigidReconstructionRenderer.Instance == null)
            return null;
        GameObject objLoaderObj = MyReflection.GetStaticMemberVariable(typeof(OBJLoader), "loaderOBJ") as GameObject;
        OBJLoader objLoaberInstance = objLoaderObj.GetComponent<OBJLoader>();
        GameObject LiveMeshesGroups = MyReflection.GetMemberVariable(objLoaberInstance, "returnObject") as GameObject;
        if (LiveMeshesGroups == null)
            return null;
        Transform liveMeshObj = LiveMeshesGroups.transform.GetChild(0);
        if (!liveMeshObj.gameObject.activeSelf)
            return null;
        return liveMeshObj.GetComponent<MeshRenderer>();
    }

    public bool LoadReconstructMesh(Action beforeLoad, Action Done)
    {
        if (!viveSR_Experience_StaticMesh.CheckModelExist())
            return false;
        if (!viveSR_Experience_StaticMesh.CheckModelLoaded() && !ViveSR_RigidReconstruction.IsScanning)
        {
            viveSR_Experience_StaticMesh.enabled = true;
            viveSR_Experience_StaticMesh.LoadMesh(true, beforeLoad, Done);
            return true;
        }
        return false;
    }

    public bool IsReconstructMeshLoading()
    {
        return viveSR_Experience_StaticMesh.ModelIsLoading;
    }

    public bool IsReconstructMeshLoaded()
    {
        return viveSR_Experience_StaticMesh.CheckModelLoaded();
    }

    GameObject reconstructStaticMeshesObj;
    public MeshRenderer[] GetReconstructStaticMeshes()
    {
        if (reconstructStaticMeshesObj == null)
        {
            //There are many 'Model' named object, we can't use this way...
            //string mesh_name = Path.GetFileNameWithoutExtension(ViveSR_Experience_StaticMesh.instance.mesh_path);
            //reconstructStaticMeshesObj = GameObject.Find(mesh_name);

            ViveSR_Experience_Recons3DAssetLoader loader =
            MyReflection.GetMemberVariable(viveSR_Experience_StaticMesh, "ReconsLoader") as ViveSR_Experience_Recons3DAssetLoader;
            if (loader.meshRnds != null && loader.meshRnds.Length > 0)
                reconstructStaticMeshesObj = loader.meshRnds[0].transform.parent.gameObject;
        }
        if (reconstructStaticMeshesObj == null)
        {
            //Debug.LogWarning("[GetReconstructMeshes] fail");
            return null;
        }
        MeshRenderer[] renders = new MeshRenderer[reconstructStaticMeshesObj.transform.childCount];
        for (int a = 0; a < reconstructStaticMeshesObj.transform.childCount; a++)
        {
            Transform t = reconstructStaticMeshesObj.transform.GetChild(a);
            renders[a] = t.GetComponent<MeshRenderer>();
        }
        return renders;
    }

    public void GetReconstructStaticMeshCollider(out List<MeshCollider> outColliders)
    {
        outColliders = null;
        string cld_name = MyReflection.GetMemberVariable(viveSR_Experience_StaticMesh, "cld_path") as string;
        cld_name = Path.GetFileNameWithoutExtension(cld_name);
        GameObject reconstructStaticColliderObj = GameObject.Find(cld_name);
        if (reconstructStaticColliderObj == null)
            return;
        GameObject colRootObj = reconstructStaticColliderObj;

        Transform meshObj = colRootObj.transform.Find("PlaneMeshColliderGroup");
        outColliders = new List<MeshCollider>(meshObj.GetComponentsInChildren<MeshCollider>());
    }

    public void GetReconstructStaticConvexCollider(out List<MeshCollider> outColliders, bool getHorizontal)
    {
        outColliders = null;
        string cld_name = MyReflection.GetMemberVariable(viveSR_Experience_StaticMesh, "cld_path") as string;
        cld_name = Path.GetFileNameWithoutExtension(cld_name);
        GameObject reconstructStaticColliderObj = GameObject.Find(cld_name);
        if (reconstructStaticColliderObj == null)
            return;
        GameObject colRootObj = reconstructStaticColliderObj;

        Transform meshObj = colRootObj.transform.Find("PlaneConvexColliderGroup");
        Transform HV = getHorizontal ? meshObj.Find("Horizontal") : meshObj.Find("Vertical");
        if (HV == null)
            Debug.LogError("[GetReconstructStaticConvexCollider] : " + (getHorizontal ? "horizontal fail" : "vertical fail"));

        outColliders = new List<MeshCollider>();
        outColliders.AddRange(new List<MeshCollider>(HV.GetComponentsInChildren<MeshCollider>()));
        /*  for (int a = 0; a < meshObj.childCount; a++)
          {
              Transform t = meshObj.GetChild(a);
              outColliders.Add(t.GetComponent<MeshCollider>());
          }*/

        Transform Oblique = meshObj.Find("Oblique");
        if (Oblique != null)
            outColliders.AddRange(new List<MeshCollider>(Oblique.GetComponentsInChildren<MeshCollider>()));

    }

    /// <summary>
    /// Get all of the collider from SRWorks's StaticQuad
    /// </summary>
    public void GetReconstructStaticQuadCollider(out List<MeshCollider> outColliders, bool getHorizontal)
    {
        outColliders = null;
        string cld_name = MyReflection.GetMemberVariable(viveSR_Experience_StaticMesh, "cld_path") as string;
        cld_name = Path.GetFileNameWithoutExtension(cld_name);
        GameObject reconstructStaticColliderObj = GameObject.Find(cld_name);
        if (reconstructStaticColliderObj == null)
            return;
        GameObject colRootObj = reconstructStaticColliderObj;
        Transform meshObj = colRootObj.transform.Find("PlaneBoundingRectColliderGroup");
        meshObj = getHorizontal ? meshObj.Find("Horizontal") : meshObj.Find("Vertical");
        if (meshObj == null)
            Debug.LogError("[GetReconstructStaticQuadCollider] : " + (getHorizontal ? "horizontal fail" : "vertical fail"));

        outColliders = new List<MeshCollider>();
        for (int a = 0; a < meshObj.childCount; a++)
        {
            Transform t = meshObj.GetChild(a);
            outColliders.Add(t.GetComponent<MeshCollider>());
        }
    }
}
