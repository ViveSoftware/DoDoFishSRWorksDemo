using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyHelpVR
{
    public static TextMesh CreateTextMeshOnHead(Transform head)
    {
        GameObject obj = new GameObject("TextMesh", new System.Type[] { typeof(MeshRenderer), typeof(TextMesh) });
        TextMesh _showText = obj.GetComponent<TextMesh>();
        _showText.transform.parent = head;
        _showText.transform.localPosition = Vector3.right * -5f + Vector3.up * 1.5f + Vector3.forward * 10;
        _showText.transform.localRotation = Quaternion.identity;
        _showText.characterSize = 0.4f;
        //backPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //backPlane.transform.parent = _showText.transform;
        //Destroy(backPlane.GetComponent<Collider>());
        return _showText;
    }
}
