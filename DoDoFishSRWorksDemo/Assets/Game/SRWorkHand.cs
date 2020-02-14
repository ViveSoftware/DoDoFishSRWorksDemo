#define UseHandtracking
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vive.Plugin.SR;

public class SRWorkHand : MySingleton<SRWorkHand>
{
    //public SkinnedMeshRenderer skinHand;
    [SerializeField] ViveHandTracking.HandRenderer HandrenderL;
    [SerializeField] ViveHandTracking.HandRenderer HandrenderR;

    public bool considerSkinColor = false;
    public bool showHand = true;
    [Range(0.01f, 0.5f)]
    public float handSizeRange = 0.15f;
    public const float Near = 0f, Far = 0.7f;
    public bool DebugShow;
    public Color farDebugColor = Color.red;

    [Header("Hand----------------------------------------")]
    public GameObject handObjNear;
    public GameObject handObjFar;

    const string DynamicHandName = "Depth Collider";

#if !UseHandtracking
    static bool isEnableDepth;
    public static void OpenDynamicHandCollider()
    {
        if (isEnableDepth)
            return;
        isEnableDepth = true;
        ViveSR_DualCameraImageCapture.EnableDepthProcess(true);
        ViveSR_DualCameraDepthCollider.UpdateDepthCollider = true;
        //ViveSR_DualCameraDepthCollider.SetColliderEnable(isOn);
        ViveSR_DualCameraDepthCollider.UpdateDepthColliderRange = true;
        ViveSR_DualCameraDepthCollider.UpdateColliderNearDistance = Near;
        ViveSR_DualCameraDepthCollider.UpdateColliderFarDistance = Far;
        _dynamicScanMesh = null;
        ViveSR_DualCameraDepthCollider.ColliderMeshVisibility = true;
        Debug.Log("[SRWorkHand] OpenDynamicHandCollider");
    }

    public static void CloseDynamicHandCollider()
    {
        if (!isEnableDepth)
            return;
        isEnableDepth = false;

        ViveSR_DualCameraImageCapture.EnableDepthProcess(false);
        ViveSR_DualCameraDepthCollider.UpdateDepthCollider = false;
        ViveSR_DualCameraDepthCollider.UpdateDepthColliderRange = false;
        ViveSR_DualCameraDepthCollider.UpdateColliderNearDistance = Near;
        ViveSR_DualCameraDepthCollider.UpdateColliderFarDistance = Far;
        _dynamicScanMesh = null;
        _getDynamicScanMesh().Clear();
        Debug.Log("[SRWorkHand] CloseDynamicHandCollider");
    }
#endif

    public static Transform GetDynamicHand()
    {
        if (_colliderHandObjs == null)
            return null;
        return _colliderHandObjs.transform;
        //Transform handCollider;
        //MyHelpNode.FindTransform(SRWorkControl.Instance.viveSR.transform, DynamicHandName, out handCollider);
        //return handCollider;
    }


    static Transform _getDynamicScanObj()
    {
        //換成手的skin mesh
#if UseHandtracking
        //return SRWorkHand.Instance.skinHand.transform;
        //return SRWorkHand.Instance.Handrender.transform.GetChild(0);
        return null;
#else
        {
            Transform handCollider;
            MyHelpNode.FindTransform(SRWorkControl.Instance.viveSR.transform, DynamicHandName, out handCollider);
            return handCollider;
        }
#endif
    }

    static Mesh _dynamicScanMesh;
    static Mesh _getDynamicScanMesh()
    {
#if UseHandtracking
        if (_dynamicScanMesh == null)
        {
            _dynamicScanMesh = _getDynamicScanObj().GetComponentInChildren<MeshFilter>().mesh;
        }
        //if (_dynamicScanMesh != null)
        //{
        //    _dynamicScanMesh.Clear();
        //    Destroy(_dynamicScanMesh);
        //}
        //_dynamicScanMesh = new Mesh();
        //_getDynamicScanObj().GetComponent<SkinnedMeshRenderer>().BakeMesh(_dynamicScanMesh);
#else
        if (_dynamicScanMesh == null)
        {
            _dynamicScanMesh = _getDynamicScanObj().GetComponent<MeshFilter>().mesh;
        }
#endif
        return _dynamicScanMesh;
    }

