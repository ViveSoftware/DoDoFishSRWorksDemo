using Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBallGameState : IState
{
    FishAIStateManager manager;
    bool firstEnter;
    bool gamePlaying;
    //int canUpdateWorldPosMatrix;
    public void EnterState(IState oldState, StatePatternBase statePatternBase)
    {
        manager = statePatternBase as FishAIStateManager;
        GameManager.Instance.StartPaintBallWallGame();
        MRWallManager.instance.UpdateWorldPosMatrix();
        firstEnter = true;
        gamePlaying = false;

        manager.fishAI.transform.GetChild(0).gameObject.SetActive(false);
        //canUpdateWorldPosMatrix = 2;
    }

    public void LeaveState()
    {
        GameManager.Instance.SetHandTouchMode();
        GameManager.Instance.ShowHitFishInfo();
        manager.fishAI.transform.GetChild(0).gameObject.SetActive(true);
        manager.fishAI.ResetFishData();

        GameManager.Instance.RecoverFishToDefaultLayer();
    }

    public string Name()
    {
        return "PaintBallGameState";
    }


    public void UpdateState()
    {
        //canUpdateWorldPosMatrix--;
        //if (canUpdateWorldPosMatrix == 0)
        //{
        //    //wait ? frame for ensure MRWallManager init
        //    //MRWallManager.instance.UpdateWorldPosMatrix();
        //}

        if (firstEnter && MRWallManager.instance.gameStartFlag)
        {
            firstEnter = false;
            gamePlaying = true;
        }

        if (gamePlaying && !MRWallManager.instance.gameStartFlag)
        {
            //wait until game over
            GameManager.Instance.ClosePaintBallWallGame();
            manager.WaitToSwitchState(FishAIStateManager.FishState.move);
        }
    }
}
