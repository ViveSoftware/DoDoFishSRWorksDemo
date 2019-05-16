using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRWallUnit : MonoBehaviour
{
    [SerializeField] private Renderer Cover;
    [SerializeField] private GameObject fish;
    [SerializeField] private Animator Anim;

    [SerializeField] private bool isPlaying = false;

    public float fishSpeed = 5;

    public Transform root;
    public MRWallManager mgr;
    public MeshFilter coverMesh, displayMesh;
    public PaintVR.PBSPaintableObject[] paintObjs;

    public bool IsPlaying { get { return isPlaying; } }

    private Tween delayTween;
    private Vector3 originPos;
    private Vector3 jumpStartPoint;
    private Vector3[] movePath;
    private bool detectShot = false;
    private PlayerHeart rentingHeart;

    public enum state
    {
        onIdel,
        onJumping,
        onPlatform,
    }
    private state curState = state.onIdel;

    private bool onJumping, onPlatform;

    public void DoTweenStart()
    {
        isPlaying = true;
    }

    public void DoTweenCallBack()
    {
        isPlaying = false;
        fish.transform.localPosition = originPos;
        fish.transform.localEulerAngles = new Vector3(0, 270, 0);
        Anim.SetInteger("States", Random.Range((int)0, (int)6));

        if (rentingHeart != null)
        {
            mgr.heartController.ReturnHeart(rentingHeart);
            rentingHeart = null;
        }
    }

    public void RotateStart()
    {
        isPlaying = true;
        detectShot = true;
        curState = state.onPlatform;
        float random = Random.Range(0.2f, 0.6f);

        fish.transform.DOLocalMoveX(-0.5f, 0.2f);
        root.DOLocalRotate(new Vector3(0, 0, 90f), 0.2f);

        if (random < 0.5f)
        {
            rentingHeart = mgr.heartController.GetPathTarget();
            if (rentingHeart != null)
            {
                delayTween = DOVirtual.DelayedCall(random, JumpingDown);
            }
            else
            {
                delayTween = DOVirtual.DelayedCall(random, RotateBack);
            }
        }
        else
        {
            delayTween = DOVirtual.DelayedCall(random, RotateBack);
        }
    }

    public void RotateBack()
    {
        detectShot = false;
        curState = state.onIdel;
        root.DOLocalRotate(Vector3.zero, 0.5f).OnComplete(DoTweenCallBack);
        fish.transform.DOLocalMoveX(0.5f, 0.5f);
    }

    public void JumpingDown()
    {
        curState = state.onJumping;

        Vector3 destination = rentingHeart.transform.position;
        movePath = new Vector3[3];
        movePath[0] = fish.transform.position;
        movePath[1] = new Vector3((fish.transform.position.x + destination.x) / 2,
            fish.transform.position.y + Random.Range(0.05f, 0.2f),
            (fish.transform.position.z + destination.z) / 2);
        movePath[2] = destination;

        fish.transform.DOPath(movePath, fishSpeed, PathType.CatmullRom).SetEase(Ease.InOutQuart).OnWaypointChange(OnWayPointChange);
        fish.transform.DOLookAt(movePath[1] - fish.transform.position, 1);

        delayTween = DOVirtual.DelayedCall(fishSpeed, ArriveDestination);
    }

    public void OnWayPointChange(int wayPointInx)
    {
        if (wayPointInx + 1 < movePath.Length)
        {
            fish.transform.DOLookAt(movePath[wayPointInx + 1] - fish.transform.position, 2.5f);
        }
    }

    public void JumpingBack()
    {
        fish.transform.DOLocalRotate(new Vector3(0, 810, 0), 1, RotateMode.FastBeyond360);
        fish.transform.DOMove(jumpStartPoint, 1);
        delayTween = DOVirtual.DelayedCall(1, RotateBack);
    }

    public void ArriveDestination()
    {
        root.DOLocalRotate(Vector3.zero, 0.5f).OnComplete(DoTweenCallBack);
    }

    public void PlayVideoMode()
    {
        fish.SetActive(false);
        transform.DOLocalRotate(Vector3.forward * 180, 0.2f);
    }

    public void UpdateWorldPosMatrix()
    {
        Cover.material.SetMatrix("_WorldMatrix", Cover.transform.localToWorldMatrix);

        //originPos = fish.transform.position;

        fish.transform.localPosition += new Vector3(-0.5f, 0, 0);
        jumpStartPoint = fish.transform.position;
        fish.transform.localPosition -= new Vector3(-0.5f, 0, 0);
    }

    public void ChangeCoverMeshVertexs(Vector3 st, Vector3 ed)
    {
        Mesh mesh = coverMesh.mesh;
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(st.x, st.y, 0.0f);
        vertices[1] = new Vector3(ed.x, ed.y, 0.0f);
        vertices[2] = new Vector3(ed.x, st.y, 0.0f);
        vertices[3] = new Vector3(st.x, ed.y, 0.0f);

        Vector3 center = new Vector3(st.x + 0.5f, -ed.y + 0.5f);

        coverMesh.transform.localPosition += new Vector3(0.0f, center.y, center.x);
        coverMesh.mesh.vertices = vertices;
        coverMesh.GetComponent<MeshCollider>().sharedMesh = coverMesh.mesh;
    }

    public void ChangeCoverMeshUV(Vector2 st, Vector2 ed)
    {
        Mesh mesh = coverMesh.mesh;
        Vector2[] uvs = new Vector2[4];

        uvs[0] = new Vector2(st.x, st.y);
        uvs[1] = new Vector2(ed.x, ed.y);
        uvs[2] = new Vector2(ed.x, st.y);
        uvs[3] = new Vector2(st.x, ed.y);

        coverMesh.mesh.uv = uvs;
        displayMesh.mesh.uv = uvs;
        //coverMesh.GetComponent<MeshCollider>().sharedMesh = coverMesh.mesh;
    }

    public void ResetUint()
    {
        fish.transform.localPosition = originPos;
        //fish.transform.localPosition += new Vector3(1.0f, 0, 0);
        fish.transform.localEulerAngles = new Vector3(0, 270, 0);
        fish.SetActive(true);
        transform.DOLocalRotate(Vector3.zero, 0.2f);
    }

    private void Start()
    {
        paintObjs[0].AddOnPaintingListener(OnPaintBallShot);
        originPos = fish.transform.localPosition;

        fish.transform.localPosition += new Vector3(-0.5f, 0, 0); 
        jumpStartPoint = fish.transform.position;
        fish.transform.localPosition -= new Vector3(-0.5f, 0, 0);

        PaintBallEventManager.StartListeningGameOverEvent(OnGameOverEvent);
    }

    private void OnDestroy()
    {
        paintObjs[0].RemoveOnPaintingListener(OnPaintBallShot);
        PaintBallEventManager.StopListeningGameOverEvent(OnGameOverEvent);
    }

    private void OnGameOverEvent()
    {
        fish.SetActive(false);
        delayTween.Kill(false);
        RotateBack();
    }

    private void OnPaintBallShot()
    {
        if (detectShot)
        {
            delayTween.Kill(false);
            detectShot = false;

            switch (curState)
            {
                case state.onJumping:
                    fish.transform.DOKill();
                    JumpingBack();
                    break;
                case state.onPlatform:
                    RotateBack();
                    break;
            }
            ScoreManager.instance.AddScore(125);
        }
    }
}
