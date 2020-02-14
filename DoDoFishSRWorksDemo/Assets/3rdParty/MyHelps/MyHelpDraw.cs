using UnityEngine;
using System.Collections;

public static class MyHelpDraw
{
    public static void RT2Tex2D(RenderTexture rt, ref Texture2D tex2D)
    {
        RenderTexture rec = RenderTexture.active;
        RenderTexture.active = rt;
        // Copy pixels from active render texture to texture2d
        tex2D.ReadPixels(new Rect(0, 0, tex2D.width, tex2D.height), 0, 0);
        tex2D.Apply();
        RenderTexture.active = rec;
    }

    public static void LineColor(Transform lineBody, Color color)
    {
        LineRenderer lineRender = MyHelpNode.FindOrAddComponent<LineRenderer>(lineBody);
        if (lineRender.startColor == color)
            return;

        lineRender.endColor = lineRender.startColor = color;
        if (lineRender.material.shader.name != "Unlit/Texture")
        {
            lineRender.material = new Material(Shader.Find("Unlit/Texture"));            
            Debug.Log("[MyHelpDraw][LineDraw] add material 'Unlit/Texture'");
        }

        Texture2D redTex = new Texture2D(2, 2, TextureFormat.RGB565, false);
        //Color fillColor = new Color(1.0f, 0.0f, 0.0f);
        Color[] fillColorArray = redTex.GetPixels();
        for (int i = 0; i < fillColorArray.Length; ++i)
            fillColorArray[i] = color;
        redTex.SetPixels(fillColorArray);
        redTex.Apply();
        lineRender.material.SetTexture("_MainTex", redTex);
    }

    public static void LineDraw(Transform lineBody, Vector3 start, Vector3 end, float startW, float endW, Color color)
    {
        LineRenderer lineRender = MyHelpNode.FindOrAddComponent<LineRenderer>(lineBody);
        lineRender.enabled = true;
        lineRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRender.receiveShadows = false;

        //add line renderer        
        lineRender.startWidth = startW;
        lineRender.endWidth = endW;
        lineRender.SetPosition(0, start);
        lineRender.SetPosition(1, end);
        //lineRender.SetColors(color, color);
        LineColor(lineBody, color);
        //MyHelpDraw.SetColor_Standard(lineRender.material, color);

    }

    public static void LineDraw2nd(Transform start, Vector3 end)
    {
        LineRenderer lineRender = start.gameObject.GetComponent<LineRenderer>();
        lineRender.SetVertexCount(4);
        lineRender.SetPosition(2, start.transform.position);
        lineRender.SetPosition(3, end);
    }

    public static void LineDrawClose(Transform start)
    {
        if (start == null) return;

        LineRenderer lineRender = start.gameObject.GetComponent<LineRenderer>();
        if (lineRender != null)
            lineRender.enabled = false;
    }

    public static Shader GetShader_DisableZ()
    {
        Shader shader = Shader.Find("Hidden/DisableZ");
        if (shader == null)
            Debug.LogError("[GetShader_DisableZ] fail --> Shader.Find(Hidden/DisableZ)");
        return shader;
    }

    public static void SetColor_DisableZ(Material mat, Color color)
    {
        if (mat == null)
            Debug.LogError("[SetColor_DisableZ] fial --> mat == null");

        mat.SetColor("_Color", new Vector4(color.r, color.g, color.b, color.a));
    }

    public static void SetColor_Standard(Material mat, Color color)
    {
        if (mat.shader.name != "Standard")
            Debug.LogWarning("[SetColor_Standard] : mat.shader.name != 'Standard'");
        mat.SetColor("_Color", new Vector4(color.r, color.g, color.b, color.a));
    }

    //public static void ShowTextOnCamera(string text, float sec = 2)
    //{
    //    GameObject textObj = new GameObject("ShowTextOnCamera");
    //    TextMesh textmesh = textObj.AddComponent<TextMesh>();
    //    MeshRenderer meshRenderer = textObj.GetComponent<MeshRenderer>();
    //    textmesh.font = FBNoticeFont;
    //    textmesh.text = text;
    //    textmesh.fontSize = 30;
    //    textmesh.characterSize = 0.02f;
    //    textmesh.anchor = TextAnchor.MiddleCenter;
    //    textmesh.alignment = TextAlignment.Center;
    //    meshRenderer.material = FBNoticeFont.material;
    //    textObj.transform.parent = VRSystem.instance.MainCameraEye.transform;
    //    textObj.transform.localPosition = Vector3.forward * 1;
    //    textObj.transform.localRotation = Quaternion.identity;

    //    if (sec > 0)
    //        Destroy(textObj, sec);
    //}
}
