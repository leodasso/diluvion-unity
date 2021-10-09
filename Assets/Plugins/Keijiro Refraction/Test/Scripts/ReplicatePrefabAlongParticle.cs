using UnityEngine;
using System.Collections;

public class ReplicatePrefabAlongParticle : MonoBehaviour
{
    public GameObject prefab;
    ParticleSystem.Particle[] particles;
    GameObject[] pool;

    void Start ()
    {
        particles = new ParticleSystem.Particle[GetComponent<ParticleSystem>().maxParticles];
        pool = new GameObject[GetComponent<ParticleSystem>().maxParticles];
        for (var i = 0; i < GetComponent<ParticleSystem>().maxParticles; i++)
        {
            pool[i] = Instantiate (prefab) as GameObject;
        }
    }
    
    void Update ()
    {
        var count = GetComponent<ParticleSystem>().GetParticles (particles);
        for (var i = 0; i < count; i++)
        {
            pool[i].transform.position = particles[i].position;
			pool[i].transform.localRotation = Quaternion.AngleAxis(particles[i].rotation, particles[i].axisOfRotation);
			pool[i].transform.localScale = Vector3.one * particles[i].size;
            pool[i].GetComponent<Renderer>().enabled = true;
        }
        for (var i = count ; i < pool.Length; i++)
        {
            pool[i].GetComponent<Renderer>().enabled = false;
        }
    }
}
