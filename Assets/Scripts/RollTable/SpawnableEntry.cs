using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Diluvion;

namespace Diluvion.Roll
{
    /// <summary>
    /// Base extension for any entry that ends up spawning something in the world
    /// </summary>
    
    public class SpawnableEntry : Entry
    {
        [Range( .1f, 2), Tooltip("How many spawn slots this takes up. Placers can spawn 3-6 objects of 1 slot size, but for example if you want to spawn" +
                 " a lot of something, like a mini-mine or a swarmer, set it's slot size smaller.")]
        public float slotSize = 1;
        
        [FoldoutGroup("Spawnable", true, 300), AssetsOnly]
        public GameObject prefab;

        [FoldoutGroup("Spawnable")]
        public PopResources resourceCost;

        public bool ignoreSpawnerRotation;

        [Tooltip("The types of positions this spawnable can be in ")]
        [FoldoutGroup("Spawnable"), AssetList]
        public List<RandomSpotFinder> positionSearches = new List<RandomSpotFinder>();
        
        int _legalPosTries = 15;


        /// <summary>
        /// Finds a valid position for the spawnable's prefab.size within the caller's radius
        /// </summary>
        /// <param name="spawnTransform"></param>
        /// <param name="callerRadius">The radius multiplier of the caller (Explorableplacer)</param>
        /// <param name="finalPosition">The valid return position</param>
        /// <param name="finalRotation">The valid return rotation</param>
        /// <returns></returns>
        public virtual bool ValidPosition(Transform spawnTransform, int callerRadius, out Vector3 finalPosition, out Quaternion finalRotation)
        {
            finalPosition = spawnTransform.position;
            
            finalRotation = ignoreSpawnerRotation ? Quaternion.identity : spawnTransform.rotation;

          // Debug.Log("Looking up Valid position for " + this.name + " with " + positionSearches.Count + " searches." + "  of size " + callerRadius);

            //SpotFinders are valid random spot generators, they return true when they have found a legal position, along with the position itself
            if (positionSearches.Count < 1) { Debug.LogWarning(this.name + " Does not have any position searches and can't be spawned"); return false; }
            foreach (RandomSpotFinder dsp in positionSearches)
            {
                if (dsp.FoundPosition(spawnTransform, (int)Width(), callerRadius, ref finalPosition, ref finalRotation))
                {               
                    return true;
                }
            }
            finalPosition = Vector3.zero;
            finalRotation = Quaternion.identity;
            Debug.Log("no legal spawn position for " + this.name, this);
            return false;
        }

        public virtual GameObject Create(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;
            Spawnable targetSpawnable = prefab.GetComponent<Spawnable>();
            Vector3 targetPos = position;
            if (targetSpawnable!= null)
            {
                
                if (targetSpawnable.useBoundingBasePos)
                    targetPos += rotation*targetSpawnable.offsetFromFloor;
            }
            return Instantiate(prefab.gameObject, targetPos, rotation, parent);
        }

        /// <summary>
        /// The technical resource cost of this
        /// </summary>
       /* public override int TechCost()
        {
            return resourceCost.techCost;
        }*/

        /// <summary>
        /// The gold value of this
        /// </summary>
        public override int Value()
        {
            return resourceCost.value;
        }

        public override int Danger()
        {
            return resourceCost.danger;
        }

        public virtual Vector3 LocalRoot()
        {
            return SpawnablePrefab<Spawnable>().localRoot;
        }

        /// <summary>
        /// Returns the radius of a sphere that fully encapsulates all the colliders of the prefab.
        /// If no prefab is defined, returns 0.
        /// </summary>
        public virtual float Width()
        {
            if (!SpawnablePrefab<Spawnable>()) return 0;

            return SpawnablePrefab<Spawnable>().Width();          
        }

        /// <summary>
        /// Returns the radius of a sphere that fully encapsulates all the colliders of the prefab.
        /// If no prefab is defined, returns 0.
        /// </summary>
        public virtual float Height()
        {
            if (!SpawnablePrefab<Spawnable>()) return 0;
            return SpawnablePrefab<Spawnable>().Height();
        }

        public virtual GameObject Prefab()
        {
            return prefab;
        }

        public T SpawnablePrefab<T>() where T : Spawnable
        {
            if (!Prefab()) return null;
            return Prefab().GetComponent<T>();
        }

        /// <summary>
        /// Additional processing on the object
        /// </summary>
        public virtual SpawnableEntry Process(PopResources resources)
        {
            return this;
        }

        /// <summary>
        /// If this Spawnable is smaller or equal than the input size
        /// </summary>
        /// <param name="inputSize"></param>
        /// <returns></returns>
        public bool ThinnerOrEqualWidth(int inputSize)
        {
            //Debug.Log(name + " input size: " + inputSize + ", width: " + Width());

            if (inputSize >= Width()) return true;
           //  Debug.Log(this.name + " is too large " + inputSize + " / " + size, this);
            return false;
        }

        public bool EqualWidth(int inputSize)
        {        
            if (inputSize != Width()) return false;
            return true;
        }

        /// <summary>
        ///Checks all the possible positionSearches for a valid direction, if there isnt any return false
        /// </summary>
        public bool CanSpawnDirection(Vector3 dir)
        {
            if (positionSearches == null || positionSearches.Count < 1) return true; // If not specified, allow spawn any direction
            for (int i = 0; i < positionSearches.Count; i++)
            {
                RandomSpotFinder dsf = positionSearches[i];
                if (dsf == null) continue;
                if (dsf.CanSpawnDirection(dir))
                    return true;
            }
            //Debug.Log("Could not find a spawn location in direction " + dir, this);
            return false;
        }
    }
}
