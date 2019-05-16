//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HTC.UnityPlugin.Curved3DUI
{
    public sealed class Curved3DHelper
    {
        public static readonly HTC.UnityPlugin.Utility.ObjectPool<Curved3DHelper> pool = new HTC.UnityPlugin.Utility.ObjectPool<Curved3DHelper>(() => new Curved3DHelper(), e => e.Initialize(), e => e.Release());

        private CurveStyle m_style;
        private Vector3 m_centerPos;
        private Quaternion m_centerRot;
        private float m_radius;
        private float m_radiusInverse;
        private float m_fragmentSizeSqr;
        private bool m_enableBackFaceMesh;

        public bool styleChanged { get; private set; }
        public bool centerPosChanged { get; private set; }
        public bool centerRotChanged { get; private set; }
        public bool radiusChanged { get; private set; }
        public bool fragmentSizeChanged { get; private set; }
        public bool populatedMeshChanged { get; private set; }
        public bool enableBackFaceMeshChanged { get; private set; }

        public MeshInfo populatedMesh { get; private set; }
        public MeshInfo tessellatedMesh { get; private set; }
        public MeshInfo curvedMesh { get; private set; }

        public void Initialize()
        {
            styleChanged = true;
            centerPosChanged = true;
            centerRotChanged = true;
            radiusChanged = true;
            fragmentSizeChanged = true;
            populatedMeshChanged = true;
            enableBackFaceMeshChanged = true;
        }

        public CurveStyle style
        {
            get { return m_style; }
            set { styleChanged |= SetProperty(ref m_style, value); }
        }

        public Vector3 centerPos
        {
            get { return m_centerPos; }
            set { centerPosChanged |= SetProperty(ref m_centerPos, value, Vector3Approximately); }
        }

        public Quaternion centerRot
        {
            get { return m_centerRot; }
            set { centerRotChanged |= SetProperty(ref m_centerRot, value, QuaternionApproximately); }
        }

        public float radius
        {
            get { return m_radius; }
            set
            {
                if (SetProperty(ref m_radius, value, Mathf.Approximately))
                {
                    radiusChanged = true;
                    m_radiusInverse = 1f / m_radius;
                }
            }
        }

        public float radiusInverse
        {
            get { return m_radiusInverse; }
            set
            {
                if (SetProperty(ref m_radiusInverse, value, Mathf.Approximately))
                {
                    radiusChanged = true;
                    m_radius = 1f / m_radiusInverse;
                }
            }
        }

        public float fagmentSizeSqr
        {
            get { return m_fragmentSizeSqr; }
            set { fragmentSizeChanged |= SetProperty(ref m_fragmentSizeSqr, value, Mathf.Approximately); }
        }

        public bool enableBackFaceMesh
        {
            get { return m_enableBackFaceMesh; }
            set { enableBackFaceMeshChanged |= SetProperty(ref m_enableBackFaceMesh, value, BoolEqual); }
        }

        public void SetPopulateMesh(VertexHelper vertexHelper)
        {
            if (populatedMesh == null)
            {
                populatedMesh = MeshInfo.pool.Get();
            }

            populatedMesh.Initialize();
            populatedMesh.CopyFrom(vertexHelper);
            populatedMeshChanged = true;
        }

        public void TessellateMesh()
        {
            if (populatedMesh == null)
            {
                throw new Exception("populatedMesh not initialized yet, SetPopulateMesh first");
            }

            if (tessellatedMesh == null)
            {
                tessellatedMesh = MeshInfo.pool.Get();
            }

            tessellatedMesh.Initialize();
            tessellatedMesh.CopyFrom(populatedMesh);
            Curved3D.Tessellate(tessellatedMesh, fagmentSizeSqr);
        }

        public void CurveMesh()
        {
            if (tessellatedMesh == null)
            {
                throw new Exception("tessellatedMesh not initialized yet, TessellateMesh first");
            }

            if (curvedMesh == null)
            {
                curvedMesh = MeshInfo.pool.Get();
            }

            curvedMesh.Initialize(tessellatedMesh);

            //if (Curved3DShaderManager.IsWorkable())
            //{
            //    Curved3DShaderManager.Instance.Dispatch(style, tessellatedMesh.positions, tessellatedMesh.normals, tessellatedMesh.tangents, curvedMesh.positions, curvedMesh.normals, curvedMesh.tangents, radiusInverse, centerPos, centerRot);
            //}
            //else
            {
                Vector3 pos;
                Quaternion rot;
                for (int i = 0, imax = tessellatedMesh.positions.Count; i < imax; ++i)
                {
                    Curved3D.Transform(style, tessellatedMesh.positions[i], Quaternion.identity, radiusInverse, centerPos, centerRot, out pos, out rot);

                    curvedMesh.positions.Add(pos);
                    curvedMesh.normals.Add(rot * tessellatedMesh.normals[i]);
                    curvedMesh.tangents.Add((Vector4)(rot * tessellatedMesh.tangents[i]) + new Vector4(0f, 0f, 0f, tessellatedMesh.tangents[i].w));
                }
            }
        }

        // reset change state
        public void Reset()
        {
            styleChanged = false;
            centerPosChanged = false;
            centerRotChanged = false;
            radiusChanged = false;
            fragmentSizeChanged = false;
            populatedMeshChanged = false;
            enableBackFaceMeshChanged = false;
        }

        public void Release()
        {
            if (populatedMesh != null) { MeshInfo.pool.Release(populatedMesh); populatedMesh = null; }
            if (tessellatedMesh != null) { MeshInfo.pool.Release(tessellatedMesh); tessellatedMesh = null; }
            if (curvedMesh != null) { MeshInfo.pool.Release(curvedMesh); curvedMesh = null; }
        }

        private static bool SetProperty<T>(ref T currentValue, T newValue, Func<T, T, bool> equalFunc = null)
        {
            if (equalFunc == null)
            {
                if (currentValue.Equals(newValue)) { return false; }
            }
            else
            {
                if (equalFunc(currentValue, newValue)) { return false; }
            }

            currentValue = newValue;
            return true;
        }

        private static bool Vector3Approximately(Vector3 a, Vector3 b) { return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z); }

        private static bool QuaternionApproximately(Quaternion a, Quaternion b) { return Mathf.Approximately(Quaternion.Angle(a, b), 0f); }

        private static bool BoolEqual(bool a, bool b) { return a == b; }
    }
}