using UnityEngine;
using System;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    public class TestDopplerSrc : MonoBehaviour
    {

        public IntPtr DopplerObject
        {
            get { return dopplerObject; }
            set { dopplerObject = value; }
        }
        private IntPtr dopplerObject = IntPtr.Zero;

        // Use this for initialization
        void Start()
        {
            //Vive3DSPAudio.CreateDoppler(this);
        }

        // Update is called once per frame
        void Update()
        {
            //Vive3DSPAudio.UpdateDoppler(this);
        }
        private void OnDestroy()
        {
            //Vive3DSPAudio.DestroyDoppler(this);
        }
    }

}
