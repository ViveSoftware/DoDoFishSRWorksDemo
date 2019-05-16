using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HTC.UnityPlugin.Vive3DSoundPerception
{
    public class AutoGen : MonoBehaviour
    {
        [SerializeField]
        private GameObject occPrefab1 = null;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                //GameObject bgmObj = GameObject.Instantiate(occPrefab1);
                GameObject.Instantiate(occPrefab1);
            }
        }
    }
}
