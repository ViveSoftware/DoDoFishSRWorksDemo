//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using UnityEngine;
using System.Collections;
using HTC.UnityPlugin.Utility;
using UnityEngine.UI;
using System;

namespace HTC.UnityPlugin.Curved3DUI
{
    // localScale should be (1f,1f,1f), or radius & fragmentSize will not be wrong
    [DisallowMultipleComponent]
    public class Curved3DEffector : BaseCurved3DElement, IMeshModifier
    {
        protected Mesh s_mesh;

        [NonSerialized]
        private Graphic m_Graphic;
        [NonSerialized]
        private Curved3DLayouter m_layouter;
        [NonSerialized]
        private Curved3DHelper m_helper;

        public bool enableBackFaceMesh;
        public MeshCollider meshCollider;

        protected Mesh workerMesh
        {
            get
            {
                if (s_mesh == null)
                {
                    s_mesh = new Mesh();
                    s_mesh.name = "Shared Curved3DUI Mesh";
                    s_mesh.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_mesh;
            }
        }

        protected Graphic graphic { get { return m_Graphic ?? (m_Graphic = GetComponent<Graphic>()); } }

        private Curved3DLayouter GetValidCurved3DLayouter()
        {
            var parent = transform.parent;
            if (parent == null) { return null; }

            var layouter = parent.GetComponent<Curved3DLayouter>();
            if (layouter == null || !layouter.enabled) { return null; }

            return layouter;
        }
#if UNITY_EDITOR
        // FIXME: need this?
        protected override void OnValidate()
        {
           // if (graphic != null) { graphic.SetVerticesDirty(); }
        }
#endif
        protected override void OnEnable()
        {
            base.OnEnable();

            if (graphic != null)
            {
              //  graphic.SetVerticesDirty();

                m_helper = Curved3DHelper.pool.Get();

                m_layouter = GetValidCurved3DLayouter();
            }
        }

        protected override void OnDisable()
        {
            m_layouter = null;

            Curved3DHelper.pool.Release(m_helper);
            m_helper = null;

            if (graphic != null) { graphic.SetVerticesDirty(); }

            base.OnDisable();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            m_layouter = GetValidCurved3DLayouter();
        }

        public void OnCurved3DLayouterChanged()
        {
            m_layouter = GetValidCurved3DLayouter();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            if (graphic != null) { graphic.SetVerticesDirty(); }
            base.OnDidApplyAnimationProperties();
        }

        public void ModifyMesh(Mesh mesh)
        {
            using (var vh = new VertexHelper(mesh))
            {
                ModifyMesh(vh);
                vh.FillMesh(mesh);
            }
        }

        public void ModifyMesh(VertexHelper vh)
        {
            if (m_helper != null)
            {
                m_helper.SetPopulateMesh(vh);
            }
        }

        // invoke by Curved3DCanvas
        public override void OnGraphicUpdateComplete()
        {
            if (graphic == null && curvedCanvas == null && m_helper != null) { return; }

            var meshChanged = false;
            switch (curvedCanvas.style)
            {
                case CurveStyle.Flat:
                case CurveStyle.Cylinder:
                case CurveStyle.UVSphere:
                    {
                        m_helper.style = curvedCanvas.style;
                        m_helper.enableBackFaceMesh = enableBackFaceMesh;
                        m_helper.radius = curvedCanvas.radius;
                        m_helper.fagmentSizeSqr = curvedCanvas.fragmentSizeSqr;

                        if (m_layouter == null)
                        {
                            m_helper.centerPos = transform.InverseTransformPoint(curvedCanvas.worldCenterPos);
                            m_helper.centerRot = curvedCanvas.worldCenterRot * Quaternion.Inverse(transform.rotation);
                        }
                        else
                        {
                            m_helper.centerPos = transform.InverseTransformPoint(curvedCanvas.worldCenterPos);
                            m_helper.centerRot = curvedCanvas.worldCenterRot * Quaternion.Inverse(transform.rotation);
                            // FIXME: optimize for layouter
                            //switch (curvedCanvas.style)
                            //{
                            //    case CurveStyle.Flat:
                            //        {
                            //            m_helper.centerPos = m_layouter.localCenterPos;
                            //            m_helper.centerRot = m_layouter.localCenterRot;
                            //            break;
                            //        }
                            //    case CurveStyle.Cylinder:
                            //        {
                            //            m_helper.centerPos = Vector3.Project(m_layouter.localCenterPos, m_layouter.localCenterRot * Vector3.back);
                            //            m_helper.centerRot = m_layouter.localCenterRot;
                            //            break;
                            //        }
                            //    case CurveStyle.UVSphere:
                            //        {
                            //            m_helper.centerPos = Vector3.ProjectOnPlane(m_layouter.localCenterPos, m_layouter.localCenterRot * Vector3.right);
                            //            m_helper.centerRot = m_layouter.localCenterRot;
                            //            break;
                            //        }
                            //}
                        }

                        if (m_helper.populatedMeshChanged || m_helper.fragmentSizeChanged)
                        {
                            m_helper.TessellateMesh();
                            m_helper.CurveMesh();

                            m_helper.curvedMesh.Fill(workerMesh, enableBackFaceMesh);
                            meshChanged = true;
                        }
                        else if (m_helper.styleChanged || m_helper.centerPosChanged || m_helper.centerRotChanged || m_helper.radiusChanged)
                        {
                            m_helper.CurveMesh();

                            m_helper.curvedMesh.Fill(workerMesh, enableBackFaceMesh);
                            meshChanged = true;
                        }
                        else if (m_helper.enableBackFaceMeshChanged)
                        {
                            m_helper.curvedMesh.Fill(workerMesh, enableBackFaceMesh);
                            meshChanged = true;
                        }

                        break;
                    }
                default:
                    {
                        m_helper.style = CurveStyle.DontCurve;
                        m_helper.enableBackFaceMesh = enableBackFaceMesh;

                        if (m_helper.styleChanged || m_helper.enableBackFaceMeshChanged)
                        {
                            m_helper.populatedMesh.Fill(workerMesh, enableBackFaceMesh);
                            meshChanged = true;
                        }

                        break;
                    }
            }

            if (meshChanged)
            {
                if (meshCollider != null) { meshCollider.sharedMesh = workerMesh; }
                graphic.canvasRenderer.SetMesh(workerMesh);

                m_helper.Reset();
            }
        }
    }
}