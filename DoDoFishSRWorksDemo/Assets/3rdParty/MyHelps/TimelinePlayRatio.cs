using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelinePlayRatio
{
    PlayableDirector PlayTimeline;

    float _destRatioRec;
    Tweener _setPlayInTweener;
    float _playRatio;
    bool isInit;
    public void Init(PlayableDirector pd)
    {
        isInit = true;
        _playRatio = 0;
        PlayTimeline = pd;
        PlayTimeline.time = 0;
        PlayTimeline.Evaluate();
        if (_setPlayInTweener != null)
            _setPlayInTweener.Kill();
    }

    public void SetPlayRatio(float destRatio, float sec = 0.5f)
    {
        if (!isInit)
            Debug.LogError("[TimelinePlayRatio][SetPlayRatio] isInit == false");
        if (_destRatioRec == destRatio)
            return;
        _destRatioRec = destRatio;

        if (_setPlayInTweener != null)
            _setPlayInTweener.Kill();

        _setPlayInTweener = DOTween.To
            (
            () => { return _playRatio; },
            _setRatio,
            destRatio, sec
            ).
            SetEase(Ease.Linear);
    }

    void _setRatio(float ratio)
    {
        _playRatio = ratio;
        float time = (float)PlayTimeline.duration * ratio;
        PlayTimeline.time = time;
        PlayTimeline.Evaluate();
    }

    public float GetRatio()
    {
        return _playRatio;
    }
}
