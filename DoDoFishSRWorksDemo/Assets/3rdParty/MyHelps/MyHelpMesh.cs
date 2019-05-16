using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MyHelpMesh
{
    public static GameObject Skinned2Mesh(Transform zombie)
    {
        SkinnedMeshRenderer[] skinnedMeshes = zombie.GetComponentsInChildren<SkinnedMeshRenderer>();
        List<CombineInstance> combineList = new List<CombineInstance>();

        GameObject parent = new GameObject();
        parent.name = zombie.name + "_Splitter" + Random.Range(0, 1000);

        MeshRenderer meshRenderer = parent.AddComponent<MeshRenderer>();
        MeshFilter mshFilter = parent.AddComponent<MeshFilter>();

        //if (skinnedMeshes.Length == 1)
        //{
        //    Debug.LogWarning("single mesh");
        //    Mesh mesh = new Mesh();
        //    skinnedMeshes[0].BakeMesh(mesh);
        //    mshFilter.mesh = mesh;
        //    meshRenderer.material = GameObject.Instantiate(skinnedMeshes[0].material);
        //}
        //else
        {
            bool isInitMaterial = false;
            for (int a = 0; a < skinnedMeshes.Length; a++)
            {
                SkinnedMeshRenderer skinRenderer = skinnedMeshes[a];

                if (skinRenderer.enabled == false)
                    continue;
                if (!isInitMaterial)
                {
                    isInitMaterial = true;
                    meshRenderer.material = GameObject.Instantiate(skinRenderer.material);
                }

                CombineInstance combine = new CombineInstance();
                Mesh mesh = new Mesh();
                skinRenderer.BakeMesh(mesh);

                combine.mesh = mesh;
                combine.transform = Matrix4x4.TRS(zombie.position, zombie.rotation, Vector3.one);// zombie.localToWorldMatrix;
                combineList.Add(combine);
            }
            mshFilter.mesh.CombineMeshes(combineList.ToArray(), true, true);
            //parent.SetActive(true);

            while (combineList.Count > 0)
            {
                GameObject.Destroy(combineList[0].mesh);
                combineList.RemoveAt(0);
            }
        }

        //if (skinnedMeshes.Length == 1)
        //{
        //    parent.transform.position = zombie.position;
        //    parent.transform.rotation = zombie.rotation;
        //}
        return parent;
    }

    /// <summary>
    /// set mesh collider from root 
    /// </summary>
    public static void _setMeshCollider(Transform root, bool isDestroy)
    {
        MeshRenderer meshrRender = root.GetComponent<MeshRenderer>();
        if (meshrRender != null)
        {
            MeshCollider collider = root.GetComponent<MeshCollider>();
            if (collider != null)
                GameObject.DestroyImmediate(collider);

            if (!isDestroy)
                collider = root.gameObject.AddComponent<MeshCollider>();
        }

        for (int a = 0; a < root.childCount; a++)
        {
            Transform t = root.GetChild(a);
            _setMeshCollider(t, isDestroy);
        }
    }


    /// <summary>
    /// 用??置所有boss子物体的MeshRenderer
    /// </summary>
    public static void SetRenderer(Transform obj, bool b)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            //renderer.updateWhenOffscreen = true;
            renderer.enabled = b;
        }

        for (int a = 0; a < obj.transform.childCount; a++)
            SetRenderer(obj.transform.GetChild(a), b);
    }

    public static void SkinUpdateOffscreen(Transform obj, bool b)
    {
        SkinnedMeshRenderer renderer = obj.GetComponent<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            if (b)
                renderer.enabled = true;
            renderer.updateWhenOffscreen = b;
        }
        for (int a = 0; a < obj.transform.childCount; a++)
            SkinUpdateOffscreen(obj.transform.GetChild(a), b);
    }

    public static bool GetLongestDirectionInRange(Vector3[] verticesArray, Vector3 detectPoint, float detectRange, out Vector3 outDirection)
    {
        outDirection = Vector3.zero;
        float sqrMaxLength = 0;
        int maxID = -1;
        for (int a = 0; a < verticesArray.Length; a++)
        {
            Vector3 dir = verticesArray[a] - detectPoint;
            float sqrDis = dir.sqrMagnitude;
            if (sqrDis < detectRange * detectRange)
            {
                if (sqrDis > sqrMaxLength)
                {
                    sqrMaxLength = sqrDis;
                    maxID = a;
                }
            }
        }
        if (maxID < 0)
            return false;
        outDirection = (verticesArray[maxID] - detectPoint).normalized;
        return true;
    }

    public static void GetAverageDirectionInRange(Vector3[] verticesArray, Vector3 detectPoint, float detectRange, out Vector3 outDirection)
    {
        int avgCount = 0;
        Vector3 dirCount = Vector3.zero;
        for (int a = 0; a < verticesArray.Length; a++)
        {
            Vector3 dir = verticesArray[a] - detectPoint;
            float sqrDis = dir.sqrMagnitude;
            if (sqrDis < detectRange * detectRange)
            {
                dirCount += dir.normalized;
                avgCount++;
            }
        }
        outDirection = dirCount / avgCount;
    }

    /// <summary>
    /// Find 2 point by calculate the longest line.
    /// </summary>
    public static void GetLongestLineFromVertices(List<Vector3> verticesList, Vector3[] verticesArray, out Vector3 nearPoint, out Vector3 farPoint)
    {
        //Debug.LogWarning("[GetLineFromVertices]start vertices : " + vertices.Count);
        nearPoint = farPoint = Vector3.zero;
        if (verticesList != null && verticesList.Count > 1000)
            return;
        if (verticesArray != null && verticesArray.Length > 1000)
            return;

        int totalCount = 0;
        float maxDisSqr = -1;

        if (verticesArray != null)
        {
            for (int a = 0; a < verticesArray.Length; a++)
            {
                for (int b = a + 1; b < verticesArray.Length; b++)
                {
                    float disSqr = (verticesArray[a] - verticesArray[b]).sqrMagnitude;
                    if (disSqr > maxDisSqr)
                    {
                        maxDisSqr = disSqr;
                        nearPoint = verticesArray[a];
                        farPoint = verticesArray[b];
                    }
                    totalCount++;
                }
            }
            //Debug.LogWarning("1.[GetLineFromVertices] vertices : " + vertices.Count + " , [totalCount] : " + totalCount + ", A : " + maxIndexA + " , B : " + maxIndexB);
            return;
        }

        int indexA = 0;//, maxIndexA = -1, maxIndexB = -1;
        List<Vector3>.Enumerator itrA = verticesList.GetEnumerator();
        totalCount = 0;
        maxDisSqr = -1;
        while (itrA.MoveNext())
        {
            List<Vector3>.Enumerator itrB = itrA;
            int indexB = indexA + 1;
            while (itrB.MoveNext())
            {
                float disSqr = (itrA.Current - itrB.Current).sqrMagnitude;
                if (disSqr > maxDisSqr)
                {
                    maxDisSqr = disSqr;
                    //maxIndexA = indexA;
                    //maxIndexB = indexB;
                    nearPoint = itrA.Current;
                    farPoint = itrB.Current;
                }
                indexB++;
                totalCount++;
            }
            indexA++;
        }

        //Debug.LogWarning("2.[GetLineFromVertices] vertices : " + vertices.Count + " , [totalCount] : " + totalCount + ", A : " + maxIndexA + " , B : " + maxIndexB);

        //Vector3 eyePos = CAMERAeYE.position;
        //float near2Eye = (nearPoint - eyePos).sqrMagnitude;
        //float far2Eye = (farPoint - eyePos).sqrMagnitude;
        //if(far2Eye < near2Eye)
        //{
        //    Vector3 tmpV = farPoint;
        //    farPoint = nearPoint;
        //    nearPoint = tmpV;
        //}
    }
}