using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Diluvion.Ships;

namespace Diluvion
{

    public class Carrier : ArkCreature
    {
        [FoldoutGroup("Movement")]
        public float patrolRadius = 5;

        [FoldoutGroup("carrier spawns")]
        [Tooltip("How long does it take between spawning individual swarmers?")]
        public float individualSpawnCooldown;
        
        [FoldoutGroup("carrier spawns")]
        [Tooltip("How long is the cooldown between spawning sequences")]
        public float spawningCooldown;
        
        [FoldoutGroup("carrier spawns")]
        public float ejectionDelay = 2;
        [FoldoutGroup("carrier spawns")]
        public float ejectionForce = 500;
        [FoldoutGroup("carrier spawns")]
        public List<Transform> spawnPoints;
        [FoldoutGroup("carrier spawns")]
        public GameObject swarmerPrefab;
        
        [ReadOnly]
        public List<Hull> swarmerHulls = new List<Hull>();

        Vector3 _initSpawnerScale;

        bool _canSpawn;

        void OnDrawGizmosSelected()
        {

            if (Application.isPlaying)
                Gizmos.DrawWireSphere(startingPosition, patrolRadius);
            else
                Gizmos.DrawWireSphere(transform.position, patrolRadius);
        }

        protected override void InitValues()
        {
            base.InitValues();
            _initSpawnerScale = spawnPoints[0].transform.localScale;
            foreach (Transform t in spawnPoints) t.localScale = Vector3.zero;

            sphereRadius = patrolRadius;
            UpdateDestination(true, startingPosition);
            _canSpawn = true;
            swarmerHulls = new List<Hull>();
        }


        protected override void Update()
        {
            base.Update();
            RotateToForwardMovement();
        }

        protected override void RotateToForwardMovement()
        {
            //look towards velocity direction
            Vector3 flatVelocity = new Vector3(myRigidBody.velocity.x, 0, myRigidBody.velocity.z);
            if (flatVelocity == Vector3.zero) return;
            Quaternion look = Quaternion.LookRotation(flatVelocity);
            rotator.transform.rotation = Quaternion.Slerp(rotator.transform.rotation, look, Time.deltaTime * rotationSpeed);
        }

        public override void SetTarget(Transform newTarget)
        {
            if (sleeping) return;
            base.SetTarget(newTarget);
            target = newTarget;
            if (_canSpawn) StartCoroutine(SpawnSequence());
        }

        protected override void FoundBridge(Bridge bridge)
        {
            base.FoundBridge(bridge);
            SetTarget(bridge.transform);
        }

        protected override void OnDestinationReached()
        {
            base.OnDestinationReached();
            UpdateDestination(true, startingPosition);
        }

        public override void DamageTaken()
        {
            base.DamageTaken();
            TargetCheck(120);
        }

        IEnumerator SpawnSequence()
        {

            if (!_canSpawn) yield break;
            if (sleeping) yield break;
            _canSpawn = false;
            SetAnimationSpeed(5);

            int numberSpawned = 0;
            glowIntensity = 0;

            // Spawn the mans
            while (numberSpawned < spawnPoints.Count)
            {
                StartCoroutine(SpawnSwarmer(spawnPoints[numberSpawned]));
                numberSpawned++;
                glowIntensity = (numberSpawned * 1.0f / spawnPoints.Count * 1.0f) * 3f;

                //Debug.Log((numberSpawned * 1.0f / spawnPoints.Count * 1.0f) * 4f + " is the intensity");
                yield return new WaitForSeconds(individualSpawnCooldown);
            }
            SetAnimationSpeed(1);
            yield return new WaitForSeconds(spawningCooldown);
            _canSpawn = true;
            StartCoroutine(SpawnSequence());
        }

        IEnumerator SpawnSwarmer(Transform fromPoint)
        {
            // Scale up to give the effect of the swarmer growing
            float progress = 0;
            while (progress < 1)
            {
                fromPoint.localScale = Vector3.Lerp(Vector3.zero, _initSpawnerScale, progress);
                yield return null;
                progress += Time.deltaTime;
            }

            // Wait for the ejection delay
            yield return new WaitForSeconds(ejectionDelay);

            // Spawn the swarmer
            GameObject newSwarmer = GameManager.Pool().Spawn(swarmerPrefab, fromPoint.position, fromPoint.rotation).gameObject;
            newSwarmer.GetComponent<Collider>().enabled = false;

            // reset scale of the spawn point
            fromPoint.transform.localScale = Vector3.zero;

            // Find components
            Rigidbody swarmerRB = newSwarmer.GetComponent<Rigidbody>();
            Swarmer swarmer = newSwarmer.GetComponent<Swarmer>();
            Hull swarmerHull = newSwarmer.GetComponent<Hull>();
            swarmerHull.myDeath += LostSwarmer;
            swarmerHulls.Add(swarmerHull);

            yield return new WaitForEndOfFrame();
            swarmer.SetTarget(target);

            // add ejection force
            swarmerRB.AddRelativeForce(Vector3.up * ejectionForce);

            yield return new WaitForSeconds(.5f);
            if (newSwarmer == null) yield break;
            // turn collider back on
            newSwarmer.GetComponent<Collider>().enabled = true;

            yield break;
        }

        /// <summary>
        /// Lost a swarmer. mourn, remove from list, move on
        /// </summary>
        void LostSwarmer(Hull swarmerLost, string why)
        {
            if (swarmerHulls.Contains(swarmerLost))
                swarmerHulls.Remove(swarmerLost);
        }


        //Kill all babies that belong to this carrier
        protected override void OnDisable()
        {
            base.OnDisable();
            KillSwarmers();
        }

        /// <summary>
        /// Kills all my babies
        /// </summary>
        void KillSwarmers()
        {
            if (swarmerHulls == null) return;
            // int swarmerCount = swarmerHulls.Count;
            List<Hull> removeSwarmerH = new List<Hull>(swarmerHulls);

            foreach (Hull h in removeSwarmerH)
                h.SelfDestruct();
        }
    }
}