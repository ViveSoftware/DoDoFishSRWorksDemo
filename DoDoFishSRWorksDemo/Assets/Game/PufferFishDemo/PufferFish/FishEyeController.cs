using Demo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishEyeController : MonoBehaviour
{
    public Transform LEyeTrack, REyeTrack, LEyeSubTracker, REyeSubTracker;
    public Transform LEyeBone, REyeBone;
    [HideInInspector] public Transform Target;

    public Vector3 MaxRotConstraints, MinRotConstraints;

    private float LEyeOffset, REyeOffset;
    [SerializeField] private bool _act;
    void Start()
    {
        LEyeOffset = LEyeTrack.transform.localPosition.x;
        REyeOffset = REyeTrack.transform.localPosition.x;
    }

    void LateUpdate()
    {
        CalculateEyeDirect();
    }

    public void ChangeLookAtTarget(Transform target)
    {
        Target = target;
    }
    public void EyeTrackEnable(bool act)
    {
        _act = act;
    }

    private void CalculateEyeDirect()
    {
        if (Target == null)
            return;
        if (!_act) return;

        float x, y, z;
        Vector3 relativePosL = Target.position - LEyeTrack.position + transform.right * LEyeOffset;
        Quaternion rotationL = Quaternion.LookRotation(relativePosL);

        LEyeTrack.transform.rotation = rotationL;

        Vector3 rotL = LEyeTrack.transform.localEulerAngles;

        float Lx = rotL.x > 180 ? rotL.x - 360 : rotL.x;
        float Ly = rotL.y > 180 ? rotL.y - 360 : rotL.y;
        float Lz = rotL.z > 180 ? rotL.z - 360 : rotL.z;

        x = Mathf.Clamp(Lx, MinRotConstraints.x, MaxRotConstraints.x);
        y = Mathf.Clamp(Ly, MinRotConstraints.y, MaxRotConstraints.y);
        z = Mathf.Clamp(Lz, MinRotConstraints.z, MaxRotConstraints.z);

        LEyeTrack.transform.localEulerAngles = new Vector3(x, y, z);

        Debug.DrawRay(LEyeTrack.position, LEyeTrack.forward, Color.blue);
        Debug.DrawRay(LEyeTrack.position, LEyeTrack.up, Color.red);
        Debug.DrawRay(LEyeTrack.position, LEyeTrack.right, Color.green);

        Vector3 relativePosR = Target.position - REyeTrack.position + transform.right * REyeOffset;
        Quaternion rotationR = Quaternion.LookRotation(relativePosR);

        REyeTrack.transform.rotation = rotationR;

        Vector3 rotR = REyeTrack.transform.localEulerAngles;

        float Rx = rotR.x > 180 ? rotR.x - 360 : rotR.x;
        float Ry = rotR.y > 180 ? rotR.y - 360 : rotR.y;
        float Rz = rotR.z > 180 ? rotR.z - 360 : rotR.z;

        x = Mathf.Clamp(Rx, MinRotConstraints.x, MaxRotConstraints.x);
        y = Mathf.Clamp(Ry, MinRotConstraints.y, MaxRotConstraints.y);
        z = Mathf.Clamp(Rz, MinRotConstraints.z, MaxRotConstraints.z);

        REyeTrack.transform.localEulerAngles = new Vector3(x, y, z);

        Debug.DrawRay(REyeTrack.position, REyeTrack.forward, Color.blue);
        Debug.DrawRay(REyeTrack.position, REyeTrack.up, Color.red);
        Debug.DrawRay(REyeTrack.position, REyeTrack.right, Color.green);

        LEyeBone.rotation = LEyeSubTracker.rotation;
        REyeBone.rotation = REyeSubTracker.rotation;
    }
}
