using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiderWeb;

namespace Diluvion
{
    public enum RoundingBias
    {
        Floor,
        Even,
        Ceil
    }

    /// <summary>
    /// Surface material contains references to all the sounds, particles, and other effects that might
    /// happen when this surface is impacted.
    /// </summary>
    [CreateAssetMenu(fileName = "surface material", menuName = "Diluvion/surface material")]
    public class SurfaceMaterial : ScriptableObject
    {

        /// <summary>
        /// The objects to spawn when impacted. These are in order of softest to hardest impact.
        /// </summary>
        [Tooltip("The objects to spawn when impacted. These are in order of softest to hardest impact.")]
        public List<GameObject> impacts = new List<GameObject>();

        /// <summary>
        /// The magnitude corresponding to the last object in the impact list. 
        /// </summary>
        [Tooltip("The magnitude corresponding to the last object in the impact list.")]
        public float maxMagnitude = 10;

        [Tooltip("The lifetime of the spawned particles.")]
        public float lifetime = 5;

        /// <summary>
        /// String linked to a wwise sfx
        /// </summary>
        public string impactSound;


      
        public GameObject GetImpact(float normalized = 0, RoundingBias bias = RoundingBias.Even)
        {
            float lerp = normalized;
            lerp = Mathf.Clamp01(lerp);
            switch (bias)
            {
                case RoundingBias.Floor:
                {
                    lerp = Mathf.FloorToInt(lerp * impacts.Count);
                    break;
                }
                case RoundingBias.Ceil:  
                {
                    lerp = Mathf.CeilToInt(lerp * impacts.Count);
                    break;
                }
                default:
                {
                    lerp = Mathf.RoundToInt(lerp * impacts.Count);
                    break;
                }
            }
            lerp = Mathf.Clamp(lerp, 0, impacts.Count - 1);
            
          //  Debug.Log("Requested: " + normalized + " index:" + (int)lerp);
            return impacts[(int)lerp];
        }
        
        /// <summary>
        /// Spawns a new impact particle at the given point and vector.
        /// </summary>
        /// <returns></returns>
        public GameObject NewImpact(Vector3 impactVector, Vector3 point)
        {
            // Choose the right particle based on impact magnitude
            float mag = impactVector.magnitude;
            float lerp = mag / maxMagnitude;
            
            GameObject prefab = GetImpact(lerp);

            // Spawn the particle
            Quaternion spawnRotation = Quaternion.identity;
            if(impactVector!=Vector3.zero)
                spawnRotation = Quaternion.LookRotation(-impactVector);

            Transform newSpawn = GameManager.Pool().Spawn(prefab, point, spawnRotation);
            if(lifetime!=0)
                GameManager.Pool().Despawn(newSpawn, lifetime);

            // play the sfx
            SpiderSound.MakeSound(impactSound, newSpawn.gameObject);

            // return the instance
            return newSpawn.gameObject;
        }

       
    }
}