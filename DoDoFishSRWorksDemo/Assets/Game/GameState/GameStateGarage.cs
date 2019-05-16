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
        public void EnterState(IState oldState, StatePatternBase statePatternBase)
        {
            manager = statePatternBase as GameStateManager;
            garageController = GameManager.Instance.StartBigCarDemo();
            waitForLeave = false;
            isOpenGate = false;
        }

        public void LeaveState()
        {

        }

        public string Name()
        {
            return "GameStateGarage";
        }

        bool isOpenGate;
        bool waitForLeave;
        public void UpdateState()
        {
            if(waitForLeave)
            {
                if (!GameManager.Instance.GarageIsClosing)
                    manager.SwitchState(GameStateManager.GameState.PUFFERFISH);
                return;
            }

            if (manager.PadKeyUp)
                garageController.LiftMachineStop();

            if (manager.PadPressL)
                garageController.GarageTurnLeft();
            else if (manager.PadPressR)
                garageController.GarageTurnRight();

            if (manager.PadPressUpward)
                garageController.LiftMachineRaiseUp();
            else if (manager.PadPressDownward)
                garageController.LiftMachineFallDown();

            if (manager.PadPressCenter && manager.PadIsFirstPress)
            {
                bool isopen = garageController.GarageSwitch();
                if (isopen && !GameManager.Instance.GarageIsClosing)
                {
                    if (!isOpenGate)
                    {
                        Debug.Log("StartGarageSetting");
                        GameManager.Instance.StartGarageSetting();
                        isOpenGate = true;
                    }
                }
                else
                {
                    isOpenGate = false;
                    GameManager.Instance.CloseGarageSetting();
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
            FixedWorldPosCover[] fwpcs = GameManager.Instance.GetGarageARMaterial();
            if (fwpcs != null)
                foreach (FixedWorldPosCover fwpc in fwpcs)
                    fwpc.GetComponent<Renderer>().sharedMaterial.SetTexture("_LeftEyeTexture", rt);
        }

        public void RRealWorldCamRTRefresh(RenderTexture rt)
        {
            FixedWorldPosCover[] fwpcs = GameManager.Instance.GetGarageARMaterial();
            if (fwpcs != null)
                foreach (FixedWorldPosCover fwpc in fwpcs)
                    fwpc.GetComponent<Renderer>().sharedMaterial.SetTexture("_RightEyeTexture", rt);
        }
    }
}