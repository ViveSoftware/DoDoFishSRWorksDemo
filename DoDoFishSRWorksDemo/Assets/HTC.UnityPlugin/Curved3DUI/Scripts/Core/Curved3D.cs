//========= Copyright 2016, HTC Corporation. All rights reserved. ===========

using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Utility;

namespace HTC.UnityPlugin.Curved3DUI
{
    public enum CurveStyle
    {
        DontCurve,
        Flat,
        UVSphere,
        Cylinder,
    }

    public static partial class Curved3D
    {
        public const int MAX_VERTEX_COUNT = 32499;  // "Mesh can not have more than 65000 vertices"
    }
}