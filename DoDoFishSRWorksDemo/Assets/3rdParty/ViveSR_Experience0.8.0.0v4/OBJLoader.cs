using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

public class OBJLoader : MonoBehaviour
{
    public delegate void LoadOBJCompleteCallback(GameObject go, bool updateIsReady);

    private static GameObject loaderOBJ = null;
    //private string inputPath = "";
  //  private bool loadCompelete = false;
    private GameObject returnObject = null;
    //private LoadOBJCompleteCallback callback = null;

    struct Triangle
    {
        public int[] indices;
    }

    public static GameObject LoadOBJFile(string fn, LoadOBJCompleteCallback cb = null, bool updataIsReady = true)
    {
        if (loaderOBJ == null)
        {
            loaderOBJ = new GameObject("Loader");
            loaderOBJ.hideFlags = HideFlags.HideInHierarchy;            
        }

        OBJLoader loader = loaderOBJ.AddComponent<OBJLoader>();
        loader._Load( fn, cb, updataIsReady );
        return loader.returnObject;
    }

    private void _Load(string fn, LoadOBJCompleteCallback cb, bool updataIsReady)
    {
        string name = Path.GetFileNameWithoutExtension(fn);
        //loadCompelete = false;
        returnObject = new GameObject(name);
        returnObject.SetActive(false);
        StartCoroutine(_LoadOBJFile(fn, cb, updataIsReady));
    }

    private static Vector2 _ReadVector2(string[] tokens)
    {
        float x = float.Parse(tokens[1]);
        float y = float.Parse(tokens[2]);
        return new Vector2(x, y);
    }
    private static Vector3 _ReadVector3(string[] tokens)
    {
        float x = float.Parse(tokens[1]);
        float y = float.Parse(tokens[2]);
        float z = float.Parse(tokens[3]);
        return new Vector3(x, y, z);
    }
    private static Color _ReadColor(string[] tokens)
    {
        float r = float.Parse(tokens[1]);
        float g = float.Parse(tokens[2]);
        float b = float.Parse(tokens[3]);
        return new Color(r, g, b);
    }

    private static string _GetActualPathOrName(string basePath, string subPath)
    {
        if (File.Exists(basePath + subPath))
            return basePath + subPath;

        else if (File.Exists(subPath))
            return subPath;

        return null;
    }

