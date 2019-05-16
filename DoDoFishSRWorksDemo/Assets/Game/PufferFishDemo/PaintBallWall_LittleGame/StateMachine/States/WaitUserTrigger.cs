using Demo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitUserTrigger : State
{
    private bool prepareExit = false;
    public override void StateEnter()
    {
        MRWallManager.instance.GameStartButton.SetActive(true);
        MRWallManager.instance.gameStartFlag = false;
        AudioManager.instance.PlayWaitUserTrigger();
        MRWallManager.instance.heartController.OnHideEvent();
        prepareExit = false;
    }

    public override void StateExit()
    {
        MRWallManager.instance.GameStartButton.SetActive(false);
    }

    public override void StateUpdate()
    {
        if (MRWallManager.instance.gameStartFlag)
        {
            if (!prepareExit)
            {
                AudioManager.instance.SmoothTurnOffBackGMusic(1f);
                prepareExit = true;
            }
            if (!AudioManager.instance.isBackGroundMusiPlaying)
            {
                stateManager.SetState((int)PaintBallStateManager.PaintBallStateEnum.GAME_INIT);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            MRWallManager.instance.TiggerStartEvent();
        }
        if (Input.GetKeyDown(KeyCode.H) && GameObject.FindObjectOfType<ARRender>() == null)
        {
            if (!prepareExit)
            {
                AudioManager.instance.SmoothTurnOffBackGMusic(1f);
                prepareExit = true;
            }
            if (!AudioManager.instance.isBackGroundMusiPlaying)
            {
                stateManager.SetState((int)PaintBallStateManager.PaintBallStateEnum.ROOM_SETTING);
            }
        }
    }
}
