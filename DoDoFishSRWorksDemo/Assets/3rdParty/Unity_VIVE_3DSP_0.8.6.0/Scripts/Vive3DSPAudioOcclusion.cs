//====================== Copyright 2016-2017, HTC.Corporation. All rights reserved. ======================
using UnityEngine;
using System;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    [AddComponentMenu("HTC/Audio/3DSP_AudioOcclusion")]

    public class Vive3DSPAudioOcclusion : MonoBehaviour
    {
        public Vive3DSPAudio.OccEngineMode occlusionEngine = Vive3DSPAudio.OccEngineMode.BasicOcclusion;
        public Vive3DSPAudio.RaycastQuality raycastQuality = Vive3DSPAudio.RaycastQuality.High;

        public bool OcclusionEffect
        {
            set { occlusionEffect = value; OnValidate(); }
            get { return occlusionEffect; }
        }
        [SerializeField]
        private bool occlusionEffect = true;

        public Vive3DSPAudio.OccMaterial OcclusionMaterial
        {
            set { occlusionMaterial = value; OnValidate(); }
            get { return occlusionMaterial; }
        }
        [SerializeField]
        private Vive3DSPAudio.OccMaterial occlusionMaterial = Vive3DSPAudio.OccMaterial.Curtain;

        public float OcclusionIntensity
        {
            set { occlusionIntensity = value; OnValidate(); }
            get { return occlusionIntensity; }
        }
        [SerializeField]
        private float occlusionIntensity = 1.5f;

        public float HighFreqAttenuation
        {
            set { highFreqAttenuation = value; OnValidate(); }
            get { return highFreqAttenuation; }
        }
        [SerializeField]
        private float highFreqAttenuation = -50.0f;

        public float LowFreqAttenuationRatio
        {
            set { lowFreqAttenuationRatio = value; OnValidate(); }
            get { return lowFreqAttenuationRatio; }
        }
        [SerializeField]
        private float lowFreqAttenuationRatio = 0.0f;

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
                    OnValidate();
                }
            }
        }
        private Vector3 pos = Vector3.zero;

        public Vive3DSPAudio.VIVE_3DSP_OCCLUSION_PROPERTY OcclusionPorperty
        {
            set { occProperty = value; }
            get { return occProperty; }
        }
        private Vive3DSPAudio.VIVE_3DSP_OCCLUSION_PROPERTY occProperty;

        private float occVolume;
        public float occRadius;

        public IntPtr OcclusionObject
        {
            get { return _occObj; }
            set { _occObj = value; }
        }
        private IntPtr _occObj = IntPtr.Zero;


        private Collider colliderType;
        private BoxCollider boxCollider;
        private SphereCollider sphereCollider;
        
        public Vector3 colliderCenterLocal;
        public Vector3 colliderSize;
        public float colliderRadius;

        public Vector3[] vertexPoints = new Vector3[8];
        public int colliderStatus;


        void Awake()
        {
            InitOcclusion();
        }

        void Start()
        {
            OnEnable();

            colliderType = GetComponent<Collider>();

            if (colliderType.GetType() == typeof(BoxCollider))
            {
                colliderStatus = 1;
            }
            else if (colliderType.GetType() == typeof(SphereCollider))
            {
                colliderStatus = 2;
            }
            else
            {
                
            }

        }

        void OnEnable()
        {
            if (InitOcclusion())
            {
                Vive3DSPAudio.EnableOcclusion(_occObj);
            }
            Vive3DSPAudio.UpdateAudioListener();
            OnValidate();
            Update();
        }

        void OnDisable()
        {
            Vive3DSPAudio.DisableOcclusion(_occObj);
        }
        

        void Update()
        {
            if (transform.hasChanged)
            {
                occVolume = transform.lossyScale.x * transform.lossyScale.y * transform.lossyScale.z;
                occRadius = Mathf.Pow((3 * occVolume) / (4 * Mathf.PI), 0.33f);
            }

            if (occlusionEngine == Vive3DSPAudio.OccEngineMode.BasicOcclusion)
            {
                occProperty.position = transform.position;
                occProperty.radius = occRadius;
            }

            if (occlusionEngine == Vive3DSPAudio.OccEngineMode.AdvancedOcclusion)
            {
                if (colliderStatus == 1) // Box
                {
                    boxCollider = GetComponent<BoxCollider>();

                    colliderCenterLocal = boxCollider.center;
                    colliderSize = boxCollider.size;

                    occProperty.position = colliderCenterLocal;
                    vertexPoints = GetBoxColliderVertexPositions(colliderCenterLocal, colliderSize);
                }
                else if (colliderStatus == 2)// Sphere
                {
                    sphereCollider = GetComponent<SphereCollider>();

                    colliderCenterLocal = sphereCollider.center;
                    colliderRadius = sphereCollider.radius;

                    occProperty.position = colliderCenterLocal;
                    occProperty.radius = colliderRadius;
                }
                else
                {
                    Debug.LogWarning("other type collider!!");
                }
            }
            
            OnValidate();
        }

        private void OnDestroy()
        {
            Vive3DSPAudio.DestroyOcclusion(this);
        }

        public void OnValidate()
        {
            occProperty.density = occlusionIntensity;
            occProperty.material = occlusionMaterial;
            occProperty.position = transform.position;
            occProperty.rhf = highFreqAttenuation;
            occProperty.lfratio = lowFreqAttenuationRatio;
            occProperty.mode = occlusionEngine;
            Vive3DSPAudio.UpdateOcclusion(this);
        }
        

        private bool InitOcclusion()
        {
            if (_occObj == IntPtr.Zero)
            {
                _occObj = Vive3DSPAudio.CreateOcclusion(this);
            }
            return _occObj != IntPtr.Zero;
        }

        
        void OnDrawGizmosSelected()
        {
            if (occlusionEngine == Vive3DSPAudio.OccEngineMode.BasicOcclusion)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(transform.position, occRadius);
            }
        }

        public Vector3[] GetBoxColliderVertexPositions(Vector3 colliderCenter, Vector3 colliderSize)
        {
            Vector3[] vertices = new Vector3[8];
            vertices[0] = boxCollider.transform.TransformPoint(colliderCenter
                            + new Vector3(colliderSize.x, colliderSize.y, colliderSize.z) * 0.5f);
            vertices[1] = boxCollider.transform.TransformPoint(colliderCenter
                            + new Vector3(-colliderSize.x, -colliderSize.y, -colliderSize.z) * 0.5f);
            vertices[2] = boxCollider.transform.TransformPoint(colliderCenter
                            + new Vector3(colliderSize.x, -colliderSize.y, colliderSize.z) * 0.5f);
            vertices[3] = boxCollider.transform.TransformPoint(colliderCenter
                            + new Vector3(-colliderSize.x, colliderSize.y, -colliderSize.z) * 0.5f);
            vertices[4] = boxCollider.transform.TransformPoint(colliderCenter
                            + new Vector3(colliderSize.x, colliderSize.y, -colliderSize.z) * 0.5f);
            vertices[5] = boxCollider.transform.TransformPoint(colliderCenter
                            + new Vector3(-colliderSize.x, -colliderSize.y, colliderSize.z) * 0.5f);
            vertices[6] = boxCollider.transform.TransformPoint(colliderCenter
                            + new Vector3(colliderSize.x, -colliderSize.y, -colliderSize.z) * 0.5f);
            vertices[7] = boxCollider.transform.TransformPoint(colliderCenter
                            + new Vector3(-colliderSize.x, colliderSize.y, colliderSize.z) * 0.5f);

            occProperty.vertex.x1 = vertices[0].x;
            occProperty.vertex.y1 = vertices[0].y;
            occProperty.vertex.z1 = vertices[0].z;
            occProperty.vertex.x2 = vertices[1].x;
            occProperty.vertex.y2 = vertices[1].y;
            occProperty.vertex.z2 = vertices[1].z;
            occProperty.vertex.x3 = vertices[2].x;
            occProperty.vertex.y3 = vertices[2].y;
            occProperty.vertex.z3 = vertices[2].z;
            occProperty.vertex.x4 = vertices[3].x;
            occProperty.vertex.y4 = vertices[3].y;
            occProperty.vertex.z4 = vertices[3].z;
            occProperty.vertex.x5 = vertices[4].x;
            occProperty.vertex.y5 = vertices[4].y;
            occProperty.vertex.z5 = vertices[4].z;
            occProperty.vertex.x6 = vertices[5].x;
            occProperty.vertex.y6 = vertices[5].y;
            occProperty.vertex.z6 = vertices[5].z;
            occProperty.vertex.x7 = vertices[6].x;
            occProperty.vertex.y7 = vertices[6].y;
            occProperty.vertex.z7 = vertices[6].z;
            occProperty.vertex.x8 = vertices[7].x;
            occProperty.vertex.y8 = vertices[7].y;
            occProperty.vertex.z8 = vertices[7].z;
            return vertices;
        }
    }
}