    [Range(0, 5)]
    public float lowPassRatio = 0.2f;
    //[Range(0, 2)]
    //public float dynamicHandDisThreshold = 0.3f;
    TextMesh _showDebug;
    Vector3 oldNearPoint, oldFarPoint;
    GameObject nearDebug, farDebug;
    //DataInfo[] _dataInfoUndistorted;
    byte[] _dummyLeftTex;
    //byte[] _getDummyLeftTex()
    //{
    //    if (_dataInfoUndistorted == null)
    //    {
    //        object obj = MyReflection.GetStaticMemberVariable(typeof(ViveSR_DualCameraImageCapture), "DataInfoUndistorted");
    //        if (obj == null)
    //        {
    //            Debug.LogError("[SRWorkHand] get DataInfoUndistorted null");
    //            return null;
    //        }
    //        _dataInfoUndistorted = obj as DataInfo[];
    //    }
    //    IntPtr ptr = _dataInfoUndistorted[(int)SeeThroughDataMask.UNDISTORTED_FRAME_LEFT].ptr;
    //    if (_dummyLeftTex == null)
    //        _dummyLeftTex = new byte[ViveSR_DualCameraImageCapture.UndistortedImageWidth * ViveSR_DualCameraImageCapture.UndistortedImageHeight * ViveSR_DualCameraImageCapture.UndistortedImageChannel];
    //    System.Runtime.InteropServices.Marshal.Copy(ptr, _dummyLeftTex, 0, _dummyLeftTex.Length);
    //    return _dummyLeftTex;
    //}

    const bool renderSkinVert = false;
    const int capW = 780, capH = 880, shiftX = 15;//The screen capture range, try&error to get these value
    const int avg_cb = 120;//YCbCr the average skin color 'cb'
    const int avg_cr = 155;//YCbCr the average skin color 'cr'
    const int skinRange = 22;//YCbCr the range of skin color
    bool IsSkinColorVert(Vector3 v, byte[] colorRGB)
    {
        Camera leftImageCamera = ViveSR_DualCameraRig.Instance.DualCameraLeft;

        Vector3 worldV = SRWorkControl.Instance.viveSR.transform.TransformPoint(v);
        //Vector3 screenP = leftImageCamera.WorldToScreenPoint(worldV);
        Vector3 viewP = leftImageCamera.WorldToViewportPoint(worldV);
        viewP.y = 1 - viewP.y;

        int startX = (int)((float)(ViveSR_DualCameraImageCapture.UndistortedImageWidth - capW) * 0.5f);
        int pixW = (int)(viewP.x * (float)capW);
        int pixX = startX + pixW + shiftX;

        int startY = (int)((float)(ViveSR_DualCameraImageCapture.UndistortedImageHeight - capH) * 0.5f);
        int pixH = (int)(viewP.y * (float)capH);
        int pixY = startY + pixH;

        if (viewP.x < 0 || viewP.y < 0 || pixX < 0 || pixY < 0)
            return false;
        if (pixX >= ViveSR_DualCameraImageCapture.UndistortedImageWidth || pixY >= ViveSR_DualCameraImageCapture.UndistortedImageHeight)
            return false;

        // Texture2D textureUndistortedLeft = (Texture2D)LeftPlaneRenderer.material.mainTexture;
        // byte[] leftTex = textureUndistortedLeft.GetRawTextureData();

        int loc = pixY * ViveSR_DualCameraImageCapture.UndistortedImageWidth + (int)pixX;
        byte r = 0, g = 0, b = 0;
        try
        {
            /* Color color = textureUndistortedLeft.GetPixel(pixX, pixY);
             r = (byte)(color.r * 255f);
             g = (byte)(color.g * 255f);
             b = (byte)(color.b * 255f);*/

            r = colorRGB[loc * 4 + 0];
            g = colorRGB[loc * 4 + 1];
            b = colorRGB[loc * 4 + 2];
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Debug.LogWarning("pixX : " + pixX + ", pixY : " + pixY);
            Debug.LogWarning("viewP : " + viewP);
        }

        //https://ccw1986.blogspot.tw/2012/11/opencvycbcr.html
        double cb, cr, y;
        y = (16 + r * 0.257f + g * 0.504f + b * 0.098f);
        cb = (128 - r * 0.148f - g * 0.291f + b * 0.439f);
        cr = (128 + r * 0.439f - g * 0.368f - b * 0.071);
        if ((cb > avg_cb - skinRange && cb < avg_cb + skinRange) &&
            (cr > avg_cr - skinRange && cr < avg_cr + skinRange))
        {
            if (renderSkinVert)
            {
                _dummyLeftTex[loc * 4 + 0] =
                _dummyLeftTex[loc * 4 + 1] =
                _dummyLeftTex[loc * 4 + 2] = 255;
            }
            return true;
        }
        return false;
    }

