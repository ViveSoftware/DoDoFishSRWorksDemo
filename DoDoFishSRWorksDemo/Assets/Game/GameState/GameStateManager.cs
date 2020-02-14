using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
    public class GameStateManager : StatePatternBase
    {
        public MyInput myInput { get; private set; }

        public interface IGameStateUpdateRealWorldCameraImage
        {
            void LRealWorldCamRTRefresh(RenderTexture rt);
            void RRealWorldCamRTRefresh(RenderTexture rt);
        }

        public enum GameState
        {
            SRWORK_LOADING,
            SELECTWALL,
            PUFFERFISH,
            ROBOT,
            GARAGE,
            size
        }

        public override void DebugLog(string text, int level = 0)
        {
            Debug.Log(text);
        }

        protected override void OnCreateState(out IState[] pleaseCreateThisList)
        {
            pleaseCreateThisList = new IState[(int)GameState.size];

            pleaseCreateThisList[(int)GameState.SRWORK_LOADING] = new GameStateSRWorksLoading();
            pleaseCreateThisList[(int)GameState.SELECTWALL] = new GameStateSelectWall();
            pleaseCreateThisList[(int)GameState.PUFFERFISH] = new GameStatePufferFish();
            pleaseCreateThisList[(int)GameState.ROBOT] = new GameStateRobot();
            pleaseCreateThisList[(int)GameState.GARAGE] = new GameStateGarage();

            SwitchState(GameState.SRWORK_LOADING);

            myInput = new MyInput(HTC.UnityPlugin.Vive.HandRole.RightHand);
        }

        //public bool PadKeyUp, PadPressL, PadPressR, PadPressUpward, PadPressDownward, PadPressCenter, PadTouchL, PadTouchR, PadIsFirstPress;
        //Vector2 padRightTouchAxis, padRightPressAxis;
        //bool padRightPress, padRightFirstPress;

        public override void Update()
        {
            base.Update();
            myInput.Update();
            //PadIsFirstPress = PadTouchL = PadTouchR = PadPressCenter =
            //    PadPressUpward = PadPressDownward = PadPressL = PadPressR = PadKeyUp = false;

            //padRightTouchAxis = ViveInput.GetPadTouchAxis(HandRole.RightHand);

            //const float threshold = 0.4f;
            //if (padRightTouchAxis.x > threshold)
            //    PadTouchL = true;
            //else if (padRightTouchAxis.x < -threshold)
            //    PadTouchR = true;

            ////check PadKeyUp
            //padRightPressAxis = ViveInput.GetPadPressAxis(HandRole.RightHand);
            //if (padRightPress)
            //{
            //    padRightPress = false;
            //    if (padRightPressAxis.x == 0 && padRightPressAxis.y == 0)
            //    {
            //        padRightFirstPress = true;
            //        PadKeyUp = true;
            //    }
            //}

            ////check PadPressLeft, PadPressRight, PadPressUp, PadPressDown
            //if (padRightPressAxis.x != 0 || padRightPressAxis.y != 0)
            //{
            //    padRightPress = true;
            //    if (padRightPressAxis.x > threshold)
            //        PadPressL = true;
            //    else if (padRightPressAxis.x < -threshold)
            //        PadPressR = true;

            //    if (padRightPressAxis.y > threshold)
            //        PadPressUpward = true;
            //    else if (padRightPressAxis.y < -threshold)
            //        PadPressDownward = true;

            //    if (padRightPressAxis.x < threshold &&
            //        padRightPressAxis.x > -threshold &&
            //        padRightPressAxis.y < threshold &&
            //        padRightPressAxis.y > -threshold)
            //        PadPressCenter = true;

            //    if (padRightFirstPress)
            //        PadIsFirstPress = true;
            //    padRightFirstPress = false;
            //    // Debug.LogWarning("padRightPress : " + padRightPress + " : " + padRightPressAxis);
            //}
        }
    }
}