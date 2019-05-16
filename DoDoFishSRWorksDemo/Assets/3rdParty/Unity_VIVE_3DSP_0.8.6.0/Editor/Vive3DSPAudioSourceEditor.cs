//====================== Copyright 2016-2017, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using UnityEditor;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    [CustomEditor(typeof(Vive3DSPAudioSource))]
    [CanEditMultipleObjects]
    public class Vive3DSPAudioSourceEditor : Editor
    {
        private SerializedProperty gain = null;
        private SerializedProperty distanceMode = null;
        private SerializedProperty drc = null;
        private SerializedProperty spatializer = null;
        private SerializedProperty reverb = null;
        private SerializedProperty occlusion = null;
        private SerializedProperty minimumDecayVolumeDb = null;
        
        private GUIContent gainLabel = new GUIContent("Gain (dB)",
            "Sets the gain of the sound source");
        private GUIContent distanceModeSwitchLabel = new GUIContent("Distance Mode Switch",
            "Switch distance mode to VIVE 3DSP");
        private GUIContent distanceModeLabel = new GUIContent("Distance Mode",
            "Sets the sound source attenuation curve");
        private GUIContent drcLabel = new GUIContent("DRC",
            "Sets the DRC feature");
        private GUIContent spatializerLabel = new GUIContent("3D Sound Effect",
           "Sets the 3D sound effect feature");
        private GUIContent reverbLabel = new GUIContent("Room Effect",
           "Sets the reverb effect feature");
        private GUIContent occlusionLabel = new GUIContent("Occlusion Effect",
           "Sets the occlusion effect feature");
        private GUIContent minimumDecayVolumeTapDbLabel = new GUIContent("Minimum Decay Volume (dB)",
            "Set minimum decay volume");
        private GUIContent minimumDecayVolumeDbLabel = new GUIContent(" ",
            "Set minimum decay volume");
        

        void OnEnable()
        {
            gain = serializedObject.FindProperty("gain");
            distanceMode = serializedObject.FindProperty("distanceMode");
            drc = serializedObject.FindProperty("drc");
            spatializer = serializedObject.FindProperty("spatializer_3d");
            reverb = serializedObject.FindProperty("reverb");
            occlusion = serializedObject.FindProperty("occlusion");
            minimumDecayVolumeDb = serializedObject.FindProperty("minimumDecayVolumeDb");
        }

        public override void OnInspectorGUI()
        {
            Vive3DSPAudioSource model = target as Vive3DSPAudioSource;

            serializedObject.Update();

            EditorGUILayout.Slider(gain, -24.0f, 24.0f, gainLabel);
            bool _dist_mode = EditorGUILayout.Toggle(distanceModeSwitchLabel, model.distanceModeSwitch);
            if (_dist_mode != model.distanceModeSwitch)
            {
                model.distanceModeSwitch = _dist_mode;
            }
            if (_dist_mode == true)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(distanceMode, distanceModeLabel);
                if (distanceMode.enumValueIndex == (int)Vive3DSPAudio.Ambisonic3dDistanceMode.QuadraticDecay
                    || distanceMode.enumValueIndex == (int)Vive3DSPAudio.Ambisonic3dDistanceMode.LinearDecay)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.LabelField(minimumDecayVolumeTapDbLabel);
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.Slider(minimumDecayVolumeDb, -96.0f, 0.0f, minimumDecayVolumeDbLabel);
                    --EditorGUI.indentLevel;
                    --EditorGUI.indentLevel;
                }
                --EditorGUI.indentLevel;
            }
            

            EditorGUILayout.PropertyField(drc, drcLabel);
            EditorGUILayout.PropertyField(spatializer, spatializerLabel);
            EditorGUILayout.PropertyField(reverb, reverbLabel);
            EditorGUILayout.PropertyField(occlusion, occlusionLabel);

            EditorGUILayout.Separator();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
    }
}


