//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HTC.UnityPlugin.Curved3DUI
{
    // handling Curved3D canvas-elements hierarchy
    [ExecuteInEditMode]
    public abstract class BaseCurved3DElement : UIBehaviour
    {
        [NonSerialized]
        private Curved3DCanvas m_curvedCanvas;

        public Curved3DCanvas curvedCanvas { get { return m_curvedCanvas; } }

        private Curved3DCanvas GetValidCurved3DCanvas()
        {
            var canvases = ListPool<Curved3DCanvas>.Get();
            GetComponentsInParent(false, canvases);
            for (int i = 0, imax = canvases.Count; i < imax; ++i)
            {
                if (canvases[i].isActiveAndEnabled) { return canvases[i]; }
            }
            ListPool<Curved3DCanvas>.Release(canvases);
            return null;
        }

        protected void ChangeCurved3DCanvas()
        {
            if (isActiveAndEnabled)
            {
                var curretCurvedCanvas = GetValidCurved3DCanvas();
                if (curretCurvedCanvas != m_curvedCanvas)
                {
                    if (m_curvedCanvas != null)
                    {
                        m_curvedCanvas.RemoveElement(this);
                    }
                    m_curvedCanvas = curretCurvedCanvas;
                    if (m_curvedCanvas != null)
                    {
                        m_curvedCanvas.AddElement(this);
                    }
                }
            }
            else
            {
                if (m_curvedCanvas != null)
                {
                    m_curvedCanvas.RemoveElement(this);
                }
                m_curvedCanvas = null;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_curvedCanvas = GetValidCurved3DCanvas();
            if (m_curvedCanvas != null)
            {
                m_curvedCanvas.AddElement(this);
            }
        }

        protected override void OnDisable()
        {
            if (m_curvedCanvas != null)
            {
                m_curvedCanvas.RemoveElement(this);
                m_curvedCanvas = null;
            }
            base.OnDisable();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            ChangeCurved3DCanvas();
        }

        public virtual void OnCurved3DCanvasChanged()
        {
            ChangeCurved3DCanvas();
        }

        public virtual void OnLayoutComplete() { }

        public virtual void OnGraphicUpdateComplete() { }
    }
}