using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    public class ParameterManager : MonoBehaviour
    {
        // Public Class Data.
        // ---------------------------------------------------------------
        [Header("Spray Angles")]
        [Range(1.0f, 60.0f)] public float defaultSprayAngle = 20.0f;
        [Range(1.0f, 15.0f)] public float minSprayAngle = 10.0f;
        [Range(30.0f, 60.0f)] public float maxSprayAngle = 60.0f;

        [Header("Spray Distance")]
        [Range(0.5f, 10.0f)] public float defaultMaxSprayDist = 1.5f;
        [Range(0.5f, 3.0f)] public float minSprayDistance = 0.5f;
        [Range(1.0f, 10.0f)] public float maxSprayDistance = 5.0f;

        [Header("Mapping Curves for Trigger")]
        public AnimationCurve triggerPressMappingCurve = null;
        public AnimationCurve sprayDistMappingCurve = null;
        public AnimationCurve sprayAlphaMappingCurve = null;
        public AnimationCurve sprayAngleHalfRatioCurve = null;
        [Range(0.001f, 0.5f)] public float minSprayAngleRatio = 0.005f;

        // Private Class Data.
        // ---------------------------------------------------------------
        private static ParameterManager parameterManager = null;
        private float currentSprayAngle;
        private float currentMaxSprayDist;
        [SerializeField]private Color currentPaintColor;
        private float currentPaintAlpha;
        private Texture currentBrushMask = null;
        private DateTime gameTime;

        // C# Properties for Public Access.
        // ---------------------------------------------------------------
        public static ParameterManager instance
        {
            get
            {
                if (parameterManager == null)
                {
                    parameterManager = FindObjectOfType(typeof(ParameterManager)) as ParameterManager;
                    if (parameterManager == null)
                    {
                        Debug.LogError("[ParameterManager::Singleton] There should be one active gameobject attached ParameterManager in scene");
                    }
                }
                return parameterManager;
            }
        }

        public float CurrentSprayAngle
        {
            get { return currentSprayAngle; }
            private set { currentSprayAngle = value; }
        }

        public float CurrentMaxSprayDist
        {
            get { return currentMaxSprayDist; }
            private set { currentMaxSprayDist = value; }
        }

        public Color CurrentPaintColor
        {
            get { return currentPaintColor; }
            private set { currentPaintColor = value; }
        }

        public float CurrentPaintAlpha
        {
            get { return currentPaintAlpha; }
            private set { currentPaintAlpha = value; }
        }

        public Texture CurrentBrushMask
        {
            get { return currentBrushMask; }
            set { currentBrushMask = value; }
        }

        public DateTime GameTime
        {
            get { return gameTime; }
            set { gameTime = value; }
        }

        // Unity Fixed Methods.
        // ---------------------------------------------------------------
        void Awake()
        {
            currentSprayAngle = defaultSprayAngle;
            currentMaxSprayDist = defaultMaxSprayDist;
            currentPaintColor = Color.white;
            currentPaintAlpha = 1.0f;

            gameTime = System.DateTime.Now;
        }

        void Start()
        {
            EventManager.StartListeningUpdateActiveColorEvent(OnUpdateActiveColor);
            EventManager.StartListeningUpdatePaintAlphaEvent(OnUpdatePaintAlpha);
            EventManager.StartListeningUpdateSprayAngleEvent(OnUpdateSprayAngle);
            EventManager.StartListeningUpdateMaxSprayDistEvent(OnUpdateMaxSprayDist);
        }

        void Update()
        {
            gameTime = gameTime.AddSeconds(Time.deltaTime);
        }

        void OnDestroy()
        {
            EventManager.StopListeningUpdateActiveColorEvent(OnUpdateActiveColor);
            EventManager.StopListeningUpdatePaintAlphaEvent(OnUpdatePaintAlpha);
            EventManager.StopListeningUpdateSprayAngleEvent(OnUpdateSprayAngle);
            EventManager.StopListeningUpdateMaxSprayDistEvent(OnUpdateMaxSprayDist);
        }

        // Private Class Methods.
        // ---------------------------------------------------------------
        private void OnUpdateActiveColor(Color newColor)
        {
            currentPaintColor = newColor;
        }

        private void OnUpdatePaintAlpha(float newAlpha)
        {
            currentPaintAlpha = newAlpha;
        }

        private void OnUpdateSprayAngle(float newAngle)
        {
            currentSprayAngle = newAngle;
        }

        private void OnUpdateMaxSprayDist(float newMaxDist)
        {
            currentMaxSprayDist = newMaxDist;
        }
    }
}