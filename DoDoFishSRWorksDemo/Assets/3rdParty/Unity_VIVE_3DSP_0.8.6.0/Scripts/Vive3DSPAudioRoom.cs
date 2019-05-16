//====================== Copyright 2016-2017, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using System;
using System.IO;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    [AddComponentMenu("HTC/Audio/3DSP_AudioRoom")]

    public class Vive3DSPAudioRoom : MonoBehaviour
    {
        public bool RoomEffect
        {
            set { roomEffect = value; OnValidate(); }
            get { return roomEffect; }
        }
        [SerializeField]
        private bool roomEffect = true;

        public Vive3DSPAudio.RoomReverbPreset ReverbPreset
        {
            set { reverbPreset = value; OnValidate(); }
            get { return reverbPreset; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomReverbPreset reverbPreset = Vive3DSPAudio.RoomReverbPreset.Generic;

        public Vive3DSPAudio.RoomPlateMaterial Ceiling
        {
            set { ceiling = value; OnValidate(); }
            get { return ceiling; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial ceiling = Vive3DSPAudio.RoomPlateMaterial.Concrete;

        public Vive3DSPAudio.RoomPlateMaterial FrontWall
        {
            set { frontWall = value; OnValidate(); }
            get { return frontWall; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial frontWall = Vive3DSPAudio.RoomPlateMaterial.Wood;

        public Vive3DSPAudio.RoomPlateMaterial BackWall
        {
            set { backWall = value; OnValidate(); }
            get { return backWall; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial backWall = Vive3DSPAudio.RoomPlateMaterial.Wood;

        public Vive3DSPAudio.RoomPlateMaterial RightWall
        {
            set { rightWall = value; OnValidate(); }
            get { return rightWall; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial rightWall = Vive3DSPAudio.RoomPlateMaterial.Carpet;

        public Vive3DSPAudio.RoomPlateMaterial LeftWall
        {
            set { leftWall = value; OnValidate(); }
            get { return leftWall; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial leftWall = Vive3DSPAudio.RoomPlateMaterial.Wood;

        public Vive3DSPAudio.RoomPlateMaterial Floor
        {
            set { floor = value; OnValidate(); }
            get { return floor; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomPlateMaterial floor = Vive3DSPAudio.RoomPlateMaterial.Concrete;

        public Vive3DSPAudio.RoomBackgroundAudioType BackgroundType
        {
            set
            {
                backgroundType = value;
                backgroundClip = GetBackgroundAudioClip();
                OnValidate();
            }
            get { return backgroundType; }
        }
        [SerializeField]
        private Vive3DSPAudio.RoomBackgroundAudioType backgroundType = Vive3DSPAudio.RoomBackgroundAudioType.None;

        public float CeilingReflectionRate
        {
            set { ceilingReflectionRate = value; OnValidate(); }
            get { return ceilingReflectionRate; }
        }
        [SerializeField]
        private float ceilingReflectionRate = 1.0f;

        public float FrontWallReflectionRate
        {
            set { frontWallReflectionRate = value; OnValidate(); }
            get { return frontWallReflectionRate; }
        }
        [SerializeField]
        private float frontWallReflectionRate = 1.0f;

        public float BackWallReflectionRate
        {
            set { backWallReflectionRate = value; OnValidate(); }
            get { return backWallReflectionRate; }
        }
        [SerializeField]
        private float backWallReflectionRate = 1.0f;

        public float RightWallReflectionRate
        {
            set { rightWallReflectionRate = value; OnValidate(); }
            get { return rightWallReflectionRate; }
        }
        [SerializeField]
        private float rightWallReflectionRate = 1.0f;

        public float LeftWallReflectionRate
        {
            set { leftWallReflectionRate = value; OnValidate(); }
            get { return leftWallReflectionRate; }
        }
        [SerializeField]
        private float leftWallReflectionRate = 1.0f;

        public float FloorReflectionRate
        {
            set { floorReflectionRate = value; OnValidate(); }
            get { return floorReflectionRate; }
        }
        [SerializeField]
        private float floorReflectionRate = 1.0f;

        public Vector3 Size
        {
            set { size = value; OnValidate(); }
            get { return size; }
        }
        /// Size of the room (normalized with respect to scale of the game object).
        [SerializeField]
        private Vector3 size = Vector3.one;

        public float ReflectionLevel
        {
            set { reflectionLevel = value; OnValidate(); }
            get { return reflectionLevel; }
        }
        [SerializeField]
        public float reflectionLevel = 0.0f;

        public float ReverbLevel
        {
            set { reverbLevel = value; OnValidate(); }
            get { return reverbLevel; }
        }
        [SerializeField]
        public float reverbLevel = 0.0f;
        
        public IntPtr RoomObject
        {
            get { return roomObject; }
            set { roomObject = value; }
        }
        private IntPtr roomObject = IntPtr.Zero;

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
                    Vive3DSPAudio.UpdateRoomPositionCallback(this);
                }
            }
        }
        private Vector3 pos = Vector3.zero;

        public Quaternion Rotation
        {
            get
            {
                return quat;
            }
            set
            {
                if (quat != value)
                {
                    quat = value;
                    Vive3DSPAudio.UpdateRoomRotationCallback(this);
                }
            }
        }
        private Quaternion quat = Quaternion.identity;

        private AudioSource audioSource = null;
        [SerializeField]
        private AudioClip userDefineClip = null;
        private AudioClip sourceClip = null;

        public bool isCurrentRoom = false;

        public AudioClip backgroundClip
        {
            get { return sourceClip; }
            set {
                if (sourceClip != value)
                {
                    sourceClip = value;
                    if (audioSource != null)
                    {
                        audioSource.clip = sourceClip;
                        PlayBackgroundAudio();
                    }
                }
            }
        }
        public float backgroundVolume {
            get { return sourceVolume; }
            set {
                sourceVolume = value;
                if (audioSource != null) {
                    audioSource.volume = (float)Math.Pow(10.0, sourceVolume * 0.05);
                }
            }
        }
        [SerializeField]
        private float sourceVolume = 0.0f;

        public bool isPlaying {
            get {
                if (audioSource != null) { return audioSource.isPlaying; }
                return false;
            }
        }

        void Awake()
        {
            if (audioSource == null) {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.hideFlags = HideFlags.HideInInspector | HideFlags.HideAndDontSave;
            }
            audioSource.spatialize = false;
            audioSource.playOnAwake = true;
            audioSource.loop = true;
            audioSource.dopplerLevel = 0.0f;
            audioSource.spatialBlend = 0.0f;
        }
        
        void Start()
        {
            InitRoom();
            backgroundClip = GetBackgroundAudioClip();
            backgroundVolume = sourceVolume;
            OnValidate();
        }

        private void OnEnable()
        {
            RoomEffect = true;
            OnValidate();
            Update();
        }

        private void OnDisable()
        {
            RoomEffect = false;
            OnValidate();
        }

        void Update()
        {
            UpdateTransform();
            if (backgroundType == Vive3DSPAudio.RoomBackgroundAudioType.UserDefine)
            {
                backgroundClip = GetBackgroundAudioClip();
            }
            Vive3DSPAudio.UpdateRoomPositionCallback(this);
        }

        void OnDestroy()
        {
            Vive3DSPAudio.DestroyRoom(this);
            roomObject = IntPtr.Zero;
            Destroy(audioSource);
        }

        void OnValidate()
        {
            if (this != null && roomObject != IntPtr.Zero)
                Vive3DSPAudio.UpdateRoom(this);

            if (roomEffect)
                PlayBackgroundAudio();
            else
                StopBackgroundAudio();
        }

        private void InitRoom()
        {
            Vive3DSPAudio.CreateRoom(this);
        }

        private AudioClip GetBackgroundAudioClip()
        {
            AudioClip tempClip;
            switch (backgroundType)
            {
                case Vive3DSPAudio.RoomBackgroundAudioType.UserDefine:
                    tempClip = userDefineClip;
                    break;
                default:
                    float[] data = Vive3DSPAudio.GetBGAudioData((int)backgroundType);
                    if (data == null)
                        tempClip = null;
                    else
                    {
                        tempClip = AudioClip.Create("BG Preset", 48000, 1, 48000, false);
                        tempClip.SetData(data, 0);
                    }
                    
                    break;
            }

            return tempClip;
        }

        public void PlayBackgroundAudio()
        {
            if ((audioSource != null) && (!isPlaying) && (backgroundType != Vive3DSPAudio.RoomBackgroundAudioType.None) && (isCurrentRoom == true) && roomEffect)
            {
                audioSource.Play();
            }
        }

        public void StopBackgroundAudio()
        {
            if ((audioSource != null) && (isPlaying)) {
                audioSource.Stop();
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        private void UpdateTransform()
        {
            Position = transform.position;
            Rotation = transform.rotation;
        }
    }
}
