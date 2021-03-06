﻿using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vive.Plugin.SR;

namespace Demo
{
    public class GameStateSRWorksLoading : IState
    {
        GameStateManager manager;
        int scanningProgress;
        float loadingDotTime;
        public static bool SkipSelectWall = false;

        public void EnterState(IState oldState, StatePatternBase statePatternBase)
        {
            manager = statePatternBase as GameStateManager;
            scanningProgress = 1;
            SkipSelectWall = false;
        }

        public void LeaveState()
        {
            ARRender.Instance.InitRenderSystem();
        }

        public string Name()
        {
            return "GameStateSRWorksLoading";
        }

        public void UpdateState()
        {
            if (ViveSR_DualCameraRig.DualCameraStatus != DualCameraStatus.WORKING)
                return;
            ARRender.Instance.VRCameraRemoveLayer(ARCameraCubemap.ARCameraCubemapLayer);
            ARRender.Instance.VRCameraRemoveLayer(TextBoard.EyeInfoTextLayer());
            Texture pnaleL = ViveSR_DualCameraRig.Instance.TrackedCameraLeft.ImagePlane.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");
            Texture pnaleR = ViveSR_DualCameraRig.Instance.TrackedCameraLeft.ImagePlane.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");
            if (pnaleL != null && pnaleR != null)
            {
                float gamma =
    (ViveSR_DualCameraRig.Instance.TrackedCameraLeft.ImagePlane.GetComponent<Renderer>().sharedMaterial.shader.name == "ViveSR/Unlit, Textured, Stencil") ?
    1.8f : 1.4f;
                SeethroughGamma sg = MyHelpNode.FindOrAddComponent<SeethroughGamma>(ViveSR_DualCameraRig.Instance.DualCameraLeft.transform);
                sg.gamma = gamma;

                sg = MyHelpNode.FindOrAddComponent<SeethroughGamma>(ViveSR_DualCameraRig.Instance.DualCameraRight.transform);
                sg.gamma = gamma;
            }


            //1: select [scan] | [load] ,2:[saving], 3:[loading], 0: select wall done, 4: [scanning], 5: [Select wall]
            if (scanningProgress == 0)
                return;

            bool padpress = (
                (manager.myInput.IsJoystick && manager.myInput.IsFirstCosmosAKey) ||
                (!manager.myInput.IsJoystick && manager.myInput.PadPressDown)
                );
            bool skipWall = (manager.myInput.IsJoystick && manager.myInput.IsFirstCosmosBKey);
            //ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Pad);
            bool triggerpress = manager.myInput.IsFirstTrigger;
            //ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger);
            if (scanningProgress == 1)//select [scan] | [load]
            {
                GameManager.Instance.EnableInfoText(true);

                if (triggerpress)
                {
                    SRWorkControl.Instance.SetScanning(true);
                    scanningProgress = 4;
                }
                else if (padpress //||
                                  //Input.GetKeyUp(KeyCode.A)
                    )
                {
                    if (StartLoadReconstructData())
                        scanningProgress = 3;
                    else
                        GameManager.Instance.ShowInfoTextAni("<line-height=150%><b><size=120%>None scanned data!\n<line-height=80%><size=100%>Press <color=#FFC000FF>Trigger</b>\n<color=white><size=80%>to start scanning room");
                }
                else if (skipWall)
                {
                    SkipSelectWall = true;
                    manager.SwitchState(GameStateManager.GameState.SELECTWALL);
                }
            }
            else if (scanningProgress == 2)//[saving]
            {
                //eyeInfoText.text = "Saving..." + savingPercent + "%";
            }
            else if (scanningProgress == 3)//[loading]
            {
                loadingDotTime += Time.deltaTime;
                if (loadingDotTime > 0.2f)
                {
                    loadingDotTime = 0;
                    int dotcount = 0;
                    string sss = GameManager.Instance.GetInfoText();
                    foreach (char c in sss)
                        if (c == '.')
                            dotcount++;
                    if (dotcount % 30 == 0)
                        sss += "\r\n";
                    sss += '.';

                    GameManager.Instance.SetInfoText(sss);
                }
            }
            else if (scanningProgress == 4)//[scanning]
            {
                GameManager.Instance.ShowInfoTextAni("<size=120%><b>Please look around to scanning...<line-height=150%><size=100%>\nPress <color=#FFC000FF>" + GameManager.Instance.PadKeyName + "</b><line-height=80%><color=white><size=80%> :\nto save scene<b><line-height=150%><size=100%>\nPress <color=#FFC000FF>Trigger</b><line-height=80%><color=white><size=80%> :\nto cancel scanning");
                if (padpress)
                {
                    SRWorkControl.Instance.SaveScanning(
                        (percentage) => { GameManager.Instance.ShowInfoTextAni("<b>Saving...<color=#FFC000FF><size=120%>" + percentage + "%"); },
                        () =>
                        {
                            //save done then load...
                            StartLoadReconstructData();
                            scanningProgress = 3;
                        }
                        );
                    scanningProgress = 2;
                }
                else if (triggerpress)
                {
                    SRWorkControl.Instance.SetScanning(false);
                    scanningProgress = 1;
                    GameManager.Instance.infoTextStart();
                }
            }
            else if (scanningProgress == 5)
            {
                if (ReconstructManager.Instance.reconstructDataAnalyzeDone)
                    manager.SwitchState(GameStateManager.GameState.SELECTWALL);
            }
        }

        bool StartLoadReconstructData()
        {
            return ReconstructManager.Instance.StartLoadReconstructData(
            () => { GameManager.Instance.ShowInfoTextAni("<b>Loading.."); },
            () =>
            {
                Debug.Log("ReconstructData Load done");
                scanningProgress = 5;
                GameManager.Instance.ShowInfoTextAni("<b>Loading...<size=120%>100%</b>\n<color=white><size=100%>Please <color=#FFC000FF><size=140%><b>select a wall</b> <size=100%><color=white> for fish to hide");
            });
        }

    }
}