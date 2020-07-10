using DG.Tweening;
using HTC.UnityPlugin.Vive;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Demo
{
    public class GameManager : MySingleton<GameManager>
    {
        public TextBoard eyeInfoText;

        [Header("Paint Ball Wall Game----------------------------------------")]
        [SerializeField]
        public Transform hidingDoor;
        [SerializeField]
        public Transform measureSizeQuad;
        [SerializeField]
        public GameObject PaintBallWallGameObj;
        [SerializeField]
        public FishAI fishAI;
        [SerializeField]
        public Transform PaintBallWallGameLocation;
        [SerializeField]
        public Transform PaintBallWallGameSizePlane;
        [SerializeField]
        public Transform PBSPaintBallGun;
        [SerializeField]
        public Material PaintBallWallGame_LiftReflectTex;
        [SerializeField]
        public Material PaintBallWallGame_FixedReflectTex;
        [SerializeField]
        private PaintVR.GBufferGenerator generator;
        Transform trackerOfMRWall;
        Transform trackerOfHealth;
        PaintBallStateManager paintBallStateManger;

        [Header("Garage----------------------------------------")]
        public Transform LiftMachine;
        [HideInInspector]
        GarageController garageController;

        [Header("Robot----------------------------------------")]
        public Transform Robot;
        RobotController robotController;

        [Header("Fairy Stick----------------------------------------")]
        public Transform FairyStick;

        [Header("Glove----------------------------------------")]
        public GameObject Glove_L, Glove_R;

        GameStateManager _gameStateManager;

        void Start()
        {
            if (LiftMachine != null)
            {
                //LiftMachine
                Renderer[] renders = LiftMachine.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renders)
                    r.enabled = false;
                CarDoorOpen[] carDoorOpens = LiftMachine.GetComponentsInChildren<CarDoorOpen>();
                for (int a = 0; a < carDoorOpens.Length; a++)
                    Destroy(carDoorOpens[a]);
            }

            //Add OnPaintingListener
            PaintVR.PBSPaintableObject[] tp = fishAI.GetComponentsInChildren<PaintVR.PBSPaintableObject>();
            List<PaintVR.PBSPaintableObject> poList = new List<PaintVR.PBSPaintableObject>(tp);
            StartCoroutine(_GBufferGeneratorInit(poList));
            foreach (PaintVR.PBSPaintableObject paintObj in poList)
                paintObj.AddOnPaintingListener(OnPaintBallShot);

            //Paint ball game
            Transform gameStateMangerObj;
            MyHelpNode.FindTransform(PaintBallWallGameObj.transform, "StateManger", out gameStateMangerObj);
            paintBallStateManger = gameStateMangerObj.GetComponent<PaintBallStateManager>();
            paintBallStateManger.SetStopMusic();
            trackerOfMRWall = MyReflection.GetMemberVariable(RoomSettingManager.instance, "trackerOfMRWall") as Transform;
            trackerOfHealth = MyReflection.GetMemberVariable(RoomSettingManager.instance, "trackerOfHealth") as Transform;
            trackerOfMRWall.position = Vector3.up * 99f;
            trackerOfHealth.position = Vector3.up * 99f;

            //Set new material
            List<MeshRenderer> renderList = new List<MeshRenderer>(GameManager.Instance.PaintBallWallGameObj.GetComponentsInChildren<MeshRenderer>());
            renderList.AddRange(GameManager.Instance.hidingDoor.GetComponentsInChildren<MeshRenderer>());
            foreach (MeshRenderer mr in renderList)
            {
                if (mr.gameObject.tag == "PaintBallWall_Lift")
                {
                    mr.material = new Material(GameManager.Instance.PaintBallWallGame_LiftReflectTex);
                }
                else if (mr.gameObject.tag == "PaintBallWall_Fix")
                {
                    mr.material = new Material(GameManager.Instance.PaintBallWallGame_FixedReflectTex);
                }
            }

            infoTextStart();

            _gameStateManager = new GameStateManager();
            _gameStateManager.Restart();
        }

        private void OnPaintBallShot()
        {
            fishAI.OnPaintBallShot();
            //StartPaintBallWallGame();
        }

        void Update()
        {
            _gameStateManager.Update();
        }

        public bool IsFishIdle()
        {
            return fishAI.IsIdleState();
        }

        public void CloseGarageSetting()
        {
            if (GarageIsClosing)
                return;
            GarageIsClosing = true;
            StartCoroutine(_waitGarageClose());
        }

        //IEnumerator _waitGarageOpen()
        //{
        //    while (garageController != null && !garageController.IsOpenDone())
        //    {
        //        yield return new WaitForEndOfFrame();
        //    }
        //    Transform find;
        //    MyHelpNode.FindTransform(LiftMachine, "Cover", out find);
        //    find.gameObject.SetActive(false);
        //    MyHelpNode.FindTransform(LiftMachine, "liftDoor", out find);
        //    find.gameObject.SetActive(false);
        //}

        public bool GarageIsClosing { get; private set; }
        IEnumerator _waitGarageClose()
        {
            Transform find;
            MyHelpNode.FindTransform(LiftMachine, "Cover", out find);
            find.gameObject.SetActive(true);
            MyHelpNode.FindTransform(LiftMachine, "liftDoor", out find);
            find.gameObject.SetActive(true);

            //garageController.GarageClose();
            while (garageController != null && !garageController.IsCloseDone())
            {
                yield return new WaitForEndOfFrame();
            }
            //LiftMachine.gameObject.SetActive(false);
            Renderer[] renderer = LiftMachine.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderer)
                r.enabled = false;

            GarageIsClosing = false;

            Debug.Log("[_waitGarageClose] close done");
        }

        public void StartGarageSetting(Vector3 pos)
        {
            //VivePoseTracker tracker = LiftMachine.GetComponent<VivePoseTracker>();
            //if (tracker.isPoseValid)
            //    LiftMachine.position = tracker.transform.position;
            ////else if (recontructFloor != null && recontructFloor.transform.childCount > 0)
            ////    LiftMachinePosition = recontructFloor.transform.GetChild(0).position;
            //else
            pos.y -= 0.1f;
            LiftMachine.position = pos;

            if (ARRender.ADVANCE_RENDER)
            {
                //We only set PaintObject to render shadow to save shadow render performance
                MyHelpLayer.ReplaceSceneLayer(LiftMachine, LayerMask.NameToLayer("Default"), ARRender.UnityRenderOnTopLayer);
                MyHelpLayer.ReplaceSceneLayer(LiftMachine, LayerMask.NameToLayer("PaintObject"), ARRender.UnityRenderOnTopLayer);
            }
            else
                MyHelpLayer.SetSceneLayer(LiftMachine, ARRender.UnityRenderOnTopLayer);

            FixedWorldPosCover[] fwpcs = LiftMachine.GetComponentsInChildren<FixedWorldPosCover>();
            foreach (FixedWorldPosCover fwpc in fwpcs)
            {
                fwpc.UpdateWorldMatrix();
                //    fwpc.GetComponent<Renderer>().sharedMaterial.SetTexture("_LeftEyeTexture", dualCameraLRT.GetRT());
                //    fwpc.GetComponent<Renderer>().sharedMaterial.SetTexture("_RightEyeTexture", dualCameraRRT.GetRT());
                fwpc.GetComponent<Renderer>().sharedMaterial.SetFloat("_Transparent", 1);
            }
            Renderer[] renders = LiftMachine.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renders)
                r.enabled = true;

            ARRender.Instance.SetDirectionalLightBack(LiftMachine);
        }

        public void ClosePufferFishDemo()
        {
            fishAI.DisableFish();
            FairyStick.gameObject.SetActive(false);
        }

        public void CloseRobotDemo()
        {
            Robot.gameObject.SetActive(false);
            robotController = null;
        }

        public void StartFishDemo()
        {
            ShowHitFishInfo();
            //ARRender.Instance.VRCamera.GetComponent<BackGroundSound>().PlaySoundLoop("backGroundSound", 1f);
            //currentDemoType = DemoType.fish;
            fishAI.gameObject.SetActive(true);
            fishAI.RestartAI();
        }

        public string PadKeyName
        {
            get
            {
                return "'AKey | Pad'";
            }
        }

        public string CarGateKeyName
        {
            get
            {
                return (MyInput.isJoystickController(ViveRole.GetDeviceIndex(HandRole.RightHand))) ? "Trigger" : "Pad";
            }
        }

        public string ChangeModeKeyName
        {
            get
            {
                return (MyInput.isJoystickController(ViveRole.GetDeviceIndex(HandRole.RightHand))) ? "AKey" : "Grip";
            }
        }

        public GarageController StartBigCarDemo()
        {
            eyeInfoText.ShowTextBigToSmaller("<align=center><size=120%><b>The car demo</b><line-height=100%><align=left><b><size=100%>\nPress <color=#FFC000FF>" + CarGateKeyName + "</b> <color=white><size=80%>: to place/open gate\n<size=100%><b>Press <color=#FFC000FF>Pad <size=70%>L, R, Up, Down <color=white><size=80%></b>: to control platform<b><size=100%><color=white>\nPress<color=#FFC000FF> " + ChangeModeKeyName + "<color=white></b> <size=80%>: to switch game mode",
                9999, true, false);
            //currentDemoType = DemoType.car;
            StartGarageSetting(Vector3.zero);
            garageController = LiftMachine.GetComponentInChildren<GarageController>(true);
            return garageController;
        }

        public RobotController StartRobotDemo()
        {
            eyeInfoText.ShowTextBigToSmaller(
            //"<b><align=center><size=120%>Robot demo\n<align=left><size=100%>Press <color=red>Trigger</b><color=white><size=80%> : to move forward\n<size=100%><b>Touch <color=red>Pad</b><color=white><size=80%> : to turn\n<size=100%><b>Press <color=red>Pad Up, Down</b> <color=white><size=80%>:\nto control height\n<size=100%><b>Press <color=red>Pad L, R</b><color=white><size=80%>:\nto switch functions<b><size=100%>\nPress <color=red>'Grip'<color=white></b> <size=80%>:\nto switch game mode"
            "<b><align=center><size=120%>Robot demo\n<line-height=100%><align=left><size=100%>Press <color=#FFC000FF>Trigger</b><color=white><size=80%> : to move forward\n<size=100%><b>Touch <color=#FFC000FF>Pad</b><color=white><size=80%> : to turn\n<size=100%><b>Press <color=#FFC000FF>Pad Up, Down</b> <color=white><size=80%>: to control height\n<size=100%><b>Press <color=#FFC000FF>Pad L, R</b><color=white><size=80%>: to switch functions<b><size=100%>\nPress <color=#FFC000FF>" + ChangeModeKeyName + "<color=white></b> <size=80%>: to switch game mode"
            , 9999, false, false
            );
            //currentDemoType = DemoType.robot;
            Robot.gameObject.SetActive(true);
            robotController = Robot.GetComponent<RobotController>();
            robotController.ShowUp(ARRender.Instance.VRCamera().transform.position, ARRender.Instance.VRCamera().transform.forward);
            robotController.stallHigh = ARRender.Instance.VRCamera().transform.position.y + 0.5f;

            ARRender.Instance.SetRobotLightBeam(robotController);
            return robotController;
        }

        public void SetHandTouchMode()
        {
            //Add rigidbody, when we don't shoot(paint) fish, thus we can hit fish.
            MyPhysics.SetBallRigidbody(fishAI.gameObject);
            SRWorkHand.Instance.SetDetectHand();
            if (SRWorkHand.GetDynamicHand() != null)
                SRWorkHand.GetDynamicHand().gameObject.layer = HandTouchFish.HandTouchLayer;
            PBSPaintBallGun.gameObject.SetActive(false);
        }

        public void StartPaintBallWallGame()
        {
            //if (PaintBallWallGameSizePlaneOrigScale == null)
            //    PaintBallWallGameSizePlaneOrigScale = PaintBallWallGameSizePlane.lossyScale.z;
            float origW = measureSizeQuad.transform.lossyScale.x;//2.8f;// original W, H is 2.8 , 1.6
            origW = origW * 3f / 7f; // 3x2 block size of 7x4 size
            Vector3 newNearPoint, newFarPoint;
            //PaintBallWallGameObj.SetActive(true);

            Debug.LogWarning("[StartPaintBallWallGame]");

            float localScale = 1;
            Transform fishHidingWall = fishAI.fishHidingWall;
            if (fishHidingWall == null)
                fishHidingWall = ReconstructManager.Instance.selectedWallRoot.transform.GetChild(0);

            //float areaQuadMax, areaQuadMin;
            //Mesh quadMesh = fishHidingWall.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            Mesh quadMesh = fishHidingWall.GetComponent<MeshFilter>().sharedMesh;
            Transform quadTransform = fishHidingWall;//.GetChild(0);
            float destW = 0;
            if (fishHidingWall != null)
            {
                PaintBallWallGameLocation.position = fishHidingWall.position;
                //PaintBallWallGameLocation.rotation = fishHidingWall.rotation;

                PaintBallWallGameLocation.LookAt(
                    fishHidingWall.position + GetFaceToPlayerNormal(fishHidingWall.position, fishHidingWall.forward),
                    Vector3.up);

                //Fit quad align with height
                //Because the game quad is W:(10m*0.7), H:(10m*0.4), so fit the height with quadMesh's height
                /*areaQuadMax = -9999;
                areaQuadMin = 9999f;
                foreach (Vector3 vec in quadMesh.vertices)
                {
                    //Z is the width
                    Vector3 vecWorld = fishHidingWall.TransformPoint(vec);
                    if (vecWorld.z > areaQuadMax)
                        areaQuadMax = vecWorld.z;
                    if (vecWorld.z < areaQuadMin)
                        areaQuadMin = vecWorld.z;
                }*/
                List<Vector3> widthList = new List<Vector3>();
                for (int a = 0; a < quadMesh.vertices.Length; a++)// ( in quadMesh.vertices)
                {
                    Vector3 vec = quadMesh.vertices[a];
                    vec = quadTransform.TransformPoint(vec);
                    vec.y = 0;
                    widthList.Add(vec);
                }

                MyHelpMesh.GetLongestLineFromVertices(widthList, null, out newNearPoint, out newFarPoint);

                destW = (newNearPoint - newFarPoint).magnitude;
                localScale = destW / origW; //Z is the width

                //Furthermore consider the area ratio
                float areaQuad = ReconstructManager.GetMeshArea(quadMesh);
                float areaConvex = ReconstructManager.GetMeshArea(fishHidingWall.GetComponent<MeshFilter>().sharedMesh);
                localScale *= areaConvex / areaQuad;
            }
            trackerOfMRWall.position = PaintBallWallGameLocation.position + PaintBallWallGameLocation.forward * 0.01f;
            Vector3 lookForward = PaintBallWallGameLocation.forward;
            lookForward.y = 0;
            trackerOfMRWall.LookAt(PaintBallWallGameLocation.position + lookForward.normalized, Vector3.up);

            localScale = Mathf.Clamp(localScale * 0.8f, 0.1f, 1.2f);//limit the max size
            trackerOfMRWall.localScale = Vector3.one * localScale;

            //Get the top offset to align with top edge
            //areaQuadMax = -9999;
            //areaQuadMin = 9999f;
            //foreach (Vector3 vec in quadMesh.vertices)
            //{
            //    //X is the height
            //    Vector3 vecWorld = quadTransform.TransformPoint(vec);
            //    if (vecWorld.y > areaQuadMax)
            //        areaQuadMax = vecWorld.y;
            //    if (vecWorld.y < areaQuadMin)
            //        areaQuadMin = vecWorld.y;
            //}
            List<Vector3> heightList = new List<Vector3>();
            for (int a = 0; a < quadMesh.vertices.Length; a++)// ( in quadMesh.vertices)
            {
                Vector3 vec = quadMesh.vertices[a];
                vec = quadTransform.TransformPoint(vec);
                vec.x = 0;
                vec.z = 0;
                heightList.Add(vec);
            }
            //float destH = areaQuadMax - areaQuadMin;
            MyHelpMesh.GetLongestLineFromVertices(heightList, null, out newNearPoint, out newFarPoint);
            float destH = (newNearPoint - newFarPoint).magnitude;

            float origH = measureSizeQuad.transform.lossyScale.y;//1.6f * localScale;// original W, H is 2.8 , 1.6
            origH = origH * 2f / 4f; // 3x2 block size of 7x4 size
            float offsetH = (destH - origH) * 0.5f;
            //float ratio = destH / destW;
            trackerOfMRWall.position = new Vector3(trackerOfMRWall.position.x, trackerOfMRWall.position.y + offsetH * 0.6f, trackerOfMRWall.position.z);

            //set health position
            Vector3 dir = PaintBallWallGameLocation.position - ARRender.Instance.VRCamera().transform.position;
            dir.y = 0;
            dir.Normalize();
            // float length = dir.magnitude;
            trackerOfHealth.position = ARRender.Instance.VRCamera().transform.position + dir * 1f;
            trackerOfHealth.LookAt(ARRender.Instance.VRCamera().transform, Vector3.up);

            ARRender.Instance.AddUnityrenderWithDepth(trackerOfHealth);
            if (ARRender.ADVANCE_RENDER)
            {
                //We only set PaintObject to render shadow to save shadow render performance
                MyHelpLayer.ReplaceSceneLayer(trackerOfMRWall, LayerMask.NameToLayer("Default"), ARRender.UnityRenderOnTopNoShadowLayer);
                MyHelpLayer.ReplaceSceneLayer(trackerOfMRWall, LayerMask.NameToLayer("PaintObject"), ARRender.UnityRenderOnTopLayer);
            }
            else
                MyHelpLayer.SetSceneLayer(trackerOfMRWall, ARRender.UnityRenderOnTopLayer);

            paintBallStateManger.SetState((int)PaintBallStateManager.PaintBallStateEnum.WAIT_USER_TRIGGER);

            MRWallManager.instance.ResetMRWall();

            //Set directional light
            ARRender.Instance.SetDirectionalLight(fishHidingWall);
        }

        //In some case, the reconstruct is back face to player, so, we get the negative normal.
        public Vector3 GetFaceToPlayerNormal(Vector3 objPos, Vector3 objNormal)
        {
            Vector3 dir2Player = ARRender.Instance.VRCamera().transform.position - objPos;
            float dotSide = Vector3.Dot(dir2Player, objNormal);
            return objNormal * ((dotSide < 0) ? -1f : 1f);
        }

        IEnumerator _GBufferGeneratorInit(List<PaintVR.PBSPaintableObject> poList)
        {
            //Must not GBufferGenerate at same time, wait until MRWallManager init gbuffer done!
            while (!MRWallManager.instance.initStatus)
                yield return new WaitForEndOfFrame();

            while (true)
            {
                if (poList.Count == 0)
                    break;

                yield return new WaitForEndOfFrame();
                generator.targetObject = poList[0].gameObject;
                generator.GBufferGeneratorInit();
                poList.RemoveAt(0);
            }
        }

        public void ClosePaintBallWallGame()
        {
            //PaintBallWallGameObj.SetActive(false);        

            trackerOfMRWall.position = Vector3.up * 99f;
            trackerOfHealth.position = Vector3.up * 99f;

            paintBallStateManger.SetStopMusic();
        }

        public void PaintGunCanPickUp()
        {
            eyeInfoText.gameObject.SetActive(true);
            eyeInfoText.ShowTextBigToSmaller("<b>Press <color=#FFC000FF>Grip</b>\n<color=white><size=80%>to pick up gun,\n\n<b><size=100%>then <color=#FFC000FF>Trigger</b>\n<color=white><size=80%>to shoot fish"
                , 999, false, true);
            ARRender.Instance.VRCamera().GetComponentInParent<BackGroundSound>().PlaySound("gunCanPickUp", 1f);

            Vector3 position = ARRender.Instance.VRCamera().transform.position + ARRender.Instance.VRCamera().transform.forward * 1f;
            Quaternion rot = Quaternion.Euler(
                (float)UnityEngine.Random.Range(0, 360),
                (float)UnityEngine.Random.Range(0, 360),
                (float)UnityEngine.Random.Range(0, 360));

            PBSPaintBallGun.gameObject.SetActive(true);
            PBSPaintBallGun.transform.position = position;
            PBSPaintBallGun.transform.rotation = rot;
            //if (PBSPaintBallGun.GetComponent<HTC.UnityPlugin.Vive.VivePoseTracker>() != null)
            //{
            //    PBSPaintBallGun.GetComponent<HTC.UnityPlugin.Vive.VivePoseTracker>().enabled = false;
            //    PBSPaintBallGun.GetComponent<HTC.UnityPlugin.Vive.VivePoseTracker>().transform.position = position;
            //    PBSPaintBallGun.GetComponent<HTC.UnityPlugin.Vive.VivePoseTracker>().transform.rotation = rot;
            //}
            PBSPaintBallGun.GetComponent<Rigidbody>().isKinematic = false;
            PBSPaintBallGun.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            PBSPaintBallGun.GetComponent<Rigidbody>().velocity = Vector3.up * 3;
            PBSPaintBallGun.GetComponent<Rigidbody>().angularVelocity = new Vector3(1.5f, 2.5f, 3.1f);

            //Must remove rigidbody, thus we can paint on mesh
            Rigidbody rb = fishAI.gameObject.GetComponent<Rigidbody>();
            GameObject.DestroyImmediate(rb);
            SRWorkHand.Instance.CloseDetectHand();
        }

        bool isFishHidingDoorOpening;
        public void OpenFishHidingDoor(Vector3 pos, Vector3 normal)
        {
            if (isFishHidingDoorOpening)
                return;
            isFishHidingDoorOpening = true;
            hidingDoor.gameObject.SetActive(true);
            hidingDoor.position = pos;
            //hidingDoor.LookAt(pos + normal);
            hidingDoor.right = -normal;
            Vector3 forward = Vector3.Cross(-normal, Vector3.up).normalized;
            hidingDoor.forward = forward;

            Transform door = hidingDoor.Find("Root");
            door.localRotation = Quaternion.identity;
            _updateFishHidingDoorMatrix();

            doorOpen(door);
            // StartCoroutine(CloseFishHidingDoor());
        }

        public void SetFishToRenderTop()
        {
            MyHelpLayer.ReplaceSceneLayer(fishAI.transform, LayerMask.NameToLayer("Default"), ARRender.UnityRenderOnTopNoShadowLayer);
            MyHelpLayer.ReplaceSceneLayer(fishAI.transform, LayerMask.NameToLayer("PaintObject"), ARRender.UnityRenderOnTopLayer);
            ARRender.Instance.SetUnityrenderWithDepthToTopLayer();
        }

        public void RecoverFishToDefaultLayer()
        {
            MyHelpLayer.ReplaceSceneLayer(fishAI.transform, ARRender.UnityRenderOnTopNoShadowLayer, LayerMask.NameToLayer("Default"));
            MyHelpLayer.ReplaceSceneLayer(fishAI.transform, ARRender.UnityRenderOnTopLayer, LayerMask.NameToLayer("PaintObject"));
            ARRender.Instance.RecoverUnityrenderWithDepthLayer();
        }

        void doorOpen(Transform door)
        {
            //yield return new WaitForEndOfFrame();
            door.DOLocalRotate(new Vector3(0, 0, 90f), 0.5f);
        }

        void _updateFishHidingDoorMatrix()
        {
            // yield return new WaitForEndOfFrame();
            foreach (MeshRenderer mr in hidingDoor.GetComponentsInChildren<MeshRenderer>())
            {
                if (mr.gameObject.tag == "PaintBallWall_Fix")
                {
                    //Debug.LogWarning("[_updateFishHidingDoorMatrix] : " + mr.gameObject.name);
                    mr.material.SetMatrix("_WorldMatrix", mr.transform.localToWorldMatrix);
                    //mr.material.SetTexture("_ReflectTextureR", dualCameraRRT.GetRT());
                    //mr.material.SetTexture("_ReflectTextureL", dualCameraLRT.GetRT());
                }
            }
        }

        bool _closeFishHidingDoorStillShow;
        public void CloseFishHidingDoor(float delay = 0, bool stillShow = true)
        {
            _closeFishHidingDoorStillShow = stillShow;
            StartCoroutine(_closeFishHidingDoor(delay));
        }

        IEnumerator _closeFishHidingDoor(float delay)
        {
            yield return new WaitForSeconds(delay);
            Transform door = hidingDoor.Find("Root");
            door.DOLocalRotate(new Vector3(0, 0, 0f), 0.5f).OnComplete(_closeFishHidingDoorDone);
        }

        void _closeFishHidingDoorDone()
        {
            isFishHidingDoorOpening = false;
            hidingDoor.gameObject.SetActive(_closeFishHidingDoorStillShow); //Not to hide the door to prevent scan gap will shown while FishHidingDoor not draw.            
        }

        public void ShowHitFishInfo()
        {
            eyeInfoText.ShowTextBigToSmaller(
                //"Try hit fish by your hand!\n<color=red>'Trigger'<color=#005500> to switch items\n'Grip' to switch game mode"
                "<align=center><size=120%><b>Try hit fish by your hand!</b>\n<b><line-height=100%><align=left><size=100%>Press <color=#FFC000FF>Trigger</b> <size=80%><color=white>: to switch item\n<b><size=100%>Press <color=#FFC000FF>" + ChangeModeKeyName + "<color=white></b> <size=80%>: to switch game mode"
                , 999, false, false
                );
        }

        public string GetInfoText()
        {
            return eyeInfoText.text;
        }

        public void SetInfoText(string text)
        {
            eyeInfoText.ShowText(text);
        }

        public void ShowInfoTextAni(string text)
        {
            //eyeInfoText.text = text;
            eyeInfoText.ShowTextBigToSmaller(text);
        }

        public void EnableInfoText(bool active)
        {
            eyeInfoText.gameObject.SetActive(active);
        }

        public void SwitchInfoText()
        {
            eyeInfoText.gameObject.SetActive(!eyeInfoText.gameObject.activeSelf);
        }

        public void infoTextStart()
        {
            eyeInfoText.ShowTextBigToSmaller("<align=center><b>Press <color=#FFC000FF>Trigger\n</b><size=80%><color=white>to start scanning room\n\n" +
                "<b><size=100%>Press <color=#FFC000FF>" + PadKeyName + "</b>\n<color=white><size=80%>to load scanning scene\n\n" +
                "<b><size=100%>Press <color=#FFC000FF>BKey</b>\n<color=white><size=80%>to skip wall"
                );
        }

        public void ShowWallSelectedInfo(Action showDone)
        {
            StartCoroutine(_showWallSelected(showDone));
        }

        IEnumerator _showWallSelected(Action showDone)
        {
            GameManager.Instance.ShowInfoTextAni("<b><size=120%>Select wall done!");
            yield return new WaitForSeconds(4);
            GameManager.Instance.ShowInfoTextAni("<b><size=120%>Try hit fish by your hand!");
            showDone();
        }

        public FixedWorldPosCover[] GetGarageARMaterial()
        {
            if (garageController == null)
                return null;
            return garageController.GetComponentsInChildren<FixedWorldPosCover>();
        }

        public void UpdateLDualCamRT(RenderTexture rt)
        {
            GameStateManager.IGameStateUpdateRealWorldCameraImage updateImage = _gameStateManager.GetCurrentState() as GameStateManager.IGameStateUpdateRealWorldCameraImage;
            if (updateImage != null)
                updateImage.LRealWorldCamRTRefresh(rt);
        }

        public void UpdateRDualCamRT(RenderTexture rt)
        {
            GameStateManager.IGameStateUpdateRealWorldCameraImage updateImage = _gameStateManager.GetCurrentState() as GameStateManager.IGameStateUpdateRealWorldCameraImage;
            if (updateImage != null)
                updateImage.RRealWorldCamRTRefresh(rt);
        }

        public bool ViveIsGrip()
        {
            if (_gameStateManager.myInput.IsJoystick)
            {
                return (_gameStateManager.myInput.IsFirstCosmosAKey
                    //&& (ReconstructManager.Instance.reconstructDataAnalyzeDone || GameStateSRWorksLoading.SkipSelectWall)
                 );
            }
            else
            {
                return (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Grip)
                    //&& ReconstructManager.Instance.reconstructDataAnalyzeDone
                 );
            }
        }
    }


}