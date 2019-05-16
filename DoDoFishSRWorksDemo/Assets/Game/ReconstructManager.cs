using HTC.UnityPlugin.Vive;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Demo
{
    /// <summary>
    /// This class process SRWorks scanning data, to create game component
    /// </summary>
    public class ReconstructManager : MySingleton<ReconstructManager>
    {
        [Header("Reconstruct load----------------------------------------")]
        //[SerializeField]
        public Material selectWallMaterial;
        [SerializeField]
        GameObject selectWallPointer;
        [HideInInspector]
        public GameObject selectedWallRoot;
        [HideInInspector]
        public GameObject recontructFloor;
        GameObject floorPlaneObj, floorPlaneObjCollider;

        public class MeshSizeData : IComparable<MeshSizeData>
        {
            public MeshCollider colliderMesh;
            public float area;
            public Vector3 meshCenterWorld;
            public Vector3 avgNormalWorld;
            public Vector3[] triNormal;
            public int CompareTo(MeshSizeData other)
            {
                if (this.area < other.area)
                    return 1;
                return -1;
            }
        }

        public bool reconstructDataAnalyzeDone { get; private set; }
        List<MeshCollider> reconstructConvexCollidersHorizontal, reconstructConvexCollidersVertical, reconstructQuadCollidersHorizontal, reconstructQuadCollidersVertical;

        public bool StartLoadReconstructData(Action beforeLoad, Action Done)
        {
            if (!SRWorkControl.Instance.LoadReconstructMesh(beforeLoad, Done))
                return false;
            StartCoroutine(_waitAnalyzeReconstructData());
            return true;
        }

        IEnumerator _waitAnalyzeReconstructData()
        {
            reconstructDataAnalyzeDone = false;
            Debug.Log("waitAnalyzeReconstructData start");
            MeshRenderer[] reconMesh;
            while (true)
            {
                //if (!SRWorkControl.Instance.IsReconstructMeshLoading())
                //    break;

                //Wait analyze, render system still not create, hide scanned mesh first, because it's ugly to see.
                reconMesh = SRWorkControl.Instance.GetReconstructStaticMeshes();
                if (reconMesh != null)
                    foreach (MeshRenderer mr in reconMesh)
                        mr.enabled = false;

                //While GetReconstructStaticConvexCollider() has value, the analyze is done
                SRWorkControl.Instance.GetReconstructStaticConvexCollider(out reconstructConvexCollidersHorizontal, true);
                SRWorkControl.Instance.GetReconstructStaticConvexCollider(out reconstructConvexCollidersVertical, false);
                if (reconstructConvexCollidersHorizontal != null)
                {
                    SRWorkControl.Instance.GetReconstructStaticQuadCollider(out reconstructQuadCollidersHorizontal, true);
                    SRWorkControl.Instance.GetReconstructStaticQuadCollider(out reconstructQuadCollidersVertical, false);

                    //Set static for prevent through
                    List<MeshCollider> allMeshCollider;
                    SRWorkControl.Instance.GetReconstructStaticMeshCollider(out allMeshCollider);
                    foreach (MeshCollider col in allMeshCollider)
                        col.gameObject.isStatic = true;
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("waitAnalyzeReconstructData done");

            //do something while loading complete...       
            reconstructDataAnalyzeDone = true;
            foreach (MeshRenderer mr in reconMesh)
            {
                mr.gameObject.isStatic = true;
                mr.enabled = true;//render system will create, enable renderer
            }
            List<MeshCollider> outColliders;
            SRWorkControl.Instance.GetReconstructStaticConvexCollider(out outColliders, true);
            foreach (MeshCollider mc in outColliders)
                mc.gameObject.isStatic = true;
            SRWorkControl.Instance.GetReconstructStaticConvexCollider(out outColliders, false);
            foreach (MeshCollider mc in outColliders)
                mc.gameObject.isStatic = true;
        }

        static void _getWallsByMaxArea(List<MeshCollider> getReconColliderList, out List<MeshSizeData> meshSizeDataList, out List<MeshCollider> ignoreList)
        {
            //SRWorkControl.Instance.GetReconstructStaticConvexCollider(out getReconColliderList);
            //SRWorkControl.Instance.GetReconstructStaticQuadCollider();
            ignoreList = new List<MeshCollider>();
            meshSizeDataList = new List<MeshSizeData>();
            List<MeshCollider>.Enumerator itr = getReconColliderList.GetEnumerator();
            while (itr.MoveNext())
            {
                Vector3[] triNormal;
                //Ignore normal dot with up is near 0 (near 1 is wall, near 0 is ground)
                Vector3 normalLocal = _getAVGNormalLocal(itr.Current.sharedMesh, itr.Current.transform, out triNormal);
                Vector3 AVGNormalWorld = (normalLocal);

                //float dot = Vector3.Dot(Vector3.up, AVGNormalWorld);
                //if (-0.3f < dot && dot < 0.3f)
                float angle = Vector3.Angle(Vector3.up, AVGNormalWorld);
                //ignore not vertical with floor
                if (90 - 30 > angle || angle > 90 + 30)
                {
                    ignoreList.Add(itr.Current);
                    continue;
                }

                //ignore not face to player
                Vector3 meshCenter = itr.Current.transform.TransformPoint(_getMeshCenter(itr.Current.sharedMesh));
                Vector3 faceToNormal = GameManager.Instance.GetFaceToPlayerNormal(meshCenter, AVGNormalWorld);
                Vector3 dir2Player = ARRender.Instance.VRCamera.transform.position - meshCenter;
                angle = Vector3.Angle(dir2Player, faceToNormal);
                if (angle > 70)
                {
                    ignoreList.Add(itr.Current);
                    continue;
                }

#if UNITY_EDITOR
                Debug.DrawLine(meshCenter, meshCenter + AVGNormalWorld * 2, Color.red);
#endif

                MeshSizeData data = new MeshSizeData();
                data.area = GetMeshArea(itr.Current.sharedMesh);
                data.meshCenterWorld = meshCenter;
                data.colliderMesh = itr.Current;
                data.avgNormalWorld = AVGNormalWorld;
                data.triNormal = triNormal;
                meshSizeDataList.Add(data);
            }
            meshSizeDataList.Sort();
        }

        public void AutoPickFloor()
        {
            List<MeshSizeData> meshSizeDataList;
            List<MeshCollider> ignoreList;
            _getPlanesByMaxArea(reconstructConvexCollidersHorizontal, out meshSizeDataList, out ignoreList);

            //if (recontructTable != null)
            //    Destroy(recontructTable);
            //recontructTable = new GameObject("RecontructTable");

            if (recontructFloor != null)
                Destroy(recontructFloor);
            recontructFloor = new GameObject("RecontructFloor");

            if (floorPlaneObj != null)
                Destroy(floorPlaneObj);
            if (floorPlaneObjCollider != null)
                Destroy(floorPlaneObjCollider);

            //Get the lowest is the floor and y must -0.5~0.5
            MeshSizeData lowestMesh = null;
            float lowestD = 9999f;
            for (int a = 0; a < meshSizeDataList.Count; a++)
            {
                if (a >= 3)
                    break;
                MeshSizeData current = meshSizeDataList[a];

                if (current.meshCenterWorld.y < lowestD)
                {

                    //if (-0.6f < current.meshCenterWorld.y && current.meshCenterWorld.y < 0.6f)
                    {
                        lowestD = current.meshCenterWorld.y;
                        lowestMesh = current;
                    }
                }
            }
            if (lowestMesh != null)
            {
                GameObject floorConvexObj = SetPivotInMeshCenter(
                    lowestMesh.colliderMesh.transform,
                    lowestMesh.colliderMesh.sharedMesh,
                     recontructFloor.transform,
                     null,
                     lowestMesh.colliderMesh.name + "_convex : " + lowestMesh.area,
                     lowestMesh.avgNormalWorld,
                     lowestMesh.meshCenterWorld);
                meshSizeDataList.Remove(lowestMesh);

                //Add floor quad for shadow rendering
                foreach (MeshCollider mc in reconstructQuadCollidersHorizontal)
                {
                    string namesQuad = mc.name.Remove(0, mc.name.IndexOf('_') + 1);
                    string namesConvex = lowestMesh.colliderMesh.name.Remove(0, lowestMesh.colliderMesh.name.IndexOf('_') + 1);
                    if (namesQuad == namesConvex)
                    {
                        //Vector3[] planeTriNormal;
                        //Vector3 AVGNormalWorld = -_getAVGNormalLocal(mc.sharedMesh, mc.transform, out planeTriNormal);
                        Vector3 center = mc.transform.TransformPoint(_getMeshCenter(mc.sharedMesh));
                        //GameObject floorPlaneObj = SetPivotInMeshCenter(mc.transform, mc.sharedMesh,
                        //                                              floorConvexObj.transform, floorColliderMaterial,
                        //                                              lowestMesh.colliderMesh.name + "_quadFloor",
                        //                                              AVGNormalWorld, center);
                        floorPlaneObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        floorPlaneObj.name = lowestMesh.colliderMesh.name + "_quadFloor";
                        floorPlaneObj.transform.parent = floorConvexObj.transform;
                        floorPlaneObj.transform.position = center;
                        floorPlaneObj.transform.rotation = Quaternion.identity;
                        floorPlaneObj.transform.localScale = Vector3.one * 2;

                        //SRWork 0.7.5.0 use scan mesh collider
                        floorPlaneObj.isStatic = true;

                        floorPlaneObj.layer = AdvanceRender.ScanLiveMeshLayer;
                        if (ARRender.ADVANCE_RENDER)
                        {
                            //In advance render floorPlaneObj is for render, so, we clone a new one for collision                            
                            floorPlaneObjCollider = GameObject.Instantiate(floorPlaneObj);
                            floorPlaneObjCollider.layer = AdvanceRender.MRCollisionFloorLayer;
                            floorPlaneObjCollider.transform.position = floorPlaneObj.transform.position;
                            floorPlaneObjCollider.transform.rotation = floorPlaneObj.transform.rotation;
                            ARRender.Instance.VRCamera.cullingMask = MyHelpLayer.RemoveMaskLayer(ARRender.Instance.VRCamera.cullingMask, AdvanceRender.MRCollisionFloorLayer);
                        }
                        else
                        {
                            floorPlaneObj.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("ViveSR/MeshCuller, Shadowed, Stencil"));
                            //floorPlaneObj.GetComponent<MeshRenderer>().enabled = false;
                        }
                        break;
                    }
                }

                //SRWork0.7.5.0 ConvexColliders default is turn off
                //turn off the original convex mesh's collider
                //foreach (MeshCollider mc in reconstructConvexCollidersHorizontal)
                //{
                //    string namesA = mc.name.Remove(0, mc.name.IndexOf('_') + 1);
                //    string namesB = lowestMesh.colliderMesh.name.Remove(0, lowestMesh.colliderMesh.name.IndexOf('_') + 1);
                //    //namesA = namesA.Remove(0, namesA.IndexOf('_'));//srwork 0.7 without '_' again
                //    //namesB = namesB.Remove(0, namesB.IndexOf('_'));
                //    if (namesA == namesB)
                //    {
                //        mc.transform.gameObject.SetActive(false);
                //    }
                //}
            }
            else
                Debug.LogWarning("[reconstructPickFloor] there are no reconstruct floor picked...");
            /*
                    //Get the highest is the table
                    MeshSizeData highestMesh = null;
                    float highestD = -9999f;
                    for (int a = 0; a < meshSizeDataList.Count; a++)
                    {
                        if (a >= 10)
                            break;
                        MeshSizeData current = meshSizeDataList[a];
                        if (current.meshCenterWorld.y > highestD &&
                            current.meshCenterWorld.y < VRCamera.transform.position.y//must not higher then player's head
                            )
                        {
                            highestD = current.meshCenterWorld.y;
                            highestMesh = current;
                        }
                    }
                    if (highestMesh != null)
                    {
                        SetPivotInMeshCenter(highestMesh.colliderMesh.transform, highestMesh.colliderMesh.sharedMesh,
                            recontructTable.transform, tableColliderMaterial,
                            highestMesh.colliderMesh.name + "_convex : " + highestMesh.area,
                            highestMesh.avgNormalWorld,
                            highestMesh.meshCenterWorld);
                        meshSizeDataList.Remove(highestMesh);
                    }
                    else
                        Debug.LogWarning("[reconstructPickFloor] there are no reconstruct table picked...");
                        */
        }

        static Vector3 _getAVGNormalLocal(Mesh mesh, Transform tranWorld, out Vector3[] triNormals)
        {
            Matrix4x4 worldMat = tranWorld.localToWorldMatrix;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            triNormals = new Vector3[mesh.triangles.Length / 3];

            // mesh.RecalculateNormals();
            for (int a = 0; a < triNormals.Length; a++)
            {
                int idA = triangles[a * 3 + 0];
                int idB = triangles[a * 3 + 1];
                int idC = triangles[a * 3 + 2];
                Vector3 A, B, C;
                A = worldMat.MultiplyPoint(vertices[idA]);
                B = worldMat.MultiplyPoint(vertices[idB]);
                C = worldMat.MultiplyPoint(vertices[idC]);
                Vector3 vecAB = B - A;
                Vector3 vecAC = C - A;
                triNormals[a] = Vector3.Cross(vecAC.normalized, vecAB.normalized);
                triNormals[a].Normalize();

                //   triNormals[a] = (mesh.normals[idA] + mesh.normals[idB] + mesh.normals[idC]) / 3f;
                //  triNormals[a].Normalize();
            }

            Vector3 normalAVG = Vector3.zero;
            for (int a = 0; a < triNormals.Length; a++)
            {
                normalAVG += triNormals[a];
                normalAVG *= 0.5f;
                normalAVG.Normalize();
            }
            //normalAVG /= (float)triNormals.Length;
            return normalAVG;
        }

        public static float GetMeshArea(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            float result = 0f;
            for (int p = 0; p < triangles.Length; p += 3)
            {
                result += (Vector3.Cross(vertices[triangles[p + 1]] - vertices[triangles[p]],
                            vertices[triangles[p + 2]] - vertices[triangles[p]])).magnitude;
            }
            result *= 0.5f;
            return result;
        }

        static Vector3 _getMeshCenter(Mesh mesh)
        {
            Vector3 center = Vector3.zero;
            Vector3[] vs = mesh.vertices;
            int a;
            for (a = 0; a < vs.Length; a++)
                center += vs[a];
            return center / (float)a;
        }

        static void _getPlanesByMaxArea(List<MeshCollider> getReconColliderList, out List<MeshSizeData> meshSizeDataList, out List<MeshCollider> ignoreList)
        {
            ignoreList = new List<MeshCollider>();
            meshSizeDataList = new List<MeshSizeData>();
            List<MeshCollider>.Enumerator itr = getReconColliderList.GetEnumerator();
            while (itr.MoveNext())
            {
                /*
                Vector3[] triNormal;
                //Ignore normal dot with up is near 0 (near 1 is wall, near 0 is ground)
                Vector3 normalLocal = _getAVGNormalLocal(itr.Current.sharedMesh, itr.Current.transform, out triNormal);
                Vector3 AVGNormalWorld = (normalLocal);
                //float dot = Vector3.Dot(Vector3.up, AVGNormalWorld);
                //if (-0.3f < dot && dot < 0.3f)
                float angle = Vector3.Angle(Vector3.up, AVGNormalWorld);
                //ignore not correspond with floor
                if (-20 > angle || angle > 20)
                {
                    ignoreList.Add(itr.Current);
                    continue;
                }
                */
                ////ignore not face to player
                Vector3 meshCenter = itr.Current.transform.TransformPoint(_getMeshCenter(itr.Current.sharedMesh));
                //Vector3 dir2Player = VRCamera.transform.position - meshCenter;
                //angle = Vector3.Angle(dir2Player, AVGNormalWorld);
                //if (Vector3.Dot(dir2Player, AVGNormalWorld) < 0)
                ////if(angle > 40)
                //{
                //    ignoreList.Add(itr.Current);
                //    continue;
                //}

                MeshSizeData data = new MeshSizeData();
                data.area = GetMeshArea(itr.Current.sharedMesh);
                data.meshCenterWorld = meshCenter;
                data.colliderMesh = itr.Current;
                data.avgNormalWorld = Vector3.zero;//AVGNormalWorld;
                data.triNormal = null;//triNormal;
                meshSizeDataList.Add(data);
            }
            meshSizeDataList.Sort();
        }

        public struct SelectWall
        {
            public GameObject selectWall;
            public GameObject origWall;
            public float area;
            public Vector3 normal, center;
            public void SetSelectColor()
            { selectWall.GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 0.5f, 0, 0.5f)); }
            public void SetUnselectColor()
            { selectWall.GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 0.5f, 0, 0.2f)); }
        }
        List<SelectWall> wallCandidates = new List<SelectWall>();

        void _cloneWallCandidate(ReconstructManager.MeshSizeData current)
        {
            bool alreadyClone = false;
            foreach (SelectWall wall in wallCandidates)
            {
                wall.SetUnselectColor();
                if (wall.origWall.GetHashCode() == current.colliderMesh.gameObject.GetHashCode())
                {
                    alreadyClone = true;
                    break;
                }
            }
            if (!alreadyClone)
            {
                GameObject cloneobj = Instantiate(current.colliderMesh.gameObject);
                cloneobj.GetComponent<Renderer>().material = Instantiate<Material>(selectWallMaterial);
                cloneobj.GetComponent<Renderer>().enabled = true;
                SelectWall selectwall;
                selectwall.origWall = current.colliderMesh.gameObject;
                selectwall.selectWall = cloneobj;
                selectwall.area = current.area;
                selectwall.normal = current.avgNormalWorld;
                selectwall.center = current.meshCenterWorld;
                wallCandidates.Add(selectwall);
                ARRender.Instance.AddUnityrenderWithDepth(cloneobj.transform);
                cloneobj.layer = ARRender.UnityRenderOnTopNoShadowLayer;

                MeshFilter mf = cloneobj.GetComponent<MeshFilter>();
                mf.mesh.SetIndices(mf.mesh.GetIndices(0).Concat(mf.mesh.GetIndices(0).Reverse()).ToArray(), MeshTopology.Triangles, 0);

                Collider col = cloneobj.GetComponent<Collider>();
                if (col != null)
                    DestroyImmediate(col);
                cloneobj.AddComponent<MeshCollider>();
            }
        }

        public void ClearWallCandidate()
        {
            while (wallCandidates.Count > 0)
            {
                SelectWall selectwall = wallCandidates[0];
                wallCandidates.RemoveAt(0);
                ARRender.Instance.RemoveUnityrenderWithDepth(selectwall.selectWall.transform);
                Destroy(selectwall.selectWall);
            }
            //wallCandidates.Clear();
        }

        public List<SelectWall> GetWallCandidate()
        {
            List<ReconstructManager.MeshSizeData> meshSizeDataList;
            List<MeshCollider> ignoreList;
            _getWallsByMaxArea(reconstructConvexCollidersVertical, out meshSizeDataList, out ignoreList);

            if (selectedWallRoot != null)
                Destroy(selectedWallRoot);
            selectedWallRoot = new GameObject("SelectedWallRoot");

            for (int a = 0; a < meshSizeDataList.Count; a++)
            {
                ReconstructManager.MeshSizeData current = meshSizeDataList[a];

                //filter area less then 0.5
                if (current.area < 0.5f)
                    continue;
                _cloneWallCandidate(current);
            }
            return wallCandidates;
        }

        public void showSelectedWall()
        {
            if (selectedWallRoot != null)
            {
                for (int a = 0; a < selectedWallRoot.transform.childCount; a++)
                {
                    Transform child = selectedWallRoot.transform.GetChild(a);
                    //  if (alphaBlendObject.IndexOf(recontructWall.transform) < 0)
                    ARRender.Instance.AddUnityrenderWithDepth(child);
                }
                StartCoroutine(_closeRenderAlphaWall());
            }
        }

        IEnumerator _closeRenderAlphaWall()
        {
            yield return new WaitForSeconds(4);
            for (int a = 0; a < selectedWallRoot.transform.childCount; a++)
            {
                Transform child = selectedWallRoot.transform.GetChild(a);
                ARRender.Instance.RemoveUnityrenderWithDepth(child);
                MyHelpLayer.SetSceneLayer(child, ARRender.MRReconstructObjectLayer);
            }
        }

        public static GameObject SetPivotInMeshCenter(Transform tran, Mesh mesh,
              Transform parentNode, Material colliderMaterial, string name, Vector3 normal, Vector3 position)
        {
            //Duplicate my wall
            GameObject wall = new GameObject(name);// current.colliderMesh.name + " : " + current.area);
            wall.isStatic = true;
            wall.transform.parent = parentNode;

            //Set pivot in mesh center
            // Transform tran = current.colliderMesh.transform;
            //  Mesh mesh = current.colliderMesh.sharedMesh;
            /*Vector3[] newVert = new Vector3[mesh.vertices.Length];
            for (int b = 0; b < mesh.vertices.Length; b++)
            {
                newVert[b] = tran.TransformPoint(mesh.vertices[b]) - current.meshCenter;//move pos in world space
                newVert[b] = tran.InverseTransformPoint(newVert[b]);//set vertex to local space
            }*/

            Matrix4x4 origMatrix = tran.localToWorldMatrix;

            wall.transform.position = position;// current.meshCenter;
            wall.transform.rotation = (normal == Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(normal/*current.avgNormalWorld*/, Vector3.up);
            wall.transform.localScale = Vector3.one;
            Matrix4x4 newMatrix = wall.transform.localToWorldMatrix;

            Matrix4x4 offsetMat = Matrix4x4.Inverse(origMatrix) * newMatrix;

            Vector3[] newVert = new Vector3[mesh.vertices.Length];
            for (int b = 0; b < mesh.vertices.Length; b++)
            {
                newVert[b] = offsetMat.inverse.MultiplyPoint(mesh.vertices[b]);
            }

            ////Set normal for collider
            //Vector3[] newVertNormal = new Vector3[newVert.Length];
            //for (int b = 0; b < mesh.triangles.Length; b++)
            //{
            //    newVertNormal[mesh.triangles[b]] += current.triNormal[b % 3];
            //    newVertNormal[mesh.triangles[b]] *= 0.5f;
            //    newVertNormal[mesh.triangles[b]].Normalize();
            //}

            Mesh newM = new Mesh();
            newM.name = mesh.name;
            newM.vertices = newVert;
            // newM.normals = newVertNormal;//The supplied array needs to be the same size as the Mesh.vertices array.
            newM.uv = null;
            newM.SetTriangles(mesh.triangles, 0);
            newM.RecalculateNormals();

            MeshFilter newMF = wall.AddComponent<MeshFilter>();
            newMF.sharedMesh = newM;
            //MeshCollider newMC = wall.AddComponent<MeshCollider>();
            //newMC.convex = true;
            //newMC.gameObject.isStatic = true;        

            if (colliderMaterial != null)
            {
                MeshRenderer newMR = wall.AddComponent<MeshRenderer>();
                newMR.material = colliderMaterial;
            }

            wall.layer = ARRender.MRReconstructObjectLayer;
            ARRender.Instance.VRCamera.cullingMask = MyHelpLayer.RemoveMaskLayer(ARRender.Instance.VRCamera.cullingMask, ARRender.MRReconstructObjectLayer);
            return wall;
        }

        public void ActiveSelectWallPointer(bool active)
        {
            selectWallPointer.SetActive(active);
        }
    }
}