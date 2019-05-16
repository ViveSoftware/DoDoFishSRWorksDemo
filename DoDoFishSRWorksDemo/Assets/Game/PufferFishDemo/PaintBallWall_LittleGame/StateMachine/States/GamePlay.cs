using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlay : State
{
    private bool prepareExit = false;
    public override void StateEnter()
    {
        AudioManager.instance.PlayGamePlayBackGroundMusic();
        CommandManager.instance.StartLevel();
        prepareExit = false;
    }

    public override void StateExit()
    {
        
    }

    public override void StateUpdate()
    {
        if (CommandManager.instance.levelFinish)// && !AudioManager.instance.isBackGroundMusiPlaying)
        {
            PaintBallEventManager.TriggerGameOverEvent();
            stateManager.SetState((int)PaintBallStateManager.PaintBallStateEnum.GAME_OVER);
        }
        else if (MRWallManager.instance.heartController.CurrentAliveHeart == 0)
        {
            if (!prepareExit)
            {
                AudioManager.instance.SmoothTurnOffBackGMusic(4f);
                CommandManager.instance.EndLevel();
                prepareExit = true;
            }
            if (!AudioManager.instance.isBackGroundMusiPlaying)
            {
                PaintBallEventManager.TriggerGameOverEvent();
                stateManager.SetState((int)PaintBallStateManager.PaintBallStateEnum.GAME_OVER);
            }
        }
    }
}
