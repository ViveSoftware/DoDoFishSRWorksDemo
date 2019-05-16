//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using UnityEngine;

namespace HTC.UnityPlugin.Curved3DUI
{
    public static partial class Curved3D
    {
        public static void Transform(CurveStyle style, Vector3 targetPos, Quaternion targetRot, float radiusInverse, Vector3 centerPos, Quaternion centerRot, out Vector3 newPos, out Quaternion newRot)
        {
            switch (style)
            {
                case CurveStyle.Flat: FlatTransform(targetPos, targetRot, radiusInverse, centerPos, centerRot, out newPos, out newRot); return;
                case CurveStyle.Cylinder: CylinderTransform(targetPos, targetRot, radiusInverse, centerPos, centerRot, out newPos, out newRot); return;
                case CurveStyle.UVSphere: UVSphereTransform(targetPos, targetRot, radiusInverse, centerPos, centerRot, out newPos, out newRot); return;
                default:
                    newPos = targetPos;
                    newRot = targetRot;
                    return;
            }
        }

        public static void CylinderTransform(Vector3 targetPos, Quaternion targetRot, float radiusInverse, Vector3 centerPos, Quaternion centerRot, out Vector3 newPos, out Quaternion newRot)
        {
            var centerForward = centerRot * Vector3.forward;
            var centerUp = centerRot * Vector3.up;
            var centerRight = centerRot * Vector3.right;
            var centerToPoint = targetPos - centerPos;
            var centerSpacePoint = new Vector3(
                Vector3.Dot(centerToPoint, centerRight),
                Vector3.Dot(centerToPoint, centerUp),
                Vector3.Dot(centerToPoint, centerForward));

            var axisY = centerSpacePoint.z >= 0f ? centerUp : -centerUp;
            var scale = Mathf.Abs(centerSpacePoint.z) * radiusInverse;

            var rot = Quaternion.AngleAxis(Mathf.Rad2Deg * centerSpacePoint.x * radiusInverse, axisY);

            newPos = centerPos + rot * (centerForward * centerSpacePoint.z) + centerUp * (centerSpacePoint.y * scale);
            newRot = targetRot * rot;
        }

        public static void UVSphereTransform(Vector3 targetPos, Quaternion targetRot, float radiusInverse, Vector3 centerPos, Quaternion centerRot, out Vector3 newPos, out Quaternion newRot)
        {
            var centerForward = centerRot * Vector3.forward;
            var centerUp = centerRot * Vector3.up;
            var centerRight = centerRot * Vector3.right;
            var centerToPoint = targetPos - centerPos;
            var centerSpacePoint = new Vector3(
                Vector3.Dot(centerToPoint, centerRight),
                Vector3.Dot(centerToPoint, centerUp),
                Vector3.Dot(centerToPoint, centerForward));

            var axisY = centerSpacePoint.z >= 0f ? centerUp : -centerUp;
            var axisX = centerSpacePoint.z >= 0f ? -centerRight : centerRight;

            var rot = Quaternion.AngleAxis(Mathf.Rad2Deg * centerSpacePoint.x * radiusInverse, axisY) * Quaternion.AngleAxis(Mathf.Rad2Deg * centerSpacePoint.y * radiusInverse, axisX);

            newPos = centerPos + rot * (centerForward * centerSpacePoint.z);
            newRot = targetRot * rot;
        }

        //public static void RingTransform(Vector3 targetPos, Quaternion targetRot, float radiusInverse, Vector3 centerPos, Quaternion centerRot, out Vector3 newPos, out Quaternion newRot)
        //{
        //    var centerForward = centerRot * Vector3.forward;
        //    var centerUp = centerRot * Vector3.up;
        //    var centerRight = centerRot * Vector3.right;
        //    var centerToPoint = targetPos - centerPos;
        //    var centerSpacePoint = new Vector3(
        //        Vector3.Dot(centerToPoint, centerRight),
        //        Vector3.Dot(centerToPoint, centerUp),
        //        Vector3.Dot(centerToPoint, centerForward));

            
        //}

        public static void FlatTransform(Vector3 targetPos, Quaternion targetRot, float radiusInverse, Vector3 centerPos, Quaternion centerRot, out Vector3 newPos, out Quaternion newRot)
        {
            var centerForward = centerRot * Vector3.forward;
            var centerUp = centerRot * Vector3.up;
            var centerRight = centerRot * Vector3.right;
            var centerToPoint = targetPos - centerPos;
            var centerSpacePoint = new Vector3(
                Vector3.Dot(centerToPoint, centerRight),
                Vector3.Dot(centerToPoint, centerUp),
                Vector3.Dot(centerToPoint, centerForward));

            var scale = Mathf.Abs(centerSpacePoint.z) * radiusInverse;

            newPos = centerPos + centerForward * centerSpacePoint.z + centerRight * (centerSpacePoint.x * scale) + centerUp * (centerSpacePoint.y * scale);
            newRot = targetRot;
        }

