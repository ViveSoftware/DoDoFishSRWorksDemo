//====================== Copyright 2016-2017, HTC.Corporation. All rights reserved. ======================
using System.Collections.Generic;
using UnityEngine;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    [AddComponentMenu("HTC/Audio/3DSP_AudioListener")]

    public class Vive3DSPAudioListener : MonoBehaviour
    {
        public float globalGain = 0.0f;
        private Vive3DSPAudioRoom currentRoom;

        public Vive3DSPAudioRoom CurrentRoom
        {
            get { return currentRoom; }
            set {
                if (currentRoom != value)
                {
                    if (currentRoom != null)
                    {
                        currentRoom.StopBackgroundAudio();
                        currentRoom.isCurrentRoom = false;
                    }

                    currentRoom = value;

                    if (currentRoom != null)
                    {
                        currentRoom.isCurrentRoom = true;
                        currentRoom.PlayBackgroundAudio();
                    }
                }
            }
        }

        public Vive3DSPAudio.OccRaycastMode occlusionMode = Vive3DSPAudio.OccRaycastMode.BinauralOcclusion;
        public Vive3DSPAudio.HeadsetType headsetType = Vive3DSPAudio.HeadsetType.Generic;

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
                    Vive3DSPAudio.UpdateListenerTransformCallback();
                }
            }
        }
        private Vector3 pos = Vector3.zero;

        public Quaternion Rotation
        {
            get { return quat; }
            set
            {
                if (quat != value)
                {
                    quat = value;
                    Vive3DSPAudio.UpdateListenerTransformCallback();
                }
            }
        }
        private Quaternion quat = Quaternion.identity;
        
        public HashSet<Vive3DSPAudioRoom> RoomList
        {
            get { return roomList; }
        }
        private HashSet<Vive3DSPAudioRoom> roomList = new HashSet<Vive3DSPAudioRoom>();


        void Awake()
        {
            Vive3DSPAudio.CreateAudioListener(this);
            OnValidate();
        }

        void OnEnable()
        {
            UpdateTransform();
        }

        void Start()
        {
            UpdateTransform();
        }

        void Update()
        {
            UpdateTransform();
        }
        private void FixedUpdate()
        {
            Vive3DSPAudio.UpdateOcclusionCoverRatio();
        }

        public void OnValidate()
        {
            Vive3DSPAudio.UpdateAudioListener();
            Vive3DSPAudio.UpdateAllOcclusion();
        }

        void OnDisable()
        {
            roomList.Clear();
        }

        private void OnDestroy()
        {
            Vive3DSPAudio.DestroyAudioListener();
        }
        private void UpdateTransform()
        {
            Position = transform.position;
            Rotation = transform.rotation;
        }
    }
}
