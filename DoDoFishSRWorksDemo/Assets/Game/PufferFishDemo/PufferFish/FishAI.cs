using Demo;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAI : MonoBehaviour
{
    FishAIStateManager _fishAIStateManager;

    [HideInInspector] public Transform PlayerEye { get { return ARRender.Instance.VRCamera().transform; } }
    [SerializeField] public bool punchTest;
    [SerializeField] public float hideSize = 0.5f;
    [SerializeField] public float fishScale = 0.8f;
    [SerializeField] GameObject punchParticlePrefab;

    [HideInInspector] public float moveTime = 1;
    [HideInInspector] public Vector3? moveGoal;
    [HideInInspector] public FishAIStateManager.FishState moveDoneState = FishAIStateManager.FishState.idle;

    [HideInInspector] public Transform fishHidingWall;
    Vector3[] fishOutPos = new Vector3[3];
    Vector3[] fishHidePos = new Vector3[3];
    [HideInInspector] public int countFishHit;

    public Vector3 GetHidePos() { return fishHidePos[0]; }
    public Vector3 GetOutPos() { return fishOutPos[0]; }
    public void SetHidePos(int id, Vector3 pos) { Debug.Assert(id < 3); fishHidePos[id] = pos; }
    public void SetOutPos(int id, Vector3 pos) { Debug.Assert(id < 3); fishOutPos[id] = pos; }

    public Vector3 getRanOutPos() { return fishOutPos[Random.Range(0, fishOutPos.Length)]; }
    public Vector3 getRanHidePos() { return fishHidePos[Random.Range(0, fishHidePos.Length)]; }
    public void GetRanPos(out Vector3 inPos, out Vector3 outPos)
    { int id = Random.Range(0, fishOutPos.Length); inPos = fishHidePos[id]; outPos = fishOutPos[id]; }

    public void ResetFishData()
    {
        countFishHit = 0;//reset hit count

        //reset paint map
        PaintVR.PBSPaintableObject[] pobjs = GetComponentsInChildren<PaintVR.PBSPaintableObject>();
        foreach (PaintVR.PBSPaintableObject o in pobjs)
            o.PaintableReset();
    }

    void Start()
    {
        RestartAI();

        FishEyeController eyeController = GetComponentInChildren<FishEyeController>();
        eyeController.Target = PlayerEye;
    }

    void Update()
    {
        _fishAIStateManager.Update();

        if (punchTest)
        {
            punchTest = false;
            PunchFish(Vector3.right + Vector3.up, 1000, transform.position + Vector3.forward * 0.8f);
        }

        Rigidbody fishRB = gameObject.GetComponent<Rigidbody>();
        if (fishRB != null)
        {
            float sqrVel = fishRB.velocity.sqrMagnitude;
            if (sqrVel > 4f * 4f)
            {
                Vector3 dir = fishRB.velocity.normalized;
                fishRB.velocity = dir * 4f;
            }
        }
        //Debug.LogWarning("[PunchFish] : velocity : " + gameObject.GetComponent<Rigidbody>().velocity.magnitude);
    }

    public void RestartAI()
    {
        _fishAIStateManager = new FishAIStateManager(this);
        _fishAIStateManager.Restart();
    }

    public void OnPaintBallShot()
    {
        _fishAIStateManager.OnPaintBallShot();
    }


    void OnCollisionEnter(Collision col)
    {
        if (col.collider.gameObject.layer != HandTouchFish.HandTouchLayer)
            return;
        //if (SRWorkHand.Instance.IsHandLeave())
        //    return;
        //Debug.LogWarning("[FishAI][OnCollisionEnter] : " + col.collider.name);
        _fishAIStateManager.OnCollisionEnter(col);
    }

    public void PunchFish(Vector3 punchV, float punchM, Vector3 contactPoint)
    {
        if (_fishAIStateManager.GetCurrentState() is FishPunchState)
            return;

        _fishAIStateManager.PlaySound("Punch" + Random.Range(1, 3 + 1), 0);
        GameObject particle = Instantiate(punchParticlePrefab);
        if (punchM == 1000f)
            particle.GetComponent<Renderer>().material.SetColor("_TintColor", Color.green);
        else
            particle.GetComponent<Renderer>().material.SetColor("_TintColor", Color.yellow);
        //     particle.transform.forward = -punchV;
        particle.transform.position = contactPoint;
        particle.layer = ARRender.GetParticleLayer();

        punchV = Vector3.Lerp(punchV, PlayerEye.forward, 0.7f);
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody>().AddForceAtPosition(punchV.normalized * punchM * 0.000001f, contactPoint, ForceMode.Force);
            gameObject.GetComponent<Rigidbody>().drag = 0.4f;
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            //Debug.LogError("[PunchFish] : velocity : " + gameObject.GetComponent<Rigidbody>().velocity.magnitude);
        }

        _fishAIStateManager.StopAllCoroutineAndTween();
        _fishAIStateManager.SwitchState(FishAIStateManager.FishState.punch);
    }

    public void DisableFish()
    {
        _fishAIStateManager.StopAllCoroutineAndTween();
        _fishAIStateManager.SwitchState(FishAIStateManager.FishState.leave);
    }

    public bool IsIdleState()
    {
        return (
            !(_fishAIStateManager.GetCurrentState() is FishHideNSeekOpenDoorState) &&
            !(_fishAIStateManager.GetCurrentState() is PaintBallGameState)
             );
    }

    public void StopShakeFishPosScale()
    {
        _fishAIStateManager.GetShakeTransform().DOKill();
        _fishAIStateManager.GetShakeTransform().DOLocalMove(Vector3.zero, 1);
        _fishAIStateManager.GetShakeTransform().DOScale(Vector3.one, 1);
    }

    public void StartShakeFishPosScale()
    {
        _fishAIStateManager.GetShakeTransform().localScale = Vector3.one;
        _fishAIStateManager.GetShakeTransform().localPosition = Vector3.zero;

        _fishAIStateManager.GetShakeTransform().DOShakePosition(Random.Range(1, 3), Random.Range(0.1f, 0.2f), 0, 180).SetEase(Ease.InQuad)/*.SetLoops(-1, LoopType.Restart)*/.OnComplete(_shakePosDone);
        _fishAIStateManager.GetShakeTransform().DOShakeScale(Random.Range(1, 3), Random.Range(0.1f, 0.2f), 0, 180).SetEase(Ease.InQuad)/*.SetLoops(-1, LoopType.Restart)*/.OnComplete(_shakeScaleDone);
        //manager.GetShakeTransform().DOShakeScale(1, 0.3f, 3).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);//Setting loops to -1 will make the tween loop infinitely. 
    }

    void _shakePosDone()
    {
        _fishAIStateManager.GetShakeTransform().DOShakePosition(Random.Range(1, 3), Random.Range(0.1f, 0.2f), 0, 180).SetEase(Ease.InQuad)/*.SetLoops(-1, LoopType.Restart)*/.OnComplete(_shakePosDone);
    }

    void _shakeScaleDone()
    {
        _fishAIStateManager.GetShakeTransform().DOShakeScale(Random.Range(1, 3), Random.Range(0.1f, 0.2f), 0, 180).SetEase(Ease.InQuad)/*.SetLoops(-1, LoopType.Restart)*/.OnComplete(_shakeScaleDone);
    }
}
