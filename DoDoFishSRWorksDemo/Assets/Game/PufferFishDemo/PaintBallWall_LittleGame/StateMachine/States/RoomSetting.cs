using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSetting : State
{

    public override void StateEnter()
    {
        RoomSettingManager.instance.StartSetting = true;
        MRWallManager.instance.heartController.ShowHeartWithoutCollison();
    }

    public override void StateExit()
    {
        RoomSettingManager.instance.StartSetting = false;
        MRWallManager.instance.heartController.OnHideEvent();
    }

    public override void StateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            stateManager.SetState((int)PaintBallStateManager.PaintBallStateEnum.WAIT_USER_TRIGGER);
        }
    }
}
