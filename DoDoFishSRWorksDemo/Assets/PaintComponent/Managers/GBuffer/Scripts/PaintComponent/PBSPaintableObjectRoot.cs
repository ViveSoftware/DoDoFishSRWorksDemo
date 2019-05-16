using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PaintVR
{
    public class PBSPaintableObjectRoot : PBSPaintableObject
    {
        /*
        // Private Class Data.
        // ---------------------------------------------------------------
        [SerializeField] private Texture2D positionMap = null;
        [SerializeField] private Texture2D normalMap = null;

        public bool defaultPath = false;

        // C# Properties for Public Access.
        // ---------------------------------------------------------------
        public override Texture2D PositionMap
        {
            set { positionMap = value; }
            get { return positionMap; }
        }

        public override Texture2D NormalMap
        {
            set { normalMap = value; }
            get { return normalMap; }
        }*/

        public void ImportPBSMap(RenderTexture position, RenderTexture normal)
        {
            positionMap = FileUtil.RenderTextureToTexture2D(position, TextureFormat.RGBAHalf);
            normalMap = FileUtil.RenderTextureToTexture2D(normal, TextureFormat.RGBAHalf);
        }
    }
}