﻿//========= Copyright 2016-2019, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.VRModuleManagement;
using UnityEditor;
using UnityEngine;

namespace HTC.UnityPlugin.Vive
{
    public class OculusRecommendedSettings : VIUVersionCheck.RecommendedSettingCollection
    {
        public OculusRecommendedSettings()
        {
            Add(new VIUVersionCheck.RecommendedSetting<bool>()
            {
                settingTitle = "Virtual Reality Supported with Oculus",
                skipCheckFunc = () => !VIUSettingsEditor.canSupportOculus,
                currentValueFunc = () => VIUSettingsEditor.supportOculus,
                setValueFunc = v => VIUSettingsEditor.supportOculus = v,
                recommendedValue = true,
            });

            Add(new VIUVersionCheck.RecommendedSetting<bool>()
            {
                settingTitle = "Multithreaded Rendering",
                skipCheckFunc = () => !VIUSettingsEditor.supportOculusGo,
#if UNITY_2017_2_OR_NEWER
                currentValueFunc = () => PlayerSettings.GetMobileMTRendering(BuildTargetGroup.Android),
                setValueFunc = v => PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, v),
#else
                currentValueFunc = () => PlayerSettings.mobileMTRendering,
                setValueFunc = v => PlayerSettings.mobileMTRendering = v,
#endif
                recommendedValue = true,
            });
        }
    }

    public static partial class VIUSettingsEditor
    {
        private const string OCULUS_DESKTOP_PACKAGE_NAME = "com.unity.xr.oculus.standalone";

        public static bool canSupportOculus
        {
            get { return OculusSettings.instance.canSupport; }
        }

        public static bool supportOculus
        {
            get { return OculusSettings.instance.support; }
            set { OculusSettings.instance.support = value; }
        }

        private class OculusSettings : VRPlatformSetting
        {
            public static OculusSettings instance { get; private set; }

            public OculusSettings() { instance = this; }

            public override int order { get { return 2; } }

            protected override BuildTargetGroup requirdPlatform { get { return BuildTargetGroup.Standalone; } }

            public override bool canSupport
            {
                get
                {
#if UNITY_2018_1_OR_NEWER
                    return activeBuildTargetGroup == BuildTargetGroup.Standalone && PackageManagerHelper.IsPackageInList(OCULUS_DESKTOP_PACKAGE_NAME);
#elif UNITY_5_5_OR_NEWER && !UNITY_5_6_0 && !UNITY_5_6_1 && !UNITY_5_6_2
                    return activeBuildTargetGroup == BuildTargetGroup.Standalone;
#else
                    return activeBuildTargetGroup == BuildTargetGroup.Standalone && VRModule.isOculusVRPluginDetected;
#endif
                    ;
                }
            }

            public override bool support
            {
                get
                {
#if UNITY_5_5_OR_NEWER
                    return canSupport && (VIUSettings.activateOculusVRModule || VIUSettings.activateUnityNativeVRModule) && OculusSDK.enabled;
#elif UNITY_5_4_OR_NEWER
                    return canSupport && VIUSettings.activateOculusVRModule && OculusSDK.enabled;
#else
                    return canSupport && VIUSettings.activateOculusVRModule && virtualRealitySupported;
#endif
                }
                set
                {
                    if (support == value) { return; }

                    VIUSettings.activateOculusVRModule = value;

#if UNITY_5_5_OR_NEWER
                    OculusSDK.enabled = value;
                    VIUSettings.activateUnityNativeVRModule = value || supportOpenVR;
#elif UNITY_5_4_OR_NEWER
                    OculusSDK.enabled = value;
#else
                    virtualRealitySupported = value;
#endif
                }
            }

            public override void OnPreferenceGUI()
            {
                const string title = "Oculus Desktop";
                if (canSupport)
                {
                    support = Foldouter.ShowFoldoutBlankWithEnabledToggle(new GUIContent(title, "Oculus Rift, Oculus Rift S"), support);
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    Foldouter.ShowFoldoutBlank();

                    if (activeBuildTargetGroup != BuildTargetGroup.Standalone)
                    {
                        GUI.enabled = false;
                        ShowToggle(new GUIContent(title, "Standalone platform required."), false, GUILayout.Width(150f));
                        GUI.enabled = true;
                        GUILayout.FlexibleSpace();
                        ShowSwitchPlatformButton(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                    }
                    else if (!PackageManagerHelper.IsPackageInList(OCULUS_DESKTOP_PACKAGE_NAME))
                    {
                        GUI.enabled = false;
                        ShowToggle(new GUIContent(title, "Oculus (Desktop) package required."), false, GUILayout.Width(230f));
                        GUI.enabled = true;
                        GUILayout.FlexibleSpace();
                        ShowAddPackageButton("Oculus (Desktop)", OCULUS_DESKTOP_PACKAGE_NAME);
                    }
                    else if (!VRModule.isOculusVRPluginDetected)
                    {
                        GUI.enabled = false;
                        ShowToggle(new GUIContent(title, "Oculus VR Plugin required."), false, GUILayout.Width(150f));
                        GUI.enabled = true;
                        GUILayout.FlexibleSpace();
                        ShowUrlLinkButton(URL_OCULUS_VR_PLUGIN);
                    }

                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}