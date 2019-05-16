using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishIdleState : IState, FishAIStateManager.IStatePunch
{
    FishAIStateManager manager;
    int idlePoseCount;
    public void EnterState(IState oldState, StatePatternBase statePatternBase)
    {
        manager = statePatternBase as FishAIStateManager;
        Vector3 toward = manager.GetPlayerHead().position - manager.GetTransform().position;
        toward.Normalize();
        manager.GetTransform().DOLookAt(toward, 2, AxisConstraint.None, Vector3.up).SetEase(Ease.InBounce).OnComplete(_faceToDone);
        manager.PlaySound("MovingSound", Random.Range(0.5f, 1.5f));

        int moveAni = Random.Range((int)FishAIStateManager.FishAni.Move1, (int)FishAIStateManager.FishAni.MoveEnd);
        manager.WaitToTriggerAnimation((FishAIStateManager.FishAni)moveAni);

        _firstEnter = true;
        idlePoseCount = 0;
    }

    void _faceToDone()
    {
        manager.GetTransform().DOLookAt(manager.GetPlayerHead().position, 2);
        randomIdle();
        manager.PlaySound("IdleSound", Random.Range(0.5f, 2.5f));
        manager.PlaySound("IdleSound", Random.Range(2.5f, 4.5f));

        //manager.WaitToSwitchState(FishAIStateManager.FishState.move, 5);
    }

    void randomIdle()
    {
        int idleAni = Random.Range((int)FishAIStateManager.FishAni.Idle1, (int)FishAIStateManager.FishAni.IdleEnd);
        manager.WaitToTriggerAnimation((FishAIStateManager.FishAni)idleAni);
        considerNext = 0;
        Debug.Log("[FishIdleState] idleAni : " + (FishAIStateManager.FishAni)idleAni);
    }

    public void LeaveState()
    {
    }

    public string Name()
    {
        return "FishIdleState";
    }

    bool _firstEnter;
    int considerNext;
    public void UpdateState()
    {
        AnimatorStateInfo info = manager.GetAnimator().GetCurrentAnimatorStateInfo(0);
        if (_firstEnter && info.normalizedTime > 0.5f)
            return;
        _firstEnter = false;

        if (considerNext > 10 && info.normalizedTime >= 0.99f)
        {
            if (idlePoseCount == 3)
                manager.SwitchState(FishAIStateManager.FishState.move);
            else
                randomIdle();
            idlePoseCount++;
        }
        considerNext++;
    }

    public void OnCollisionEnter(Vector3 punchDir, float PunchM, Vector3 contactP)
    {
        manager.AllProcessCollisionEnter(punchDir, PunchM, contactP);
    }
}
