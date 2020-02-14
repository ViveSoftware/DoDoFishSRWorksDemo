using Demo;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishHideNSeekOpenDoorState : IState, FishAIStateManager.IStatePaintBallShot
{
    FishAIStateManager manager;
    int WryFaceTime;
    int shootFishCount;
    bool fishCanBeShoot;

    public void EnterState(IState oldState, StatePatternBase statePatternBase)
    {
        manager = statePatternBase as FishAIStateManager;

        Vector3 toward = manager.GetPlayerHead().position - manager.GetTransform().position;
        toward.Normalize();
        manager.GetTransform().DOLookAt(toward, 1, AxisConstraint.None, Vector3.up).SetEase(Ease.InBounce).OnComplete(_faceToDone);

        //manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.WryFace);

        isFaceToDone = false;
        WryFaceTime = 4;
        shootFishCount = 3;
        fishCanBeShoot = false;
        manager.StartCoroutine(_startWryFace());

        GameManager.Instance.PaintGunCanPickUp();
    }

    bool isFaceToDone;
    void _faceToDone()
    {
        isFaceToDone = true;
    }

    void _moveInDone()
    {
        WryFaceTime--;
        if (WryFaceTime <= 0)
        {
            Vector3 goal = FishMoveState.GetPlayerFrontPosition(manager);
            manager.fishAI.moveGoal = goal;
            manager.fishAI.moveTime = 3;
            manager.fishAI.moveDoneState = FishAIStateManager.FishState.idle;
            manager.WaitToSwitchState(FishAIStateManager.FishState.move);
            GameManager.Instance.SetHandTouchMode();
            GameManager.Instance.ShowHitFishInfo();
            manager.fishAI.ResetFishData();
            GameManager.Instance.CloseFishHidingDoor(0.0f, false);
            GameManager.Instance.RecoverFishToDefaultLayer();
            return;
        }

        manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.Angry);
        manager.StartCoroutine(_startWryFace());
    }

    void _moveOutDone1()
    {
        manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.WryFace);
        manager.StartCoroutine(_endWryFace(2));
    }

    IEnumerator _moveOutBeShoot()
    {
        fishCanBeShoot = false;
        manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.Scared);
        yield return new WaitForSeconds(1);

        _endWryFaceMoveBack();
        //manager.GetTransform().DOMove(manager.fishAI.getRanHidePos(), 0.9f).OnComplete(_moveInDone);
    }

    Vector3 hidePos, outPos;
    IEnumerator _startWryFace()
    {
        yield return new WaitForSeconds(1);
        manager.fishAI.GetRanPos(out hidePos, out outPos);
        manager.fishAI.transform.position = hidePos;
        manager.GetTransform().DOMove(outPos, 1.0f).OnComplete(_moveOutDone1);

        Vector3 normal = GameManager.Instance.GetFaceToPlayerNormal(manager.fishAI.fishHidingWall.position, manager.fishAI.fishHidingWall.forward);
        Vector3 pos = hidePos + normal * manager.fishAI.hideSize + Vector3.up * 0.155f;//fish's pivot is at buttom
        GameManager.Instance.OpenFishHidingDoor(pos, normal);
    }

    IEnumerator _endWryFace(float sec)
    {
        fishCanBeShoot = true;
        yield return new WaitForSeconds(sec);
        fishCanBeShoot = false;

        _endWryFaceMoveBack();
    }

    void _endWryFaceMoveBack()
    {
        GameManager.Instance.CloseFishHidingDoor(0.5f);
        manager.GetTransform().DOMove(hidePos, 1.0f).OnComplete(_moveInDone);
    }

    public void LeaveState()
    {
    }

    public string Name()
    {
        return "FishHideNSeekOpenDoorState";
    }
    
    public void UpdateState()
    {
        if (!isFaceToDone)
            return;

        manager.GetTransform().LookAt(manager.fishAI.PlayerEye);
    }

    public void OnPaintBallShot()
    {
        if (!fishCanBeShoot)
            return;
        fishCanBeShoot = false;

        shootFishCount--;
        if (shootFishCount == 0)
        {
            manager.StopAllCoroutineAndTween();
            manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.Scared);
            manager.StartCoroutine(_hideStartGame());
        }
        else
        {
            manager.StopAllCoroutineAndTween();
            manager.StartCoroutine(_moveOutBeShoot());
        }
        manager.StopSound();
    }

    IEnumerator _hideStartGame()
    {
        yield return new WaitForSeconds(1);

        //Move back         
        manager.GetTransform().DOMove(hidePos, 0.8f).SetEase(Ease.OutQuart).OnComplete(_hideStartGameDone);
    }

    void _hideStartGameDone()
    {
        manager.WaitToSwitchState(FishAIStateManager.FishState.paintBallGame, 1);
        GameManager.Instance.CloseFishHidingDoor(0.5f, false);
    }
}
