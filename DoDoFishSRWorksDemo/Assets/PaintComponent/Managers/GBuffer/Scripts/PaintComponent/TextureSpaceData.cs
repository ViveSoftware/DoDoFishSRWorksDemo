using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    public class TextureSpaceData
    {
        private const int DefaultTexSize = 2048;

        private RenderTexture albedoTexture = null;

        private Mesh textureSpaceMesh = null;
        private List<Vector3> textureSpaceVertices = null;
        private List<Vector2> textureSpaceUVs = null;
        private List<int> textureSpaceIndices = null;

        public RenderTexture AlbedoTexture
        {
            get { return albedoTexture; }
            set { albedoTexture = value; }
        }

        public Mesh TextureSpaceMesh
        {
            get { return textureSpaceMesh; }
        }

        public List<Vector3> TextureSpaceVertices
        {
            get { return textureSpaceVertices; }
        }

        public List<Vector2> TextureSpaceUVs
        {
            get { return textureSpaceUVs; }
        }

        public List<int> TextureSpaceIndices
        {
            get { return textureSpaceIndices; }
        }

        // ---------------------------------------------------
        // Desc: Constructor.
        // ---------------------------------------------------
        public TextureSpaceData(string objName)
        {
            textureSpaceMesh = new Mesh();
            textureSpaceVertices = new List<Vector3>();
            textureSpaceUVs = new List<Vector2>();
            textureSpaceIndices = new List<int>();

            albedoTexture = new RenderTexture(DefaultTexSize, DefaultTexSize, 0, RenderTextureFormat.ARGB32);
        }

        public TextureSpaceData(string objName, GBufferResolution resolution)
        {
            textureSpaceMesh = new Mesh();
            textureSpaceVertices = new List<Vector3>();
            textureSpaceUVs = new List<Vector2>();
            textureSpaceIndices = new List<int>();

            albedoTexture = new RenderTexture((int)resolution, (int)resolution, 0, RenderTextureFormat.ARGB32);
        }

        // ---------------------------------------------------
        // Desc: Clear texture space data.
        // ---------------------------------------------------
        public void ClearTextureSpaceData()
        {
            textureSpaceMesh.Clear();
            textureSpaceVertices.Clear();
            textureSpaceUVs.Clear();
            textureSpaceIndices.Clear();
        }

        public void AddOneQuad(Vector3 uv0, Vector3 uv1, Vector3 uv2, Vector3 uv3)
        {
            int numV = textureSpaceVertices.Count;
            textureSpaceVertices.Add(uv0);
            textureSpaceVertices.Add(uv1);
            textureSpaceVertices.Add(uv2);
            textureSpaceVertices.Add(uv3);

            textureSpaceUVs.Add(new Vector2(0.0f, 0.0f));
            textureSpaceUVs.Add(new Vector2(0.0f, 1.0f));
            textureSpaceUVs.Add(new Vector2(1.0f, 1.0f));
            textureSpaceUVs.Add(new Vector2(1.0f, 0.0f));

            textureSpaceIndices.Add(numV + 0);
            textureSpaceIndices.Add(numV + 1);
            textureSpaceIndices.Add(numV + 2);
            textureSpaceIndices.Add(numV + 0);
            textureSpaceIndices.Add(numV + 2);
            textureSpaceIndices.Add(numV + 3);
        }
    }
}
