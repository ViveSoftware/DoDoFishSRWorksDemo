using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishLeaveState : IState
{
    FishAIStateManager manager;
    public void EnterState(IState oldState, StatePatternBase statePatternBase)
    {
        manager = statePatternBase as FishAIStateManager;
        Vector3 toward = manager.GetPlayerHead().position - manager.GetTransform().position;
        toward.Normalize();
        manager.GetTransform().DOLookAt(-toward, 2, AxisConstraint.None, Vector3.up).SetEase(Ease.InBounce);
        manager.PlaySound("MovingSound", Random.Range(0.5f, 1.5f));

        int moveAni = Random.Range((int)FishAIStateManager.FishAni.Move1, (int)FishAIStateManager.FishAni.MoveEnd);
        manager.WaitToTriggerAnimation((FishAIStateManager.FishAni)moveAni);

        manager.GetTransform().DOScale(0, 2).OnComplete(fishScaleDone);
    }

    void fishScaleDone()
    {
        manager.GetTransform().gameObject.SetActive(false);
    }

    public void LeaveState()
    {
        
    }

    public string Name()
    {
        return "FishLeaveState";
    }

    public void UpdateState()
    {
        
    }
}
