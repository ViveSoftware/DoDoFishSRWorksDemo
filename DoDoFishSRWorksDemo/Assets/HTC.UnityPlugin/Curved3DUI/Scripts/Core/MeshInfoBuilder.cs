//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HTC.UnityPlugin.Curved3DUI
{
    // build MeshInfo from VertexHelper
    public partial class MeshInfo
    {
        private class SimpleVert
        {
            public static HTC.UnityPlugin.Utility.ObjectPool<SimpleVert> pool = new HTC.UnityPlugin.Utility.ObjectPool<SimpleVert>(() => new SimpleVert());

            public Vector3 pos;
            public Vector2 uv0;

            public override bool Equals(object obj)
            {
                if (obj is SimpleVert)
                {
                    var sv = (SimpleVert)obj;
                    return
                        Mathf.Approximately(this.pos.x, sv.pos.x) &&
                        Mathf.Approximately(this.pos.y, sv.pos.y) &&
                        Mathf.Approximately(this.pos.z, sv.pos.z) &&
                        Mathf.Approximately(this.uv0.x, sv.uv0.x) &&
                        Mathf.Approximately(this.uv0.y, sv.uv0.y);
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return pos.GetHashCode() << 2 ^ uv0.GetHashCode();
            }
        }

        private static readonly Dictionary<SimpleVert, int> mergedVertexTable = new Dictionary<SimpleVert, int>();
        private static readonly List<SimpleVert> mergedVertexList = new List<SimpleVert>();
        private static readonly List<UIVertex> splitVertexList = new List<UIVertex>();

        public void CopyFrom(VertexHelper vertexHelper)
        {
            if (vertexHelper.currentVertCount == 0) { return; }

            vertexHelper.GetUIVertexStream(splitVertexList);

            // retrieve triangle index, vertexHelper doesn't provide interface to retrieve triangle index
            // FIXME: more effeciant way?

            for (int i = 0; i < splitVertexList.Count; i += 3)
            {
                if (splitVertexList[i + 0].position == splitVertexList[i + 1].position ||
                    splitVertexList[i + 1].position == splitVertexList[i + 2].position ||
                    splitVertexList[i + 2].position == splitVertexList[i + 0].position)
                {
                    // not a triangle, skip it
                    continue;
                }

                for (int j = 0; j < 3; ++j)
                {
                    int index;

                    var vert = SimpleVert.pool.Get();
                    vert.pos = splitVertexList[i + j].position;
                    vert.uv0 = splitVertexList[i + j].uv0;

                    if (!mergedVertexTable.TryGetValue(vert, out index))
                    {
                        index = positions.Count;
                        mergedVertexTable.Add(vert, index);
                        mergedVertexList.Add(vert);

                        positions.Add(splitVertexList[i + j].position);
                        colors.Add(splitVertexList[i + j].color);
                        uv0s.Add(splitVertexList[i + j].uv0);
                        uv1s.Add(splitVertexList[i + j].uv1);
                        normals.Add(splitVertexList[i + j].normal);
                        tangents.Add(splitVertexList[i + j].tangent);
                    }
                    else
                    {
                        SimpleVert.pool.Release(vert);
                    }

                    indices.Add(index);
                }
            }

            for (int i = mergedVertexList.Count - 1; i >= 0; --i)
            {
                SimpleVert.pool.Release(mergedVertexList[i]);
            }

            mergedVertexTable.Clear();
            mergedVertexList.Clear();
            splitVertexList.Clear();
        }

        public void CopyFrom(MeshInfo srcMeshInfo)
        {
            positions.AddRange(srcMeshInfo.positions);
            normals.AddRange(srcMeshInfo.normals);
            tangents.AddRange(srcMeshInfo.tangents);
            colors.AddRange(srcMeshInfo.colors);
            uv0s.AddRange(srcMeshInfo.uv0s);
            uv1s.AddRange(srcMeshInfo.uv1s);
            indices.AddRange(srcMeshInfo.indices);
        }
    }
}