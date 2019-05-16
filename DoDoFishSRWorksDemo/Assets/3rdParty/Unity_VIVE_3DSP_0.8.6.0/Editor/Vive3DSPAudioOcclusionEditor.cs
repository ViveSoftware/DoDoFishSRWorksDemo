//====================== Copyright 2016-2017, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using UnityEditor;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    [CustomEditor(typeof(Vive3DSPAudioOcclusion))]
    [CanEditMultipleObjects]
    public class Vive3DSPAudioOcclusionEditor : Editor
    {
        private SerializedProperty occlusionEffect = null;
        private SerializedProperty occlusionMaterial = null;
        private SerializedProperty occlusionIntensity = null;
        private SerializedProperty highFreqAttenuation = null;
        private SerializedProperty lowFreqAttenuationRatio = null;
        private SerializedProperty occlusionEngine = null;
        private SerializedProperty raycastQuality = null;

        private GUIContent occlusionEffectLabel = new GUIContent("Occlusion Effect", 
            "ON or OFF occlusion effect");
        private GUIContent occlusionMaterialLabel = new GUIContent("Occlusion Material", 
            "Set material for occlusion object");
        private GUIContent occlusionIntensityLabel = new GUIContent("Occlusion Intensity", 
            "Set occlusion intensity");
        private GUIContent highFreqAttenuationTapLabel = new GUIContent("High Freq. Attenuation (dB)",
            "Set high frequency attenuation level, default cut-off frequency is 5kHz");
        private GUIContent lowFreqAttenuationRatioTapLabel = new GUIContent("Low Freq. Attenuation Ratio",
            "Set low frequency attenuation ratio");
        private GUIContent highFreqAttenuationLabel = new GUIContent(" ",
            "Set high frequency attenuation level, default cut-off frequency is 5kHz");
        private GUIContent lowFreqAttenuationRatioLabel = new GUIContent(" ",
            "Set low frequency attenuation ratio");
        private GUIContent surfaceMaterialLabel = new GUIContent("Room Surface Material",
            "Set room surface materials for reverb effect");
        private GUIContent occlusionEngineLabel = new GUIContent("Occlusion Engine",
            "Set occlusion Engine");
        private GUIContent raycastQualityLabel = new GUIContent("Quality",
            "Set occlusion quality in raycast mode");

        void OnEnable()
        {
            occlusionEffect = serializedObject.FindProperty("occlusionEffect");
            occlusionMaterial = serializedObject.FindProperty("occlusionMaterial");
            occlusionIntensity = serializedObject.FindProperty("occlusionIntensity");
            highFreqAttenuation = serializedObject.FindProperty("highFreqAttenuation");
            lowFreqAttenuationRatio = serializedObject.FindProperty("lowFreqAttenuationRatio");
            occlusionEngine = serializedObject.FindProperty("occlusionEngine");
            raycastQuality = serializedObject.FindProperty("raycastQuality");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(occlusionEffect, occlusionEffectLabel);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(occlusionMaterial, occlusionMaterialLabel);
            if (occlusionMaterial.enumValueIndex == (int)Vive3DSPAudio.OccMaterial.UserDefine)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.LabelField(highFreqAttenuationTapLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.Slider(highFreqAttenuation, -50.0f, 0.0f, highFreqAttenuationLabel);
                --EditorGUI.indentLevel;
                EditorGUILayout.LabelField(lowFreqAttenuationRatioTapLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.Slider(lowFreqAttenuationRatio, 0.0f, 1.0f, lowFreqAttenuationRatioLabel);
                --EditorGUI.indentLevel;
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.Separator();
            EditorGUILayout.Slider(occlusionIntensity, 1.0f, 2.0f, occlusionIntensityLabel);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(occlusionEngine, occlusionEngineLabel);
            if (occlusionEngine.enumValueIndex == (int)Vive3DSPAudio.OccEngineMode.RaycastOcclusion)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(raycastQuality, raycastQualityLabel);
                --EditorGUI.indentLevel;
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSurfaceMaterial(SerializedProperty surfaceMaterial)
        {
            surfaceMaterialLabel.text = surfaceMaterial.displayName;
            EditorGUILayout.PropertyField(surfaceMaterial, surfaceMaterialLabel);
        }
    }
}

