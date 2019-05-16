#define USE_3DSP
using UnityEngine;
using DG.Tweening;
using System.Collections;
#if USE_3DSP
using HTC.UnityPlugin.Vive3DSoundPerception;
#endif

public abstract class AudioManagerBase : MonoBehaviour
{
    AudioSource audioSource, fadeInAudioSource, oneShotAudioSource;

#if USE_3DSP
    //Vive3DSPAudioSource htcAudioSource;
#endif
    //public AudioClip GarageUpStart;
    //public AudioClip GarageUpping;
    //public AudioClip GarageUpEnd;
    //public AudioClip EngineStart;

    string[] AudioNames;

    void Awake()
    {
        MyReflection.GetMemberVariable<AudioClip>(this, out AudioNames);
        audioSource = gameObject.AddComponent<AudioSource>();
        fadeInAudioSource = gameObject.AddComponent<AudioSource>();
        oneShotAudioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = fadeInAudioSource.playOnAwake = oneShotAudioSource.playOnAwake = false;

#if USE_3DSP
        //htcAudioSource = gameObject.AddComponent<Vive3DSPAudioSource>();
        oneShotAudioSource.spatialize = fadeInAudioSource.spatialize = audioSource.spatialize = true;
        oneShotAudioSource.spatialBlend = fadeInAudioSource.spatialBlend = audioSource.spatialBlend = 1;
#endif
    }

    public void PlayOneShot(string audioclipname, float SoundVolume = 1)
    {
        AudioClip ac = TryGetAudioClip(audioclipname);
        oneShotAudioSource.volume = SoundVolume;
        oneShotAudioSource.PlayOneShot(ac);

#if USE_3DSP

#endif
    }

    private void OnDisable()
    {
        StopSound();
    }

    public void StopSound()
    {
        StopAllCoroutines();
        _isPlaying = false;
        audioSource.Stop();
        audioSource.clip = null;
        fadeInAudioSource.Stop();
        fadeInAudioSource.clip = null;
        DOTween.Kill(audioSource);
        DOTween.Kill(fadeInAudioSource);
    }

    bool _isPlaying;
    public bool IsSoundPlaying()
    {
        return _isPlaying && audioSource.isPlaying;
    }

    public void PlaySoundLoop(string audioclipname, float SoundVolume = 1, float fadeInSec = 0, float delaySec = 0)
    {
        _isPlaying = true;
        if (delaySec > 0)
            StartCoroutine(_waitPlaySound(audioclipname, true, SoundVolume, fadeInSec, delaySec));
        else
            _playSound(audioclipname, true, SoundVolume, fadeInSec, 1);
    }

    public void PlaySound(string audioclipname, float SoundVolume = 1, float fadeInSec = 0, float delaySec = 0, float pitch = 1)
    {
        _isPlaying = true;
        if (delaySec > 0)
            StartCoroutine(_waitPlaySound(audioclipname, false, SoundVolume, fadeInSec, delaySec, pitch));
        else
            _playSound(audioclipname, false, SoundVolume, fadeInSec, 1);
    }

    IEnumerator _waitPlaySound(string audioclipname, bool isLoop, float SoundVolume = 1, float fadeInSec = 1, float delaySec = 0, float pitch = 1)
    {
        yield return new WaitForSeconds(delaySec);
        _playSound(audioclipname, isLoop, SoundVolume, fadeInSec, pitch);
    }

    void _playSound(string audioclipname, bool isLoop, float SoundVolume = 1, float fadeInSec = 1, float pitch = 1)
    {
        AudioClip ac = TryGetAudioClip(audioclipname);

        if (audioSource.isPlaying)
        {
            Debug.Assert(fadeInAudioSource != null);
            fadeInAudioSource.clip = ac;
            fadeInAudioSource.loop = isLoop;
            fadeInAudioSource.pitch = pitch;
            fadeInAudioSource.Play();
            fadeInAudioSource.DOFade(SoundVolume, fadeInSec).OnComplete(OnFadeInDone);
            audioSource.DOFade(0, fadeInSec);
            // Debug.Log("[AudioManagerBase][_playSound] : fadeIn : " + fadeInAudioSource.clip.name + " , fadeOut : " + audioSource.clip.name);
        }
        else
        {
            audioSource.clip = ac;
            audioSource.loop = isLoop;
            audioSource.pitch = pitch;
            audioSource.Play();
            audioSource.volume = 0;
            audioSource.DOFade(SoundVolume, fadeInSec);
            // Debug.Log("[AudioManagerBase][_playSound] : fadeIn : " + audioSource.clip.name);
        }
    }

    void OnFadeInDone()
    {
        Debug.Log("[AudioManagerBase][OnFadeInDone] : fadeIn : " + fadeInAudioSource.clip.name + " , fadeOut stop : " + audioSource.clip.name);

        audioSource.Stop();
        audioSource.clip = null;
        AudioSource tmp = audioSource;

        audioSource = fadeInAudioSource;
        fadeInAudioSource = tmp;
    }

    void StopSound(float fadeOutSec = 1)
    {
        if (fadeInAudioSource.isPlaying)
            fadeInAudioSource.Stop();
        if (audioSource.isPlaying)
            audioSource.DOFade(0, fadeOutSec).OnComplete(OnFadeOutDone);
    }

    void OnFadeOutDone()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }

    public void WaitPlayOneShot(string audioclipname, float SoundVolume = 1, float delaySec = 0)
    {
        StartCoroutine(_waitPlayOneShot(audioclipname, SoundVolume, delaySec));
    }

    IEnumerator _waitPlayOneShot(string audioclipname, float SoundVolume, float delaySec = 0)
    {
        yield return new WaitForSeconds(delaySec);
        audioSource.PlayOneShot(TryGetAudioClip(audioclipname), SoundVolume);
    }

    public AudioClip TryGetAudioClip(string name)
    {
        foreach (string sourcename in AudioNames)
            if (sourcename == name)
                return MyReflection.GetMemberVariable(this, name) as AudioClip;

        Debug.LogError("Can't found AudioClipname : " + name);
        return null;
    }

}
