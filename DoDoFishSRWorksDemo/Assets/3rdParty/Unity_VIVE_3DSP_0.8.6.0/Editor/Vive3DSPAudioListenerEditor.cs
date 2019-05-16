//====================== Copyright 2016-2017, HTC.Corporation. All rights reserved. ======================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    [CustomEditor(typeof(Vive3DSPAudioListener))]
    public class Vive3DSPAudioListenerEditor : Editor
    {
        private SerializedProperty globalGain = null;
        private SerializedProperty occlusionMode = null;
        private SerializedProperty headset = null;

        private GUIContent globalGainLabel = new GUIContent("Global Gain (dB)",
            "Set the global gain of the system");
        private GUIContent occlusionModeLabel = new GUIContent("Occlusion Mode",
            "Set occlusion mode");
        private GUIContent headsetLabel = new GUIContent("Headset Model",
            "Set the headset to compensate ");

        void OnEnable()
        {
            globalGain = serializedObject.FindProperty("globalGain");
            occlusionMode = serializedObject.FindProperty("occlusionMode");
            headset = serializedObject.FindProperty("headsetType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Slider(globalGain, -24.0f, 24.0f, globalGainLabel);
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(occlusionMode, occlusionModeLabel);
            EditorGUILayout.Separator();

            if (Application.isPlaying)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            EditorGUILayout.PropertyField(headset, headsetLabel);
            

            serializedObject.ApplyModifiedProperties();
        }
    }
}