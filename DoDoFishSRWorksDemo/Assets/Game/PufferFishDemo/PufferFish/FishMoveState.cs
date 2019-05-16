using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishMoveState : IState, FishAIStateManager.IStatePunch
{
    FishAIStateManager manager;

    public static Vector3 GetPlayerFrontPosition(FishAIStateManager manager)
    {
        Transform head = manager.GetPlayerHead();
        Vector3 goal;
        goal = head.forward;
        goal.y = 0;
        goal.Normalize();
        goal = head.position + goal * Random.Range(0.7f, 1.0f);
        goal += head.right * Random.Range(-0.3f, 0.3f);
        //goal += Vector3.up * Random.Range(-0.1f, 0.1f);
        return goal;
    }

    public void EnterState(IState oldState, StatePatternBase statePatternBase)
    {
        manager = statePatternBase as FishAIStateManager;
        Vector3 goal;
        float moveTime;
        if (manager.fishAI.moveGoal.HasValue)
        {            
            goal = manager.fishAI.moveGoal.Value;
            manager.fishAI.moveGoal = null;
            moveTime = manager.fishAI.moveTime;
        }
        else
        {
            goal = FishMoveState.GetPlayerFrontPosition(manager);
            moveTime = 3;
        }

        manager.GetTransform().DOMove(goal, moveTime).SetEase(Ease.InOutQuad).OnComplete(_moveDone);
        manager.GetTransform().DOLookAt((goal - manager.GetTransform().position).normalized, 2);

        int moveAni = Random.Range((int)FishAIStateManager.FishAni.Move1, (int)FishAIStateManager.FishAni.MoveEnd);
        manager.WaitToTriggerAnimation((FishAIStateManager.FishAni)moveAni);

        manager.PlaySound("MovingSound", Random.Range(0.1f, 1.5f));
        manager.PlaySound("MovingSound", Random.Range(1.5f, 2.5f));

        manager.fishAI.StartShakeFishPosScale();
    }

    public void LeaveState()
    {
        manager.fishAI.StopShakeFishPosScale();
    }

    public string Name()
    {
        return "FishMoveState";
    }

    public void OnCollisionEnter(Vector3 punchDir, float PunchM, Vector3 contactP)
    {
        manager.AllProcessCollisionEnter(punchDir, PunchM, contactP);
    }

    public void UpdateState()
    {
    }

    void _moveDone()
    {
        manager.GetTransform().DOKill();

        manager.SwitchState(manager.fishAI.moveDoneState);
        manager.fishAI.moveDoneState = FishAIStateManager.FishState.idle;
    }

}
