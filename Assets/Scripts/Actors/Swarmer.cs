using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using Diluvion.Ships;

namespace Diluvion
{

    /// <summary>
    /// An ark creature that typically travel in groups. Swarmers fly erratically toward their target to perform a kamikaze attack.
    /// </summary>
    public class Swarmer : ArkCreature
    {
        public float damage = 2;
        public float hoverRadius = 3;

        [DynamicRange(0, "maxWaitTime")]
        public float minWaitTime = 1;

        [DynamicRange("minWaitTime", 30)]
        public float maxWaitTime = 2;

        bool _accelerating = false;
        float maxAnimatorSpeed = 20;
        float _waitTimer = 0;
        //Hull _hull;

        void OnDrawGizmosSelected()
        {
            if (!target) return;

            Gizmos.color = new Color(0, 1, 0, .2f);
            Gizmos.DrawSphere(target.position, hoverRadius);

            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawWireSphere(destination, minDistToDestination);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            myRigidBody.isKinematic = true;

            StartCoroutine(EnableCollision());

            if (!InRangeOfPlayer()) Sleep();
        }


        /// <summary>
        /// Wait and enable collider as a failsafe, to make sure that if the spawn sequence of a carrier is interrupted,
        /// i still have a collider.
        /// </summary>
        /// <returns></returns>
        IEnumerator EnableCollision()
        {
            yield return new WaitForSeconds(.5f);
            GetComponent<Collider>().enabled = true;
        }

        protected override void InitValues()
        {
            base.InitValues();
            animator.speed = Random.Range(.2f, .5f);
        }

        public override void SetTarget(Transform newTarget)
        {
            base.SetTarget(newTarget);
            target = newTarget;
            myRigidBody.isKinematic = false;
            StartCoroutine(SetNewPoint());
            UpdateDestination(true);

            if (newTarget == null) return;
            if (newTarget != PlayerManager.PlayerShip().transform) return;
            
            CombatMusic.CreatureCombat();
            CombatMusic.ShotsFired(transform.position);
            //CombatZone.Get().SetPlayerFiredUpon();
        }

        protected override void EngineSound()
        {
            if (GetComponent<AKTriggerCallback>())
                GetComponent<AKTriggerCallback>().Callback();
        }

        void AwakenSound()
        {
            EngineSound();
            if (GetComponent<AKTriggerPositive>())
                GetComponent<AKTriggerPositive>().TriggerPos();

        }


        protected override void FoundBridge(Bridge bridge)
        {
            base.FoundBridge(bridge);
            AwakenSound();
            SetTarget(bridge.transform);
        }


        /// <summary>
        /// Waits a random time between min and max wait time, then sets a new random point on sphere around the target.
        /// </summary>
        IEnumerator SetNewPoint()
        {
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            UpdateDestination(true);
            StartCoroutine(SetNewPoint());

            if (target == PlayerManager.PlayerShip())
            {
                CombatMusic.CreatureCombat();
                CombatMusic.ShotsFired(transform.position);
            }
        }


        protected override void Update()
        {
            if (!target) return;
            else if (myRigidBody.isKinematic) myRigidBody.isKinematic = false;

            base.Update();

            sphereRadius = hoverRadius;

            RotateToForwardMovement();

            // Change animator speed based on how close I am to target
            float distToTarget = Vector3.Distance(transform.position, target.transform.position);
            float normalizedDist = Mathf.Clamp01(distToTarget / 20);
            float animSpeed = Mathf.Lerp(maxAnimatorSpeed, 1, normalizedDist);
            animator.speed = animSpeed;
        }

        protected override void OnDestinationReached()
        {
            base.OnDestinationReached();
            UpdateDestination(true);
        }


        // When hitting the target, do damage and destroy myself
        void OnCollisionEnter(Collision other)
        {
            // if hit a creature, just bounce off
            ArkCreature creature = other.gameObject.GetComponent<ArkCreature>();
            if (creature)
            {
                return;
            }
            
            
            Impactable impactable = other.gameObject.GetComponent<Impactable>();
            if (impactable != null) impactable.Impact(myRigidBody.velocity, other.contacts[0].point);

            // deal damage to the other thing
            IDamageable damageAble = other.gameObject.GetComponent<IDamageable>();
            if (damageAble == null) return;

            damageAble.Damage(damage, 1, gameObject);
            MyHull().SelfDestruct();
            //if (_hull) _hull.SelfDestruct();
        }

        protected override void FixedUpdate()
        {
            SleepCheck();
            if (!target) return;
            base.FixedUpdate();
        }
    }
}