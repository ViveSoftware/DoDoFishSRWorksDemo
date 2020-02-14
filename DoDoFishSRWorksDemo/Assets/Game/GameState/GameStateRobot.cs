using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
    public class GameStateRobot : IState
    {
        GameStateManager manager;
        RobotController robotController;
        public void EnterState(IState oldState, StatePatternBase statePatternBase)
        {
            manager = statePatternBase as GameStateManager;
            robotController = GameManager.Instance.StartRobotDemo();
            GameManager.Instance.EnableInfoText(true);
        }

        public void LeaveState()
        {
            GameManager.Instance.CloseRobotDemo();
        }

        public string Name()
        {
            return "GameStateRobot";
        }

        public void UpdateState()
        {
            if (manager.myInput.IsFirstCosmosBKey)
            {
                GameManager.Instance.SwitchInfoText();
            }
            bool isTurning = false;
            if (manager.myInput.PadLTouch)
            {
                robotController.TurnLeft();
                isTurning = true;
            }
            else if (manager.myInput.PadRTouch)
            {
                robotController.TurnRight();
                isTurning = true;
            }

            if (!manager.myInput.PadLPress && !manager.myInput.PadRPress && !manager.myInput.PadUpPress && !manager.myInput.PadDnPress)
            {
                robotController.Stop();
            }

            if (manager.myInput.IsJoystick)
            {
                if (manager.myInput.IsFirstBumper)
                    robotController.SwitchDiscoLight();
            }
            else
            {
                if (manager.myInput.PadLPress && manager.myInput.PadPressDown ||
                    manager.myInput.PadRPress && manager.myInput.PadPressDown)
                {
                    robotController.SwitchDiscoLight();
                }
            }

            if (robotController.GetDiscoLightRoot().gameObject.activeSelf)
                MyHelpLayer.SetSceneLayer(robotController.GetDiscoLightRoot(), ARRender.UnityRenderWithDepthNoShadowLayer);

            if (!isTurning)
            {
                if (manager.myInput.PadUpPress)
                {
                    //robotController.MoveForwrad();
                    robotController.stallHigh += Time.deltaTime * 0.9f;
                    robotController.stallHigh = Mathf.Clamp(robotController.stallHigh, 0.5f, 10);
                }
                else if (manager.myInput.PadDnPress)
                {
                    //robotController.MoveBack();
                    robotController.stallHigh -= Time.deltaTime * 0.9f;
                    robotController.stallHigh = Mathf.Clamp(robotController.stallHigh, 0.5f, 10);
                }
            }

            float triggerRatio = ViveInput.GetTriggerValue(HandRole.RightHand);
            if (triggerRatio > 0)
            {
                if (robotController != null)
                    robotController.MoveForwrad(triggerRatio);
                //Debug.Log("tvalue : " + tvalue);
            }

            if (GameManager.Instance.ViveIsGrip())
            {
                manager.SwitchState(GameStateManager.GameState.GARAGE);
            }

            //Recover to idle state
            if (ViveInput.GetPressUp(HandRole.RightHand, ControllerButton.Trigger)
                || triggerRatio < 0.01f)
            {
                robotController.Stop();
            }
        }
    }

}