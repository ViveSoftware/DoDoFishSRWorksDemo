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
        Low = 512,
        Medium = 1024,
        High = 2048,
        VeryHigh = 4096
    }

    [System.Serializable]
    public class GBufferTexData
    {
        public int size;
        public float[] positionData;
        public float[] normalData;
    }
}