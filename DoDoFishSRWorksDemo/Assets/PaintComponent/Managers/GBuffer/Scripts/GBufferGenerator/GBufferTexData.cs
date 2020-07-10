using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    // Enumerator Definitions.
    // ---------------------------------------------------------------
    public enum GBufferFormat
    {
        Half,
        Float
    }

    public enum GBufferResolution
    {
        Low = 64,
        Medium = 128,
        High = 256,
        VeryHigh = 256
    }

    [System.Serializable]
    public class GBufferTexData
    {
        public int size;
        public float[] positionData;
        public float[] normalData;
    }
}