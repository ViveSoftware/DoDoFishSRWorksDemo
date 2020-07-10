using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneProjector : MonoBehaviour
{
    public enum PositionFitType
    {
        None,
        FromHandModel,
        FromHandRender,
    }
    public bool NotUpdateBone;
    public PositionFitType positionFitType = BoneProjector.PositionFitType.FromHandModel;
    [SerializeField] ViveHandTracking.HandRenderer handRenderer;
    //public float offsetRoot;
    public enum AXISTYPE
    {
        None,
        XYZ2_ZXY,
        XYZ2_YZX,
        XYZ2_nXYnZ,
        XYZ2_nZXnY,
    }
    public AXISTYPE rootAxisType = AXISTYPE.XYZ2_ZXY;
    //public AXISTYPE axisType = AXISTYPE.XYZ2_ZXY;

    public Transform RootBoneOrig, RootBoneDest;

    [Serializable]
    public class RelateBone
    {
        [HideInInspector] public Transform origHandrenderer;
        public Transform orig;
        public Transform dest;
        public PalmAxis palmAxis;//Important : this is the finger's axis to palm, the axis is base on RootBoneDest's axis

        [HideInInspector] public Vector3 recForward;
        [HideInInspector] public float recLength;
        [HideInInspector] public Transform recPose;
        [HideInInspector] public float? origDestLength;
    }
    //public RelateBone[] RelateBones;
    public RelateBone[] RelateThumb = new RelateBone[3];
    public RelateBone[] RelateIndex = new RelateBone[4];
    public RelateBone[] RelateMiddle = new RelateBone[4];
    public RelateBone[] RelateRing = new RelateBone[4];
    public RelateBone[] RelatePinky = new RelateBone[4];

    GameObject cloneDestHierarchy;
    void Start()
    {
        //duplicate another dest bone for wear glove pose.
        cloneDestHierarchy = Instantiate(RootBoneDest.gameObject);
        cloneDestHierarchy.transform.parent = transform;
        cloneDestHierarchy.name = "cloneDestHierarchy";
        cloneDestHierarchy.transform.localPosition = Vector3.zero;
        cloneDestHierarchy.transform.localRotation = Quaternion.identity;
        //cloneDestHierarchy.hideFlags = HideFlags.HideInHierarchy;

        foreach (RelateBone relate in RelateThumb)
            MyHelpNode.FindTransform(cloneDestHierarchy.transform, relate.dest.name, out relate.recPose);
        foreach (RelateBone relate in RelateIndex)
            MyHelpNode.FindTransform(cloneDestHierarchy.transform, relate.dest.name, out relate.recPose);
        foreach (RelateBone relate in RelateMiddle)
            MyHelpNode.FindTransform(cloneDestHierarchy.transform, relate.dest.name, out relate.recPose);
        foreach (RelateBone relate in RelateRing)
            MyHelpNode.FindTransform(cloneDestHierarchy.transform, relate.dest.name, out relate.recPose);
        foreach (RelateBone relate in RelatePinky)
            MyHelpNode.FindTransform(cloneDestHierarchy.transform, relate.dest.name, out relate.recPose);

        UpdateBonePose();


    }

    Quaternion AxisTranslate(AXISTYPE type, Quaternion rot)
    {
        if (type == AXISTYPE.None)
        {
            return rot;
        }

        Vector3 angles = rot.eulerAngles;

        if (type == AXISTYPE.XYZ2_ZXY)
        {
            Vector3 r = rot * Vector3.right;//X
            Vector3 u = rot * Vector3.up;//Y
            Vector3 f = rot * Vector3.forward;//Z
            Matrix4x4 mat = Matrix4x4.identity;
            mat.SetColumn(0, new Vector4(f.x, f.y, f.z, 0));
            mat.SetColumn(1, new Vector4(r.x, r.y, r.z, 0));
            mat.SetColumn(2, new Vector4(u.x, u.y, u.z, 0));
            return mat.rotation;
        }
        if (type == AXISTYPE.XYZ2_YZX)
        {
            Vector3 r = rot * Vector3.right;//X
            Vector3 u = rot * Vector3.up;//Y
            Vector3 f = rot * Vector3.forward;//Z
            Matrix4x4 mat = Matrix4x4.identity;
            mat.SetColumn(0, new Vector4(u.x, u.y, u.z, 0));
            mat.SetColumn(1, new Vector4(f.x, f.y, f.z, 0));
            mat.SetColumn(2, new Vector4(r.x, r.y, r.z, 0));
            return mat.rotation;
        }
        else if (type == AXISTYPE.XYZ2_nXYnZ)
        {
            angles.z *= -1;
            angles.x *= -1;
            return Quaternion.Euler(angles);
        }
        else if (type == AXISTYPE.XYZ2_nZXnY)
        {
            Vector3 r = rot * Vector3.right;//X
            Vector3 u = rot * Vector3.up;//Y
            Vector3 f = rot * Vector3.forward;//Z
            f *= -1f; u *= -1f;
            Matrix4x4 mat = Matrix4x4.identity;
            mat.SetColumn(0, new Vector4(f.x, f.y, f.z, 0));
            mat.SetColumn(1, new Vector4(r.x, r.y, r.z, 0));
            mat.SetColumn(2, new Vector4(u.x, u.y, u.z, 0));
            return mat.rotation;
        }
        return Quaternion.identity;
    }

    bool origHandrendererDone;
    void Update()
    {
        if (!NotUpdateBone)
        {
            UpdateBonePose();
        }

        if (positionFitType != PositionFitType.None)
        {
            if (positionFitType == PositionFitType.FromHandRender &&
                handRenderer != null && !origHandrendererDone)
            {
                origHandrendererDone = true;
                //1, 2, 2, 3, 3, 4, // thumb
                //5, 6, 6, 7, 7, 8, // index
                //9, 10, 10, 11, 11, 12, // middle
                //13, 14, 14, 15, 15, 16, // ring
                //17, 18, 18, 19, 19, 20, // pinky
                int count = 1;
                foreach (RelateBone rb in RelateThumb) { MyHelpNode.FindTransform(handRenderer.transform, "point" + count, out rb.origHandrenderer); count++; }
                count = 5;
                foreach (RelateBone rb in RelateIndex) { MyHelpNode.FindTransform(handRenderer.transform, "point" + count, out rb.origHandrenderer); count++; }
                count = 9;
                foreach (RelateBone rb in RelateMiddle) { MyHelpNode.FindTransform(handRenderer.transform, "point" + count, out rb.origHandrenderer); count++; }
                count = 13;
                foreach (RelateBone rb in RelateRing) { MyHelpNode.FindTransform(handRenderer.transform, "point" + count, out rb.origHandrenderer); count++; }
                count = 17;
                foreach (RelateBone rb in RelatePinky) { MyHelpNode.FindTransform(handRenderer.transform, "point" + count, out rb.origHandrenderer); count++; }
            }

            float averageScale = _setNextPosition(RelateThumb);
            averageScale += _setNextPosition(RelateIndex);
            averageScale += _setNextPosition(RelateMiddle);
            averageScale += _setNextPosition(RelateRing);
            averageScale += _setNextPosition(RelatePinky);
            averageScale /= 5f;
            RootBoneDest.localScale = Vector3.one * averageScale;
        }

        cloneDestHierarchy.transform.position =
        //RootBoneDest.position = 
        RootBoneOrig.position;

        cloneDestHierarchy.transform.rotation =
        //RootBoneDest.rotation =         
        AxisTranslate(rootAxisType, RootBoneOrig.rotation);

        // _setFinger(1, RelateBones.Length - 1);

        //_setFinger(20, 20); 系統根本沒在更新此bone的資訊，所以不要去update他
        //_setFinger(4, 4);

        //_setFinger(1, 3);
        //_setFinger(4, 7);
        //_setFinger(8, 11);
        //_setFinger(12, 15);
        //_setFinger(16, 19);

        _setFinger2(RelateThumb);
        _setFinger2(RelateIndex);
        _setFinger2(RelateMiddle);
        _setFinger2(RelateRing);
        _setFinger2(RelatePinky);
    }

    //void _setFinger(int start, int end, PalmAxis palmAxis)
    //{
    //    for (int a = start; a <= end; a++)
    //    {
    //        RelateBones[a].dest.localRotation = AxisTranslate(axisType, RelateBones[a].orig.localRotation);
    //    }
    //}

    public enum PalmAxis
    {
        X,
        Y,
        Z,
        nZ,
    }

    void _setFinger2(RelateBone[] RelateBones)
    {
        for (int a = 0; a < RelateBones.Length; a++)
        {
            if (a == RelateBones.Length - 1)
                break;
            RelateBone relate = RelateBones[a];
            RelateBone relateNext = RelateBones[a + 1];

            Vector3 forward = Vector3.zero;
            float length = 0;
            if (positionFitType == PositionFitType.FromHandRender && handRenderer != null)
            {
                forward = relateNext.origHandrenderer.position - relate.origHandrenderer.position;
                length = forward.magnitude;
                forward /= length;
            }
            else
            {
                forward = relateNext.orig.position - relate.orig.position;
                length = forward.magnitude;
                forward /= length;
            }

            //Init origLength BTW
            if (relate.origDestLength == null)
            {
                relate.origDestLength = (relateNext.dest.position - relate.dest.position).magnitude;
            }

            Vector3 r, f, u;
            r = f = u = Vector3.zero;
            if (relate.palmAxis == PalmAxis.Y)
            {
                Vector3 palmDir = RootBoneDest.up;//relate.dest.rotation * Vector3.up;
                r = forward;
                f = Vector3.Cross(forward, palmDir);
                u = Vector3.Cross(f, r);
            }
            else if (relate.palmAxis == PalmAxis.Z)
            {
                Vector3 palmDir = RootBoneDest.forward;//relate.dest.rotation * Vector3.forward;
                r = -forward;
                u = Vector3.Cross(forward, palmDir);
                f = Vector3.Cross(r, u);
            }
            else if (relate.palmAxis == PalmAxis.nZ)
            {
                Vector3 palmDir = -RootBoneDest.forward;//relate.dest.rotation * Vector3.forward;
                r = forward;
                f = Vector3.Cross(forward, palmDir); ;
                u = Vector3.Cross(palmDir, forward);
                u = Vector3.Cross(f, r);
            }

            Matrix4x4 mat = Matrix4x4.identity;
            mat.SetColumn(0, new Vector4(r.x, r.y, r.z, 0));
            mat.SetColumn(1, new Vector4(u.x, u.y, u.z, 0));
            mat.SetColumn(2, new Vector4(f.x, f.y, f.z, 0));

            //if (!NotUpdateBone)
            //    relate.dest.rotation = mat.rotation;
            relate.recPose.rotation = mat.rotation;
            relate.recForward = forward;
            relate.recLength = length;
        }
    }

    public void UpdateBonePose()
    {
        //update hand tracking pose to skin model bone
        RootBoneDest.position = cloneDestHierarchy.transform.position;
        RootBoneDest.rotation = cloneDestHierarchy.transform.rotation;
        foreach (RelateBone relate in RelateThumb)
            relate.dest.rotation = relate.recPose.rotation;
        foreach (RelateBone relate in RelateIndex)
            relate.dest.rotation = relate.recPose.rotation;
        foreach (RelateBone relate in RelateMiddle)
            relate.dest.rotation = relate.recPose.rotation;
        foreach (RelateBone relate in RelateRing)
            relate.dest.rotation = relate.recPose.rotation;
        foreach (RelateBone relate in RelatePinky)
            relate.dest.rotation = relate.recPose.rotation;
    }

    float _setNextPosition(RelateBone[] relateBones)
    {
        float averageScale = 1, totalOrigLength = 0, totalRecLength = 0;
        for (int a = 0; a < relateBones.Length; a++)
        {
            if (a == relateBones.Length - 1)
                break;
            RelateBone relate = relateBones[a];
            RelateBone relateNext = relateBones[a + 1];
            if (relateNext.origDestLength.HasValue)
            {
                totalOrigLength += relate.origDestLength.Value;
                totalRecLength += relate.recLength;
            }
        }
        if (totalOrigLength > 0)
            averageScale = totalRecLength / totalOrigLength;
        return averageScale;
        //relateBones[0].dest.localScale = Vector3.one * averageScale;

        //for (int a = 0; a < relateBones.Length; a++)
        //{
        //    if (a == 0)
        //        continue;
        //    RelateBone relate = relateBones[a];
        //    RelateBone relatePrev = relateBones[a - 1];
        //    relate.dest.position = relatePrev.dest.position + relatePrev.recForward * relatePrev.recLength;
        //}

    }
}
