using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using UnityEngine.UI.Extensions;

namespace Diluvion
{

    public class Jaeger : ArkCreature
    {

        [FoldoutGroup("Attack"), Tooltip("The total amount of shots fired before self destructing")]
        public int totalShots;
        public List<Transform> weaponPoints;
        public Transform gatlingRotator;
        public float gatlingRotationSpeed = 3;
        [FoldoutGroup("Attack"), Tooltip("Cooldown between individual shots")]
        public float shotCooldown;              // Cooldown between shots in gatling mode
        [FoldoutGroup("Attack"), Tooltip("The time it takes to warmup between acquiring target and beginning to fire")]
        public float shotWarmup;                // Warmup between acquiring target and firing
        [FoldoutGroup("Attack"), Tooltip("The recoil force from each shot fired")]
        public float recoilForce = 50;
        [FoldoutGroup("Attack"), Tooltip("How far I get from the target before beginning the attack")]
        public float attackRadius = 10;
        
        [FoldoutGroup("Movement"), Tooltip("The force applied to jumping back when I awaken")]
        public float awakeForce = 1000;
        public float selfDestructTime = 5;
        int _shotsFired;
        bool _firing;
        bool _selfDestructing;
        float _gatlingRotation;
        bool _gatling;                           // am I doing the gatling behavior
        bool _awake;

        protected override void InitValues()
        {
            base.InitValues();
            _shotsFired = 0;
        }

        protected override void Update()
        {
            if (target == null) return;

            base.Update();
            if (!_selfDestructing) RotateToTarget();

            if (_gatling)
            {
                _gatlingRotation += Time.deltaTime * gatlingRotationSpeed;

                if (glowIntensity < 3)
                    glowIntensity += Time.deltaTime;
            }

            gatlingRotator.transform.Rotate(0, 0, _gatlingRotation * Time.deltaTime, Space.Self);

            if (!_firing)
                MoveToTarget(attackRadius);
        }

        protected override void FixedUpdate()
        {
            SleepCheck();
            if (target == null || _gatling || _selfDestructing) return;
            base.FixedUpdate();
        }

        protected override void FoundBridge(Bridge bridge)
        {
            base.FoundBridge(bridge);
            SetTarget(bridge.transform);
        }

        public override void SetTarget(Transform newTarget)
        {
            if (_awake) return;
            base.SetTarget(newTarget);
            myRigidBody.isKinematic = false;
            target = newTarget;
            Awaken();
        }

        public override void DamageTaken()
        {
            base.DamageTaken();
            if (target == null) TargetCheck(100);

        }

        void OnCollisionEnter(Collision collision)
        {

            if (target == null) return;
            if (!_selfDestructing) StartCoroutine(GatlingFire());
        }

        void Awaken()
        {
            if (_awake) return;
            myRigidBody.AddForce(transform.forward * awakeForce);
            _awake = true;
        }

        protected override void OnDestinationReached()
        {
            base.OnDestinationReached();
            StartCoroutine(GatlingFire());
        }

        IEnumerator GatlingFire()
        {

            if (_gatling) yield break;
            animator.SetTrigger("firing");
            _gatling = true;

            yield return new WaitForSeconds(shotWarmup);

            _firing = true;

            AlignWeapons();

            while (_shotsFired < totalShots)
            {
                FireOneShot();
                yield return new WaitForSeconds(shotCooldown);
            }

            _firing = false;

            StartCoroutine(SelfDestruct());
            yield break;
        }


        IEnumerator SelfDestruct()
        {
            _selfDestructing = true;
            float timer = 0;

            myRigidBody.drag = 1;
            myRigidBody.angularDrag = .2f;

            while (timer < selfDestructTime)
            {

                yield return null;
                _gatlingRotation += Time.deltaTime;
                timer += Time.deltaTime;

                Debug.Log("destruct timer: " + timer);

                // give a wobble to the rotation
                gatlingRotator.transform.Rotate(0, 0, _gatlingRotation * gatlingRotationSpeed * Time.deltaTime, Space.Self);
            }

            //explode
            GetComponent<Hull>().SelfDestruct();
            //GetComponent<Hull>().Damage(999, transform.position, gameObject);
            yield break;
        }

        /// <summary>
        /// Aligns the weapons to point at a point attackRadius away
        /// </summary>
        void AlignWeapons()
        {
            if (!target) return;

            // Find the point
            Ray toTarget = new Ray(transform.position, rotator.transform.forward * attackRadius);
            Vector3 aimPoint = toTarget.GetPoint(attackRadius);

            foreach (Transform t in weaponPoints)
                t.LookAt(aimPoint);
        }

        void FireOneShot()
        {

            int totalWeapons = weaponPoints.Count;

            // Get the right weapon index by getting the remainder of shots fired / total weapons.
            int fireIndex = _shotsFired % totalWeapons;

            // Pick the appropriate weapon point
            Transform currentWeapon = weaponPoints[fireIndex];

            // Add force at a point to give the weapon recoil
            Vector3 recoilV3 = currentWeapon.forward * -1 * recoilForce;
            myRigidBody.AddForceAtPosition(recoilV3, currentWeapon.position);
            glowIntensity += 0.03f;
            FireWeapon(currentWeapon, false);
            _shotsFired++;
        }
    }
}