//====================== Copyright 2016-2017, HTC.Corporation. All rights reserved. ======================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    [CustomEditor(typeof(Vive3DSPAudioRoom))]
    public class Vive3DSPAudioRoomEditor : Editor
    {
        private SerializedProperty roomEffect = null;
        //private SerializedProperty roomReverbPreset = null;
        private SerializedProperty Ceiling = null;
        private SerializedProperty FrontWall = null;
        private SerializedProperty BackWall = null;
        private SerializedProperty RightWall = null;
        private SerializedProperty LeftWall = null;
        private SerializedProperty Floor = null;
        private SerializedProperty ceilingReflectionRate = null;
        private SerializedProperty frontWallReflectionRate = null;
        private SerializedProperty backWallReflectionRate = null;
        private SerializedProperty rightWallReflectionRate = null;
        private SerializedProperty leftWallReflectionRate = null;
        private SerializedProperty floorReflectionRate = null;
        private SerializedProperty reflectionLevel = null;
        private SerializedProperty reverbLevel = null;
        private SerializedProperty userDefineClip = null;
        private SerializedProperty size = null;

        private GUIContent roomEffectLabel = new GUIContent("Room Effect",
            "Reverb effect enable/disable");
        private GUIContent roomPresetLabel = new GUIContent("Room Reverb Preset",
            "Room Reverb Preset");
        private GUIContent SurfaceMaterialsLabel = new GUIContent("Room Surface Material",
            "Set room surface materials for reverb effect");
        private GUIContent surfaceMaterialLabel = new GUIContent("Room Surface Material",
            "Set room surface materials for reverb effect");
        private GUIContent ceilingReflectionRateLabel = new GUIContent("Reflection Rate",
            "Set reflection rate of ceiling");
        private GUIContent frontWallReflectionRateLabel = new GUIContent("Reflection Rate",
            "Set reflection rate of ceiling");
        private GUIContent backWallReflectionRateLabel = new GUIContent("Reflection Rate",
            "Set reflection rate of ceiling");
        private GUIContent rightWallReflectionRateLabel = new GUIContent("Reflection Rate",
            "Set reflection rate of ceiling");
        private GUIContent leftWallReflectionRateLabel = new GUIContent("Reflection Rate",
            "Set reflection rate of ceiling");
        private GUIContent floorReflectionRateLabel = new GUIContent("Reflection Rate",
            "Set reflection rate of ceiling");
        private GUIContent reflectionLevelLabel = new GUIContent("Reflection Level (dB)",
            "Set reflection level for reverb effect");
        private GUIContent reverbLevelLable = new GUIContent("Reverb Level (dB)",
            "Set reverb level for reverb effect");
        private GUIContent backgroundTypeLabel = new GUIContent("Background Audio",
            "Set background audio type in the room");
        private GUIContent backgroundVolumeLabel = new GUIContent("Background Volume",
            "Set background audio volume in the room in dB scale");
        private GUIContent backgroundAudioClipLabel = new GUIContent("Background Audio Clip",
            "Set background audio clip");
        private GUIContent sizeLabel = new GUIContent("Room size",
            "Set the room size");

        void OnEnable()
        {
            roomEffect = serializedObject.FindProperty("roomEffect");
            //roomReverbPreset = serializedObject.FindProperty("reverbPreset");
            Ceiling = serializedObject.FindProperty("ceiling");
            FrontWall = serializedObject.FindProperty("frontWall");
            BackWall = serializedObject.FindProperty("backWall");
            RightWall = serializedObject.FindProperty("rightWall");
            LeftWall = serializedObject.FindProperty("leftWall");
            Floor = serializedObject.FindProperty("floor");
            ceilingReflectionRate = serializedObject.FindProperty("ceilingReflectionRate");
            frontWallReflectionRate = serializedObject.FindProperty("frontWallReflectionRate");
            backWallReflectionRate = serializedObject.FindProperty("backWallReflectionRate");
            rightWallReflectionRate = serializedObject.FindProperty("rightWallReflectionRate");
            leftWallReflectionRate = serializedObject.FindProperty("leftWallReflectionRate");
            floorReflectionRate = serializedObject.FindProperty("floorReflectionRate");
            reflectionLevel = serializedObject.FindProperty("reflectionLevel");
            reverbLevel = serializedObject.FindProperty("reverbLevel");
            userDefineClip = serializedObject.FindProperty("userDefineClip");
            size = serializedObject.FindProperty("size");
        }

        public override void OnInspectorGUI()
        {
            var model = target as Vive3DSPAudioRoom;

            serializedObject.Update();
            
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(roomEffect, roomEffectLabel);
            //EditorGUILayout.PropertyField(roomReverbPreset, roomPresetLabel);
            Vive3DSPAudio.RoomReverbPreset _preset = (Vive3DSPAudio.RoomReverbPreset)EditorGUILayout.EnumPopup(roomPresetLabel, model.ReverbPreset);
            //if (roomReverbPreset.enumValueIndex == (int)Vive3DSPAudio.RoomReverbPreset.UserDefine)
            if (_preset != model.ReverbPreset)
                model.ReverbPreset = _preset;
            if (_preset == Vive3DSPAudio.RoomReverbPreset.UserDefine)
            {
                EditorGUILayout.LabelField(SurfaceMaterialsLabel);
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(Ceiling);
                if (Ceiling.enumValueIndex == (int)Vive3DSPAudio.RoomPlateMaterial.UserDefine)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.Slider(ceilingReflectionRate, 0.0f, 1.0f, ceilingReflectionRateLabel);
                    --EditorGUI.indentLevel;
                }
                EditorGUILayout.PropertyField(FrontWall);
                if (FrontWall.enumValueIndex == (int)Vive3DSPAudio.RoomPlateMaterial.UserDefine)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.Slider(frontWallReflectionRate, 0.0f, 1.0f, frontWallReflectionRateLabel);
                    --EditorGUI.indentLevel;
                }
                EditorGUILayout.PropertyField(BackWall);
                if (BackWall.enumValueIndex == (int)Vive3DSPAudio.RoomPlateMaterial.UserDefine)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.Slider(backWallReflectionRate, 0.0f, 1.0f, backWallReflectionRateLabel);
                    --EditorGUI.indentLevel;
                }
                EditorGUILayout.PropertyField(RightWall);
                if (RightWall.enumValueIndex == (int)Vive3DSPAudio.RoomPlateMaterial.UserDefine)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.Slider(rightWallReflectionRate, 0.0f, 1.0f, rightWallReflectionRateLabel);
                    --EditorGUI.indentLevel;
                }
                EditorGUILayout.PropertyField(LeftWall);
                if (LeftWall.enumValueIndex == (int)Vive3DSPAudio.RoomPlateMaterial.UserDefine)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.Slider(leftWallReflectionRate, 0.0f, 1.0f, leftWallReflectionRateLabel);
                    --EditorGUI.indentLevel;
                }
                EditorGUILayout.PropertyField(Floor);
                if (Floor.enumValueIndex == (int)Vive3DSPAudio.RoomPlateMaterial.UserDefine)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.Slider(floorReflectionRate, 0.0f, 1.0f, floorReflectionRateLabel);
                    --EditorGUI.indentLevel;
                }
                --EditorGUI.indentLevel;
                EditorGUILayout.Separator();
                EditorGUILayout.Slider(reflectionLevel, -30.0f, 10.0f, reflectionLevelLabel);
                EditorGUILayout.Slider(reverbLevel, -30.0f, 10.0f, reverbLevelLable);
                EditorGUILayout.Separator();
            }


            Vive3DSPAudio.RoomBackgroundAudioType _type = (Vive3DSPAudio.RoomBackgroundAudioType)EditorGUILayout.EnumPopup(backgroundTypeLabel, model.BackgroundType);
            if (_type != model.BackgroundType)
                model.BackgroundType = _type;
            if (_type == Vive3DSPAudio.RoomBackgroundAudioType.UserDefine)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(userDefineClip, backgroundAudioClipLabel);
                --EditorGUI.indentLevel;
            }
            
            float _vol = EditorGUILayout.Slider(backgroundVolumeLabel, model.backgroundVolume, -96.0f, 0.0f);
            if (_vol != model.backgroundVolume)
                model.backgroundVolume = _vol;

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(size, sizeLabel);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                if (!Application.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }

        private void DrawSurfaceMaterial(SerializedProperty surfaceMaterial)
        {
            surfaceMaterialLabel.text = surfaceMaterial.displayName;
            EditorGUILayout.PropertyField(surfaceMaterial, surfaceMaterialLabel);
        }
    }
}
