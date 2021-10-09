using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiderWeb;
using HeavyDutyInspector;


namespace Diluvion.Roll
{
    /// <summary>
    /// ScriptableObject that finds a spot that works based on the input object
    /// </summary>
    public abstract class RandomSpotFinder : ScriptableObject
    {
        public Vector3 dimensionMultiplier = new Vector3(.5f, .5f, 1f);
        public int maxTryCount = 15;
        public List<SpawnCheck> checks = new List<SpawnCheck>();
        [Button("Explain conditions", "ExplainConditions", true)]
        public bool conditions;

        protected int tryCount = 0;


        /// <summary>
        /// Base FoundPosition Loop, tries maxtryCount times to find a position by performing validchecks on random spots within the size
        /// </summary>
        public virtual bool FoundPosition(Transform referenceTransform, int spawnableRadius, int areaRadius, ref Vector3 position, ref Quaternion rotation)
        {
            tryCount = 0;

            //We will try to find position maxTryCount times
            while (tryCount < maxTryCount)
            {
                //For this try we use this working position
                position = RandomSpot(referenceTransform, areaRadius);
                //Debug.Log("Trying Spot: " + position  + " at size " + areaRadius + " with " + spawnableRadius);

                //If we got through our checks and none came up false, we have found a valid position
                if (ValidCheck(position, spawnableRadius, ref position, ref rotation))
                    return true;

                tryCount++;
            }
            Debug.Log("Could not find a valid spot despite " + maxTryCount + " tries");
            return false;
        }

        public void ExplainConditions()
        {
            Debug.Log(ToString(), this);
        }

        /// <summary>
        /// Checks to see if this spotFinder allows spawning in the input direction, based on the directions of the checks
        /// </summary>
        public virtual bool CanSpawnDirection(Vector3 rootDirection)
        {
            //if we have a raycast check, this spotfinder requires a nearby place
            for (int i = 0; i < checks.Count; i++)
            {
                SpawnCheck check = checks[i];
                System.Type checkType = check.GetType();
                if(checkType == typeof(RaycastCheck))
                {
                    if (rootDirection == Vector3.zero) return false;
                    RaycastCheck dr = check as RaycastCheck;
                    float rootDot = Vector3.Dot(dr.castDirection, rootDirection);
                    //Debug.Log("Checking root " + rootDirection + " and " + dr.castDirection + " / " + rootDot);
                    if (rootDot < 0.1f)
                        return false;
                }

            }
            return true;
        }




        /// <summary>
        /// Base abstract check, this will return true if the input spot  can fit something of size
        /// </summary>
        public abstract bool ValidCheck(Vector3 startPos, float radius, ref Vector3 pos, ref Quaternion rot);
        public abstract override string ToString();
        /// <summary>
        /// Rolls a random spot inside a cube centered at the referenceTransform with dimensions xyz*size
        /// </summary>   
        public Vector3 RandomSpot(Transform referenceTransform, int size)
        {
            return referenceTransform.position + (referenceTransform.rotation * Chance.RandomPointOnRectangle((int)size * dimensionMultiplier.x, (int)size * dimensionMultiplier.y, (int)size * dimensionMultiplier.z));
        }
    }
}
