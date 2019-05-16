//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace HTC.UnityPlugin.Curved3DUI
{
    // represent Mesh in List form
    public partial class MeshInfo
    {
        public static readonly ObjectPool<MeshInfo> pool = new ObjectPool<MeshInfo>(() => new MeshInfo(), null, e => e.Release());

        private MeshInfo m_reference;

        private List<Vector3> m_positions;
        private List<Vector3> m_normals;
        private List<Vector4> m_tangents;

        private List<Color32> m_colors;
        private List<Vector2> m_uv0s;
        private List<Vector2> m_uv1s;
        private List<int> m_indices;

        public List<Vector3> positions { get { return m_positions; } }
        public List<Vector3> normals { get { return m_normals; } }
        public List<Vector4> tangents { get { return m_tangents; } }

        public List<Color32> colors { get { return m_reference == null ? m_colors : m_reference.colors; } }
        public List<Vector2> uv0s { get { return m_reference == null ? m_uv0s : m_reference.uv0s; } }
        public List<Vector2> uv1s { get { return m_reference == null ? m_uv1s : m_reference.uv1s; } }
        public List<int> indices { get { return m_reference == null ? m_indices : m_reference.indices; } }

        private static void InitializeList<T>(ref List<T> target)
        {
            if (target == null) { target = ListPool<T>.Get(); }
            else { target.Clear(); }
        }

        private static void ReleaseList<T>(ref List<T> target)
        {
            if (target != null) { ListPool<T>.Release(target); target = null; }
        }

        public void Initialize(MeshInfo refMeshInfo = null)
        {
            InitializeList(ref m_positions);
            InitializeList(ref m_normals);
            InitializeList(ref m_tangents);
            if (refMeshInfo == null)
            {
                InitializeList(ref m_colors);
                InitializeList(ref m_uv0s);
                InitializeList(ref m_uv1s);
                InitializeList(ref m_indices);
            }
            else
            {
                m_reference = refMeshInfo;
                ReleaseList(ref m_colors);
                ReleaseList(ref m_uv0s);
                ReleaseList(ref m_uv1s);
                ReleaseList(ref m_indices);
            }
        }

        public void Release()
        {
            m_reference = null;
            ReleaseList(ref m_positions);
            ReleaseList(ref m_normals);
            ReleaseList(ref m_tangents);
            ReleaseList(ref m_colors);
            ReleaseList(ref m_uv0s);
            ReleaseList(ref m_uv1s);
            ReleaseList(ref m_indices);
        }

        public void Fill(Mesh mesh, bool backFace = false)
        {
            if (backFace)
            {
                var temp_mesh = pool.Get();
                temp_mesh.Initialize();
                temp_mesh.CopyFrom(this);

                // copy again for back face vertice
                temp_mesh.positions.AddRange(positions);
                temp_mesh.tangents.AddRange(tangents);
                temp_mesh.colors.AddRange(colors);
                temp_mesh.uv0s.AddRange(uv0s);
                temp_mesh.uv1s.AddRange(uv1s);

                if (temp_mesh.normals.Capacity < temp_mesh.normals.Count * 2)
                {
                    temp_mesh.normals.Capacity = temp_mesh.normals.Count * 2;
                }
                for (int i = 0, imax = normals.Count; i < imax; ++i)
                {
                    // flip normals
                    temp_mesh.normals.Add(-normals[i]);
                }

                if (temp_mesh.indices.Capacity < temp_mesh.indices.Count * 2)
                {
                    temp_mesh.indices.Capacity = temp_mesh.indices.Count * 2;
                }
                for (int i = 0, imax = indices.Count - 1; i < imax; i += 3)
                {
                    // add reversed triangles
                    temp_mesh.indices.Add(positions.Count + indices[i]);
                    temp_mesh.indices.Add(positions.Count + indices[i + 2]);
                    temp_mesh.indices.Add(positions.Count + indices[i + 1]);
                }

                temp_mesh.Fill(mesh, false);
                pool.Release(temp_mesh);
            }
            else
            {
                mesh.Clear();
                mesh.SetVertices(positions);
                mesh.SetColors(colors);
                mesh.SetUVs(0, uv0s);
                mesh.SetUVs(1, uv1s);
                mesh.SetNormals(normals);
                mesh.SetTangents(tangents);
                mesh.SetTriangles(indices, 0);
                mesh.RecalculateBounds();
            }
        }
    }
}