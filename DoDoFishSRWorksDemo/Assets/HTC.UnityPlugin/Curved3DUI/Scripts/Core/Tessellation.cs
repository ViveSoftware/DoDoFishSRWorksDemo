//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace HTC.UnityPlugin.Curved3DUI
{
    public static partial class Curved3D
    {
        private struct MidVert
        {
            public int Index;
            public int Opposing;
            public int Right;
            public int Left;
            public bool NeedCutAgain;
            public float Length;
        }

        private static readonly int[] s_vi = new int[3];
        private static readonly MidVert[] s_midVerts = new MidVert[3];
        private static readonly Dictionary<uint, int> s_indiceAssociate = new Dictionary<uint, int>();

        // Tessellate srcMesh into destMesh
        public static void Tessellate(MeshInfo mesh, float maxFragSizeSqr)
        {
            var largeTri = ListPool<int>.Get();
            var largeTriNext = ListPool<int>.Get();

            for (int i = 0, imax = mesh.indices.Count; i < imax; i += 3)
            {
                largeTri.Add(i);
            }

            while (largeTri.Count > 0)
            {
                for (int t = 0, imax = largeTri.Count; t < imax; ++t)
                {
                    var triNum = largeTri[t];
                    var populatedCount = 0;
                    var needCutAgain = false;

                    s_vi[0] = mesh.indices[triNum + 0];
                    s_vi[1] = mesh.indices[triNum + 1];
                    s_vi[2] = mesh.indices[triNum + 2];

                    if (mesh.positions.Count < MAX_VERTEX_COUNT && NeedInterpolate(mesh.positions[s_vi[0]], mesh.positions[s_vi[1]], maxFragSizeSqr, out needCutAgain, out s_midVerts[populatedCount].Length))
                    {
                        s_midVerts[populatedCount].Index = GetInterpolate(mesh, s_indiceAssociate, s_vi[0], s_vi[1]);
                        s_midVerts[populatedCount].Opposing = s_vi[2];
                        s_midVerts[populatedCount].Right = s_vi[0];
                        s_midVerts[populatedCount].Left = s_vi[1];
                        s_midVerts[populatedCount].NeedCutAgain = needCutAgain;
                        ++populatedCount;
                    }

                    if (mesh.positions.Count < MAX_VERTEX_COUNT && NeedInterpolate(mesh.positions[s_vi[1]], mesh.positions[s_vi[2]], maxFragSizeSqr, out needCutAgain, out s_midVerts[populatedCount].Length))
                    {
                        s_midVerts[populatedCount].Index = GetInterpolate(mesh, s_indiceAssociate, s_vi[1], s_vi[2]);
                        s_midVerts[populatedCount].Opposing = s_vi[0];
                        s_midVerts[populatedCount].Right = s_vi[1];
                        s_midVerts[populatedCount].Left = s_vi[2];
                        s_midVerts[populatedCount].NeedCutAgain = needCutAgain;
                        ++populatedCount;
                    }

                    if (mesh.positions.Count < MAX_VERTEX_COUNT && NeedInterpolate(mesh.positions[s_vi[2]], mesh.positions[s_vi[0]], maxFragSizeSqr, out needCutAgain, out s_midVerts[populatedCount].Length))
                    {
                        s_midVerts[populatedCount].Index = GetInterpolate(mesh, s_indiceAssociate, s_vi[2], s_vi[0]);
                        s_midVerts[populatedCount].Opposing = s_vi[1];
                        s_midVerts[populatedCount].Right = s_vi[2];
                        s_midVerts[populatedCount].Left = s_vi[0];
                        s_midVerts[populatedCount].NeedCutAgain = needCutAgain;
                        ++populatedCount;
                    }

                    switch (populatedCount)
                    {
                        case 1:
                            mesh.indices[triNum + 0] = s_midVerts[0].Index;
                            mesh.indices[triNum + 1] = s_midVerts[0].Opposing;
                            mesh.indices[triNum + 2] = s_midVerts[0].Right;

                            mesh.indices.Add(s_midVerts[0].Index);
                            mesh.indices.Add(s_midVerts[0].Left);
                            mesh.indices.Add(s_midVerts[0].Opposing);

                            if (s_midVerts[0].NeedCutAgain)
                            {
                                largeTriNext.Add(triNum);
                                largeTriNext.Add(mesh.indices.Count - 3);
                            }

                            break;
                        case 2:
                            if (s_midVerts[0].Right == s_midVerts[1].Left)
                            {
                                mesh.indices[triNum + 0] = s_midVerts[0].Right;
                                mesh.indices[triNum + 1] = s_midVerts[0].Index;
                                mesh.indices[triNum + 2] = s_midVerts[1].Index;

                                if (s_midVerts[0].Length > s_midVerts[1].Length)
                                {
                                    mesh.indices.Add(s_midVerts[1].Index);
                                    mesh.indices.Add(s_midVerts[0].Index);
                                    mesh.indices.Add(s_midVerts[0].Opposing);

                                    mesh.indices.Add(s_midVerts[1].Opposing);
                                    mesh.indices.Add(s_midVerts[0].Opposing);
                                    mesh.indices.Add(s_midVerts[0].Index);
                                }
                                else
                                {
                                    mesh.indices.Add(s_midVerts[1].Index);
                                    mesh.indices.Add(s_midVerts[0].Index);
                                    mesh.indices.Add(s_midVerts[1].Opposing);

                                    mesh.indices.Add(s_midVerts[1].Opposing);
                                    mesh.indices.Add(s_midVerts[0].Opposing);
                                    mesh.indices.Add(s_midVerts[1].Index);
                                }
                            }
                            else// if (midVerts[0].Left == midVerts[1].Right)
                            {
                                mesh.indices[triNum + 0] = s_midVerts[1].Right;
                                mesh.indices[triNum + 1] = s_midVerts[1].Index;
                                mesh.indices[triNum + 2] = s_midVerts[0].Index;

                                if (s_midVerts[0].Length > s_midVerts[1].Length)
                                {
                                    mesh.indices.Add(s_midVerts[0].Index);
                                    mesh.indices.Add(s_midVerts[1].Index);
                                    mesh.indices.Add(s_midVerts[0].Opposing);

                                    mesh.indices.Add(s_midVerts[0].Opposing);
                                    mesh.indices.Add(s_midVerts[1].Opposing);
                                    mesh.indices.Add(s_midVerts[0].Index);
                                }
                                else
                                {
                                    mesh.indices.Add(s_midVerts[0].Index);
                                    mesh.indices.Add(s_midVerts[1].Index);
                                    mesh.indices.Add(s_midVerts[1].Opposing);

                                    mesh.indices.Add(s_midVerts[0].Opposing);
                                    mesh.indices.Add(s_midVerts[1].Opposing);
                                    mesh.indices.Add(s_midVerts[1].Index);
                                }
                            }

                            if (s_midVerts[0].NeedCutAgain || s_midVerts[1].NeedCutAgain)
                            {
                                largeTriNext.Add(triNum);
                                largeTriNext.Add(mesh.indices.Count - 6);
                                largeTriNext.Add(mesh.indices.Count - 3);
                            }

                            break;
                        case 3:
                            mesh.indices[triNum + 0] = s_midVerts[0].Index;
                            mesh.indices[triNum + 1] = s_midVerts[1].Index;
                            mesh.indices[triNum + 2] = s_midVerts[2].Index;

                            mesh.indices.Add(s_midVerts[0].Index);
                            mesh.indices.Add(s_midVerts[2].Index);
                            mesh.indices.Add(s_midVerts[1].Opposing);

                            mesh.indices.Add(s_midVerts[1].Index);
                            mesh.indices.Add(s_midVerts[0].Index);
                            mesh.indices.Add(s_midVerts[2].Opposing);

                            mesh.indices.Add(s_midVerts[2].Index);
                            mesh.indices.Add(s_midVerts[1].Index);
                            mesh.indices.Add(s_midVerts[0].Opposing);

                            if (s_midVerts[0].NeedCutAgain || s_midVerts[1].NeedCutAgain || s_midVerts[2].NeedCutAgain)
                            {
                                largeTriNext.Add(triNum);
                                largeTriNext.Add(mesh.indices.Count - 9);
                                largeTriNext.Add(mesh.indices.Count - 6);
                                largeTriNext.Add(mesh.indices.Count - 3);
                            }

                            break;
                    }
                }

                if (mesh.positions.Count >= MAX_VERTEX_COUNT)
                {
                    break;
                }

                // shift oversized triangle list for next loop
                ListPool<int>.Release(largeTri);
                largeTri = largeTriNext;
                largeTriNext = ListPool<int>.Get();
            }

            ListPool<int>.Release(largeTri);
            ListPool<int>.Release(largeTriNext);
            s_indiceAssociate.Clear();
        }

        private static bool NeedInterpolate(Vector3 v0, Vector3 v1, float sizeSqr, out bool needInterpolateAgain, out float lengthSqr)
        {
            lengthSqr = (v0 - v1).sqrMagnitude;
            needInterpolateAgain = lengthSqr > 4f * sizeSqr;
            return lengthSqr > sizeSqr;
        }

        private static int GetInterpolate(MeshInfo mesh, Dictionary<uint, int> associate, int i0, int i1)
        {
            int midIndex;
            uint indicePair0 = ((uint)i0 << 16) | (uint)i1;
            uint indicePair1 = ((uint)i1 << 16) | (uint)i0;

            if (!associate.TryGetValue(indicePair0, out midIndex) && !associate.TryGetValue(indicePair1, out midIndex))
            {
                midIndex = mesh.positions.Count;
                associate.Add(indicePair0, midIndex);
                associate.Add(indicePair1, midIndex);

                mesh.positions.Add((mesh.positions[i0] + mesh.positions[i1]) * 0.5f);
                mesh.colors.Add(Color32.Lerp(mesh.colors[i0], mesh.colors[i1], 0.5f));
                mesh.uv0s.Add((mesh.uv0s[i0] + mesh.uv0s[i1]) * 0.5f);
                mesh.uv1s.Add((mesh.uv1s[i0] + mesh.uv1s[i1]) * 0.5f);

                var newNormal = (mesh.normals[i0] + mesh.normals[i1]).normalized;
                var newTangent = (Vector3)mesh.tangents[i0];

                Vector3.OrthoNormalize(ref newNormal, ref newTangent);

                mesh.normals.Add(newNormal);
                mesh.tangents.Add(new Vector4(newTangent.x, newTangent.y, newTangent.z, mesh.tangents[i0].w));
            }

            return midIndex;
        }
    }
}