using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    public class PBSPaintableMiniature : PBSPaintableObject
    {
        [SerializeField] private PaintableObject originObj;
        [SerializeField] private PBSPaintableObject PBSoriginObj;

        public PBSPaintableObject OriginObj { set { PBSoriginObj = value; } }

        public override bool addToDitionary
        {
            get
            {
                return false;
            }
        }

        public override GameObject Model
        {
            get
            {
                return originObj.Model;
            }
        }

        public override Texture OrigAlbedoTexture
        {
            get
            {
                return originObj.OrigAlbedoTexture;
            }
        }

        public override ModelData PaintModelData
        {
            get
            {
                return originObj.PaintModelData;
            }
        }

        public override TextureSpaceData PaintTextureData
        {
            get
            {
                return originObj.PaintTextureData;
            }
        }

        public override Texture2D PositionMap
        {
            set { PBSoriginObj.PositionMap = value; }
            get { return PBSoriginObj.PositionMap; }
        }

        public override Texture2D NormalMap
        {
            set { PBSoriginObj.NormalMap = value; }
            get { return PBSoriginObj.NormalMap; }
        }

        private void Start()
        {
            if (PBSoriginObj == null)
            {
                UndoManager.instance.PaintableObjs.TryGetValue(gameObject.name, out PBSoriginObj);
                if (PBSoriginObj == null)
                {
                    Debug.Log("PaintableMiniature :: Can't find mapping Paintable Object.");
                }
            }
            else
            {
                GetComponent<Renderer>().material.SetTexture("_MainTex", PBSoriginObj.PaintTextureData.AlbedoTexture);
                originObj = PBSoriginObj.GetComponent<PaintableObject>();
            }
        }

        protected override void PaintableInit()
        {
            
        }
    }
}

