using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitClove : MonoBehaviour
{
    Camera Cam { get { return Demo.ARRender.Instance.VRCamera(); } }
    public bool isRightHand;
    public GameObject[] gloveMeshes;
    public float ZDis = 0.2f;
    public float YDis = 0.05f;
    public float InitTime = 2;
    public bool ReStart = true;
    float _initTime;
    bool initDone;
    ViveHandTracking.ModelRenderer handModelRenderer;
    void Start()
    {
        handModelRenderer = GetComponentInChildren<ViveHandTracking.ModelRenderer>(true);
        ReStart = true;
    }

    bool UpdateBonePoseOnce;
    void Update()
    {
        if (ReStart)
        {
            ShowGlove(false);
            GetComponent<BoneSimilarity>().enabled = false;
            handModelRenderer.enabled = false;
            ReStart = false;
            _initTime = InitTime;
            initDone = false;
        }

        _initTime -= Time.deltaTime;
        if (_initTime < 0 && !initDone)
        {
            initDone = true;
            Vector3 xzDir = Cam.transform.forward;
            xzDir.y = 0;
            xzDir.Normalize();
            transform.position = Cam.transform.position +
                xzDir * ZDis +
                Vector3.up * YDis;

            Vector3 rDir = Cam.transform.forward;
            rDir.y = 0;
            rDir.Normalize();
            transform.position = transform.position + rDir * ((isRightHand) ? 0.1f : -0.1f);

            transform.right = -Cam.transform.forward;

            //CloneGlove();
            ShowGlove(true);
            GetComponent<BoneSimilarity>().enabled = true;
            handModelRenderer.enabled = true;

            if (!UpdateBonePoseOnce)
            {
                UpdateBonePoseOnce = true;
                GetComponent<BoneProjector>().UpdateBonePose();
                handModelRenderer.GetComponentInChildren<SkinnedMeshRenderer>(true).enabled = false;
            }
        }
    }

    public void ShowGlove(bool enable)
    {
        foreach (GameObject gloveMeshe in gloveMeshes)
        {
            Renderer[] renders = gloveMeshe.GetComponentsInChildren<Renderer>();
            foreach (Renderer hide in renders)
                hide.enabled = enable;
        }
    }

    //List<GameObject> cloneGloves = new List<GameObject>();
    //public void CloneGlove()
    //{
    //    ClearCloneGlove();
    //    foreach (Renderer glove in gloveMeshes)
    //        cloneGloves.Add(MyHelpMesh.Skinned2Mesh(glove.transform));
    //}

    //public void ClearCloneGlove()
    //{
    //    while (cloneGloves.Count > 0)
    //    {
    //        Destroy(cloneGloves[0]);
    //        cloneGloves.RemoveAt(0);
    //    }
    //}
}
