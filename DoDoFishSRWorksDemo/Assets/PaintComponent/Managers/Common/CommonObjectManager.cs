using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PaintVR
{
    public class CommonObjectManager : MonoBehaviour
    {
        // Private Class Data.
        // ---------------------------------------------------------------

        private static CommonObjectManager objectManager = null;
        //private Material splatMaterial = null;
		private Material fillMaterial = null;
        private Texture2D whiteTexture = null;
        private Texture2D blackTexture = null;
        private Texture2D exponentialFadeOutLTex = null;
        private Texture2D exponentialFadeOutRTex = null;

        // C# Properties for Public Access.
        // ---------------------------------------------------------------
        public static CommonObjectManager instance
        {
            get
            {
                if (objectManager == null)
                {
                    objectManager = FindObjectOfType(typeof(CommonObjectManager)) as CommonObjectManager;
                    if (objectManager == null)
                    {
                        Debug.LogError("[CommonObjectManager::Singleton] There should be one active gameobject attached CommonObjectManager in scene");
                    }
                }
                return objectManager;
            }
        }

		public Material TextureFillMaterial
		{
			get { return fillMaterial; }
			private set { fillMaterial = value; }
		}

        public Texture2D WhiteTex
        {
            get { return whiteTexture; }
        }

        public Texture2D BlackTex
        {
            get { return blackTexture; }
        }

        public Texture2D ExponentialFadeOutLTex
        {
            get { return exponentialFadeOutLTex; }
        }

        public Texture2D ExponentialFadeOutRTex
        {
            get { return exponentialFadeOutRTex; }
        }

        // Unity Fixed Methods.
        // ---------------------------------------------------------------
        void Awake()
        {
            // Create default assets.
            CreateDefaultAssets();
        }

        // Private Class Methods.
        // ---------------------------------------------------------------
        private void CreateDefaultAssets()
        {
            CreateWhiteTexture();
            CreateBlackTexture();
            CreateFadeOutTextures();
        }

        private void CreateWhiteTexture()
        {
            whiteTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
            whiteTexture.name = "White Texture";
        }

        private void CreateBlackTexture()
        {
            blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            blackTexture.SetPixel(0, 0, Color.black);
            blackTexture.Apply();
            blackTexture.name = "Black Texture";
        }

        private void CreateFadeOutTextures()
        {
            const int Width = 256;
            const int Height = 16;
            exponentialFadeOutLTex = new Texture2D(Width, Height, TextureFormat.ARGB32, true);
            for (int h = 0 ; h < Height ; ++h)
            {
                for (int w = 0 ; w < Width ; ++w)
                {
                    float b = (float)w / (float)(Width - 1);
                    float exp = Mathf.Pow(b, 3.0f);
                    exponentialFadeOutLTex.SetPixel(w, h, new Color(exp, exp, exp, 1));
                }
            }
            exponentialFadeOutLTex.Apply();
            exponentialFadeOutLTex.name = "FadeOut L Texture";
            exponentialFadeOutLTex.wrapMode = TextureWrapMode.Clamp;

            exponentialFadeOutRTex = new Texture2D(Width, Height, TextureFormat.ARGB32, true);
            for (int h = 0 ; h < Height ; ++h)
            {
                for (int w = 0 ; w < Width ; ++w)
                {
                    float b = 1.0f - (float)w / (float)(Width - 1);
                    float exp = Mathf.Pow(b, 3.0f);
                    exponentialFadeOutRTex.SetPixel(w, h, new Color(exp, exp, exp, 1));
                }
            }
            exponentialFadeOutRTex.Apply();
            exponentialFadeOutRTex.name = "FadeOut R Texture";
            ExponentialFadeOutRTex.wrapMode = TextureWrapMode.Clamp;
        }
    }
}