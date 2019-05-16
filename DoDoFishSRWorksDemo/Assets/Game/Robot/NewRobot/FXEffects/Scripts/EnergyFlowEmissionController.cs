using UnityEngine;
using System.Collections;

namespace Breakout
{
    public class EnergyFlowEmissionController : MonoBehaviour
    {
        public bool startRandomly = true;
        [Range(0.0f, 1.0f)] public float horizontalEmissiveFlowSpeed = 0.075f;
        [Range(0.0f, 1.0f)] public float verticalEmissiveFlowSpeed = 0.075f;
        [Range(0.0f, 1.0f)] public float emissiveMaskScale = 0.25f;

        private Material material = null;
        private Vector2 currentShift = Vector2.zero;

        void Start()
        {
            material = GetComponent<MeshRenderer>().material;

            // Initialize a random shift.
            if (startRandomly)
            {
                currentShift.x = Random.Range(0.0f, 1.0f);
                currentShift.y = Random.Range(0.0f, 1.0f);
                material.SetVector("_EnergyFlowParams", new Vector4(currentShift.x, currentShift.y, emissiveMaskScale, 0.0f));
            }
        }

        void Update()
        {
            UpdateEmissionMask();
        }

        private void UpdateEmissionMask()
        {
            currentShift.x += horizontalEmissiveFlowSpeed * Time.deltaTime;
            currentShift.y += verticalEmissiveFlowSpeed * Time.deltaTime;
            if (currentShift.x > 1.0f)
            {
                currentShift.x -= 1.0f;
            }
            if (currentShift.y > 1.0f)
            {
                currentShift.y -= 1.0f;
            }

            material.SetVector("_EnergyFlowParams", new Vector4(currentShift.x, currentShift.y, emissiveMaskScale, 0.0f));
        }
    }
}