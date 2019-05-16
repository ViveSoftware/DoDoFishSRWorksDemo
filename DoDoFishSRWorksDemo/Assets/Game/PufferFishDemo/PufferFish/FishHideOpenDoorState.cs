using Demo;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishHideOpenDoorState : IState
{
    FishAIStateManager manager;

    public void EnterState(IState oldState, StatePatternBase statePatternBase)
    {
        manager = statePatternBase as FishAIStateManager;

        //use out pos to prevent transparent the door.
        Vector3 goal = manager.fishAI.GetOutPos();
        manager.GetTransform().DOMove(goal, 1.4f).SetEase(Ease.InOutQuad).OnComplete(_moveDone);
        manager.GetTransform().DOLookAt(goal , 2);
        //  manager.GetTransform().(goal[0], 2);

        int moveAni = Random.Range((int)FishAIStateManager.FishAni.Move1, (int)FishAIStateManager.FishAni.MoveEnd);
        manager.WaitToTriggerAnimation((FishAIStateManager.FishAni)moveAni);

        manager.PlaySound("MovingSound", Random.Range(0.1f, 1.5f));
        manager.PlaySound("MovingSound", Random.Range(1.5f, 2.5f));

        openOnce = false;

        manager.fishAI.StopShakeFishPosScale();
        GameManager.Instance.SetFishToRenderTop();
    }

    public void LeaveState()
    {
    }

    public string Name()
    {
        return "FishHideOpenDoorState";
    }
    
    bool openOnce;
    public void UpdateState()
    {
        if (openOnce)
            return;
        Vector3 goal = manager.fishAI.GetOutPos();
        float dis = Vector3.SqrMagnitude(manager.fishAI.transform.position - goal);
        float detectDis = manager.fishAI.hideSize * 2;
        if (dis < detectDis * detectDis)
        {
            Vector3 normal = GameManager.Instance.GetFaceToPlayerNormal(manager.fishAI.fishHidingWall.position, manager.fishAI.fishHidingWall.forward);
            Vector3 pos = manager.fishAI.GetHidePos() + normal * manager.fishAI.hideSize + Vector3.up * 0.155f;//fish's pivot is at buttom
            GameManager.Instance.OpenFishHidingDoor(pos, normal);
            //UnityEditor.EditorApplication.isPaused = true;
            openOnce = true;
        }
    }

    void _moveDone()
    {
        manager.GetTransform().DOKill();
        //arraive the out position, move into the hide position
        manager.GetTransform().DOMove(manager.fishAI.GetHidePos(), 0.5f).SetEase(Ease.InOutQuad).OnComplete(_moveDoneHide);
        manager.GetTransform().DOLookAt(manager.fishAI.GetHidePos() , 2f);
        //manager.GetTransform().LookAt(manager.fishAI.GetHidePos());
    }

    void _moveDoneHide()
    {
        manager.GetTransform().DOKill();
        manager.SwitchState(FishAIStateManager.FishState.hideNseekOpenDoor);
        GameManager.Instance.CloseFishHidingDoor();
    }

}
