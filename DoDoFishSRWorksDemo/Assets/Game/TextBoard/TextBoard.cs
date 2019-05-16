#define USE_TEXTPRO
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBoard : MonoBehaviour
{
    public float lenth = 1;
    public float scale = 1;
    public Transform VRCamera;
    public int followAngle = 30;
    public Ease easeType = Ease.OutQuart;
    public float moveSpeed = 2;

#if USE_TEXTPRO
    public TextMeshProUGUI textmesh;
#else
    public TextMesh textmesh;
#endif
    void Start()
    {
#if USE_TEXTPRO
        Canvas.GetDefaultCanvasMaterial().SetInt(
     "unity_GUIZTestMode",
     (int)UnityEngine.Rendering.CompareFunction.Disabled
   );
#endif
    }

    void Update()
    {
        transform.localScale = new Vector3(scale, scale, 1);
        transform.LookAt(VRCamera, Vector3.up);

        Vector3 v1 = (transform.position - VRCamera.position).normalized;
        float angle = Vector3.Angle(v1, VRCamera.forward);
        if (angle > followAngle && !isMoving
            || mustMove
            )
        {
            mustMove = false;
            isMoving = true;
            recPos = VRCamera.forward * lenth;
            transform.DOMove(recPos, moveSpeed).SetEase(easeType).OnComplete(moveDone);
            /*Vector3 dir = (recPos - VRCamera.position);
            dir.y = 0;
            transform.DOLookAt(-dir.normalized, 2);*/
            //Debug.Log("[TextBoard] : move to user's eye front");

            ShowTextBigToSmaller(null, waitSec, ismoveUp, false);
        }
    }

#if !USE_TEXTPRO
    Tweener characterSizeTweener;
#endif
    public void Bigger()
    {
        transform.GetChild(0).DOKill();
        transform.GetChild(0).DOScale(1, 0.5f);
        transform.GetChild(0).DOLocalMoveY(0, 0.5f);
        Transform quard;
        MyHelpNode.FindTransform(transform.GetChild(0), "Quad", out quard);
        quard.transform.DOKill();
        quard.transform.DOScaleX(1, 0.5f);
        quard.transform.DOScaleY(1, 0.5f);

#if !USE_TEXTPRO
        //0.7~1
        if (characterSizeTweener != null) characterSizeTweener.Kill();
        characterSizeTweener = DOTween.To(() => textmesh.characterSize, x => textmesh.characterSize = x, 0.7f, 0.5f);
#endif
    }

    IEnumerator _BigToSmaller(bool moveUp, float __waitSec)
    {
        transform.GetChild(0).DOKill();
        transform.GetChild(0).localPosition = Vector3.zero;
        transform.GetChild(0).localScale = Vector3.one;
#if !USE_TEXTPRO
        textmesh.characterSize = 0.7f;
#endif

        //transform.GetChild(0).DOMove(Vector3.zero, 0.2f);
        //transform.GetChild(0).DOScale(Vector3.one, 0.2f);
        //if (characterSizeTweener != null) { characterSizeTweener.Kill(); }
        //characterSizeTweener = DOTween.To(() => textmesh.characterSize, x => textmesh.characterSize = x, 0.7f, 0.2f);

        Transform quard;
        MyHelpNode.FindTransform(transform.GetChild(0), "Quad", out quard);
        quard.transform.DOKill();
        quard.transform.DOScaleX(1, 0.2f);
        quard.transform.DOScaleY(1, 0.2f);

        yield return new WaitForSeconds(3);

        transform.GetChild(0).DOScale(0.8f, 2);
        transform.GetChild(0).DOLocalMoveY((moveUp) ? 30 : -30, 2);
        quard.transform.DOScaleX(1.2f, 2);
        quard.transform.DOScaleY(0.7f, 2f);

#if !USE_TEXTPRO
        if (characterSizeTweener != null) { characterSizeTweener.Kill(); }
        characterSizeTweener = DOTween.To(() => textmesh.characterSize, x => textmesh.characterSize = x, 0.9f, 2);
#endif
        yield return new WaitForSeconds(__waitSec);

        //gameObject.SetActive(false);
        show(false);
    }

    void show(bool render)
    {
        Renderer[] renders = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renders)
            r.enabled = render;

        textmesh.enabled = render;
    }

    IEnumerator waitBigger(bool moveUp, float __waitSec)
    {
        Bigger();
        yield return new WaitForSeconds(1.2f);

        if (BigToSmallerCor != null)
            StopCoroutine(BigToSmallerCor);
        BigToSmallerCor = StartCoroutine(_BigToSmaller(moveUp, __waitSec));
    }

    float waitSec;
    bool ismoveUp;
    Coroutine BigToSmallerCor, waitBiggerCor;

    public void ShowText(string info)
    {
        if (info != null)
            text = info;
    }

    public void ShowTextBigToSmaller(string info, float __waitSec = 999, bool moveUp = false, bool __mustMove = true)
    {
        mustMove = __mustMove;
        ismoveUp = moveUp;
        waitSec = __waitSec;
        if (info != null)
        {
            text = info;
        }
        else
            return;

        //gameObject.SetActive(true);
        show(true);

        if (waitBiggerCor != null)
            StopCoroutine(waitBiggerCor);
        if (BigToSmallerCor != null)
            StopCoroutine(BigToSmallerCor);

        waitBiggerCor = StartCoroutine(waitBigger(moveUp, __waitSec));
    }
    bool mustMove = true;
    bool isMoving;
    void moveDone()
    {
        isMoving = false;
    }

    Vector3 recPos;
    private void OnEnable()
    {
        recPos = VRCamera.forward * lenth;
    }

    public string text
    {
        get
        {
            return textmesh.text;
        }
        set
        {
            textmesh.text = value;
        }
    }

}
