using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishEntranceState : IState
{
    FishAIStateManager manager;

    public void EnterState(IState oldState, StatePatternBase statePatternBase)
    {
        manager = statePatternBase as FishAIStateManager;

        manager.StopAllCoroutineAndTween();
        manager.GetTransform().localScale = Vector3.one * 0.0001f;
        manager.GetTransform().DOScale(manager.fishAI.fishScale, 2).OnComplete(OnEntranceScaleDone);

        Transform head = manager.GetPlayerHead();
        Vector3 goal = new Vector3();
        goal = head.position + head.forward * 1.5f;
        goal.x += Random.Range(-0.5f, 0.0f);
        goal.y += Random.Range(-0.25f, 0.25f);
        goal.z += Random.Range(-0.5f, 0.0f);
        manager.GetTransform().position = goal;
        manager.GetTransform().LookAt(head.position);
        //manager.WaitToSwitchState(FishAIStateManager.FishState.move, 6);



        _firstEnter = true;
    }

    public void LeaveState()
    {
        manager.fishAI.StopShakeFishPosScale();
    }

    public string Name()
    {
        return "FishEntranceState";
    }

    bool _firstEnter;
    public void UpdateState()
    {
        AnimatorStateInfo info = manager.GetAnimator().GetCurrentAnimatorStateInfo(0);
        if (_firstEnter && info.normalizedTime > 0.5f)
            return;
        _firstEnter = false;

        if (info.IsName("wake_up") && info.normalizedTime >= 0.99f)
        {
            manager.SwitchState(FishAIStateManager.FishState.move);
        }
    }

    void OnEntranceScaleDone()
    {
        manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.WakeUp, 2);
        manager.fishAI.StartShakeFishPosScale();
    }

}
