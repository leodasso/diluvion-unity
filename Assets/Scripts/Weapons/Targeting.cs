/*

using UnityEngine;
using System.Collections;
using HeavyDutyInspector;
using Diluvion.Sonar;

namespace Diluvion
{

    [RequireComponent(typeof(BoxCollider))]
    public class Targeting : MonoBehaviour
    {
        public float armingTime = 0.4f;
        public bool randomArmingTime = false;

        [HideConditional(true, "randomArmingTime", true)]
        public float minArmingTime = 0;

        public bool leading = false;
        public Vector3 targetLoc;
        public SonarStats target;
        public Drive drive;
        [Range(0, 1)]
        public float successChance = .5f;   //between 0 and 1
        [Range(0, 1)]
        public float chargeAmount = 1;
        public enum TorpedoFails { EarlyDestruct, SpinOut };
        public bool armed = false;

        SphereCollider proximityTrigger;
        Munition munition;
        Explosive explosive;
        int proxLayerInt = 0;
        Rigidbody rb;
        bool failed = false;
        Vector3 initScale;


        void Awake()
        {

            initScale = transform.localScale;
            //set up projectile's collider       
            drive = GetComponent<Drive>();
            explosive = GetComponent<Explosive>();
            munition = GetComponent<Munition>();
            proxLayerInt = LayerMask.NameToLayer("Munition");
            rb = GetComponent<Rigidbody>();
        }


        public IEnumerator Arming()
        {
            armed = false;

            //set explosive based on charge amount
            //explosive.multiplier = chargeAmount;
            rb.angularDrag = .02f;
            yield return new WaitForFixedUpdate();

            Vector3 torque = new Vector3(0, 4500, -60000);
            rb.AddRelativeTorque(torque * (.2f + chargeAmount));

            float scale = .7f + chargeAmount * 1.5f;

            Vector3 totalScale = new Vector3(initScale.x * scale, initScale.y * scale, initScale.z);
            transform.localScale = totalScale;

            yield return new WaitForSeconds(.3f);

            rb.angularDrag = 1;

            if (target) SetTargetLocation(target.transform.position);

            float waitTime = armingTime;
            if (randomArmingTime)
                waitTime = Random.Range(minArmingTime, armingTime);

            yield return new WaitForSeconds(waitTime);

            // Determine if rolled success
            RollForHitChance();

            armed = true;
            drive.driving = true;
            yield return true;
        }

        Vector3 currentVel;
        void Update()
        {
            if (failed) { Debug.DrawRay(transform.position, Vector3.up * 15, Color.red, 0.01f); return; }

            //If my target no longer exists
            if (target == null)
            {
                StartCoroutine(SpinOut());
                failed = true;
                return;
            }

            if (armed)
            {

                if (target)
                {
                    Vector3 tLoc = target.transform.position;
                    if (leading)
                    {
                        currentVel = MyVelocity();
                        SetTargetLocation(tLoc + (target.Lead(transform, currentVel, drive.CurrentSpeed()) - tLoc) / 2);
                    }
                    else SetTargetLocation(tLoc);
                }
                else SetTargetLocation(transform.forward * 5);
            }

            //  Debug.DrawLine(transform.position, targetLoc, Color.red, 0.01f);
            if (!target.gameObject.activeInHierarchy)
            {
                StartCoroutine(SpinOut());
                failed = true;
            }

        }


        void RollForHitChance()
        {

            float dice = Random.Range(0.0f, 1.0f);

            //If the dice is less than success chance, then it was successful!  
            if (dice <= successChance)
            {
                //PLAY SUCCESS SOUND
                GetComponent<AKTriggerPositive>().TriggerPos();
                return;
            }

            //Cause a failure
            int failTypes = System.Enum.GetValues(typeof(TorpedoFails)).Length;
            int failIndex = Random.Range(0, failTypes);

            TorpedoFails myFailType = (TorpedoFails)failIndex;

            StartCoroutine(myFailType.ToString());
        }

        IEnumerator EarlyDestruct()
        {

            MakeFailNoise();

            float waitTime = 1; // seconds

            yield return new WaitForSeconds(waitTime);
            drive.Break();
            yield return new WaitForSeconds(waitTime);
            //munition.Impact();

            yield break;
        }

        IEnumerator SpinOut()
        {

            MakeFailNoise();

            float waitTime = 1;
            float spinTime = 2;
            float initSpinTime = spinTime;
            Vector3 spinTorque = new Vector3(200, 150, 250);

            yield return new WaitForSeconds(waitTime);

            while (spinTime > 0)
            {

                float intensity = initSpinTime - spinTime;
                rb.AddRelativeTorque(spinTorque * intensity);
                spinTime -= Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            //munition.Impact();

            yield break;
        }

        void MakeFailNoise()
        {
            if (GetComponent<AKTriggerNegative>())
                GetComponent<AKTriggerNegative>().TriggerNeg();
        }

        void OnDespawned()
        {
            if (proximityTrigger)
                proximityTrigger.enabled = false;

            failed = false;
            target = null;
            armed = false;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        void SetTargetLocation(Vector3 v3)
        {
            targetLoc = v3;
            drive.targetLoc = targetLoc;
            Debug.DrawLine(transform.position, targetLoc, Color.red, 0.01f);

        }

        PseudoVelocity psv;
        Vector3 MyVelocity()
        {
            if (psv != null) return psv.MyVelocity();
            psv = GetComponent<PseudoVelocity>();

            if (psv != null) return psv.MyVelocity();
            psv = gameObject.AddComponent<PseudoVelocity>();
            psv.velocitySamples = 1;

            return psv.MyVelocity();
        }
    }
}
*/