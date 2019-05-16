using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;

namespace PaintVR
{
    public class PBSPaintableObject : PaintableObject
    {
        // Private Class Data.
        // ---------------------------------------------------------------
        [SerializeField] protected Texture2D positionMap = null;
        [SerializeField] protected Texture2D normalMap = null;
        public bool registerUndoMgr = true;

        //public bool defaultPath = false;

        // C# Properties for Public Access.
        // ---------------------------------------------------------------
        public virtual Texture2D PositionMap
        {
            set { positionMap = value; }
            get { return positionMap; }
        }

        public virtual Texture2D NormalMap
        {
            set { normalMap = value; }
            get { return normalMap; }
        }

        protected override void PaintableInit()
        {
            CreateModelData();
            CreatePaintTextureData();
            origAlbedoTexture = paintModelData.ModelMaterial.GetTexture("_MainTex");

            Texture srcTexture = OrigAlbedoTexture;
            if (!paintOnOriginalTexture || srcTexture == null)
            {
                srcTexture = CommonObjectManager.instance.WhiteTex;
            }

            Graphics.Blit(srcTexture, PaintTextureData.AlbedoTexture);
            PaintModelData.ModelMaterial.SetTexture("_MainTex", PaintTextureData.AlbedoTexture);

            if(registerUndoMgr)
                RegisterUndoMgr();
        }

        public void CreatePBSMap(int size)
        {
            positionMap = new Texture2D(size, size, TextureFormat.RGBAHalf, false);
            positionMap.wrapMode = TextureWrapMode.Clamp;
            positionMap.filterMode = FilterMode.Bilinear;

            normalMap = new Texture2D(size, size, TextureFormat.RGBAHalf, false);
            normalMap.wrapMode = TextureWrapMode.Clamp;
            normalMap.filterMode = FilterMode.Bilinear;
        }

        public void SetPositionTexData(int x, int y, int blockWidth, int blockHeight, Color[] positions)
        {
            positionMap.SetPixels(x, y, blockWidth, blockHeight, positions);
            positionMap.Apply();
        }

        public void SetNormalTexData(int x, int y, int blockWidth, int blockHeight, Color[] normals)
        {
            normalMap.SetPixels(x, y, blockWidth, blockHeight, normals);
            normalMap.Apply();
        }

        public void RegisterUndoMgr()
        {
            UndoManager.instance.RegisterPaintableObject(this);
        }

        public void UnregisterUndoMgr()
        {
            UndoManager.instance.UnRegisterPaintableObject(this);
        }
    }
}