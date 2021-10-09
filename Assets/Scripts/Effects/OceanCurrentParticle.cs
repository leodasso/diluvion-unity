using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Diluvion
{

    public class OceanCurrentParticle : MonoBehaviour
    {

        public static OceanCurrentParticle oceanCurrentParticle;
        public float maxSpeed = 80;
        public List<OceanCurrent> currents = new List<OceanCurrent>();  // any currents the player is in
        public float streamSpeedMultiplier = 2;

        Quaternion streamRotation;
        float streamVelocity = 0;

        ParticleSystem particle;

        public static bool Exists()
        {
            return oceanCurrentParticle != null;
        }

        public static OceanCurrentParticle Get()
        {
            if (oceanCurrentParticle == null)
                oceanCurrentParticle = FindObjectOfType<OceanCurrentParticle>();
            return oceanCurrentParticle;
        }
        
        

        // Use this for initialization
        void Start()
        {
            particle = GetComponent<ParticleSystem>();
            particle.Stop();
        }


        // Update is called once per frame
        void Update()
        {

            transform.rotation = Quaternion.Slerp(transform.rotation, streamRotation, Time.deltaTime * 5);

            float speed = Mathf.Clamp(streamVelocity * streamSpeedMultiplier, 0, maxSpeed);
            particle.startSpeed = speed;
        }


        public void EnterStream(OceanCurrent currentStream)
        {
            if (currentStream == null) return;

            particle.Play();
            streamRotation = currentStream.transform.rotation;
            if (!currents.Contains(currentStream))
            {
                currents.Add(currentStream);
                if(currentStream.parameters)
                    streamVelocity = currentStream.parameters.force;
            }
        }


        public void ExitStream(OceanCurrent currentStream)
        {

            if (currents.Contains(currentStream)) currents.Remove(currentStream);
            if (particle == null) return;
            if (currents.Count < 1) particle.Stop();
        }
        public void Reset()
        {
            if (particle == null) return;
            particle.Stop();
            currents = new List<OceanCurrent>();
        }

    }
}