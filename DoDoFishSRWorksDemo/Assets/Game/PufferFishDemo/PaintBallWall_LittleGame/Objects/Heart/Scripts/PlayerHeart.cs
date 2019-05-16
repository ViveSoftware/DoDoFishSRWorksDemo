using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHeart : MonoBehaviour
{
    public enum state
    {
        idel,
        broken
    }
    public state curState { get { return _curState; } }
    private state _curState = state.idel;
    [SerializeField] private Renderer heartRender;
    [SerializeField] private ParticleSystem explosion;

    private UnityEvent OnBrokenEvet = new UnityEvent();

    public void AddOnBrokenListener(UnityAction method)
    {
        OnBrokenEvet.AddListener(method);
    }

    public void RemoveOnBrokenListener(UnityAction method)
    {
        OnBrokenEvet.RemoveListener(method);
    }

    public void InvokeOnBrokenEvent()
    {
        OnBrokenEvet.Invoke();
    }

    public void ShowHeartWithoutCollison()
    {
        _curState = state.broken;
        heartRender.enabled = true;
    }

    public void ResetPlayerHeart()
    {
        _curState = state.idel;
        heartRender.enabled = true;
    }

    public void HidePlayerHeart()
    {
        _curState = state.broken;
        heartRender.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PaintMesh" && _curState == state.idel)
        {
            heartRender.enabled = false;
            explosion.Play();

            _curState = state.broken;
            InvokeOnBrokenEvent();
        }
    }
    // Use this for initialization
    private void Start ()
    {
        heartRender.transform.DOLocalRotate(Vector3.up * 360, 3f, RotateMode.LocalAxisAdd).SetLoops(int.MaxValue).SetEase(Ease.Linear);
    }

}
