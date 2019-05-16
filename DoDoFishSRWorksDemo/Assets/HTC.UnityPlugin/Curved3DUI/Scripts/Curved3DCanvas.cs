//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using HTC.UnityPlugin.Utility;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HTC.UnityPlugin.Curved3DUI
{
    // handling Curved3D canvas-elements hierarchy
    [RequireComponent(typeof(Canvas))]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class Curved3DCanvas : UIBehaviour, ICanvasElement
    {
        [NonSerialized]
        private Canvas m_canvas;
        [NonSerialized]
        private IndexedSet<BaseCurved3DElement> m_elements = new IndexedSet<BaseCurved3DElement>();

        public CurveStyle style = CurveStyle.Cylinder;
        public Vector3 centerPos = new Vector3(0f, 0f, -100f);
        public Vector3 centerRot = new Vector3(0f, 0f, 0f);
        public float radius = 100f;
        public float segments = 36f;

        private Vector3 m_cacheCenterPos;
        private Quaternion m_cacheCenterRot;
        private float m_cacheFragmentSizeSqr;

        public Canvas canvas { get { return m_canvas ?? (m_canvas = GetComponent<Canvas>()); } }

        public Vector3 worldCenterPos { get { return m_cacheCenterPos; } }
        public Quaternion worldCenterRot { get { return m_cacheCenterRot; } }
        public float fragmentSizeSqr { get { return m_cacheFragmentSizeSqr; } }

        protected override void OnEnable()
        {
            base.OnEnable();

            var elements = HTC.UnityPlugin.Utility.ListPool<BaseCurved3DElement>.Get();
            GetComponentsInChildren(elements);
            for (int i = elements.Count - 1; i >= 0; --i)
            {
                if (elements[i].curvedCanvas != null && elements[i].curvedCanvas.transform.IsChildOf(transform)) { continue; }
                if (!elements[i].isActiveAndEnabled) { continue; }
                elements[i].OnCurved3DCanvasChanged();
            }
            HTC.UnityPlugin.Utility.ListPool<BaseCurved3DElement>.Release(elements);
        }

        protected override void OnDisable()
        {
            for (int i = m_elements.Count - 1; i >= 0; --i)
            {
                m_elements[i].OnCurved3DCanvasChanged();
            }
            m_elements.Clear();

            base.OnDisable();
        }

        protected virtual void Update()
        {
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
        }

        public virtual void AddElement(BaseCurved3DElement element)
        {
            m_elements.AddUnique(element);
        }

        public virtual void RemoveElement(BaseCurved3DElement element)
        {
            m_elements.Remove(element);
        }

        #region implement ICanvasElement
        private bool isLatePreRender;// work around for skipping GraphicUpdateComplete()

        public void Rebuild(CanvasUpdate executing)
        {
            isLatePreRender = executing == CanvasUpdate.LatePreRender;
        }

        public void LayoutComplete()
        {
            if (!isLatePreRender)
            {
                m_cacheCenterPos = transform.TransformPoint(centerPos);
                m_cacheCenterRot = transform.rotation * Quaternion.Euler(centerRot);
                m_cacheFragmentSizeSqr = CalculateFragmentSizeSqr(radius, segments);

                for (int i = m_elements.Count - 1; i >= 0; --i)
                {
                    m_elements[i].OnLayoutComplete();
                }
            }
            else
            {
                GraphicUpdateComplete();
            }
        }

        public void GraphicUpdateComplete()
        {
            for (int i = m_elements.Count - 1; i >= 0; --i)
            {
                m_elements[i].OnGraphicUpdateComplete();
            }
        }
        #endregion

        private static float CalculateFragmentSizeSqr(float radius, float segments)
        {
            if (radius <= 0f)
            {
                return float.PositiveInfinity;
            }
            else
            {
                var size = 2f * Mathf.PI * radius / segments;
                return size * size;
            }
        }
    }
}