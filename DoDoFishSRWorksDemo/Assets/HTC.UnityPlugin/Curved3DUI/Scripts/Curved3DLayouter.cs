//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using System;
using UnityEngine;

namespace HTC.UnityPlugin.Curved3DUI
{
    // place all children at curved surface
    [DisallowMultipleComponent]
    public class Curved3DLayouter : BaseCurved3DElement
    {
        [NonSerialized]
        private Curved3DHelper m_helper;

        public Vector3 localCenterPos { get { return m_helper.centerPos; } }
        public Quaternion localCenterRot { get { return m_helper.centerRot; } }

        private void NotifyChildEffectors()
        {
            for (int i = transform.childCount - 1; i >= 0; --i)
            {
                var child = transform.GetChild(i);
                if (!child.gameObject.activeSelf) { continue; }

                var e = transform.GetChild(i).GetComponent<Curved3DEffector>();
                if (e == null || !e.enabled) { continue; }

                e.OnCurved3DLayouterChanged();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            NotifyChildEffectors();

            m_helper = Curved3DHelper.pool.Get();
        }

        protected override void OnDisable()
        {
            Curved3DHelper.pool.Release(m_helper);
            m_helper = null;

            NotifyChildEffectors();

            base.OnDisable();
        }

        // invoke by Curved3DCanvas
        public override void OnLayoutComplete()
        {
            if (curvedCanvas == null && m_helper != null) { return; }

            switch (curvedCanvas.style)
            {
                case CurveStyle.Flat:
                case CurveStyle.Cylinder:
                case CurveStyle.UVSphere:
                    {
                        m_helper.style = curvedCanvas.style;
                        m_helper.centerPos = transform.InverseTransformPoint(curvedCanvas.worldCenterPos);
                        m_helper.centerRot = curvedCanvas.worldCenterRot * Quaternion.Inverse(transform.rotation);
                        m_helper.radius = curvedCanvas.radius;
                        m_helper.fagmentSizeSqr = curvedCanvas.fragmentSizeSqr;

                        break;
                    }
                default:
                    {
                        m_helper.style = CurveStyle.DontCurve;

                        break;
                    }
            }

            if (m_helper.styleChanged || m_helper.radiusChanged || m_helper.centerPosChanged || m_helper.centerRotChanged)
            {
                if (transform.childCount > 0)
                {
                    Vector3 pos;
                    Quaternion rot;
                    Curved3D.Transform(m_helper.style, Vector3.zero, Quaternion.identity, m_helper.radiusInverse, m_helper.centerPos, m_helper.centerRot, out pos, out rot);

                    for (int i = transform.childCount - 1; i >= 0; --i)
                    {
                        var child = transform.GetChild(i);

                        child.localPosition = pos;
                        child.localRotation = rot;
                    }
                }

                m_helper.Reset();
            }
        }
    }
}