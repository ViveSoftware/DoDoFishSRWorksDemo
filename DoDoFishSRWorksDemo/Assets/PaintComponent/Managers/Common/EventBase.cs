using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    public delegate void Callback();

    public delegate void Callback<T>(T arg1);

    public delegate void Callback<T, U>(T arg1, U arg2);

    public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);


    public class EventBase : MonoBehaviour
    {
        // Private Class Data.
        // ---------------------------------------------------------------
        private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

        // Public Class Methods.
        // ---------------------------------------------------------------
        public static void CleanUpAllEvents()
        {
            eventTable.Clear();
        }

        public static void PrintAllEvents()
        {
            Debug.Log("[EventManager::PaintAllEvents] Event List: ");
            foreach(KeyValuePair<string, Delegate> entry in eventTable)
            {
                Debug.Log("\t\t" + entry.Key + "\t" + entry.Value);
            }
            Debug.Log("[EventManager::PaintAllEvents] Finish\n");
        }

        public static void AddListener(string eventName, Callback handler)
        {
            bool isValid = TryToAddListener(eventName, handler);
            if (isValid)
            {
                eventTable[eventName] = (Callback)eventTable[eventName] + handler;
            }
        }

        public static void AddListener<T>(string eventName, Callback<T> handler)
        {
            bool isValid = TryToAddListener(eventName, handler);
            if (isValid)
            {
                eventTable[eventName] = (Callback<T>)eventTable[eventName] + handler;
            }
        }

        public static void AddListener<T, U>(string eventName, Callback<T, U> handler)
        {
            bool isValid = TryToAddListener(eventName, handler);
            if (isValid)
            {
                eventTable[eventName] = (Callback<T, U>)eventTable[eventName] + handler;
            }
        }

        public static void AddListener<T, U, V>(string eventName, Callback<T, U, V> handler)
        {
            bool isValid = TryToAddListener(eventName, handler);
            if (isValid)
            {
                eventTable[eventName] = (Callback<T, U, V>)eventTable[eventName] + handler;
            }
        }

        public static void RemoveListener(string eventName, Callback handler)
        {
            bool isValid = TryToRemoveListener(eventName, handler);
            if (isValid)
            {
                eventTable[eventName] = (Callback)eventTable[eventName] - handler;
            }
            OnListenerRemoved(eventName);
        }

        public static void RemoveListener<T>(string eventName, Callback<T> handler)
        {
            bool isValid = TryToRemoveListener(eventName, handler);
            if (isValid)
            {
                eventTable[eventName] = (Callback<T>)eventTable[eventName] - handler;
            }
            OnListenerRemoved(eventName);
        }

        public static void RemoveListener<T, U>(string eventName, Callback<T, U> handler)
        {
            bool isValid = TryToRemoveListener(eventName, handler);
            if (isValid)
            {
                eventTable[eventName] = (Callback<T, U>)eventTable[eventName] - handler;
            }
            OnListenerRemoved(eventName);
        }

        public static void RemoveListener<T, U, V>(string eventName, Callback<T, U, V> handler)
        {
            bool isValid = TryToRemoveListener(eventName, handler);
            if (isValid)
            {
                eventTable[eventName] = (Callback<T, U, V>)eventTable[eventName] - handler;
            }
            OnListenerRemoved(eventName);
        }

        public static void TriggerEvent(string eventName)
        {
            Delegate listener;
            if (eventTable.TryGetValue(eventName, out listener))
            {
                Callback callback = listener as Callback;
                if (callback != null)
                {
                    callback();
                }
                else
                {
                    Debug.LogError("[EventManager::TriggerEvent] Cannot find callback for event: " + eventName);
                }
            }
        }

        public static void TriggerEvent<T>(string eventName, T arg1)
        {
            Delegate listener;
            if (eventTable.TryGetValue(eventName, out listener))
            {
                Callback<T> callback = listener as Callback<T>;
                if (callback != null)
                {
                    callback(arg1);
                }
                else
                {
                    Debug.LogError("[EventManager::TriggerEvent] Cannot find callback for event: " + eventName);
                }
            }
        }

        public static void TriggerEvent<T, U>(string eventName, T arg1, U arg2)
        {
            Delegate listener;
            if (eventTable.TryGetValue(eventName, out listener))
            {
                Callback<T, U> callback = listener as Callback<T, U>;
                if (callback != null)
                {
                    callback(arg1, arg2);
                }
                else
                {
                    Debug.LogError("[EventManager::TriggerEvent] Cannot find callback for event: " + eventName);
                }
            }
        }

        public static void TriggerEvent<T, U, V>(string eventName, T arg1, U arg2, V arg3)
        {
            Delegate listener;
            if (eventTable.TryGetValue(eventName, out listener))
            {
                Callback<T, U, V> callback = listener as Callback<T, U, V>;
                if (callback != null)
                {
                    callback(arg1, arg2, arg3);
                }
                else
                {
                    Debug.LogError("[EventManager::TriggerEvent] Cannot find callback for event: " + eventName);
                }
            }
        }

        // Private Class Methods.
        // ---------------------------------------------------------------
        private static bool TryToAddListener(string eventName, Delegate listenerAdded)
        {
            if (!eventTable.ContainsKey(eventName))
            {
                eventTable.Add(eventName, null);
            }

            Delegate listener = eventTable[eventName];
            if (listener != null && listener.GetType() != listenerAdded.GetType())
            {
                Debug.LogError("[EventManager::TryToAddListener] Add listener with inconsistent listener for event: " + eventName);
                return false;
            }

            return true;
        }

        private static bool TryToRemoveListener(string eventName, Delegate listenerRemoved)
        {
            if (eventTable.ContainsKey(eventName))
            {
                Delegate listener = eventTable[eventName];
                if (listener == null)
                {
                    Debug.LogError("[EventManager::TryToRemoveListener] Remove null listener: " + eventName);
                    return false;
                }
                else if (listener.GetType() != listenerRemoved.GetType())
                {
                    Debug.LogError("[EventManager::TryToRemoveListener] Remove listener with inconsistent listener type");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("[EventManager::TryToRemoveListener] Remove listener with unregister event: " + eventName);
                return false;
            }

            return true;
        }

        private static void OnListenerRemoved(string eventName)
        {
            if (eventTable.ContainsKey(eventName))
            {
                if (eventTable[eventName] == null)
                {
                    eventTable.Remove(eventName);
                }
            }
        }
    }
}