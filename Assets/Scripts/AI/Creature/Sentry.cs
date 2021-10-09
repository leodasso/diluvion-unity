using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using Diluvion.Ships;

namespace Diluvion
{

    /// <summary>
    /// Sentry is an ark creature that patrols an area with a searchlight. If pinged,
    /// it'll aim the searchlight right at what pinged. If it holds the searchlight over the target
    /// long enough, it'll send out a warning to all other ark creatures. As long as it has a target,
    /// it will remain still, and point the light at the target.
    /// </summary>
    public class Sentry : ArkCreature
    {

        [Tooltip("If the target is within this distance, I'll detect it whether it's in the spotlight or not.")]
        public float tooCloseRadius = 15;
        
        [FoldoutGroup("Movement")]
        public float patrolRadius;
        [Tooltip("How long must it hold light over target before it sends out a warning?")]
        public float acquireTime = 5;
        [Tooltip("Time it takes to forget a target")]
        public float forgetTargetTime = 10;
        [Tooltip("The torque applied to me when I'm shot")]
        public float spinTorque = 500;
        public GameObject warningTriggerPrefab;
        public Light spotLight;
        public float minDotForDetection = .9f;

        bool canMove = true;
        float acquireProgress = 0;
        bool canRotate = true;


        void OnDrawGizmosSelected()
        {

            Gizmos.color = Color.white;
            if (Application.isPlaying)
                Gizmos.DrawWireSphere(startingPosition, patrolRadius);
            else
                Gizmos.DrawWireSphere(transform.position, patrolRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * detectionRadius);
        }

        protected override void InitValues()
        {
            base.InitValues();

            sphereRadius = patrolRadius;
            UpdateDestination(true, startingPosition);
        }

        protected override void Update()
        {
            base.Update();

            spotLight.range = detectionRadius + 10;

            if (!canRotate) return;
            if (target) RotateToTarget();
            else RotateToForwardMovement();
        }

        protected override void FixedUpdate()
        {
            if (canMove)
                base.FixedUpdate();
        }

        public void OnPinged(Bridge bridge)
        {
            SetTarget(bridge.transform);
        }

        /// <summary>
        /// Sets the target to the given transform.
        /// </summary>
        public override void SetTarget(Transform newTarget)
        {
            if (!CanSeeTarget(newTarget)) return;
            if (target) return;
            if (Vector3.Distance(newTarget.position, transform.position) > detectionRadius) return;

            Debug.Log("Target sighted: " + newTarget.name + " suspicious...");

            base.SetTarget(newTarget);
            target = newTarget;
            canMove = false;

            StartCoroutine(ForgetTarget(forgetTargetTime));
            StartCoroutine(FlashTheLight());
            StartCoroutine(ConfirmTarget());
        }

        IEnumerator ForgetTarget(float delay)
        {

            yield return new WaitForSeconds(delay);
            target = null;
            canMove = true;
        }

        /// <summary>
        /// Flashs the light blinky blinky as a visual cue that the player has been spotted.
        /// </summary>
        IEnumerator FlashTheLight()
        {

            float initLightIntensity = spotLight.intensity;
            float flashSpeed = .2f;

            while (true)
            {
                spotLight.intensity = initLightIntensity * 2;
                yield return new WaitForSeconds(flashSpeed);
                spotLight.intensity = initLightIntensity / 2;
                yield return new WaitForSeconds(flashSpeed);

                if (target == null)
                {
                    spotLight.intensity = initLightIntensity;
                    yield break;
                }
            }
        }


        /// <summary>
        /// If the target is visible for the acquire time, will confirm target, send out warning.
        /// </summary>
        IEnumerator ConfirmTarget()
        {

            float progress = 0;

            // Take time to acquire the target.
            while (progress < acquireTime)
            {
                // If the target went behind terrain, lose it.
                if (!CanSeeTarget(target))
                {
                    Debug.Log("Target " + target.name + " lost after " + progress + " seconds.");
                    StartCoroutine(ForgetTarget(2));
                    yield break;
                }

                progress += Time.deltaTime;
                yield return null;
            }

            // Send out a warning
            SendWarning();
        }

        void SendWarning()
        {
            GetComponent<AKTriggerCallback>().Callback();
            GameObject newTrigger = Instantiate(warningTriggerPrefab, transform.position, Quaternion.identity) as GameObject;
            ArkCreatureTrigger triggerScript = newTrigger.GetComponent<ArkCreatureTrigger>();

            triggerScript.newTarget = target;
        }

        /// <summary>
        /// Returns true if the given target is within the spotlight.
        /// </summary>
        bool TargetInSpotlight(Transform t)
        {

            if (t == null) return false;

            Vector3 directionToTarget = t.transform.position - transform.position;
            float targetDOT = Vector3.Dot(rotator.transform.forward.normalized, directionToTarget.normalized);

            return targetDOT >= minDotForDetection;
        }


        public override void DamageTaken()
        {
            base.DamageTaken();
            StartCoroutine(Spinout());
        }

        IEnumerator Spinout()
        {

            yield return new WaitForFixedUpdate();
            canRotate = false;
            myRigidBody.AddTorque(new Vector3(0, spinTorque, 0));
            TargetCheck(120);
            yield return new WaitForSeconds(2);
            myRigidBody.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.identity;
            canRotate = true;
        }

        protected override void FoundBridge(Bridge bridge)
        {

            // If the target isn't in the spotlight and is farther than the near radius, ignore it.
            if (!TargetInSpotlight(bridge.transform) &&
                (Vector3.Distance(bridge.transform.position, transform.position) > tooCloseRadius)) return;

            base.FoundBridge(bridge);
            SetTarget(bridge.transform);
        }


        protected override void OnDestinationReached()
        {
            base.OnDestinationReached();
            UpdateDestination(true, startingPosition);
        }
    }
}