    class FindHandData : IComparable<FindHandData>
    {
        public float cameraPlaneDis;
        public Vector3 vert;
        public int CompareTo(FindHandData other)
        {
            if (this.cameraPlaneDis < other.cameraPlaneDis)
                return -1;
            return 1;
        }
    }

    [HideInInspector]
    public Vector3 HandNear, HandFar;

    List<FindHandData> findHandDataList = new List<FindHandData>();
    //List<int> skinColorVecID = new List<int>(2048);
    List<Vector3> tempVertices = new List<Vector3>(4096);

    void _findHandFromVerticesUpdate2(int[] triangles, Vector3[] vertices, out Vector3 outNearPoint, out Vector3 outFarPoint, out Quaternion handRot)
    {
        //handSize = 0;
        if (_colliderHandMeshes != null)
            _colliderHandMeshes.sharedMesh.Clear();

        Vector3 newNearPoint, newFarPoint;
        //MyHelpMesh.GetLineFromVertices(null, vertices, out newNearPoint, out newFarPoint);

        //get 10 point which is nearest with camera plane
        findHandDataList.Clear();
        Vector3 localEyePos = SRWorkControl.Instance.viveSR.transform.InverseTransformPoint(SRWorkControl.Instance.eye.position);
        Vector3 localEyeDir = SRWorkControl.Instance.viveSR.transform.InverseTransformDirection(SRWorkControl.Instance.eye.forward);

        Vector3 cameraNormal = localEyeDir;
        //cameraNormal.y = 0; cameraNormal.Normalize();
        Plane cameraPlane = new Plane(cameraNormal, localEyePos);

        byte[] colorRGB = null;
        tempVertices.Clear();
        //if (considerSkinColor)
        //{
        //    int vid = 0;
        //    colorRGB = _getDummyLeftTex();
        //    foreach (Vector3 v in vertices)
        //    {
        //        if (IsSkinColorVert(v, colorRGB))
        //            tempVertices.Add(vertices[vid]);
        //        vid++;
        //    }

        //    if (renderSkinVert)
        //    {
        //        Texture2D tex = ViveSR_DualCameraRig.Instance.DualCameraImageRenderer.UndistortedLeft[0].mainTexture as Texture2D;
        //        tex.LoadRawTextureData(_dummyLeftTex);
        //        tex.Apply();

        //        //byte[] bytes = tex.EncodeToJPG();
        //        //System.IO.File.WriteAllBytes("d:/234.jpg", bytes);
        //        //UnityEditor.EditorApplication.isPaused = true;
        //    }
        //}
        //else
        tempVertices.AddRange(vertices);

        //consider nearest from all dynamic vertices
        //foreach (Vector3 v in vertices)
        foreach (Vector3 v in tempVertices)
        {
            //limit nearest not too high
            if (Mathf.Abs(v.y - localEyePos.y) > 0.4f)
                continue;

            FindHandData data = new FindHandData();
            data.cameraPlaneDis = cameraPlane.GetDistanceToPoint(v);
            data.vert = v;
            findHandDataList.Add(data);
        }
        findHandDataList.Sort();

        //get the closest points.
        float nearestDis = 99999;
        FindHandData nearestData = null;
        for (int a = 0; a < 10; a++)
        {
            if (a == findHandDataList.Count)
                break;
            float sqrDis = (findHandDataList[a].vert - localEyePos).sqrMagnitude;
            if (sqrDis < nearestDis)
            {
                nearestDis = sqrDis;
                nearestData = findHandDataList[a];
            }
        }

        if (nearestData == null)
        {
            outNearPoint = oldNearPoint;
            outFarPoint = oldFarPoint;
            handRot = Quaternion.identity;
            return;
        }

        newNearPoint = nearestData.vert;

        //get 10 point which is farest with camera plane
        findHandDataList.Clear();
        foreach (Vector3 v in tempVertices)//get farest point from skin color vertices
        {
            //limit farest not too high
            if (Mathf.Abs(v.y - localEyePos.y) > 0.5f)
            {
                continue;
            }

            FindHandData data = new FindHandData();
            data.cameraPlaneDis = cameraPlane.GetDistanceToPoint(v);
            data.vert = v;
            findHandDataList.Add(data);
        }
        findHandDataList.Sort();

        //get the point which is farest from newNearPoint
        float maxDis = 0;
        newFarPoint = newNearPoint;
        for (int a = 0; a < findHandDataList.Count; a++)
        {
            //  if (a == 10)//there are too many other point, so cannot limit the amount
            //      break;

            FindHandData data = findHandDataList[findHandDataList.Count - a - 1];

            float sqrDis = (data.vert - newNearPoint).sqrMagnitude;
            //float dddd = (data.vert - newNearPoint).magnitude;
            if (sqrDis > maxDis && sqrDis < 0.5f * 0.5f)
            {
                maxDis = sqrDis;
                newFarPoint = data.vert;
            }
        }

        //fix the hand direction by camera forward
        Vector3 newDirection = newFarPoint - newNearPoint;
        if (Vector3.Dot(localEyeDir, newDirection.normalized) < 0)
        {
            Vector3 rec = newNearPoint;
            newNearPoint = newFarPoint;
            newFarPoint = rec;
        }

        newNearPoint = Vector3.Lerp(oldNearPoint, newNearPoint, lowPassRatio);
        newFarPoint = Vector3.Lerp(oldFarPoint, newFarPoint, lowPassRatio);

        //save old position
        oldFarPoint = newFarPoint;
        oldNearPoint = newNearPoint;

        //get hand size in range
        //float handSize = 0;
        List<int> handIB = new List<int>();
        List<Vector3> handVB = new List<Vector3>();
        int count = 0;
        for (int a = 0; a < triangles.Length; a += 3)
        {
            Vector3 vA = vertices[triangles[a + 0]];
            Vector3 vB = vertices[triangles[a + 1]];
            Vector3 vC = vertices[triangles[a + 2]];

            //if (considerSkinColor)
            //{
            //    if (!IsSkinColorVert(vA, colorRGB) &&
            //       !IsSkinColorVert(vB, colorRGB) &&
            //       !IsSkinColorVert(vC, colorRGB)
            //        )
            //        continue;
            //}

            Vector3 dA = newFarPoint - vA;
            Vector3 dB = newFarPoint - vB;
            Vector3 dC = newFarPoint - vC;

            if (
                dA.sqrMagnitude < handSizeRange * handSizeRange ||
                dB.sqrMagnitude < handSizeRange * handSizeRange ||
                dC.sqrMagnitude < handSizeRange * handSizeRange)
            {
                //get area
                //https://answers.unity.com/questions/291923/area-of-a-triangle-this-code-seems-to-work-why.html
                //Vector3 V = Vector3.Cross(vA - vB, vA - vC);
                //handSize += V.magnitude * 0.5f;

                handVB.Add(vA);
                handVB.Add(vB);
                handVB.Add(vC);

                handIB.Add(count + 0);
                handIB.Add(count + 1);
                handIB.Add(count + 2);
                count += 3;
            }
        }

        //recalculate far point
        if (handVB.Count > 0)
        {
            newFarPoint = Vector3.zero;
            foreach (Vector3 v in handVB)
                newFarPoint += v;
            newFarPoint /= handVB.Count;
        }

        //set point to world coordinate
        HandFar = outFarPoint = newFarPoint;// + SRWorkControl.Instance.viveSR.transform.position;
        HandNear = outNearPoint = newNearPoint;// + SRWorkControl.Instance.viveSR.transform.position;
        handRot = Quaternion.identity;

        if (showHand)
        {
            _colliderHandMeshes.sharedMesh.Clear();
            if (handVB.Count > 0)
            {
                for (int a = 0; a < handVB.Count; a++)
                    handVB[a] += SRWorkControl.Instance.viveSR.transform.position;
                _colliderHandMeshes.sharedMesh.SetVertices(handVB);
                _colliderHandMeshes.sharedMesh.SetIndices(handIB.ToArray(), MeshTopology.Triangles, 0);
                _colliderHandMeshRenderer.material.color = farDebugColor;
            }
            _colliderHandObjs.GetComponent<MeshRenderer>().enabled = true;
            _getDynamicScanObj().GetComponent<Renderer>().enabled = false;
        }
        else
        {
            _colliderHandObjs.GetComponent<MeshRenderer>().enabled = false;
            _getDynamicScanObj().GetComponent<Renderer>().enabled = true;
        }


        _showDebug.gameObject.SetActive(false);
        if (Debug.isDebugBuild && DebugShow)
        {
            if (nearDebug == null)
                nearDebug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (farDebug == null)
                farDebug = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nearDebug.transform.localScale = Vector3.one * 0.05f;
            farDebug.transform.localScale = Vector3.one * 0.05f;
            //Vector3.Lerp(Vector3.one * handSize, farDebug.transform.localScale, 0.1f);

            nearDebug.transform.position = outNearPoint + SRWorkControl.Instance.viveSR.transform.position;
            farDebug.transform.position = outFarPoint + SRWorkControl.Instance.viveSR.transform.position;
            nearDebug.GetComponent<Renderer>().material.color = Color.red;
            farDebug.GetComponent<Renderer>().material.color = Color.red;

            Destroy(nearDebug.GetComponent<Collider>());
            Destroy(farDebug.GetComponent<Collider>());

            float farDis = cameraPlane.GetDistanceToPoint(HandFar);

            _showDebug.gameObject.SetActive(true);
            _showDebug.text =
                //"handSize : " + handSize + Environment.NewLine +
                "farDis : " + farDis;
        }
    }



