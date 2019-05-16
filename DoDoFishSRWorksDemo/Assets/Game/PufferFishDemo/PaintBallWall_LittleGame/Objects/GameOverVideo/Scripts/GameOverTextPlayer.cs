using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameOverTextPlayer : MonoBehaviour
{
    public GameOverTextUnit[] units;
    //public AudioClip[] voiceClip;
    //public AudioSource audioSource;
    private bool isPlayResult = false;

    public void Start()
    {
        //audioSource = GetComponent<AudioSource>();
    }

    public void PlayResultAnim(TweenCallback callback)
    {
        if (isPlayResult) return;

        float delayTime = 0.5f;
        isPlayResult = true;
        
        units[1].scores = ScoreManager.instance.curScore;
        units[3].scores = MRWallManager.instance.heartController.CurrentAliveHeart;
        units[5].floating = ScoreManager.instance.curHitRate;

        int totalScore = (int)(ScoreManager.instance.curScore * (1 + ScoreManager.instance.curHitRate / 100)) + MRWallManager.instance.heartController.CurrentAliveHeart * 1000;
        units[7].scores = totalScore;

        string evaluate = "D-";
        //audioSource.clip = voiceClip[3];
        if (totalScore > 5000)
        {
            evaluate = "D";
            //audioSource.clip = voiceClip[3];
        }
        if (totalScore > 10000)
        {
            evaluate = "C";
            //audioSource.clip = voiceClip[3];
        }
        if (totalScore > 20000)
        {
            evaluate = "B";
            //audioSource.clip = voiceClip[2];
        }
        if (totalScore > 40000)
        {
            evaluate = "A";
            //audioSource.clip = voiceClip[1];
        }
        if (totalScore > 60000)
        {
            evaluate = "S";
            //audioSource.clip = voiceClip[0];
        }
        units[8].characters = evaluate;

        for (int i = 0; i < units.Length; i++)
        {
            DOVirtual.DelayedCall(delayTime, units[i].PlayTextAnim);
            delayTime += 1.2f;
        }

        DOVirtual.DelayedCall(units.Length * 1.2f, callback);
        DOVirtual.DelayedCall(units.Length * 1.2f, PlayVoice);
    }

    public void PlayVoice()
    {
        //audioSource.Play();
    }

    public void OnResetEvent()
    {
        foreach (var unit in units)
        {
            unit.OnResetEvent();
        }
        isPlayResult = false;
    }
}
