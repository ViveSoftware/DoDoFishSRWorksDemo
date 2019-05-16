using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAIStateManager : StatePatternBase
{
    public interface IStatePunch
    {
        void OnCollisionEnter(Vector3 punchDir, float PunchM, Vector3 contactP);
    }

    public interface IStatePaintBallShot
    {
        void OnPaintBallShot();
    }

    public enum FishState
    {
        entrance,
        idle,
        move,
        hideOpenDoor,
        leave,
        punch,
        angry,
        hideNseekOpenDoor,
        paintBallGame,
        size,
    }

    public enum FishAni
    {
        Idle1,
        Idle2,
        Idle3,
        Idle4,
        Idle5,
        Idle6,
        Idle7,
        Idle8,
        Idle9,
        Idle10,
        IdleEnd,

        Move1,
        Move2,
        MoveEnd,

        WakeUp,
        Angry,
        WryFace,
        Scared,
    }

    public FishAI fishAI;


    public void StopSound()
    {
        fishAI.GetComponent<PufferFishSound>().StopSound();
    }

    public void PlaySound(string soundName, float delaySec = 0)
    {
        fishAI.GetComponent<PufferFishSound>().PlaySound(soundName, 0.5f, 0, delaySec);
    }

    public FishAIStateManager(FishAI fai)
    {
        fishAI = fai;
    }

    public Animator GetAnimator()
    {
        return fishAI.GetComponent<Animator>();
    }

    public Transform GetPlayerHead()
    {
        return fishAI.PlayerEye;
    }

    public Transform GetTransform()
    {
        return fishAI.transform;
    }

    public Transform GetShakeTransform()
    {
        return fishAI.transform.GetChild(0);
    }

    protected override void OnCreateState(out IState[] pleaseCreateThisList)
    {
        StopAllCoroutineAndTween();

        pleaseCreateThisList = new IState[(int)FishState.size];

        pleaseCreateThisList[(int)FishState.entrance] = new FishEntranceState();
        pleaseCreateThisList[(int)FishState.idle] = new FishIdleState();
        pleaseCreateThisList[(int)FishState.move] = new FishMoveState();
        pleaseCreateThisList[(int)FishState.hideOpenDoor] = new FishHideOpenDoorState();
        pleaseCreateThisList[(int)FishState.leave] = new FishLeaveState();
        pleaseCreateThisList[(int)FishState.punch] = new FishPunchState();
        pleaseCreateThisList[(int)FishState.angry] = new FishAngryState();
        pleaseCreateThisList[(int)FishState.hideNseekOpenDoor] = new FishHideNSeekOpenDoorState();
        pleaseCreateThisList[(int)FishState.paintBallGame] = new PaintBallGameState();

        SwitchState(FishState.entrance);
    }

    public override void DebugLog(string text, int level = 0)
    {
        Debug.Log(text);
    }

    Coroutine wait2State;
    public void WaitToSwitchState(FishState state, float delaySec = 0)
    {
        if (wait2State != null)
        {
            fishAI.StopCoroutine(wait2State);
            wait2State = null;
        }
        if (delaySec > 0)
            wait2State = fishAI.StartCoroutine(_waitToSwitchState(state, delaySec));
        else
            SwitchState(state);
    }

    IEnumerator _waitToSwitchState(FishState state, float delaySec)
    {
        yield return new WaitForSeconds(delaySec);
        SwitchState(state);
    }

    public void StartCoroutine(IEnumerator coroutine)
    {
        fishAI.StartCoroutine(coroutine);
    }

    public void WaitToTriggerAnimation(FishAni ani, float delaySec = 0)
    {
        if (delaySec > 0)
            fishAI.StartCoroutine(_waitToTriggerAnimation(ani, delaySec));
        else
            GetAnimator().SetTrigger(ani.ToString());
    }

    IEnumerator _waitToTriggerAnimation(FishAni ani, float delaySec)
    {
        yield return new WaitForSeconds(delaySec);
        GetAnimator().SetTrigger(ani.ToString());
    }

    public void OnPaintBallShot()
    {
        IStatePaintBallShot paintShotState = GetCurrentState() as IStatePaintBallShot;
        if (paintShotState != null)
            paintShotState.OnPaintBallShot();
    }

    float collisionCountDown;
    override public void Update()
    {
        base.Update();

        /*collisionCountDown -= Time.deltaTime;
        if (collisionCountDown < 0 && hasSavePunch)
        {
            hasSavePunch = false;
            Debug.LogWarning("Fish hit by 'Collider Mesh'");
            IStatePunch statePunch = GetCurrentState() as IStatePunch;
            if (statePunch != null)
                statePunch.OnCollisionEnter(savePunchDir, savePunchM, saveContactP);
        }*/
    }

    //public bool hasSavePunch;
    //Vector3 savePunchDir;
    //float savePunchM;
    //Vector3 saveContactP;
    public void OnCollisionEnter(Collision col)
    {
        /*if (collisionCountDown > 0 && hasSavePunch && col.collider.name != SRWorkHand.GetDynamicHand().name)
        {
            collisionCountDown = 0;
            hasSavePunch = false;
        }

        if (collisionCountDown > 0)
            return;
        collisionCountDown = 0.3f;//setting the 0.5 sec not collision again

        if (!hasSavePunch && col.collider.name == SRWorkHand.GetDynamicHand().name)
        {
            savePunchDir = Vector3.up;
            savePunchM = 1000;
            saveContactP = fishAI.transform.position;
            hasSavePunch = true;
            return;
        }*/

        Vector3 punchDir = col.relativeVelocity;
        float PunchM = punchDir.magnitude;
        punchDir = punchDir / PunchM;
        //Debug.LogWarning("[OnCollisionEnter] : " + col.relativeVelocity.magnitude + " , name : " + col.transform.name);
        if (PunchM <= 0)
            return;

        Vector3 contactP = col.contacts[0].point;

        // Debug.LogWarning("[OnCollisionEnter] : " + col.transform.name);       
        Debug.LogWarning("Fish hit by 'racketObj'");

        IStatePunch punchState = GetCurrentState() as IStatePunch;
        if (punchState != null)
            punchState.OnCollisionEnter(punchDir, PunchM, contactP);
    }

    public void AllProcessCollisionEnter(Vector3 punchDir, float PunchM, Vector3 contactP)
    {
        //Vector3 punchDir = col.relativeVelocity;
        //float PunchM = punchDir.magnitude;
        //Vector3 contactP = col.contacts[0].point;
        //if (col.collider.name == SRWorkControl.DynamicHandName)
        //{
        //    punchDir = Vector3.up;
        //    PunchM = 1000;
        //    contactP = fishAI.transform.position;
        //}

        //if (PunchM > 4f)
        {
            Debug.LogWarning("[AllProcessCollisionEnter] : " + PunchM);
            fishAI.PunchFish(punchDir, PunchM, contactP);
        }
    }

    public void StopAllCoroutineAndTween()
    {
        if (fishAI.GetComponent<PufferFishSound>() != null)
            fishAI.GetComponent<PufferFishSound>().StopAllCoroutines();
        fishAI.StopAllCoroutines();

        GetTransform().DOKill();
        GetShakeTransform().DOKill();
    }
}