    private static GameObject _colliderHandObjs;
    private static MeshFilter _colliderHandMeshes = new MeshFilter();
    private static MeshRenderer _colliderHandMeshRenderer;
    void _createHandDetectMesh()
    {
        if (_colliderHandObjs != null)
        {
            _colliderHandMeshes.sharedMesh.Clear();
            return;
        }
        _colliderHandObjs = new GameObject("SRWorksHand");
        _colliderHandObjs.transform.SetParent(gameObject.transform, false);

        _colliderHandMeshes = _colliderHandObjs.AddComponent<MeshFilter>();
        _colliderHandMeshes.mesh = new Mesh();
        _colliderHandMeshes.mesh.MarkDynamic();

        _colliderHandMeshRenderer = _colliderHandObjs.AddComponent<MeshRenderer>();
        _colliderHandMeshRenderer.material = new Material(Shader.Find("ViveSR/Wireframe"))
        {
            color = new Color(0f, 0.94f, 0f)
        };
        _colliderHandMeshes.sharedMesh.Clear();
    }

    public Camera GestureCamera;
    bool isDetectingHand;
    public void SetDetectHand()
    {
        if (isDetectingHand)
            return;
        isDetectingHand = true;

        Debug.Log("SetDetectHand");

#if UseHandtracking
        //SRWorkControl.Instance.CloseDepth();
        GestureCamera.GetComponent<ViveHandTracking.GestureProvider>().enabled = true;
        HandrenderL.transform.gameObject.SetActive(true);
        HandrenderR.transform.gameObject.SetActive(true);
#else
        SRWorkHand.OpenDynamicHandCollider();
        _createHandDetectMesh();
        _getDynamicScanObj().GetComponent<Collider>().enabled = false;
#endif

        if (_showDebug == null)
            _showDebug = MyHelpVR.CreateTextMeshOnHead(SRWorkControl.Instance.eye);
    }

