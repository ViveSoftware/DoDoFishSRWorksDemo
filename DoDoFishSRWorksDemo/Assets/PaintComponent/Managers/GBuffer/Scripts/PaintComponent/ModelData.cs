using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    public class ModelData
    {
        private MeshFilter meshFilter;
        private Renderer meshRenderer;
        private Mesh mesh;
        private int numSubMeshes;
        private int numTotalVertices = 0;
        private int numTotalTriangles = 0;

        public MeshFilter ModelGeometry
        {
            get { return meshFilter; }
        }

        public Renderer ModelRenderer
        {
            get { return meshRenderer; }
        }

        public Material ModelMaterial
        {
            get { return meshRenderer.material; }
        }

        public ModelData(GameObject modelObj, bool withoutMeshFilter = false)
        {
            if (!withoutMeshFilter)
            {
                meshFilter = modelObj.GetComponentInChildren<MeshFilter>();
                if (meshFilter == null)
                {
                    mesh = modelObj.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
                }
                else
                {
                    mesh = meshFilter.mesh;
                }
                numSubMeshes = mesh.subMeshCount;
                numTotalVertices = mesh.vertexCount;
                numTotalTriangles = mesh.triangles.Length / 3;
            }
            meshRenderer = modelObj.GetComponentInChildren<Renderer>();
        }

        public void PrintModelInformation()
        {
            Debug.Log("[TextureShop] Mesh Information: ");
            Debug.Log("[TextureShop] START-----------------------------------------");
            Debug.Log("[TextureShop] Total Vertices: " + numTotalVertices);
            Debug.Log("[TextureShop] Total Triangles: " + numTotalTriangles);
            Debug.Log("[TextureShop] Num. SubMeshes: " + numSubMeshes);
            Debug.Log("[TextureShop] END-------------------------------------------");
        }
    }
}