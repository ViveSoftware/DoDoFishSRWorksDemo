using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://gamedev.stackexchange.com/questions/113440/how-can-i-make-particles-glow-and-cast-light-on-its-surroundings

[RequireComponent(typeof(ParticleSystem))]
public class ParticleLights : MonoBehaviour
{
    public Light lightSample;
    public bool UseEmissionColor;
    ParticleSystem _system;
    ParticleSystem.Particle[] _particles;
    Light[] _lights;
    static GameObject ParticleLightsRoot;
    public GameObject particleLightNode;

    void Start()
    {
        if (ParticleLightsRoot == null)
        {
            ParticleLightsRoot = GameObject.Find("ParticleLightsRoot");
            if (ParticleLightsRoot == null)
            {
                ParticleLightsRoot = new GameObject("ParticleLightsRoot");
            }
        }
        particleLightNode = new GameObject(name);
        particleLightNode.transform.parent = ParticleLightsRoot.transform;

        _system = GetComponent<ParticleSystem>();
        _particles = new ParticleSystem.Particle[_system.main.maxParticles];

        _refreshLight();
    }

    void _refreshLight()
    {
        lightSample.enabled = false;
        if (_lights != null)
        {
            for (int i = 0; i < _lights.Length; i++)
                Destroy(_lights[i]);
        }

        _lights = new Light[_system.main.maxParticles];
        for (int i = 0; i < _lights.Length; i++)
        {
            GameObject obj = new GameObject("light" + i);
            _lights[i] = obj.AddComponent<Light>();//(Light)Instantiate(lightPrefab);
            _lights[i].transform.parent = particleLightNode.transform;
        }
    }

    private void OnEnable()
    {
        if (particleLightNode != null)
            particleLightNode.SetActive(true);
    }

    private void OnDisable()
    {
        if (particleLightNode != null)
        {
            particleLightNode.SetActive(false);
            for (int i = 0; i < _lights.Length; i++)
                _lights[i].enabled = false;
        }
    }

    public bool Refresh;
    void Update()
    {
        if (Refresh || Input.GetKeyDown(KeyCode.R))
        {
            Refresh = false;
            _refreshLight();
        }

        int count = _system.GetParticles(_particles);
        for (int i = 0; i < count; i++)
        {
            _lights[i].enabled = true;
            //_lights[i].gameObject.SetActive(true);
            _lights[i].transform.position = _particles[i].position;
            if (_system.main.simulationSpace == ParticleSystemSimulationSpace.Local)
                _lights[i].transform.position += _system.transform.position;

            // Looks like the GetCurrentColor function was added in 5.3.
            // You can use other methods in earlier Unity versions.
            if (UseEmissionColor)
                _lights[i].color = _system.GetComponent<ParticleSystemRenderer>().trailMaterial.GetColor("_EmissionColor");
            else
                _lights[i].color = _particles[i].GetCurrentColor(_system);

            _lights[i].type = lightSample.type;
            _lights[i].range = lightSample.range;
            _lights[i].intensity = lightSample.intensity;
        }

        for (int i = count; i < _particles.Length; i++)
        {
            //_lights[i].gameObject.SetActive(false);
            _lights[i].enabled = false;
        }
    }
}