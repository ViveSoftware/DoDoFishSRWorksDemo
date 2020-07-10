using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static BoneProjector;

[RequireComponent(typeof(BoneProjector))]
public class BoneSimilarity : MonoBehaviour
{
    public Transform handOcclusion;
    BoneProjector boneProjector;
    public float angleCount = 0;
    public float disCount = 0;

    public float angleOK = 100;
    public float disOK = 0.02f;

    TimelinePlayRatio timelinePlayRatio = new TimelinePlayRatio();
    ViveHandTracking.ModelRenderer handModelRenderer;
    private void Start()
    {
        timelinePlayRatio.Init(GetComponent<PlayableDirector>());
        boneProjector = GetComponent<BoneProjector>();
        handModelRenderer = GetComponentInChildren<ViveHandTracking.ModelRenderer>(true);
    }

    private void Update()
    {
        if (!handModelRenderer.GetComponentInChildren<SkinnedMeshRenderer>(true).gameObject.activeInHierarchy)
        {
            timelinePlayRatio.Init(GetComponent<PlayableDirector>());
            return;
        }

        disCount = (boneProjector.RootBoneDest.position - boneProjector.RootBoneOrig.position).magnitude;

        //.RootBoneDest
        angleCount = 0;
        angleCount += _fingerAngle(boneProjector.RelateThumb);
        angleCount += _fingerAngle(boneProjector.RelateIndex);
        angleCount += _fingerAngle(boneProjector.RelateMiddle);
        angleCount += _fingerAngle(boneProjector.RelateRing);
        angleCount += _fingerAngle(boneProjector.RelatePinky);

        if (angleCount < angleOK && disCount < disOK)
        {
            if (!isWearing)
                timelinePlayRatio.SetPlayRatio(1, 5);
            isWearing = true;
        }
        else
        {
            if (isWearing)
                timelinePlayRatio.SetPlayRatio(0);
            isWearing = false;
        }

        if (timelinePlayRatio.GetRatio() >= 1)
        {
            _wearDone();
            enabled = false;
        }
    }

    public bool isWearDone = false;
    bool isWearing;
    void _wearDone()
    {
        boneProjector.NotUpdateBone = false;
        boneProjector.UpdateBonePose();
        boneProjector.positionFitType = BoneProjector.PositionFitType.FromHandRender;
        //GetComponent<InitClove>().ShowGlove(true);
        //GetComponent<InitClove>().ClearCloneGlove();

        if (handOcclusion != null)
        {
            Renderer[] renders = handOcclusion.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renders)
                r.enabled = false;
        }

        isWearDone = true;
    }

    float _fingerAngle(RelateBone[] RelateBones)
    {
        float angle = 0;
        for (int a = 0; a < RelateBones.Length; a++)
        {
            if (a == RelateBones.Length - 1)
                break;
            RelateBone relate = RelateBones[a];
            RelateBone relateNext = RelateBones[a + 1];
            Vector3 forwardDest = relateNext.dest.position - relate.dest.position;
            float length = forwardDest.magnitude;
            forwardDest /= length;
            angle += Vector3.Angle(forwardDest, relate.recForward);
        }
        return angle;
    }
}
