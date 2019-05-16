//====================== Copyright 2016-2017, HTC.Corporation. All rights reserved. ======================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    [AddComponentMenu("HTC/Audio/3DSP_AudioSource")]

    public class Vive3DSPAudioSource : MonoBehaviour
    {
        public AudioSource audioSource
        {
            get { return source; }
            set { source = value; }
        }
        private AudioSource source;

        public Vector3 Position
        {
            get
            {
                return pos;
            }
            set
            {
                if (pos != value)
                {
                    pos = value;
                }
            }
        }
        private Vector3 pos = Vector3.zero;

        public bool distanceModeSwitch
        {
            get
            {
                return sourceDistanceModeSwitch;
            }
            set
            {
                sourceDistanceModeSwitch = value;
            }
        }
        [SerializeField]
        private bool sourceDistanceModeSwitch = false;

        public float Gain
        {
            set { gain = value; }
            get { return gain; }
        }
        [SerializeField]
        private float gain = 0.0f;

        public Vive3DSPAudio.Ambisonic3dDistanceMode DistanceMode
        {
            set { distanceMode = value; }
            get { return distanceMode; }
        }
        [SerializeField]
        private Vive3DSPAudio.Ambisonic3dDistanceMode distanceMode = Vive3DSPAudio.Ambisonic3dDistanceMode.RealWorld;

        public float MinimumDecayVolumeDb
        {
            set { minimumDecayVolumeDb = value; }
            get { return minimumDecayVolumeDb; }
        }
        [SerializeField]
        private float minimumDecayVolumeDb = -96.0f;

        public float Drc
        {
            set {
                if (value > 0.5)
                    drc = true;
                else
                    drc = false;
            }
            get {
                if (drc)
                    return 1.0f;
                else
                    return 0.0f;
            }
        }
        [SerializeField]
        private bool drc = true;

        public float Spatializer3d
        {
            set
            {
                if (value > 0.5)
                    spatializer_3d = true;
                else
                    spatializer_3d = false;
            }
            get
            {
                if (spatializer_3d)
                    return 1.0f;
                else
                    return 0.0f;
            }
        }
        [SerializeField]
        private bool spatializer_3d = true;

        public float Reverb
        {
            set
            {
                if (value > 0.5)
                    reverb = true;
                else
                    reverb = false;
            }
            get
            {
                if (reverb)
                    return 1.0f;
                else
                    return 0.0f;
            }
        }
        [SerializeField]
        private bool reverb = true;

        public float Occlusion
        {
            set
            {
                if (value > 0.5)
                    occlusion = true;
                else
                    occlusion = false;
            }
            get
            {
                if (occlusion)
                    return 1.0f;
                else
                    return 0.0f;
            }
        }
        [SerializeField]
        private bool occlusion = true;

        public bool isPlaying
        {
            get
            {
                if (audioSource != null)
                {
                    return audioSource.isPlaying;
                }
                return false;
            }
        }

        public bool isVirtual
        {
            get {
                if (audioSource != null)
                {
                    return audioSource.isVirtual;
                }
                return true;
            }
        }

        public HashSet<Vive3DSPAudioRoom> RoomList
        {
            get { return roomList; }
        }
        private HashSet<Vive3DSPAudioRoom> roomList = new HashSet<Vive3DSPAudioRoom>();
        
        // Native audio spatializer effect data.
        public enum EffectData
        {
            Gain = 0,
            DistanceMode,
            EnableDRC,
            EnableSpatialAudioEffect,
            EnableRoomEffect,
            EnableOcclusionEffect,
            CurrentRoom,
            MonoCoverOcclusion,
            MonoCoverRatio,
            StereoCoverOcclusion,
            StereoCoverRatioL,
            StereoCoverRatioR
        }

        void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
            if ((audioSource.spatialize == false) && (audioSource.spatialBlend == 0.0f))
            {
                audioSource.spatialBlend = 1.0f;
            }
        }

        void OnEnable()
        {
            if (audioSource == null) return;
            
            audioSource.enabled = true;
            InitSource();
            
            if (audioSource.playOnAwake && !isPlaying)
            {
                Play();
            }

            Update();
        }

        void OnDisable()
        {
            if (isPlaying)
            {
                audioSource.Stop();
            }
            Vive3DSPAudio.DestroySource(this);
            roomList.Clear();
        }

        void Start()
        {
            if (audioSource.playOnAwake && !isPlaying)
            {
                Play();
            }
            UpdateTransform();
        }

        void Update()
        {
            UpdateTransform();
            if (!isPlaying)
            {
                audioSource.Pause();
            }

            if (distanceModeSwitch)
            {
                audioSource.rolloffMode = AudioRolloffMode.Custom;
                audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff,
                                               AnimationCurve.Linear(audioSource.minDistance, 1.0f,
                                                                     audioSource.maxDistance, 1.0f));
            }
            else
            {
                distanceMode = Vive3DSPAudio.Ambisonic3dDistanceMode.Constant;
            }

            // Update effect data
            if (audioSource.spatialize)
            {
                audioSource.SetSpatializerFloat((int)EffectData.Gain, gain);
                audioSource.SetSpatializerFloat((int)EffectData.DistanceMode, (float)distanceMode);
                audioSource.SetSpatializerFloat((int)EffectData.EnableDRC, Drc);
                audioSource.SetSpatializerFloat((int)EffectData.EnableSpatialAudioEffect, Spatializer3d);
                audioSource.SetSpatializerFloat((int)EffectData.EnableRoomEffect, Reverb);
                audioSource.SetSpatializerFloat((int)EffectData.EnableOcclusionEffect, Occlusion);
            }
        }
        
        void OnDestroy()
        {
            Vive3DSPAudio.DestroySource(this);
        }

        public void Play()
        {
            if (audioSource != null)
            {
                audioSource.Play();
                InitSource();
            }
            else
            {
                Debug.LogWarning("Audio source not initialized. Audio playback not supported " +
                                  "until after Awake() and OnEnable(). Try calling from Start() instead.");
            }
        }

        public void Stop()
        {
            if (audioSource != null && isPlaying)
            {
                audioSource.Stop();
            }
        }

        private bool InitSource()
        {
            if (audioSource != null)
            {
                audioSource.spatialize = true;
                Vive3DSPAudio.CreateSource(this);
                Vive3DSPAudio.ResetSource(this);
            }
            return true;
        }
        
        private void UpdateTransform()
        {
            Position = transform.position;
        }
    }
}
