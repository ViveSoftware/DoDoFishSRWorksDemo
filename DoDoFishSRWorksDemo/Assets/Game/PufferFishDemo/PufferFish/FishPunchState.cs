using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishPunchState : IState
{
    FishAIStateManager manager;
    public void EnterState(IState oldState, StatePatternBase statePatternBase)
    {
        manager = statePatternBase as FishAIStateManager;

        manager.WaitToSwitchState(FishAIStateManager.FishState.move, 2);
        Vector3 goal = FishMoveState.GetPlayerFrontPosition(manager);
        manager.fishAI.moveGoal = goal;
        manager.fishAI.moveTime = 1.5f;
        manager.fishAI.moveDoneState = FishAIStateManager.FishState.angry;

        int punchAni = Random.Range(0, 3);
        if (punchAni == 0)
            manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.Angry);
        else if (punchAni == 1)
            manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.WryFace);
        else if (punchAni == 2)
            manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.Scared);

        manager.fishAI.countFishHit++;
    }

    public void LeaveState()
    {
    }

    public string Name()
    {
        return "FishPunchState";
    }

    public void UpdateState()
    {

    }
}
