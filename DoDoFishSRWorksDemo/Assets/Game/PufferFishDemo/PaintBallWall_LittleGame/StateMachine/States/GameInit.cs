using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInit : State
{
    public override void StateEnter()
    {
        MRWallManager.instance.heartController.OnResetEvent();
       // MRWallManager.instance.ResetMRWall();
    }

    public override void StateExit()
    {

    }

    public override void StateUpdate()
    {
        if (MRWallManager.instance.initStatus)
        {
            stateManager.SetState((int)PaintBallStateManager.PaintBallStateEnum.GAME_PLAY);
        }
    }
}
