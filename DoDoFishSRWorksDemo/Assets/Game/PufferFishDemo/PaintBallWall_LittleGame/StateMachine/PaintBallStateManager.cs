using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBallStateManager : StateManager
{
    public enum PaintBallStateEnum
    {
        WAIT_USER_TRIGGER,
        ROOM_SETTING,
        GAME_INIT,
        GAME_PLAY,
        GAME_OVER,
        Size
    }

    public override void InitStates()
    {
        if (statesObj == null)
        {
            statesObj = new GameObject("Game States");
        }
        statesObj.transform.parent = gameObject.transform;

        states = new State[(int)PaintBallStateEnum.Size];
        states[(int)PaintBallStateEnum.WAIT_USER_TRIGGER] = statesObj.AddComponent<WaitUserTrigger>();
        states[(int)PaintBallStateEnum.ROOM_SETTING] = statesObj.AddComponent<RoomSetting>();
        states[(int)PaintBallStateEnum.GAME_INIT] = statesObj.AddComponent<GameInit>();
        states[(int)PaintBallStateEnum.GAME_PLAY] = statesObj.AddComponent<GamePlay>();
        states[(int)PaintBallStateEnum.GAME_OVER] = statesObj.AddComponent<GameOver>();

        for (int i = 0; i < states.Length; i++)
        {
            states[i].Init(this, i);
        }
    }

    // Use this for initialization
    void Start()
    {
        InitStates();
        //SetState((int)PaintBallStateEnum.WAIT_USER_TRIGGER);
    }

    public void SetStopMusic()
    {
        AudioManager.instance.SmoothTurnOffBackGMusic(0);
    }
}
