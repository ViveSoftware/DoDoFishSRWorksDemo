using Demo;
using DG.Tweening;
using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    public class PBSPaintBallGunController : MonoBehaviour
    {
        private const int DepthMapSize = 1024;

        [Header("Bullet")]
        [SerializeField]
        private GameObject bulletPrefab;
        //[SerializeField] private float bulletSpeedMilePerSec = 100;
        //[SerializeField] private float offsetBulletStart = 0.015f;


        // DEBUG ONLY.
        [Header("DEBUG ONLY")]
        [SerializeField]
        private bool useGBufferRT = false;
        [SerializeField] private GBufferGenerator gBufferGen = null;

        // Private Class Data.
        // ---------------------------------------------------------------
        [SerializeField] private Shader depthShader = null;
        [SerializeField] private Material pbsSprayMaterial = null;
        private GameObject depthCamObj = null;
        private Camera depthCam = null;
        private RenderTexture depthRT = null;
        [SerializeField] private Material sprayMaterial = null;
        private PBSPaintableObject pbsPaintObjController = null;

        [Header("ObjReference")]
        [SerializeField]
        protected GameObject spraySourceObj = null;
        [SerializeField] protected GameObject sprayVisRayObj = null;
        [SerializeField] protected GameObject targetHintObj = null;
        [SerializeField] protected GameObject sprayEffectObj = null;
        [SerializeField] protected MeshRenderer bodyMeshRenderer = null;
        [SerializeField] private Color[] colors;
        [SerializeField] private Texture defaultPattern;

        private ParticleSystem particle;
        private LineRenderer visRay;
        private float shotFXTimer = 0;
        private bool shotEffect = false;
        [SerializeField] private float effectsDisplayTime = 0.1f;
        [SerializeField] private float hitPointToCamera = 0.5f;
        private AudioSource audioSource;

        public LayerMask hitLayerMask = 1 << PaintComponentDefine.PaintObjectLayer;

        // Unity Fixed Methods.
        // ---------------------------------------------------------------
        void Awake()
        {
            PaintComponentDefine.init();

            particle = sprayEffectObj.GetComponent<ParticleSystem>();
            visRay = sprayVisRayObj.GetComponent<LineRenderer>();

            visRay.enabled = false;
        }

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            // For PBS SprayCan.
            CreateDepthCamera();
            sprayMaterial = new Material(pbsSprayMaterial);
        }

        void Update()
        {
            SimulateSpray();

            if (shotEffect)
            {
                if (shotFXTimer >= effectsDisplayTime)
                {
                    DisableShotEffect();
                }
                shotFXTimer += Time.deltaTime;
            }
        }

        void OnEnable()
        {
            OnActiveColorUpdate(ParameterManager.instance.CurrentPaintColor);
            EventManager.StartListeningUpdateActiveColorEvent(OnActiveColorUpdate);
        }

        void OnDisable()
        {
            EventManager.StopListeningUpdateActiveColorEvent(OnActiveColorUpdate);
        }

        void OnDestroy()
        {
            Destroy(sprayMaterial);
        }



        // Protected Class Methods.
        // ---------------------------------------------------------------
        public void SimulateSpray()
        {

            // Detect whether the spray button is pressed.
            if (
                (GetComponent<StickyGrabbable>() != null && GetComponent<StickyGrabbable>().isGrabbed && ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger)) ||
                (GetComponent<VivePoseTracker>() != null && GetComponent<VivePoseTracker>().enabled && ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger)) ||
                (Input.GetKeyDown(KeyCode.Space) && GameObject.FindObjectOfType<ARRender>() == null)
                )
            {
                Ray ray = new Ray(spraySourceObj.transform.position, spraySourceObj.transform.forward);
                ViveInput.TriggerHapticPulse(HandRole.RightHand, 500);
#if UNITY_EDITOR
                // DebugUtil.DrawVisRay(ray, 1000.0f, Color.white);
#endif

                //float triggerValue = ViveInput.GetTriggerValue(HandRole.RightHand, false);
                //float mappedPressAmount = 1;
                // float sprayAngle = PaintUtil.ComputeSprayAngle(mappedPressAmount, ParameterManager.instance.CurrentSprayAngle, ParameterManager.instance.minSprayAngleRatio);
                // float sprayDist = PaintUtil.ComputeSprayDistance(mappedPressAmount, ParameterManager.instance.sprayDistMappingCurve, ParameterManager.instance.CurrentMaxSprayDist);
                float sprayAngle = ParameterManager.instance.CurrentSprayAngle;
                float sprayDist = 200;
                //float sprayAlpha = ComputeSprayAlpha(mappedPressAmount, ParameterManager.instance.sprayAlphaMappingCurve);

                // int layer = 1 << PaintComponentDefine.PaintObjectLayer;
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, sprayDist, hitLayerMask) && hitInfo.transform.tag == PaintComponentDefine.PaintMeshTag)
                {
                    sprayDist = Vector3.Distance(ray.origin, hitInfo.point);

                    pbsPaintObjController = hitInfo.collider.gameObject.GetComponent<PBSPaintableObject>();
                    if (pbsPaintObjController == null)
                    {
                        Debug.LogWarning("hitInfo.collider not has PBSPaintableObject : " + hitInfo.collider.name);
                    }
                    else
                    {
                        TextureSpaceData paintTextureData = pbsPaintObjController.PaintTextureData;
                        pbsPaintObjController.InvokeOnPaintingEvent();

                        if (useGBufferRT)
                        {
                            GBufferGenerator posTexGen = gBufferGen;
                            sprayMaterial.SetTexture("_PosMap", posTexGen.PositionMap);
                            sprayMaterial.SetTexture("_NormalMap", posTexGen.NormalMap);
                        }
                        else
                        {
                            sprayMaterial.SetTexture("_PosMap", pbsPaintObjController.PositionMap);
                            sprayMaterial.SetTexture("_NormalMap", pbsPaintObjController.NormalMap);
                        }

                        Vector3 hitPos = hitInfo.point;
                        sprayMaterial.SetVector("_SprayCenter", new Vector4(hitPos.x, hitPos.y, hitPos.z, 1.0f));

                        // Set spray color.
                        sprayMaterial.SetColor("_SprayColor", ParameterManager.instance.CurrentPaintColor);

                        // Set spray source.
                        //Vector3 spraySrc = spraySourceObj.transform.position;
                        Vector3 spraySrc = hitInfo.point - ray.direction * hitPointToCamera;
                        sprayMaterial.SetVector("_SpraySource", new Vector4(spraySrc.x, spraySrc.y, spraySrc.z, 1.0f));

                        // Compute spray cutoff.
                        float ratio = ParameterManager.instance.sprayAngleHalfRatioCurve.Evaluate(sprayAngle);
                        sprayMaterial.SetFloat("_SprayInnerAngle", Mathf.Cos(Mathf.Deg2Rad * sprayAngle * ratio));
                        sprayMaterial.SetFloat("_SprayOuterAngle", Mathf.Cos(Mathf.Deg2Rad * sprayAngle));

                        // Set spray alpha.
                        sprayMaterial.SetFloat("_SprayAlpha", 1f);

                        Matrix4x4 worldMat = hitInfo.collider.gameObject.GetComponent<Renderer>().localToWorldMatrix;
                        Matrix4x4 normalMat = worldMat.inverse.transpose;
                        sprayMaterial.SetMatrix("_WorldMatrix", worldMat);
                        sprayMaterial.SetMatrix("_NormalMatrix", normalMat);

                        // Update depth map.
                        float distance = Vector3.Distance(spraySourceObj.transform.position, spraySrc);
                        UpdateDepthCam(sprayAngle, distance);
                        sprayMaterial.SetTexture("_DepthMap", depthRT);

                        // Build light matrix.
                        Matrix4x4 lightViewProjMat = depthCam.projectionMatrix * depthCam.worldToCameraMatrix;
                        sprayMaterial.SetMatrix("_LightViewProjMatrix", lightViewProjMat);
                        sprayMaterial.SetMatrix("_LightViewMatrix", depthCam.worldToCameraMatrix);

                        // Set brush pattern mask.
                        sprayMaterial.SetTexture("_CookieMap", defaultPattern);

                        RenderTexture rt = RenderTexture.GetTemporary(paintTextureData.AlbedoTexture.width, paintTextureData.AlbedoTexture.height, 0, paintTextureData.AlbedoTexture.format);
                        Graphics.Blit(paintTextureData.AlbedoTexture, rt);
                        Graphics.Blit(rt, paintTextureData.AlbedoTexture, sprayMaterial);
                        RenderTexture.ReleaseTemporary(rt);

                        // Update color history.
                        EventManager.TriggerUpdateColorHistoryEvent(ParameterManager.instance.CurrentPaintColor);
                        EnableShotEffect(spraySourceObj.transform.position, hitInfo.point);
                    }
                }
                else
                {
                    hitInfo.point = ray.origin + sprayDist * ray.direction;
                    if (ScoreManager.instance != null)
                        ScoreManager.instance.ShotEvent(false);
                }

                EnableShotEffect(spraySourceObj.transform.position, hitInfo.point);
                particle.Play();
                RandomColorSelect();
            }
        }

        private void RandomColorSelect()
        {
            int inx = Random.Range((int)0, colors.Length - 1);
            EventManager.TriggerUpdateActiveColorEvent(colors[inx]);
        }

        // Private Class Methods.
        // ---------------------------------------------------------------
        private void CreateDepthCamera()
        {
            // Create depth render target.
            depthRT = new RenderTexture(DepthMapSize, DepthMapSize, 24, RenderTextureFormat.ARGBFloat);

            // Create depth camera.
            depthCamObj = new GameObject("Depth Camera");
            depthCamObj.transform.parent = transform;
            depthCam = depthCamObj.AddComponent<Camera>();
            depthCam.clearFlags = CameraClearFlags.SolidColor;
            depthCam.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            depthCam.aspect = 1.0f;
            depthCam.rect = new Rect(0, 0, 1, 1);
            depthCam.stereoTargetEye = StereoTargetEyeMask.None;
            depthCam.depth = -2;
            depthCam.targetTexture = depthRT;
            depthCam.cullingMask = (1 << LayerMask.NameToLayer("PaintObject")) | (1 << LayerMask.NameToLayer("BlockObject"));
            depthCam.enabled = false;
            depthCam.allowHDR = true;
            depthCam.nearClipPlane = 0.001f;
            depthCam.farClipPlane = 2.0f * 2;

            depthCamObj.transform.position = spraySourceObj.transform.position;
            depthCamObj.transform.forward = spraySourceObj.transform.forward;
        }

        private void UpdateDepthCam(float sprayAngle, float distance)
        {
            depthCam.transform.localPosition = new Vector3(0.01f, -0.01f, -distance);
            depthCam.transform.localEulerAngles = new Vector3(0, 180, Random.Range(0, 360));
            depthCam.fieldOfView = 2.0f * sprayAngle;
            depthCam.RenderWithShader(depthShader, "");
        }

        private void OnActiveColorUpdate(Color newColor)
        {
            if (ARRender.Instance != null)
            {
                if (ARRender.Instance.VRCamera.GetComponent<DeferredLightMap>() != null)
                {
                    bodyMeshRenderer.material.SetColor("_EmissionColor", newColor * 2);
                    bodyMeshRenderer.material.SetColor("_Color", Color.black);
                    bodyMeshRenderer.transform.GetChild(0).GetComponent<Light>().color = newColor;
                }
                else
                    bodyMeshRenderer.material.SetColor("_Color", newColor);
            }
            else
            {
                bodyMeshRenderer.material.SetColor("_Color", newColor);
            }
        }

        void _bulletDone(GameObject bullet)
        {
            Destroy(bullet);
        }

        private void EnableShotEffect(Vector3 start, Vector3 end)
        {
            /*GameObject bullet = Instantiate(bulletPrefab);
            
            Vector3 dir = end - start;
            float length = dir.magnitude;
            bullet.transform.rotation = Quaternion.LookRotation(dir / length, Vector3.up);
            BulletTrail trail = bullet.GetComponent<BulletTrail>();
            float flySec = length / bulletSpeedMilePerSec;

            bullet.transform.position = start - dir*offsetBulletStart;
            bullet.transform.DOMove(end, flySec).OnComplete(trail.BulletDestroy);*/

            //StartCoroutine(trail.BulletDestroy(1));

            visRay.startColor = ParameterManager.instance.CurrentPaintColor;
            visRay.endColor = ParameterManager.instance.CurrentPaintColor;
            visRay.SetPosition(0, start);
            visRay.SetPosition(1, end);
            visRay.enabled = true;

            shotFXTimer = 0;
            shotEffect = true;

            audioSource.Play();
        }
         
        private void DisableShotEffect()
        {
            visRay.enabled = false;
            shotEffect = false;
        }

        //public static float ComputeSprayAlpha(float pressAmount, AnimationCurve alphaMappingCurve)
        //{
        //    // Note: this function assumes the signal of trigger press has been adjusted.
        //    return alphaMappingCurve.Evaluate(pressAmount);
        //}
    }
}