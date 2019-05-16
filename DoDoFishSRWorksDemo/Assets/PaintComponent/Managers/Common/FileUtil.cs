using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PaintVR
{
    public static class FileUtil
    {
        public static readonly string TextureFolder = "Textures";
        public static readonly string AlbedoTexFileName = "albedo_result"; 

        public static Texture2D RenderTextureToTexture2D(RenderTexture rt, TextureFormat format = TextureFormat.ARGB32)
        {
            Texture2D tex = new Texture2D(rt.width, rt.height, format, false);

            RenderTexture currentRT = RenderTexture.active;

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = currentRT;

            return tex;
        }

        public static void ColorArrayToFloatArray(Color[] colors, out float[] floatData)
        {
            floatData = new float[colors.Length * 4];
            for (int i = 0 ; i < colors.Length ; ++i)
            {
                floatData[4 * i    ] = colors[i].r;
                floatData[4 * i + 1] = colors[i].g;
                floatData[4 * i + 2] = colors[i].b;
                floatData[4 * i + 3] = colors[i].a;
            }
        }

        public static void FloatArrayToColorArray(float[] floatData, out Color[] colors)
        {
            colors = new Color[floatData.Length / 4];
            for (int i = 0 ; i < colors.Length; ++i)
            {
                colors[i].r = floatData[4 * i    ];
                colors[i].g = floatData[4 * i + 1];
                colors[i].b = floatData[4 * i + 2];
                colors[i].a = floatData[4 * i + 3];
            }
        }

        public static void SaveTexture(RenderTexture rt, string Path = null)
        {
            // First copy the content of render texture to a Texture2D.
            Texture2D saveTex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);

            RenderTexture currentRT = RenderTexture.active;

            RenderTexture.active = rt;
            saveTex.ReadPixels(new Rect(0, 0, saveTex.width, saveTex.height), 0, 0);
            saveTex.Apply();

            RenderTexture.active = currentRT;

            // Save albedo texture as a PNG file.
            byte[] bytes = saveTex.EncodeToPNG();

            if (Path == null)
            {
                Path = Application.dataPath + "//" + TextureFolder + "//" + AlbedoTexFileName + ".png";
            }
            File.WriteAllBytes(Path, bytes);
        }

        public static Texture2D LoadTextureFromImage(string path)
        {
            const int ThumbResolution = 512;

            Texture2D origTex = new Texture2D(16, 16);
            byte[] imgData = File.ReadAllBytes(path);
            origTex.LoadImage(imgData);

            // Resize texture.
            Texture2D tex = new Texture2D(ThumbResolution, ThumbResolution, TextureFormat.ARGB32, true);
            Color[] pixels = new Color[ThumbResolution * ThumbResolution];
            for (int h = 0 ; h < ThumbResolution ; ++h)
            {
                for (int w = 0 ; w < ThumbResolution ; ++w)
                {
                    int index = h * ThumbResolution + w;
                    float u = (float)w / (float)ThumbResolution;
                    float v = (float)h / (float)ThumbResolution;
                    pixels[index] = origTex.GetPixelBilinear(u, v);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}