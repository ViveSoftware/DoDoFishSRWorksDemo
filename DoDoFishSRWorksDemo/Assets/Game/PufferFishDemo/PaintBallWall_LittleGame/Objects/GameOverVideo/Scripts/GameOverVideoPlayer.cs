using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class GameOverVideoPlayer : MonoBehaviour
{
    [SerializeField] private Transform horizonCover;
    [SerializeField] private Transform verticalCover;
    [SerializeField] private Renderer MovieTexScreen;
    [SerializeField] private VideoPlayer gameOverVideo;
    [SerializeField] private Texture emptyTex;
    [SerializeField] private GameOverTextPlayer textPlayer;

    public bool isPlaying { get { return _isPlaying; } }
    private bool _isPlaying = false;

    private Vector3 verticalStartPos, HorizonStartPos;

    private static GameOverVideoPlayer manager;
    public static GameOverVideoPlayer instance
    {
        get
        {
            if (manager == null)
            {
                manager = FindObjectOfType(typeof(GameOverVideoPlayer)) as GameOverVideoPlayer;
                if (manager == null)
                {
                    Debug.LogError("[UndoManager::Singleton] There should be one active gameobject attached UndoManager in scene");
                }
            }
            return manager;
        }
    }

    private void Start()
    {
        verticalStartPos = verticalCover.localPosition; HorizonStartPos = horizonCover.localPosition;
    }

    public void StartPlay()
    {
        _isPlaying = true;
        DOVirtual.DelayedCall(3f, MoveVerticalCover);
    }

    public void MoveVerticalCover()
    {
        
        verticalCover.DOLocalMoveY(5, 0.5f).OnComplete(PlayMovieTexture);
    }

    public void PlayMovieTexture()
    {
        verticalCover.gameObject.SetActive(false);
        gameOverVideo.gameObject.SetActive(true);
        gameOverVideo.Stop();
        gameOverVideo.Play();
        DOVirtual.DelayedCall(1.5f, MoveHorizonCover);
    }

    public void MoveHorizonCover()
    {
        horizonCover.DOLocalMoveZ(8, 2f).OnComplete(DisplayGameResult);
    }

    public void DisplayGameResult()
    {
        textPlayer.PlayResultAnim(PlayResultComplete);
        gameOverVideo.gameObject.SetActive(false);
    }

    public void PlayResultComplete()
    {
        _isPlaying = false;
    }

    public void OnResetEvent()
    {
        verticalCover.gameObject.SetActive(true);
        verticalCover.localPosition = verticalStartPos;
        horizonCover.localPosition = HorizonStartPos;
        MovieTexScreen.material.SetTexture("_MainTex", emptyTex);
        textPlayer.OnResetEvent();
    }
}
