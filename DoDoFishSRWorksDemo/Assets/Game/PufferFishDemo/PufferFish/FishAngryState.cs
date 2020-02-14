using Demo;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAngryState : IState, FishAIStateManager.IStatePunch
{
    FishAIStateManager manager;
    public void EnterState(IState oldState, StatePatternBase statePatternBase)
    {
        manager = statePatternBase as FishAIStateManager;

        Vector3 toward = manager.GetPlayerHead().position - manager.GetTransform().position;
        toward.Normalize();
        manager.GetTransform().DOLookAt(toward, 2, AxisConstraint.None, Vector3.up).SetEase(Ease.InOutElastic).OnComplete(_faceToDone);
        _firstEnter = true;
        isfaceToDone = false;

        manager.fishAI.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        manager.fishAI.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    bool isfaceToDone;
    void _faceToDone()
    {
        Vector3 toward = manager.GetPlayerHead().position - manager.GetTransform().position;
        manager.GetTransform().DOLookAt(toward, 2);
        manager.WaitToTriggerAnimation(FishAIStateManager.FishAni.Angry);

        manager.PlaySound("IdleSound", Random.Range(0.5f, 2.5f));
        //manager.PlaySound("IdleSound", Random.Range(2.5f, 4.5f));
        isfaceToDone = true;
    }

    public void LeaveState()
    {

    }

    public string Name()
    {
        return "FishAngryState";
    }

    public void OnCollisionEnter(Vector3 punchDir, float PunchM, Vector3 contactP)
    {
        manager.AllProcessCollisionEnter(punchDir, PunchM, contactP);
    }

    bool _firstEnter;
    public void UpdateState()
    {
        if (!isfaceToDone)
            return;

        AnimatorStateInfo info = manager.GetAnimator().GetCurrentAnimatorStateInfo(0);
        if (_firstEnter && info.normalizedTime > 0.5f)
            return;
        _firstEnter = false;

        if (manager.fishAI.countFishHit > 2)
        {
            //if (info.IsName("Angry3"))
            //{
            //    if (info.normalizedTime >= 0.99f)
            //    {
            //        setHidePosition();
            //        manager.WaitToSwitchState(FishAIStateManager.FishState.moveAStar);
            //    }
            //}

            //if (
            //    !(info.IsName("Angry1") || info.IsName("Angry2") || info.IsName("Angry3"))
            //    )
            //{
            //    setHidePosition();
            //    manager.WaitToSwitchState(FishAIStateManager.FishState.moveAStar);
            //}
            if (
                (info.IsName("Angry3") && (info.normalizedTime >= 0.99f)) ||
                !(info.IsName("Angry1") || info.IsName("Angry2") || info.IsName("Angry3"))
                )
            {
                if (ReconstructManager.Instance.selectedWallRoot != null)
                {
                    if (ReconstructManager.Instance.selectedWallRoot.transform.childCount > 0)
                    {
                        setHidePosition(manager);
                    }
                    else
                    {
                        //XRDemo.Instance.ShowInfo("<b><size=120><color=red>Cannot find a wall</b>\n<color=white>Please select your scanning wall");
                        //XRDemo.Instance.ResetReconstructData();
                        //if (XRDemo.Instance.recontructWall.transform.childCount > 0)
                        //{
                        //    //XRDemo.Instance.ShowInfo("<b><size=120%>Find hiding wall!");
                        //    //manager.StartCoroutine(showFindWall());
                        //}
                    }
                }
                else
                {
                    manager.WaitToSwitchState(FishAIStateManager.FishState.move);
                }
            }
        }
        else
        {
            if (
                (info.IsName("Angry3") && (info.normalizedTime >= 0.99f)) ||
                !(info.IsName("Angry1") || info.IsName("Angry2") || info.IsName("Angry3"))
                )
            {
                manager.WaitToSwitchState(FishAIStateManager.FishState.move);
            }
        }
    }

    static void setHidePosition(FishAIStateManager manager)
    {
        //if (XRDemo.Instance.recontructWall != null)
        //{
        //random get a wall
        manager.fishAI.fishHidingWall = ReconstructManager.Instance.selectedWallRoot.transform.GetChild(Random.Range(0, ReconstructManager.Instance.selectedWallRoot.transform.childCount));

        Vector3 wallForward = manager.fishAI.fishHidingWall.forward;
        Vector3 dir2Player = manager.fishAI.PlayerEye.transform.position - manager.fishAI.fishHidingWall.position;
        if (Vector3.Dot(dir2Player, wallForward) < 0)
            wallForward *= -1f;

        Vector3 hidePosition = manager.fishAI.fishHidingWall.position - wallForward * manager.fishAI.hideSize;
        List<Vector3> hideWorldPoss = Get3HideWorldPosList(manager);
        for (int a = 0; a < hideWorldPoss.Count; a++)
        {
            Vector3 dirHide = hideWorldPoss[a] - manager.fishAI.fishHidingWall.position;
            float hideLength = dirHide.magnitude;
            dirHide /= hideLength;
            Vector3 hidePos = hidePosition + dirHide * hideLength * 0.5f;
            manager.fishAI.SetHidePos(a, hidePos);
            Vector3 outPos = hidePos + wallForward * manager.fishAI.hideSize * 2;
            manager.fishAI.SetOutPos(a, outPos);
        }

        manager.WaitToSwitchState(FishAIStateManager.FishState.hideOpenDoor);
    }

    static List<Vector3> Get3HideWorldPosList(FishAIStateManager manager)
    {
        //random get 3 position
        MeshFilter mf = manager.fishAI.fishHidingWall.GetComponent<MeshFilter>();
        List<Vector3> meshvertices = new List<Vector3>();
        foreach (Vector3 v in mf.mesh.vertices)
            meshvertices.Add(v);

        List<Vector3> hidePoss = new List<Vector3>();
        while (meshvertices.Count > 0)
        {
            int ranINDX = Random.Range(0, meshvertices.Count - 1);
            Vector3 pos = meshvertices[ranINDX];
            meshvertices.RemoveAt(ranINDX);

            pos = manager.fishAI.fishHidingWall.TransformPoint(pos);
            pos.y = Mathf.Max(pos.y, 1);//saturate not lower then 1m

            Vector3 dir = pos - manager.fishAI.fishHidingWall.position;
            float length = dir.magnitude;
            dir = dir / length;

            //pick position min length > 0.5m
            if (length < 0.5f)
                continue;

            //saturate not lower then 1m
            if (pos.y < 1)
                pos.y = 1;

            hidePoss.Add(pos);
            if (hidePoss.Count == 3)
                break;
        }

        if (hidePoss.Count != 3)
            Debug.LogError("[FishAngryState][setHidePosition] : hidePoss.Count != 3 : " + hidePoss.Count);
        return hidePoss;
    }

    static void setHidePosition(out Vector3 hidePos, out Vector3 outPos, FishAIStateManager manager, Vector3 goOutVec)
    {
        hidePos = manager.fishAI.fishHidingWall.position - manager.fishAI.fishHidingWall.forward * manager.fishAI.hideSize;
        goOutVec = manager.fishAI.fishHidingWall.TransformPoint(goOutVec);
        goOutVec.y = Mathf.Max(goOutVec.y, 1);//saturate not lower then 1m
        Vector3 dirWall = goOutVec - manager.fishAI.fishHidingWall.position;
        float hideSize = dirWall.magnitude;
        dirWall /= hideSize;
        outPos = hidePos + dirWall * hideSize * ((goOutVec.y == 1) ? 2f : 1.3f);
    }

}
