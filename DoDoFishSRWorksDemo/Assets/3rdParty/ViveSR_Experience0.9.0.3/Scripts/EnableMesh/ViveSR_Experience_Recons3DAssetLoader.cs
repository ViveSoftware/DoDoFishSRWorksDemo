using UnityEngine;

namespace Vive.Plugin.SR.Experience
{
    public class ViveSR_Experience_Recons3DAssetLoader : MonoBehaviour
    {
        public bool isColliderReady, isMeshReady;
        public MeshRenderer[] meshRnds;
        public MeshRenderer[] cldRnds;
        System.Action done;

        private void LoadMeshDoneCallBack(GameObject go, string semanticFileName, bool updateIsReady)
        {            
            meshRnds = go.GetComponentsInChildren<MeshRenderer>();
            int numRnds = meshRnds.Length;
            for (int id = 0; id < numRnds; ++id)
            {
                meshRnds[id].sharedMaterial.shader = Shader.Find("ViveSR/MeshCuller, Shadowed, Stencil");
                meshRnds[id].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRnds[id].gameObject.layer = LayerMask.NameToLayer(ViveSR_RigidReconstruction.ReconsMeshLayerName);
            }
            if(updateIsReady) isMeshReady = true;
            if(done != null) done();
        }

        private void LoadColliderDoneCallBack(GameObject go, string semanticFileName, bool updateIsReady)
        {           
            if (ViveSR_RigidReconstructionColliderManager.ProcessDataAndGenColliderInfo(go) == true)
            {
                ViveSR_RigidReconstructionColliderManager cldPool = go.AddComponent<ViveSR_RigidReconstructionColliderManager>();
                Rigidbody rigid = go.AddComponent<Rigidbody>();
                rigid.isKinematic = true;
                rigid.useGravity = false;

                cldPool.OrganizeHierarchy();

                cldRnds = go.GetComponentsInChildren<MeshRenderer>(true);
            }
            if(updateIsReady) isColliderReady = true;
        }

        public GameObject LoadMeshObj(string path, System.Action done = null)
        {
            isMeshReady = false;
            this.done = done;
            return OBJLoader.LoadOBJFile(path, LoadMeshDoneCallBack);
        }

        public GameObject LoadColliderObj(string path)
        {
            isColliderReady = false;
            return OBJLoader.LoadOBJFile(path, LoadColliderDoneCallBack);
        }   
    }
}