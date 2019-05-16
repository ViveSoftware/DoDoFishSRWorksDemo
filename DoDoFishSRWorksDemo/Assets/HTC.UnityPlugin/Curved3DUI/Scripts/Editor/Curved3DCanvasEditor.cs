//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using UnityEditor;
using UnityEngine;

namespace HTC.UnityPlugin.Curved3DUI
{
    [CustomEditor(typeof(Curved3DCanvas))]
    [CanEditMultipleObjects]
    public class Curved3DCanvasEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        protected virtual void OnSceneGUI()
        {
            if (target == null) return;

            var color = new Color(0f, 1f, 1f, 0.5f);
            var script = target as Curved3DCanvas;

            var centerPos = script.transform.TransformPoint(script.centerPos);
            var centerRot = script.transform.rotation * Quaternion.Euler(script.centerRot);
            //var centerForward = script.worldCenterForward;
            var radius = script.transform.TransformVector(script.radius, 0f, 0f).magnitude;

            EditorGUI.BeginChangeCheck();
            var newPos = Handles.PositionHandle(centerPos, centerRot);
            var posChanged = EditorGUI.EndChangeCheck();

            EditorGUI.BeginChangeCheck();
            var newRot = Handles.RotationHandle(centerRot, centerPos);
            var rotChanged = EditorGUI.EndChangeCheck();

            EditorGUI.BeginChangeCheck();
            Handles.color = color;
            var newRadius = Handles.RadiusHandle(centerRot, centerPos, radius);
            var radiusChanged = EditorGUI.EndChangeCheck();

            if (posChanged || rotChanged || radiusChanged)
            {
                Undo.RecordObject(target, "Curve Center Changed");

                if (posChanged) { script.centerPos = script.transform.InverseTransformPoint(newPos); }
                if (rotChanged) { script.centerRot = (newRot * Quaternion.Inverse(script.transform.rotation)).eulerAngles; }
                if (radiusChanged) { script.radius = script.transform.InverseTransformVector(newRadius, 0f, 0f).magnitude; }

                EditorUtility.SetDirty(target);
            }
        }
    }
}