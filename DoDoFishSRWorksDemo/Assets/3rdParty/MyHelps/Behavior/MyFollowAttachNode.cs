using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyFollowAttachNode : MonoBehaviour
{
    public bool RigidbodyUpdate = false;
    public Transform attachNode;
    public float posRatio = 1, rotRatio = 1, scaleRatio = 0;

    Rigidbody UseRB;
    Matrix4x4 offsetMat;

    private void Start()
    {
        offsetMat = attachNode.worldToLocalMatrix * transform.localToWorldMatrix;

        if (RigidbodyUpdate)
        {
            UseRB = GetComponent<Rigidbody>();
            UseRB.isKinematic = true;//rigidbody follow object must use 'isKinematic'
        }
    }

    private void LateUpdate()
    {
        Matrix4x4 mat = attachNode.localToWorldMatrix * offsetMat;

        if (UseRB != null)
        {
            UseRB.MovePosition(Vector3.Lerp(transform.position, mat.GetColumn(3), posRatio));
            if (rotRatio >= 0)
                UseRB.MoveRotation(Quaternion.Slerp(transform.rotation, mat.rotation, rotRatio));
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, mat.GetColumn(3), posRatio);
            transform.rotation = Quaternion.Slerp(transform.rotation, mat.rotation, rotRatio);
        }
        transform.localScale = Vector3.Lerp(transform.localScale, attachNode.localScale, scaleRatio);
    }
}
