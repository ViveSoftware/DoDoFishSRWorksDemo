using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventAudio : MonoBehaviour
{
    public bool playAudio = true;

    public void playaudio(string audioname)
    {
        if (!playAudio)
            return;      
        AudioManagerBase audioManager = GetComponent<AudioManagerBase>();
        audioManager.PlaySound(audioname);
    }

    public void playaudioNode(string nodeName_audioname)
    {
        if (!playAudio)
            return;
        string[] names = nodeName_audioname.Split('_');
        Transform node;
        MyHelpNode.FindTransform(transform, names[0], out node);
        AudioManagerBase audioManager = node.GetComponent<AudioManagerBase>();
        audioManager.PlaySound(names[1]);
    }
}
