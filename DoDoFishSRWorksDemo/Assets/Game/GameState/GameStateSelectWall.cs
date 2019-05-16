using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Demo.ReconstructManager;

namespace Demo
{
    public class GameStateSelectWall : IState
    {
        GameStateManager manager;
        bool alreadySelected;
        public void EnterState(IState oldState, StatePatternBase statePatternBase)
        {
            alreadySelected = false;
            manager = statePatternBase as GameStateManager;
            ARRender.Instance.InitRenderSystem();
            ReconstructManager.Instance.ClearWallCandidate();
            ReconstructManager.Instance.AutoPickFloor();
            ReconstructManager.Instance.ActiveSelectWallPointer(true);
        }

        public void LeaveState()
        {
            ReconstructManager.Instance.ActiveSelectWallPointer(false);
        }

        public string Name()
        {
            return "GameStateSelectWall";
        }

        public void UpdateState()
        {
            if (alreadySelected)
                return;
            List<SelectWall> wallCandidates = ReconstructManager.Instance.GetWallCandidate();

            HTC.UnityPlugin.Utility.RigidPose rightHandPose = VivePose.GetPose(HandRole.RightHand);
            Matrix4x4 matRot = Matrix4x4.Rotate(rightHandPose.rot);
            Vector3 rightArmPos = rightHandPose.pos;
            Vector3 rightHandDir = matRot.GetColumn(2);
            RaycastHit hit;
            int hitLayer = (1 << ARRender.UnityRenderOnTopNoShadowLayer);
            if (Physics.Raycast(rightArmPos, rightHandDir, out hit, 99999, hitLayer))
            {
                foreach (SelectWall wall in wallCandidates)
                {
                    int hashL = wall.selectWall.GetHashCode();
                    int hashH = hit.collider.gameObject.GetHashCode();
                    if (hashL == hashH)
                    {
                        wall.SetSelectColor();

                        bool triggerpress = ViveInput.GetPress(HandRole.RightHand, ControllerButton.Trigger);
                        if (triggerpress)
                        {
                            GameObject finalWall = ReconstructManager.SetPivotInMeshCenter(
                                wall.origWall.transform,
                                wall.origWall.GetComponent<MeshCollider>().sharedMesh,
                                ReconstructManager.Instance.selectedWallRoot.transform,
                                ReconstructManager.Instance.selectWallMaterial,
                                wall.origWall.name + "_convexWall : " + wall.area,
                                wall.normal, wall.center);

                            finalWall.layer = AdvanceRender.ScanLiveMeshLayer;                            
                            break;
                        }
                    }
                }
            }

            alreadySelected = CheckSelectDone(wallCandidates);
        }

        bool CheckSelectDone(List<SelectWall> wallCandidates)
        {
            Transform selectedWallRoot = ReconstructManager.Instance.selectedWallRoot.transform;
            if (selectedWallRoot.childCount == 0)
            {
                //Debug.LogWarning("[reconstructPickWall] there are no reconstruct wall picked...");
                return false;
            }
            else
            {
                ARRender.Instance.SetDirectionalLight(selectedWallRoot.transform.GetChild(Random.Range(0, selectedWallRoot.childCount)));

                ReconstructManager.Instance.ClearWallCandidate();
                ReconstructManager.Instance.showSelectedWall();
                GameManager.Instance.ShowWallSelectedInfo(_showWallSelectedDone);
                return true;
            }
        }

        void _showWallSelectedDone()
        {
            Debug.Log("[GameStateSelectWall] wallSelectedDone start puffersh demo");
            manager.SwitchState(GameStateManager.GameState.PUFFERFISH);
        }
    }
}