    private void LateUpdate()
    {
        if (isDetectingHand)
        {
#if UseHandtracking
            handObjNear.SetActive(HandrenderL.transform.gameObject.activeInHierarchy);
            handObjFar.SetActive(HandrenderR.transform.gameObject.activeInHierarchy);

            handObjNear.transform.position = HandrenderL.transform.position + HandrenderL.transform.up * 0.1f;
            handObjNear.transform.rotation = Quaternion.identity;

            handObjFar.transform.position = HandrenderR.transform.position + HandrenderR.transform.up * 0.1f;
            handObjFar.transform.rotation = Quaternion.identity;
#else
            Mesh handMesh = SRWorkHand._getDynamicScanMesh();
            if (handMesh != null)
            {
                Vector3 outNearPoint, outFarPoint;
                Quaternion handRot = Quaternion.identity;
                SRWorkHand.Instance._findHandFromVerticesUpdate2(handMesh.triangles, handMesh.vertices, out outNearPoint, out outFarPoint, out handRot);
                if (handObjNear != null)
                {
                    handObjNear.transform.position = outNearPoint;
                    handObjNear.transform.rotation = handRot;
                }
                if (handObjFar != null)
                {
                    handObjFar.transform.position = outFarPoint;
                    handObjFar.transform.rotation = handRot;
                }
            }
#endif
        }
    }

    public void CloseDetectHand()
    {
        if (!isDetectingHand)
            return;
        isDetectingHand = false;
        Debug.Log("CloseDetectHand");

#if UseHandtracking
        GestureCamera.GetComponent<ViveHandTracking.GestureProvider>().enabled = false;
        HandrenderL.transform.gameObject.SetActive(false);
        HandrenderR.transform.gameObject.SetActive(false);
#else
        SRWorkHand.CloseDynamicHandCollider();
        // StopCoroutine(waitDetectHandCoroutine);
        //waitDetectHandCoroutine = null;
#endif
    }
}