    private static Texture2D _LoadTexture(string filename)
    {
        if (!File.Exists(filename))
            return null;

        string ext = Path.GetExtension(filename).ToLower();
        Texture2D texture = null;
        if (ext == ".png" || ext == ".jpg")
        {
            texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(filename));
        }
        else
        {
            Debug.Log("Unsupported Texture Format: " + filename);
        }
        return texture;
    }

    private static Material[] _LoadMTLFile(string filepath)
    {
        Material currentMaterial = null;
        List<Material> mtrList = new List<Material>();
        FileInfo mtlFileInfo = new FileInfo(filepath);
        StreamReader sReader = mtlFileInfo.OpenText();
        string mtlFileDir = mtlFileInfo.Directory.FullName + Path.DirectorySeparatorChar;

        while (!sReader.EndOfStream)
        {
            string ln = sReader.ReadLine();

            string l = ln.Trim().Replace("  ", " ");
            string[] tokens = l.Split(' ');
            string data = l.Remove(0, l.IndexOf(' ') + 1);

            if (tokens[0] == "newmtl")
            {
                if (currentMaterial != null)
                    mtrList.Add(currentMaterial);

                currentMaterial = new Material(Shader.Find("Standard"));
                currentMaterial.name = data;
            }
            else if (tokens[0] == "map_Kd")
            {
                string texPath = _GetActualPathOrName(mtlFileDir, data);
                if (texPath != null)
                    currentMaterial.SetTexture("_MainTex", _LoadTexture(texPath));
            }
            else if (tokens[0] == "Kd")
            {
                currentMaterial.SetColor("_Color", _ReadColor(tokens));
            }
            else if (tokens[0] == "Ks")
            {
                currentMaterial.SetColor("_SpecColor", _ReadColor(tokens));
            }
        }

        if (currentMaterial != null)
            mtrList.Add(currentMaterial);

        return mtrList.ToArray();
    }

    IEnumerator _LoadOBJFile(String inputPath, LoadOBJCompleteCallback callback, bool updataIsReady)
    {
      //  string filename = Path.GetFileNameWithoutExtension(inputPath);

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Triangle> triangleList = new List<Triangle>();

        // group info
        List<string> groupNames = new List<string>();
        List<string> groupMtrNames = new List<string>();
        List<int> groupFirstTriID = new List<int>();
        List<int> groupLastTriID = new List<int>();

        string curMtrName = "";
        string curGroupName = "default";
        int numGroups = 0, numGroupMtrs = 0;

        Material[] materialPool = null;
        FileInfo OBJFileInfo = new FileInfo(inputPath);
        StreamReader sReader = OBJFileInfo.OpenText();

        int curLines = 0;
        while ( !sReader.EndOfStream )
        {
            string ln = sReader.ReadLine();
            ++curLines;

            if (ln.Length > 0 && ln[0] != '#')
            {
                string l = ln.Trim().Replace("  "," ");
                string[] tokens = l.Split(' ');
                string sData = l.Remove(0, l.IndexOf(' ') + 1);

                //load all material
                if (tokens[0] == "mtllib")
                {
                    string mtrPath = _GetActualPathOrName(OBJFileInfo.Directory.FullName + Path.DirectorySeparatorChar, sData);
                    if (mtrPath != null)
                        materialPool = _LoadMTLFile(mtrPath);

                }
                else if (tokens[0] == "g")
                {
                    curGroupName = sData;
                    if (!groupNames.Contains(curGroupName)) // need ??
                    {
                        // update previous group
                        if (numGroups != 0)
                            groupLastTriID.Add(triangleList.Count - 1);

                        groupNames.Add(curGroupName);
                        groupFirstTriID.Add(triangleList.Count);
                        numGroups = groupNames.Count;
                    }
                }
                else if (tokens[0] == "usemtl")
                {
                    curMtrName = sData;
                }
                else if (tokens[0] == "v")
                {
                    vertices.Add(_ReadVector3(tokens));
                }
                else if (tokens[0] == "vn")
                {
                    normals.Add(_ReadVector3(tokens));
                }
                else if (tokens[0] == "vt")
                {
                    uvs.Add(_ReadVector2(tokens));
                }
                else if (tokens[0] == "f")
                {
                    if ( (tokens.Length - 1) != 3 )
                    {
                        Debug.Log("support obj face with triangle only");
                        continue;
                    }

                    if ( numGroups != numGroupMtrs )
                    {
                        for (int i = numGroupMtrs; i < numGroups; ++i)
                            groupMtrNames.Add(curMtrName);

                        numGroupMtrs = groupMtrNames.Count;
                    }

                    Triangle triangle = new Triangle();
                    triangle.indices = new int[3];

                    for (int i = 1; i <= 3; i++)
                    {
                        string[] elementComps = tokens[i].Split('/');
                        triangle.indices[i - 1] = int.Parse(elementComps[0]) - 1;
                    }
                    triangleList.Add(triangle);
                }
            }

            if ((curLines > 0) && (curLines % 5000 == 0))
                yield return new WaitForEndOfFrame();
        }

        // update last group
        if (numGroups > 0)
            groupLastTriID.Add(triangleList.Count - 1);

        if (groupNames.Count == 0)
            groupNames.Add("default");

        //build objects
        List<Thread> partialObjReaderThreads = new List<Thread>();
        List<PartialObjReader> partialObjReaders = new List<PartialObjReader>();
        int totalGroups = groupNames.Count;
        for (int groupID = 0; groupID < totalGroups; ++groupID)
        {
            string name = groupNames[groupID];
            int startID = groupFirstTriID[groupID];
            int lastID = groupLastTriID[groupID];
            
            GameObject subObject = new GameObject(name);
            subObject.transform.parent = returnObject.transform;
            subObject.transform.localScale = new Vector3(-1, 1, 1);

            PartialObjReader reader = new PartialObjReader();
            reader.name = name;
            reader.subObject = subObject;
            reader.firstTriangleID = startID;
            reader.lastTriangleID = lastID;
            reader.src_triList = triangleList;
            reader.src_vertices = vertices;
            reader.src_normals = normals;
            reader.src_uvs = uvs;

            partialObjReaders.Add(reader);

            Thread thread = new Thread(reader.LoadPartialMesh);
            partialObjReaderThreads.Add(thread);
            thread.Start();

            yield return new WaitForEndOfFrame();
        }

        for (int readerID = 0; readerID < partialObjReaders.Count; ++readerID)
        {
            while (!partialObjReaders[readerID].finished)
                yield return new WaitForEndOfFrame();
        }

        for (int readerID = 0; readerID < partialObjReaders.Count; ++readerID)
        {
            string groupMtrName = groupMtrNames[readerID];
            Mesh mesh = new Mesh();
            mesh.name = partialObjReaders[readerID].subObject.name;
            mesh.vertices = partialObjReaders[readerID].out_Verts;
            mesh.normals = partialObjReaders[readerID].out_Normals;
            mesh.uv = partialObjReaders[readerID].out_UVs;
            mesh.SetTriangles(partialObjReaders[readerID].out_Indices, 0);
            //mesh.RecalculateBounds();

            MeshFilter mf = partialObjReaders[readerID].subObject.AddComponent<MeshFilter>();
            MeshRenderer mr = partialObjReaders[readerID].subObject.AddComponent<MeshRenderer>();

            if (materialPool != null)
                mr.material = Array.Find(materialPool, x => x.name == groupMtrName);
            if (mr.material == null)
                mr.material = new Material(Shader.Find("Standard"));
            mf.mesh = mesh;

            yield return new WaitForEndOfFrame();
        }

        sReader.Close();
        //loadCompelete = true;
        returnObject.SetActive(true);
        if (callback != null) callback(returnObject, updataIsReady);
        yield return null;
    }

    private class PartialObjReader
    {
        public GameObject subObject;
        public string name;
        public int firstTriangleID;
        public int lastTriangleID;
        
        public List<Triangle> src_triList;
        public List<Vector3> src_vertices;
        public List<Vector3> src_normals;
        public List<Vector2> src_uvs;

        public Vector3[] out_Verts;
        public Vector3[] out_Normals;
        public Vector2[] out_UVs;
        public int[] out_Indices;
        
        public bool finished = false;

        public void LoadPartialMesh()
        {                        
            List<Vector3> vertList = new List<Vector3>();
            List<Vector3> normalList = new List<Vector3>();
            List<Vector2> uvList = new List<Vector2>();
            Dictionary<int, int> remapTable = new Dictionary<int, int>();
            int totalTri = (lastTriangleID - firstTriangleID) + 1;

            List<Triangle> triangles = src_triList.GetRange(firstTriangleID, totalTri);
            if (totalTri > 0)
            {
                out_Indices = new int[totalTri * 3];

                for (int i = 0; i < totalTri; i++)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        int idx = triangles[i].indices[j];
                        if (remapTable.ContainsKey(idx))
                        {
                            out_Indices[i * 3 + j] = remapTable[idx];
                        }
                        else
                        {
                            vertList.Add(src_vertices[idx]);
                            if (src_normals.Count > 0) normalList.Add(src_normals[idx]);
                            if (src_uvs.Count > 0) uvList.Add(src_uvs[idx]);
                            remapTable[idx] = vertList.Count - 1;
                            out_Indices[i * 3 + j] = remapTable[idx];
                        }
                    }
                }
            }

            //get returned data
            out_Verts = vertList.ToArray();
            if (normalList.Count > 0) out_Normals = normalList.ToArray();
            if (uvList.Count > 0) out_UVs = uvList.ToArray();
            finished = true;       
        }
    }

}