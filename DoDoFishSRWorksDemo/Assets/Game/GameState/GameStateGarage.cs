using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
    public class GameStateGarage : IState, GameStateManager.IGameStateUpdateRealWorldCameraImage
    {
        GameStateManager manager;
        GarageController garageController;
        bool selectPosition;
        Vector3 garagePos;
        Transform DoorHintPosition;

        public void EnterState(IState oldState, StatePatternBase statePatternBase)
        {
            manager = statePatternBase as GameStateManager;
            garageController = GameManager.Instance.StartBigCarDemo();
            waitForLeave = false;
            isOpenGate = false;
            selectPosition = true;
            garagePos = Vector3.zero;

            MyHelpNode.FindTransform(GameManager.Instance.LiftMachine, "LiftDoorHint", out DoorHintPosition);
            DoorHintPosition.gameObject.SetActive(true);
        }

        public void LeaveState()
        {
            MyHelpDraw.LineDrawClose(garageController.transform);
        }

        public string Name()
        {
            return "GameStateGarage";
        }

        bool isOpenGate;
        bool waitForLeave;
        public void UpdateState()
        {
            if (manager.myInput.IsFirstCosmosBKey)
            {
                GameManager.Instance.SwitchInfoText();
            }

            if (waitForLeave)
            {
                if (!GameManager.Instance.GarageIsClosing)
                    manager.SwitchState(GameStateManager.GameState.PUFFERFISH);
                return;
            }

            HTC.UnityPlugin.Utility.RigidPose rightHandPose = VivePose.GetPose(HandRole.RightHand);
            Matrix4x4 matRot = Matrix4x4.Rotate(rightHandPose.rot);
            Vector3 rightArmPos = rightHandPose.pos;
            Vector3 rightHandDir = matRot.GetColumn(2);

            if (selectPosition)
            {
                bool ok = false;
                RaycastHit[] hits = Physics.RaycastAll(rightArmPos, rightHandDir, 99999);
                if (hits != null && hits.Length > 0)
                {
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.transform.name.ToLower().Contains("floor"))
                        {
                            GameManager.Instance.StartGarageSetting(hit.point);

                            MyHelpDraw.LineDraw(garageController.transform, rightArmPos, hit.point, 0.1f, 0.1f, Color.green);
                            MyHelpDraw.LineColor(garageController.transform, Color.green);
                            if (ViveInput.GetPressUp(HandRole.RightHand, ControllerButton.Trigger))
                            {
                                selectPosition = false;
                                garagePos = hit.point;
                            }
                            ok = true;
                            break;
                        }
                    }
                }
                if (!ok)
                {
                    MyHelpDraw.LineDraw(garageController.transform, rightArmPos, rightArmPos + rightHandDir * 20, 0.1f, 0.1f, Color.red);
                    MyHelpDraw.LineColor(garageController.transform, Color.red);
                }
            }
            else
            {
                MyHelpDraw.LineDraw(garageController.transform, rightArmPos, rightArmPos + rightHandDir * 1, 0.1f, 0.1f, Color.yellow);
                MyHelpDraw.LineColor(garageController.transform, Color.yellow);
                DoorHintPosition.gameObject.SetActive(false);

                //if (manager.myInput.PadKeyUp)
                //garageController.LiftMachineStop();

                if (manager.myInput.PadLPress)
                    garageController.GarageTurnLeft();
                else if (manager.myInput.PadRPress)
                    garageController.GarageTurnRight();
                else if (manager.myInput.PadUpPress)
                    garageController.LiftMachineRaiseUp();
                else if (manager.myInput.PadDnPress)
                    garageController.LiftMachineFallDown();
                else
                    garageController.LiftMachineStop();

                if ((manager.myInput.IsJoystick && manager.myInput.IsFirstTrigger) ||
                    (!manager.myInput.IsJoystick && manager.myInput.PadPressDown)
                    ///*manager.myInput.PadCPress && */manager.myInput.PadPressDown
                    )
                {
                    bool isopen = garageController.GarageSwitch();
                    if (isopen && !GameManager.Instance.GarageIsClosing)
                    {
                        if (!isOpenGate)
                        {
                            Debug.Log("StartGarageSetting");
                            GameManager.Instance.StartGarageSetting(garagePos);
                            isOpenGate = true;
                        }
                    }
                    else
                    {
                        isOpenGate = false;
                        GameManager.Instance.CloseGarageSetting();
                    }
                }

                if(ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Pad))
                {
                    Vector3 pos = GameManager.Instance.LiftMachine.position;
                    GameManager.Instance.StartGarageSetting(pos);
                }
            }

            if (GameManager.Instance.ViveIsGrip() && !waitForLeave)
            {
                garageController.GarageClose();
                waitForLeave = true;
                GameManager.Instance.CloseGarageSetting();
            }
        }

        public void LRealWorldCamRTRefresh(RenderTexture rt)
        {
            MyCameraPreRender prerender = MyHelpNode.FindOrAddComponent<MyCameraPreRender>(ARRender.Instance._UnityRenderOnTopCameraLL.transform);
            FixedWorldPosCover[] fwpcs = GameManager.Instance.GetGarageARMaterial();
            Material[] mats = new Material[fwpcs.Length];
            for (int a = 0; a < fwpcs.Length; a++)
                mats[a] = fwpcs[a].GetComponent<Renderer>().sharedMaterial;
            prerender.mats = mats;
            prerender.matName = "_LeftEyeTexture";
            prerender.rt = rt;

            //FixedWorldPosCover[] fwpcs = GameManager.Instance.GetGarageARMaterial();
            //if (fwpcs != null)
            //    foreach (FixedWorldPosCover fwpc in fwpcs)
            //        fwpc.GetComponent<Renderer>().sharedMaterial.SetTexture("_LeftEyeTexture", rt);
        }

        public void RRealWorldCamRTRefresh(RenderTexture rt)
        {
            MyCameraPreRender prerender = MyHelpNode.FindOrAddComponent<MyCameraPreRender>(ARRender.Instance._UnityRenderOnTopCameraRR.transform);
            FixedWorldPosCover[] fwpcs = GameManager.Instance.GetGarageARMaterial();
            Material[] mats = new Material[fwpcs.Length];
            for (int a = 0; a < fwpcs.Length; a++)
                mats[a] = fwpcs[a].GetComponent<Renderer>().sharedMaterial;
            prerender.mats = mats;
            prerender.matName = "_LeftEyeTexture";
            prerender.rt = rt;

            //FixedWorldPosCover[] fwpcs = GameManager.Instance.GetGarageARMaterial();
            //if (fwpcs != null)
            //    foreach (FixedWorldPosCover fwpc in fwpcs)
            //        fwpc.GetComponent<Renderer>().sharedMaterial.SetTexture("_RightEyeTexture", rt);
        }
    }
}