        public static void InverseTransform(CurveStyle style, Vector3 targetPos, Quaternion targetRot, float radius, Vector3 centerPos, Quaternion centerRot, out Vector3 newPos, out Quaternion newRot)
        {
            switch (style)
            {
                case CurveStyle.Flat: FlatInverseTransform(targetPos, targetRot, radius, centerPos, centerRot, out newPos, out newRot); return;
                case CurveStyle.Cylinder: CylinderInverseTransform(targetPos, targetRot, radius, centerPos, centerRot, out newPos, out newRot); return;
                case CurveStyle.UVSphere: UVSphereInverseTransform(targetPos, targetRot, radius, centerPos, centerRot, out newPos, out newRot); return;
                default:
                    newPos = targetPos;
                    newRot = Quaternion.identity;
                    return;
            }
        }

        public static void CylinderInverseTransform(Vector3 targetPos, Quaternion targetRot, float radius, Vector3 centerPos, Quaternion centerRot, out Vector3 newPos, out Quaternion newRot)
        {
            var centerForward = centerRot * Vector3.forward;
            var centerUp = centerRot * Vector3.up;
            var centerRight = centerRot * Vector3.right;
            var centerToPoint = targetPos - centerPos;
            var centerSpacePoint = new Vector3(
                Vector3.Dot(centerToPoint, centerRight),
                Vector3.Dot(centerToPoint, centerUp),
                Vector3.Dot(centerToPoint, centerForward));
            var pointRadius = centerToPoint.magnitude;

            var vectorOnPlane = Vector3.ProjectOnPlane(centerToPoint, centerUp);

            var degreeX = Mathf.Sign(centerSpacePoint.x) * Vector3.Angle(centerForward, vectorOnPlane) * Mathf.Deg2Rad;

            newPos = centerPos + centerForward * Mathf.Sign(centerSpacePoint.z) * pointRadius + centerRight * degreeX * radius + centerUp * (centerSpacePoint.y * radius / pointRadius);
            newRot = targetRot * Quaternion.FromToRotation(vectorOnPlane, centerForward);
        }

        public static void UVSphereInverseTransform(Vector3 targetPos, Quaternion targetRot, float radius, Vector3 centerPos, Quaternion centerRot, out Vector3 newPos, out Quaternion newRot)
        {
            var centerForward = centerRot * Vector3.forward;
            var centerUp = centerRot * Vector3.up;
            var centerRight = centerRot * Vector3.right;
            var centerToPoint = targetPos - centerPos;
            var centerSpacePoint = new Vector3(
                Vector3.Dot(centerToPoint, centerRight),
                Vector3.Dot(centerToPoint, centerUp),
                Vector3.Dot(centerToPoint, centerForward));
            var pointRadius = centerToPoint.magnitude;

            var vectorOnPlane = Vector3.ProjectOnPlane(centerToPoint, centerUp);

            var degreeX = Mathf.Sign(centerSpacePoint.x) * Vector3.Angle(centerForward, vectorOnPlane) * Mathf.Deg2Rad;
            var degreeY = Mathf.Sign(centerSpacePoint.y) * Vector3.Angle(vectorOnPlane, centerToPoint) * Mathf.Deg2Rad;

            newPos = centerPos + centerForward * Mathf.Sign(centerSpacePoint.z) * pointRadius + centerRight * degreeX * radius + centerUp * degreeY * radius;
            newRot = targetRot * Quaternion.FromToRotation(centerToPoint, centerForward);
        }

        public static void FlatInverseTransform(Vector3 targetPos, Quaternion targetRot, float radius, Vector3 centerPos, Quaternion centerRot, out Vector3 newPos, out Quaternion newRot)
        {
            var centerForward = centerRot * Vector3.forward;
            var centerUp = centerRot * Vector3.up;
            var centerRight = centerRot * Vector3.right;
            var centerToPoint = targetPos - centerPos;
            var centerSpacePoint = new Vector3(
                Vector3.Dot(centerToPoint, centerRight),
                Vector3.Dot(centerToPoint, centerUp),
                Vector3.Dot(centerToPoint, centerForward));
            var pointRadius = centerToPoint.magnitude;

            newPos = centerPos + centerForward * Mathf.Sign(centerSpacePoint.z) * pointRadius + centerRight * (centerSpacePoint.x * radius / pointRadius) + centerUp * (centerSpacePoint.y * radius / pointRadius);
            newRot = targetRot;
        }
    }
}