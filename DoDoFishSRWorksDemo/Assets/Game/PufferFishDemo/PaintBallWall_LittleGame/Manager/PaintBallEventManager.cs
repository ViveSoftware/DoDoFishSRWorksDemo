using PaintVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBallEventManager : EventBase
{
    private static string gameOverEventName = "Game Over Event";

    //Change ref image
    public static void StartListeningGameOverEvent(Callback function)
    {
        AddListener(gameOverEventName, function);
    }

    public static void StopListeningGameOverEvent(Callback function)
    {
        RemoveListener(gameOverEventName, function);
    }

    public static void TriggerGameOverEvent()
    {
        TriggerEvent(gameOverEventName);
    }
}
