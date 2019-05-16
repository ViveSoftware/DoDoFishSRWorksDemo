using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public bool isBackGroundMusiPlaying { get { return BackGoundAudio.isPlaying; } }

    private AudioSource BackGoundAudio;
    [SerializeField] private AudioClip GamePlay;
    [SerializeField] private AudioClip WaitPlayer;
    [SerializeField] private AudioClip GameOver;

    private IEnumerator VolumeControlIEnumerator = null;

    private static AudioManager manager = null;
    public static AudioManager instance
    {
        get
        {
            if (manager == null)
            {
                manager = FindObjectOfType(typeof(AudioManager)) as AudioManager;
                if (manager == null)
                {
                    Debug.LogError("[AudioManager::Singleton] There should be one active gameobject attached UndoManager in scene");
                }
            }
            return manager;
        }
    }

    private void Awake()
    {
        BackGoundAudio = GetComponent<AudioSource>();
        BackGoundAudio.Stop();
    }

    public void PlayGamePlayBackGroundMusic()
    {
        BackGoundAudio.clip = GamePlay;
        BackGoundAudio.Play();
    }

    public void PlayWaitUserTrigger()
    {
        BackGoundAudio.clip = WaitPlayer;
        BackGoundAudio.Play();
    }

    public void PlayGameOverBackGroundMusic()
    {
        BackGoundAudio.clip = GameOver;
        BackGoundAudio.Play();
    }

    public void SmoothTurnOffBackGMusic(float duration)
    {
        if (VolumeControlIEnumerator == null)
        {
            VolumeControlIEnumerator = SmoothTurnOffBackGMusicCoroutine(duration);
            StartCoroutine(VolumeControlIEnumerator);
        }
    }

    private IEnumerator SmoothTurnOffBackGMusicCoroutine(float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            BackGoundAudio.volume = Mathf.Lerp(1, 0, timer / duration);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();
        BackGoundAudio.Stop();
        BackGoundAudio.volume = 1;
        VolumeControlIEnumerator = null;
    }
}
