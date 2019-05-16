using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PaintVR
{
    public class PaintableObject : MonoBehaviour
    {
        // Public Class Data.
        // ---------------------------------------------------------------
        public string paintObjectID
        {
            get { return gameObject.name; }
        }
        public bool paintOnOriginalTexture = true;
        public GBufferResolution resolution = GBufferResolution.High;

        [Range(0.001f, 1.0f)] public float splatScaleX = 0.1f;
        [Range(0.001f, 1.0f)] public float splatScaleY = 0.1f;

        // Protected Class Data.
        // ---------------------------------------------------------------
        protected ModelData paintModelData = null;
        protected TextureSpaceData paintTextureData = null;
        protected Texture origAlbedoTexture = null;

        protected UnityEvent OnPaintingEvet = new UnityEvent();

        // C# Properties for Public Access.
        // ---------------------------------------------------------------
        public virtual bool addToDitionary
        {
            get { return true; }
        }
        public virtual GameObject Model
        {
            get { return this.gameObject; }
        }

        public virtual ModelData PaintModelData
        {
            get { return paintModelData; }
            private set { paintModelData = value; }
        }

        public virtual TextureSpaceData PaintTextureData
        {
            get { return paintTextureData; }
            private set { paintTextureData = value; }
        }

        public virtual Texture OrigAlbedoTexture
        {
            get { return origAlbedoTexture; }
            private set { origAlbedoTexture = value; }
        }

        // Unity Fixed Methods.
        // ---------------------------------------------------------------
        void Awake()
        {
            PaintableInit();
        }

        public void PaintableReset()
        {
            Texture srcTexture = OrigAlbedoTexture;
            if (!paintOnOriginalTexture || srcTexture == null)
            {
                srcTexture = CommonObjectManager.instance.WhiteTex;
            }
            Graphics.Blit(srcTexture, PaintTextureData.AlbedoTexture);
            PaintModelData.ModelMaterial.SetTexture("_MainTex", PaintTextureData.AlbedoTexture);
        }

        protected virtual void PaintableInit()
        {
            CreateModelData();
            CreatePaintTextureData();
            origAlbedoTexture = paintModelData.ModelMaterial.GetTexture("_MainTex");
        }

        // Public Class Methods.
        // ---------------------------------------------------------------
        public void AddOnPaintingListener(UnityAction method)
        {
            OnPaintingEvet.AddListener(method);
        }

        public void RemoveOnPaintingListener(UnityAction method)
        {
            OnPaintingEvet.RemoveListener(method);
        }

        public void InvokeOnPaintingEvent()
        {
            OnPaintingEvet.Invoke();
        }

        // Protected Class Methods.
        // ---------------------------------------------------------------
        protected void CreateModelData(bool withoutMeshFilter = false)
        {
            paintModelData = new ModelData(this.gameObject, withoutMeshFilter);
            // paintModelData.PrintModelInformation();
        }

        protected void CreatePaintTextureData()
        {
            paintTextureData = new TextureSpaceData(this.gameObject.name, resolution);
        }
    }
}
