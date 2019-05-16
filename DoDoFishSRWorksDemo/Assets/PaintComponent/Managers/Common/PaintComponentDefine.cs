using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PaintVR
{
    public static class PaintComponentDefine
    {
        // Name of layers.
        //UnityException: NameToLayer is not allowed to be called from a MonoBehaviour constructor(or instance field initializer), call it in Awake or Start instead
        public static int PositionMapDataLayer;// = LayerMask.NameToLayer("PositionMapData");
        public static int NormalMapDataLayer;// = LayerMask.NameToLayer("NormalMapData");
        public static int PaintObjectLayer;// = LayerMask.NameToLayer("PaintObject");
        public static int PaintToolBoxLayer;// = LayerMask.NameToLayer("PaintToolBox");

        // Name of tags.
        public static string PaintMeshTag = "PaintMesh";
        public static string PaintToolBoxTag = "PaintToolBox";

        public static void init()
        {
            PositionMapDataLayer = LayerMask.NameToLayer("PositionMapData");
            NormalMapDataLayer = LayerMask.NameToLayer("NormalMapData");
            PaintObjectLayer = LayerMask.NameToLayer("PaintObject");
            PaintToolBoxLayer = LayerMask.NameToLayer("PaintToolBox");
            //PaintMeshTag = "PaintMesh";
            //PaintToolBoxTag = "PaintToolBox";
        }
    }
}