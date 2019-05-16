using HTC.UnityPlugin.Vive;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : State
{
    private bool playGameOverVideo = false;
    private bool prepareExit = false;

    public override void StateEnter()
    {
        playGameOverVideo = false;
        prepareExit = false;

        AudioManager.instance.PlayGameOverBackGroundMusic();
        MRWallManager.instance.heartController.OnHideEvent();
    }

    public override void StateExit()
    {
        MRWallManager.instance.ResetMRWall();
        ScoreManager.instance.OnResetEvent();
        GameOverVideoPlayer.instance.OnResetEvent();
    }

    public override void StateUpdate()
    {
        if (!MRWallManager.instance.CheckUnitStatus() && !playGameOverVideo)
        {
            MRWallManager.instance.PlayGameOverVideo();
            playGameOverVideo = true;
        }
        else if (playGameOverVideo && !GameOverVideoPlayer.instance.isPlaying)
        {
            if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger) || Input.GetKeyDown(KeyCode.Space))
            {
                if (!prepareExit)
                {
                    AudioManager.instance.SmoothTurnOffBackGMusic(1f);
                    prepareExit = true;
                }
            }
            if (!AudioManager.instance.isBackGroundMusiPlaying)
            {
                stateManager.SetState((int)PaintBallStateManager.PaintBallStateEnum.WAIT_USER_TRIGGER);
            }
        }
    }
}
