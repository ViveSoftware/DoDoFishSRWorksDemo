using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    public class EventManager : EventBase
    {
        // Private Class Data.
        // ---------------------------------------------------------------
        private static string changeRefImgEventName = "Change Ref Img Event";
        private static string pauseGameEventName = "Pause Game Event";
        private static string resumeGameEventName = "Resume Game Event";

        private static string updateActiveColorEventName = "Update Active Color Event";
        private static string updatePaintAlphaEventName = "Update Paint Alpha Event";
        private static string updateSprayAngleEventName = "Update Spray Angle Event";
        private static string updateMaxSprayDistEventName = "Update Max Spray Dist Event";
        private static string updateColorHistoryEventName = "Update Color History Event";

        private static string miniModelHoverEventName = "Mini-Model Hover Event";
        private static string miniModelUnHoverEventName = "Mini-Model UnHover Event";

        // Public Static Class Methods.
        // ---------------------------------------------------------------
        //Change ref image
        public static void StartListeninChangeRefImgEvent(Callback function)
        {
            AddListener(changeRefImgEventName, function);
        }

        public static void StopListeningChangeRefImgEvent(Callback function)
        {
            RemoveListener(changeRefImgEventName, function);
        }

        public static void TriggerChangeRefImgEvent()
        {
            TriggerEvent(changeRefImgEventName);
        }

        // Pause game.
        public static void StartListeningPauseGameEvent(Callback function)
        {
            AddListener(pauseGameEventName, function);
        }

        public static void StopListeningPauseGameEvent(Callback function)
        {
            RemoveListener(pauseGameEventName, function);
        }

        public static void TriggerPauseGameEvent()
        {
            TriggerEvent(pauseGameEventName);
        }

        // Resume game.
        public static void StartListeningResumeGameEvent(Callback function)
        {
            AddListener(resumeGameEventName, function);
        }

        public static void StopListeningResumeGameEvent(Callback function)
        {
            RemoveListener(resumeGameEventName, function);
        }

        public static void TriggerResumeGameEvent()
        {
            TriggerEvent(resumeGameEventName);
        }

        // Update spray color.
        public static void StartListeningUpdateActiveColorEvent(Callback<Color> function)
        {
            AddListener<Color>(updateActiveColorEventName, function);
        }

        public static void StopListeningUpdateActiveColorEvent(Callback<Color> function)
        {
            RemoveListener<Color>(updateActiveColorEventName, function);
        }

        public static void TriggerUpdateActiveColorEvent(Color newColor)
        {
            TriggerEvent<Color>(updateActiveColorEventName, newColor);
        }

        // Update paint alpha.
        public static void StartListeningUpdatePaintAlphaEvent(Callback<float> function)
        {
            AddListener<float>(updatePaintAlphaEventName, function);
        }

        public static void StopListeningUpdatePaintAlphaEvent(Callback<float> function)
        {
            RemoveListener<float>(updatePaintAlphaEventName, function);
        }

        public static void TriggerUpdatePaintAlphaEvent(float newAlpha)
        {
            TriggerEvent<float>(updatePaintAlphaEventName, newAlpha);
        }

        // Update spray angle.
        public static void StartListeningUpdateSprayAngleEvent(Callback<float> function)
        {
            AddListener<float>(updateSprayAngleEventName, function);
        }

        public static void StopListeningUpdateSprayAngleEvent(Callback<float> function)
        {
            RemoveListener<float>(updateSprayAngleEventName, function);
        }

        public static void TriggerUpdateSprayAngleEvent(float newAngle)
        {
            TriggerEvent(updateSprayAngleEventName, newAngle);
        }

        // Update max spray dist.
        public static void StartListeningUpdateMaxSprayDistEvent(Callback<float> function)
        {
            AddListener<float>(updateMaxSprayDistEventName, function);
        }

        public static void StopListeningUpdateMaxSprayDistEvent(Callback<float> function)
        {
            RemoveListener<float>(updateMaxSprayDistEventName, function);
        }

        public static void TriggerUpdateMaxSprayDistEvent(float newAngle)
        {
            TriggerEvent(updateMaxSprayDistEventName, newAngle);
        }

        // Insert a color into color history.
        public static void StartListeningUpdateColorHistoryEvent(Callback<Color> function)
        {
            AddListener<Color>(updateColorHistoryEventName, function);
        }

        public static void StopListeningUpdateColorHistoryEvent(Callback<Color> function)
        {
            RemoveListener<Color>(updateColorHistoryEventName, function);
        }

        public static void TriggerUpdateColorHistoryEvent(Color newColor)
        {
            TriggerEvent<Color>(updateColorHistoryEventName, newColor);
        }

        // Mini-model hover and unhover events.
        public static void StartListeningMiniModelHoverEvent(Callback function)
        {
            AddListener(miniModelHoverEventName, function);
        }

        public static void StopListeningMiniModelHoverEvent(Callback function)
        {
            RemoveListener(miniModelHoverEventName, function);
        }

        public static void TriggerMiniModelHoverEvent()
        {
            TriggerEvent(miniModelHoverEventName);
        }

        public static void StartListeningMiniModelUnHoverEvent(Callback function)
        {
            AddListener(miniModelUnHoverEventName, function);
        }

        public static void StopListeningMiniModelUnHoverEvent(Callback function)
        {
            RemoveListener(miniModelUnHoverEventName, function);
        }

        public static void TriggerMiniModelUnHoverEvent()
        {
            TriggerEvent(miniModelUnHoverEventName);
        }
    }
}