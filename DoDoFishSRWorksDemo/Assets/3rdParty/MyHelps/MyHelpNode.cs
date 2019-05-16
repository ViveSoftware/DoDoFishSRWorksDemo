using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class MyHelpNode
{
    public static void GetAllComponents(Transform root, ref List<Transform> outList)
    {
        outList.Add(root);

        for (int a = 0; a < root.childCount; a++)
            GetAllComponents(root.GetChild(a), ref outList);
    }

    public static void FindTransform(Transform root, string findName, out Transform outFind, bool isContain = false, bool toLower = false)
    {
        outFind = null;
        _findTransform(root, findName, ref outFind, isContain, toLower);
    }

    public static void _findTransform(Transform root, string findName, ref Transform outFind, bool isContain, bool toLower)
    {
        if (outFind != null)
            return;

        //Debug.Log("find : " + root.name);
        string rootName = (toLower) ? root.name.ToLower() : root.name;
        if (rootName == findName)
        {
            //Debug.Log("find success xxxxxxxxxxxxxxxxxxxxxxxxx: " + rootName);
            outFind = root;
            return;
        }
        else if (isContain && rootName.Contains(findName))
        {
            outFind = root;
            return;
        }

        for (int a = 0; a < root.childCount; a++)
            _findTransform(root.GetChild(a), findName, ref outFind, isContain, toLower);
    }
    /*
    public static MeshRenderer AddMesh(GameObject obj, PrimitiveType type, Shader shader)
    {
        return AddMesh(obj, type, Color.white, shader);
    }

    public static MeshRenderer AddMesh(GameObject obj, PrimitiveType type, Color color)
    {
        return AddMesh(obj, type, color, null);
    }

    public static MeshRenderer AddMesh(GameObject obj, PrimitiveType type, Color color, Shader shader)
    {
        //http://answers.unity3d.com/questions/635991/how-to-assign-icosphere-to-meshfilter-with-script.html
        GameObject temp = GameObject.CreatePrimitive(type);
        temp.SetActive(false);

        Material diffuse = null;
        if (shader != null)
        {
            diffuse = new Material(shader);
        }
        else
        {
            //diffuse = MonoBehaviour.Instantiate(temp.GetComponent<MeshRenderer>().sharedMaterial) as Material;
            diffuse = new Material(Shader.Find("Standard"));
            diffuse.SetColor("_Color", color);
        }

        //Save material to asset


#if UNITY_EDITOR
        string assetFileName =
            diffuse.shader.name.Replace('/', '.')
            + color.ToString().Replace("RGBA", "") + ".mat";

        diffuse = MyHelpAsset.TryToCreateAsset<Material>("Assets/Resources", "MyHelpNode", assetFileName, diffuse);
#else
        string assetFileName =
            diffuse.shader.name.Replace('/', '.')
            + color.ToString().Replace("RGBA", "");

        diffuse = Resources.Load<Material>("MyHelpNode/"+ assetFileName);
        if(diffuse == null)
            Debug.LogError("[Not found] ---> MyHelpNode/"+ assetFileName);
#endif

        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = obj.AddComponent<MeshFilter>();
        Mesh mesh = temp.GetComponent<MeshFilter>().mesh;// MonoBehaviour.Instantiate(temp.GetComponent<MeshFilter>().mesh) as Mesh;      (must not use clone,or,it'll not save in prefeb)  

        //Save mesh to asset(not work)
        //string meshAssetFileName = type.ToString() + "_mesh";
        //mesh = MyHelpAsset.TryToCreateAsset<Mesh>("Assets/Resources", "MyHelpNode", meshAssetFileName, mesh);

        meshFilter.mesh = mesh;
        MonoBehaviour.DestroyImmediate(temp);

        //set renderer
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = obj.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        renderer.material = diffuse;

        return renderer;
    }
    */
    internal static void SetWorldRotation(Transform transform, object p)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Notice: this function will reset position rotation!
    /// </summary>
    public static Transform FindOrNew_InChild(Transform parent, string name)
    {
        Transform find = null;
        FindTransform(parent, name, out find);

        if (find == null)
        {
            GameObject obj = new GameObject();
            obj.name = name;
            find = obj.transform;
        }
        find.transform.parent = parent;
        find.transform.localPosition = Vector3.zero;
        find.transform.localRotation = Quaternion.identity;
        find.transform.localScale = Vector3.one;
        return find;
    }


    public static T FindOrAddComponent<T>(Transform node) where T : Component
    {
        T component = node.gameObject.GetComponent<T>();
        if (component == null)
            component = node.gameObject.AddComponent<T>();
        return component;
    }


    public static float GetWorldLength(List<Transform> nodeList)
    {
        float length = 0;
        Vector3 oldPos = nodeList[0].position;
        for (int a = 1; a < nodeList.Count; a++)
        {
            Vector3 currPos = nodeList[a].position;
            length += (oldPos - currPos).magnitude;
            oldPos = currPos;
        }
        return length;
    }

    /// <summary>
    /// 取得包含型態的node list
    /// </summary>
    public static List<T> FindTypesList<T>(Transform root)
    {
        List<T> list = new List<T>();
        _findTypes<T>(root, ref list);
        return list;
    }

    static void _findTypes<T>(Transform child, ref List<T> list)
    {
        T aaa = child.gameObject.GetComponent<T>();
        if (aaa.ToString() != "null")//用T 要用字串判斷null`,e04
        {
            Debug.LogWarning("save light : " + aaa);
            list.Add(aaa);
        }

        for (int a = 0; a < child.childCount; a++)
            _findTypes<T>(child.GetChild(a), ref list);
    }
    
    public static void SetLocalPosition(Transform root, Vector3 localpos)
    {
        root.localPosition = localpos;
        for (int a = 0; a < root.childCount; a++)
            SetLocalPosition(root.GetChild(a), localpos);
    }

    public static int[] GetFlattenListHash(Transform root)
    {
        List<Transform> outFlattenList;
        GetFlattenList(root, out outFlattenList);
        int[] list = new int[outFlattenList.Count];
        int count = 0;
        foreach (Transform t in outFlattenList)
        {
            list[count] = t.gameObject.GetHashCode();
            count++;
        }
        return list;
    }

    public static void GetFlattenList(Transform root, out List<Transform> outFlattenList)
    {
        outFlattenList = new List<Transform>();
        _getFlattenList(root, ref outFlattenList);
    }

    public static void _getFlattenList(Transform root, ref List<Transform> outFlattenList)
    {
        outFlattenList.Add(root);
        for (int a = 0; a < root.childCount; a++)
        {
            _getFlattenList(root.GetChild(a), ref outFlattenList);
        }
    }

    public static void GetFlattenList(Transform root, out List<Transform> outFlattenList, out int[] level01ArrayID, int getlevel)
    {
        GetFlattenList(root, out outFlattenList);

        List<Transform> Level0_1List = GetLevelChild(root, getlevel);
        level01ArrayID = new int[Level0_1List.Count];
        for (int a = 0; a < Level0_1List.Count; a++)
            level01ArrayID[a] = outFlattenList.IndexOf(Level0_1List[a]);
    }

    public static List<Transform> GetLevelChild(Transform root, int getlevel)
    {
        int currentLevel = 0;
        List<Transform> outLevelChild = new List<Transform>();
        _getLevelChild(root, getlevel, ref currentLevel, ref outLevelChild);
        return outLevelChild;
    }

    static void _getLevelChild(Transform root, int getlevel, ref int currentLevel, ref List<Transform> outLevelChild)
    {
        if (currentLevel <= getlevel)
            outLevelChild.Add(root);

        for (int a = 0; a < root.childCount; a++)
        {
            currentLevel++;
            _getLevelChild(root.GetChild(a), getlevel, ref currentLevel, ref outLevelChild);
            currentLevel--;
        }

            

        //List<Transform> Level0_1List = new List<Transform>();
        //Level0_1List.Add(root);
        //for (int a = 0; a < root.childCount; a++)
        //    Level0_1List.Add(root.GetChild(a));
        //return Level0_1List;
    }

    /// <summary>
    /// 將src整棵樹的pose，設定給dest整棵樹
    /// </summary>
    public static void CopyTreePose(Transform srcRoot, Transform destRoot)
    {
        destRoot.localPosition = srcRoot.localPosition;
        destRoot.localRotation = srcRoot.localRotation;
        destRoot.localScale = srcRoot.localScale;
        for (int a = 0; a < srcRoot.childCount; a++)
        {
            Transform srcChild = srcRoot.GetChild(a);
            Transform destChild = destRoot.GetChild(a);
            CopyTreePose(srcChild, destChild);
        }
    }

    public static void SetWorldPosition(Transform root, Vector3 worldpos)
    {
        root.position = worldpos;
        for (int a = 0; a < root.childCount; a++)
            SetWorldPosition(root.GetChild(a), worldpos);
    }

    public static void SetWorldRotation(Transform root, Quaternion worldrot)
    {
        root.rotation = worldrot;
        for (int a = 0; a < root.childCount; a++)
            SetWorldRotation(root.GetChild(a), worldrot);
    }

    public static void CloseColliderRigidBody(Transform parent, float drag)
    {
        Collider col = parent.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
        Rigidbody rigid = parent.GetComponent<Rigidbody>();
        if (rigid != null)
        {
            // rigid.useGravity = false;
            rigid.isKinematic = false;
            rigid.drag = drag;
        }

        for (int a = 0; a < parent.childCount; a++)
        {
            CloseColliderRigidBody(parent.GetChild(a), drag);
        }
    }

    public static void FindParentTag(Transform childNode, string tagName, ref Transform find)
    {
        if (find != null)
            return;

        if (childNode != null && childNode.tag.Contains(tagName))
            find = childNode;

        if (childNode != null)
            FindParentTag(childNode.parent, tagName, ref find);
    }

    public static void FindParentTag(Transform childNode, string[] tagNames, ref Transform find)
    {
        if (find != null)
            return;

        foreach (string name in tagNames)
        {
            if (childNode != null && childNode.tag.Contains(name))
            {
                find = childNode;
                break;
            }
        }

        if (childNode != null)
            FindParentTag(childNode.parent, tagNames, ref find);
    }

    public static void FindParentTransform(Transform childNode, ref Transform find)
    {
        if (find != null)
            return;
        if (childNode.parent != null)
            FindParentTransform(childNode.parent, ref find);
        else
            find = childNode;
    }
}
