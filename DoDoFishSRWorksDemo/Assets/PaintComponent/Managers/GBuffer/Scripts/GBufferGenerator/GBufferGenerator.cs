using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace PaintVR
{
    public class GBufferGenerator : MonoBehaviour
    {
        // Class Definitions.
        public class UVMesh
        {
            public List<Vector3> uvMeshVertices = null;
            public List<Color> uvMeshColors = null;
            public List<int> uvMeshIndices = null;

            public UVMesh()
            {
                uvMeshVertices = new List<Vector3>();
                uvMeshColors = new List<Color>();
                uvMeshIndices = new List<int>();
            }

            public UVMesh(UVMesh uvMesh)
            {
                uvMeshVertices = new List<Vector3>(uvMesh.uvMeshVertices.Count);
                for (int i = 0 ; i < uvMesh.uvMeshVertices.Count ; ++i)
                {
                    uvMeshVertices.Add(uvMesh.uvMeshVertices[i]);
                }

                uvMeshColors = new List<Color>(uvMesh.uvMeshColors.Count);
                for (int i = 0 ; i < uvMesh.uvMeshColors.Count ; ++i)
                {
                    uvMeshColors.Add(uvMesh.uvMeshColors[i]);
                }

                uvMeshIndices = new List<int>(uvMesh.uvMeshIndices.Count);
                for (int i = 0 ; i < uvMesh.uvMeshIndices.Count ; ++i)
                {
                    uvMeshIndices.Add(uvMesh.uvMeshIndices[i]);
                }
            }

            public void Clear()
            {
                uvMeshVertices.Clear();
                uvMeshColors.Clear();
                uvMeshIndices.Clear();
            }
        }

        // Public Class Data.
        // ---------------------------------------------------------------
        [Header("GBuffer Setting")]
        public GBufferFormat gBufferFormat = GBufferFormat.Half;

        // Private Class Data.
        // ---------------------------------------------------------------
        private readonly int GBufferNumPixelSamples = 1;
        private readonly FilterMode GBufferFilterMode = FilterMode.Point;

        [Header("Specified GameObject")]
        [SerializeField] private Camera posMapCam = null;
        [SerializeField] private Camera normalMapCam = null;
        public GameObject targetObject = null;

        [Header("Shader")]
        [SerializeField] private Shader genGBufferShader = null;
        [SerializeField] private Shader genBinaryMaskShader = null;
        [SerializeField] private Shader dilateMaskShader = null;
        [SerializeField] private Shader refineGBufferShader = null;

        public int DilateCount;

        private bool isGBufferReady = false;
        public bool GBufferReady
        {
            get { return isGBufferReady; }
        }
        private Material genGBufferMaterial = null;
        private Material genBinaryMaskMaterial = null;
        private Material dilateMaskMaterial = null;
        private Material refineGBufferMaterial = null;
        private Mesh[] posDataMeshes = null;
        private Mesh[] normalDataMeshes = null;

        private RenderTexture positionBufferRT = null;
        private RenderTexture normalBufferRT = null;
        private RenderTexture binaryMaskRT = null;
        private RenderTexture dilatedMaskRT = null;

        private RenderTexture positionMapTP = null;
        private RenderTexture normalMapTP = null;

        private RenderTexture positionMap = null;
        private RenderTexture normalMap = null;

        // Enable for debugging.
        // [SerializeField] private RenderTexture binaryMask = null;
        // [SerializeField] private RenderTexture dilatedBinaryMask = null;
        // [SerializeField] private RenderTexture dilatedPosMap = null;
        // [SerializeField] private RenderTexture dilatedNormalMap = null;

        // C# Properties for Public Access.
        // ---------------------------------------------------------------
        public RenderTexture PositionMap
        {
            get { return positionMap; }
        }

        public RenderTexture NormalMap
        {
            get { return normalMap; }
        }

        // Unity Fixed Methods.
        // ---------------------------------------------------------------
        public void GBufferGeneratorInit()
        {
            // Prepare GBuffer.
            isGBufferReady = true;

            // Generate render targets.
            int texSize = (int)targetObject.GetComponent<PBSPaintableObject>().resolution;
            RenderTextureFormat texFormat = GetGBufferFormat(gBufferFormat);
            isGBufferReady &= CreateRenderTextures(texSize, texFormat);

            // Generate render-to-texture materials.
            isGBufferReady &= CreateMaterials();

            // Generate UV meshes.
            isGBufferReady &= GenerateGBufferMeshes();

            UpdateGBufferToTarget();
        }

        void Update()
        {
        }

        // Public Class Methods.
        // ---------------------------------------------------------------
        public bool SaveGBufferToFile(string fileName, bool isFullPath = false, GBufferResolution gBufferResolution = GBufferResolution.High)
        {
            if (!isGBufferReady)
            {
                Debug.LogError("[GBufferGenerator::SaveGBufferToFile] Sorry ... The GBuffer is not ready yet ... ");
                return false;
            }

            Debug.Log("[GBufferGenerator::SaveGBufferToFile] Save GBuffer ...");
            TextureFormat format = (gBufferFormat == GBufferFormat.Half) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;

            // Read buffer from GPU.
            Texture2D positionTex = FileUtil.RenderTextureToTexture2D(positionMap, format);
            Texture2D normalTex = FileUtil.RenderTextureToTexture2D(normalMap, format);

            // Prepare data for serization.
            GBufferTexData gBuffer = new GBufferTexData();
            gBuffer.size = GetGBufferSize(gBufferResolution);
            FileUtil.ColorArrayToFloatArray(positionTex.GetPixels(), out gBuffer.positionData);
            FileUtil.ColorArrayToFloatArray(normalTex.GetPixels(), out gBuffer.normalData);

            // Save GBuffer data to file.

            string path;

            if (isFullPath)
            {
                path = fileName;
            }
            else
            {
                path = Application.dataPath + "/" + fileName + ".gbuf";
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Create(path);
            binaryFormatter.Serialize(fileStream, gBuffer);
            fileStream.Close();

            Debug.Log("Save GBuffer To Path: " + path + " Finished!");

            return true;
        }

        public static int GetGBufferSize(GBufferResolution resolution)
        {
            switch (resolution)
            {
                case GBufferResolution.Low:
                    return 512;
                case GBufferResolution.Medium:
                    return 1024;
                case GBufferResolution.High:
                    return 2048;
                case GBufferResolution.VeryHigh:
                    return 4096;
                default:
                    return 1024;    
            }
        }

        public static RenderTextureFormat GetGBufferFormat(GBufferFormat format)
        {
            switch (format)
            {
                case GBufferFormat.Half:
                    return RenderTextureFormat.ARGBHalf;
                case GBufferFormat.Float:
                    return RenderTextureFormat.ARGBFloat;
                default:
                    return RenderTextureFormat.ARGBHalf;
            }
        }

        // Private Class Methods.
        // ---------------------------------------------------------------
        private void RenderGBuffer()
        {
            // Setup render targets.
            posMapCam.targetTexture = positionBufferRT;
            normalMapCam.targetTexture = normalBufferRT;

            // Draw UV meshes.
            for (int i = 0 ; i <posDataMeshes.Length; ++i)
            {
                Graphics.DrawMesh(posDataMeshes[i], Vector3.zero, Quaternion.identity, genGBufferMaterial, PaintComponentDefine.PositionMapDataLayer);
                Graphics.DrawMesh(normalDataMeshes[i], Vector3.zero, Quaternion.identity, genGBufferMaterial, PaintComponentDefine.NormalMapDataLayer);
            }
            
            for (int i = 0; i < DilateCount; i++)
            {
                // Generate binary mask.
                Graphics.Blit(positionBufferRT, binaryMaskRT, genBinaryMaskMaterial);

                // Generate dilated binary mask.
                Graphics.Blit(binaryMaskRT, dilatedMaskRT, dilateMaskMaterial);

                // Copy texture data to temporary texture buffer
                Graphics.Blit(positionBufferRT, positionMapTP);
                Graphics.Blit(normalBufferRT, normalMapTP);

                // Refine pos map.
                refineGBufferMaterial.SetTexture("_OrigMask", binaryMaskRT);
                refineGBufferMaterial.SetTexture("_DilatedMask", dilatedMaskRT);
                Graphics.Blit(positionMapTP, positionBufferRT, refineGBufferMaterial);
                Graphics.Blit(normalMapTP, normalBufferRT, refineGBufferMaterial);
            }

            // Copy texture data to temporary texture buffer
            Graphics.Blit(positionBufferRT, positionMap);
            Graphics.Blit(normalBufferRT, normalMap);
        }

        private bool CreateMaterials()
        {
            if ((genGBufferShader == null) || (genBinaryMaskShader == null) ||
                (dilateMaskShader == null) || (refineGBufferShader == null))
            {
                Debug.LogError("[GBufferGenerator::CreateMaterials] Create materials failed");
                return false;
            }

            genGBufferMaterial = new Material(genGBufferShader);
            genBinaryMaskMaterial = new Material(genBinaryMaskShader);
            dilateMaskMaterial = new Material(dilateMaskShader);
            refineGBufferMaterial = new Material(refineGBufferShader);
            return true;
        }

        private bool CreateRenderTextures(int texSize, RenderTextureFormat texFormat)
        {
            // Create position buffer.
            positionBufferRT = new RenderTexture(texSize, texSize, 0, texFormat);
            positionBufferRT.antiAliasing = GBufferNumPixelSamples;
            positionBufferRT.filterMode = GBufferFilterMode;
        
            // Create normal buffer.
            normalBufferRT = new RenderTexture(texSize, texSize, 0, texFormat);
            normalBufferRT.antiAliasing = GBufferNumPixelSamples;
            normalBufferRT.filterMode = GBufferFilterMode;

            // Create binary mask.
            binaryMaskRT = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.RHalf);
            binaryMaskRT.antiAliasing = GBufferNumPixelSamples;
            binaryMaskRT.filterMode = GBufferFilterMode;

            // Create dilation buffer.
            dilatedMaskRT = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.RHalf);
            dilatedMaskRT.antiAliasing = GBufferNumPixelSamples;
            dilatedMaskRT.filterMode = GBufferFilterMode;

            // Create final position map.
            positionMap = new RenderTexture(texSize, texSize, 0, texFormat);
            positionMap.antiAliasing = GBufferNumPixelSamples;
            positionMap.filterMode = FilterMode.Bilinear;

            // Create final normal map.
            normalMap = new RenderTexture(texSize, texSize, 0, texFormat);
            normalMap.antiAliasing = GBufferNumPixelSamples;
            normalMap.filterMode = FilterMode.Bilinear;

            // Create temporary position map.
            positionMapTP = new RenderTexture(texSize, texSize, 0, texFormat);
            positionMapTP.antiAliasing = GBufferNumPixelSamples;
            positionMapTP.filterMode = FilterMode.Bilinear;

            // Create temporary normal map.
            normalMapTP = new RenderTexture(texSize, texSize, 0, texFormat);
            normalMapTP.antiAliasing = GBufferNumPixelSamples;
            normalMapTP.filterMode = FilterMode.Bilinear;

            if ((positionBufferRT == null) || (normalBufferRT == null) ||
                (binaryMaskRT == null) || (dilatedMaskRT == null) || (positionMapTP == null) || (normalMapTP == null))
            {
                Debug.LogError("[GBufferGenerator::CreateRenderTextures] Create render targets failed");
                return false;
            }
            return true;
        }

        private bool GenerateGBufferMeshes()
        {
            Mesh mesh = GetMesh();
            if (mesh == null)
            {
                Debug.LogError("[GBufferGenerator::GenerateGBufferMeshes] Cannot find mesh");
                return false;
            }
            CreateUVDataMesh(mesh, mesh.vertices, out posDataMeshes);
            CreateUVDataMesh(mesh, mesh.normals, out normalDataMeshes);
            return true;
        }

        private void CreateUVDataMesh(Mesh inputMesh, Vector3[] insertData, out Mesh[] outputMeshes)
        {
            // Get original mesh data.
            Vector2[] uvs = inputMesh.uv;
            int[] indices = inputMesh.triangles;
            int numTriangles = indices.Length / 3;

            // Create a uv mesh.
            // Note: the unity has limitation for vertex size (65535), so we need 
            //       to create multiple meshes if needed.
            const int UNITY_MAX_VERTICES = 65535;
            int numMeshes = Mathf.CeilToInt((float)indices.Length / (float)UNITY_MAX_VERTICES);
            outputMeshes = new Mesh[numMeshes];

            List<UVMesh> meshList = new List<UVMesh>();
            UVMesh uvMesh = new UVMesh();
            int verticesInMesh = 0;
            int trianglesInMesh = 0;
            for (int triIndex = 0 ; triIndex < numTriangles ; ++triIndex)
            {
                // If verticesInMesh == 0, create a new submesh.
                if (verticesInMesh == 0)
                {
                    uvMesh.Clear();
                }

                int index = 3 * triIndex;
                int tid0 = indices[index + 0];
                int tid1 = indices[index + 1];
                int tid2 = indices[index + 2];

                uvMesh.uvMeshVertices.Add(new Vector3(uvs[tid0].x, uvs[tid0].y, 1.0f));
                uvMesh.uvMeshVertices.Add(new Vector3(uvs[tid1].x, uvs[tid1].y, 1.0f));
                uvMesh.uvMeshVertices.Add(new Vector3(uvs[tid2].x, uvs[tid2].y, 1.0f));

                uvMesh.uvMeshColors.Add(new Color(insertData[tid0].x, insertData[tid0].y, insertData[tid0].z, 1.0f));
                uvMesh.uvMeshColors.Add(new Color(insertData[tid1].x, insertData[tid1].y, insertData[tid1].z, 1.0f));
                uvMesh.uvMeshColors.Add(new Color(insertData[tid2].x, insertData[tid2].y, insertData[tid2].z, 1.0f));

                uvMesh.uvMeshIndices.Add(3 * trianglesInMesh + 0);
                uvMesh.uvMeshIndices.Add(3 * trianglesInMesh + 1);
                uvMesh.uvMeshIndices.Add(3 * trianglesInMesh + 2);

                verticesInMesh += 3;
                trianglesInMesh++;

                // If trianglesInSubMesh == UNITY_MAX_VERTICES or there is no further triangles, push the submesh.
                if (verticesInMesh == UNITY_MAX_VERTICES || triIndex == numTriangles - 1)
                {
                    UVMesh newMesh = new UVMesh(uvMesh);
                    meshList.Add(newMesh);
                    verticesInMesh = 0;
                    trianglesInMesh = 0;
                }
            }

            for (int i = 0 ; i < numMeshes ; ++i)
            {
                outputMeshes[i] = new Mesh();
                outputMeshes[i].vertices = meshList[i].uvMeshVertices.ToArray();
                outputMeshes[i].colors = meshList[i].uvMeshColors.ToArray();
                outputMeshes[i].triangles = meshList[i].uvMeshIndices.ToArray();
            }
        }

        private Mesh GetMesh()
        {
            Mesh mesh = null;
            MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                mesh = meshFilter.mesh;
            }
            else
            {
                SkinnedMeshRenderer meshRenderer = targetObject.GetComponent<SkinnedMeshRenderer>();
                mesh = meshRenderer.sharedMesh;
            }
            return mesh;
        }

        private void UpdateGBufferToTarget()
        {
            RenderGBuffer();
            posMapCam.Render();
            normalMapCam.Render();

            PBSPaintableObject paintableObject = targetObject.GetComponent<PBSPaintableObject>();
            TextureFormat format = (gBufferFormat == GBufferFormat.Half) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            paintableObject.PositionMap = FileUtil.RenderTextureToTexture2D(posMapCam.targetTexture, format);
            paintableObject.NormalMap = FileUtil.RenderTextureToTexture2D(normalMapCam.targetTexture, format);
        }

        private void Awake()
        {
            //if(targetObject != null)
            //    GBufferGeneratorInit();
        }

    }
}