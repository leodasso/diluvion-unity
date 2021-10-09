using UnityEngine;
using System.Collections;
using Diluvion.Ships;
using SpiderWeb;
using Sirenix.OdinInspector;


namespace Diluvion
{

    public enum DroneMode
    {
        passive,
        launch,
        targeting
    }

    [RequireComponent(typeof(Rigidbody))]
    public class Drone : ArkCreature
    {

        [Tooltip("A point is chosen on a sphere of this radius. That point becomes the destination for me, and when I arrive" +
                 "there, I'll begin the attack.")]
        public float targetSphereRadius = 5;    //radius the point is chosen from
        [FoldoutGroup("Movement"), Tooltip("The force applied to the drone away from its target when it's damaged")]
        public float retreatForce = 1000;
        
        [Tooltip("If the target moves farther than this from me, I'll return to passive mode")]
        public float maxTargetDistance = 120;
        
        [FoldoutGroup("Movement")]
        public bool keepByTarget;               //stay at the spherePoint on update if true.
        [Tooltip("When waking up from passive, how long before I begin the position & attack pattern")]
        public float launchTime = 3;
        [FoldoutGroup("Attack"), Tooltip("The number of shots I'll fire before needing to find a new attack position.")]
        public int volleySize = 5;
        [FoldoutGroup("Attack"), Tooltip("Cooldown between shots in a volley")]
        public float fireCooldown = .3f;
        public GameObject weaponPoint;
        public bool playCombatMusic;

        [FoldoutGroup("Movement")]
        public float launchForce = 4000;
        DroneMode mode = DroneMode.passive;
        float _difficultyMeter = 1;      //doubles when HP is low
        float _breathTimeOffset ;
        bool _attacking;


        void OnDrawGizmosSelected()
        {
            if (target)
            {
                Gizmos.color = new Color(.5f, 1, 1, .2f);
                Gizmos.DrawSphere(target.position, targetSphereRadius);
                Gizmos.color = Color.red;
            }
        }

        protected override void InitValues()
        {
            base.InitValues();
            sphereRadius = targetSphereRadius;
            if (animator) animator.speed = .1f;

            if (!InRangeOfPlayer()) Sleep();
            // Set random breath
            _breathTimeOffset = Random.Range(0f, 2f);
        }

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            raycastLayers = Calc.IncludeLayer("Terrain");
            //turn off collision damage
            GetComponent<Hull>().CollisionDamage(false);

        }



        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            //lerp the radius back to default
            sphereRadius = Mathf.Lerp(sphereRadius, targetSphereRadius, Time.deltaTime * 6);

            // For passive drones, create a glow intensity for a 'breathing' effect
            if (mode == DroneMode.passive)
                glowIntensity = Mathf.Sin((Time.time) + _breathTimeOffset) + 1;

            // Targeting mode
            if (target && mode == DroneMode.targeting)
            {
                // If I'm too far from target, go passive.
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (distToTarget >= maxTargetDistance)
                {
                    mode = DroneMode.passive;
                    CombatMusic.RemoveCombatant(gameObject);
                    return;
                }

                //set animator speed 
                if (animator != null)
                    animator.speed = Mathf.Clamp(myRigidBody.velocity.magnitude, .2f, 3.5f * _difficultyMeter);

                RotateToTarget();

                if (keepByTarget) UpdateDestination(false);
            }
        }

        protected override void FixedUpdate()
        {
            if (mode == DroneMode.targeting) base.FixedUpdate();

            else SleepCheck();
        }



        protected override void OnDestinationReached()
        {
            base.OnDestinationReached();
            if (IsTargetLos())
            {
                if (!_attacking) StartCoroutine(FullAttackVolley());
            }
            else
            {
                atDestination = false;
                UpdateDestination(false);
            }
        }

        /// <summary>
        /// Another ark creature in my detection radius has a target - Now that's my target too.
        /// </summary>
        /// <param name="friendsTarget">Friend's target.</param>
        protected override void FriendHasTarget(Transform friendsTarget)
        {
            base.FriendHasTarget(friendsTarget);
            if (mode != DroneMode.passive) return;
            SetTarget(friendsTarget);
        }

        /// <summary>
        /// A bridge has wondered into my detection radius - set it as my target.
        /// </summary>
        /// <param name="bridge">Bridge.</param>
        protected override void FoundBridge(Bridge bridge)
        {
            base.FoundBridge(bridge);

            // If im sleeping when the bridge is found, awaken and set the target
            if (mode != DroneMode.passive)
                StartCoroutine(Awaken(bridge.transform, .1f));


            target = bridge.transform;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void DamageTaken()
        {
            base.DamageTaken();

            if (mode == DroneMode.passive)
            {
                TargetCheck(120);
                return;
            }

            //Drone got scared!  Increase his radius from target
            sphereRadius += 10;

            //reset position immediately
            UpdateDestination(true);

            if (target)
            {
                //get direction from target
                Vector3 targDirection = (transform.position - target.transform.position).normalized;
                targDirection += transform.up * .5f;

                //add force away from target
                myRigidBody.AddForce(targDirection * retreatForce);
            }

            //make it more difficult!
            if (GetComponent<Hull>().currentHealth < 5) _difficultyMeter = 2;
            
            SpiderSound.MakeSound("Play_Drone_Damaged", gameObject);
        }

        public override void Killed(Hull hullThatDied, string killer)
        {
            SpiderSound.MakeSound("Play_Drone_Destroyed", gameObject);
        }

        public override void SetTarget(Transform newTarget)
        {
            base.SetTarget(newTarget);
            StartCoroutine(Awaken(newTarget, .5f));
        }


        /// <summary>
        /// Awakens the drone, and sets the target to whatever awakened it
        /// </summary>
        /// <param name="newTarget"></param>
        IEnumerator Awaken(Transform newTarget, float waitTime)
        {
            if (mode != DroneMode.passive) yield break;
            //change mode to launch
            mode = DroneMode.launch;

            yield return new WaitForSeconds(waitTime);


            if (newTarget == PlayerManager.PlayerShip().transform)
            {
                CombatMusic.AddCombatant(gameObject);
                CombatMusic.CreatureCombat();
            }

            //set target
            target = newTarget;

            if (animator != null)
                animator.speed = 2;
            
            SpiderSound.MakeSound("Play_Drone_Detect_Player", gameObject);

            myRigidBody.isKinematic = false;
            myRigidBody.AddRelativeForce(new Vector3(0, launchForce, 0));

            // Wait for the launch to happen
            yield return new WaitForSeconds(launchTime);

            //change modes
            mode = DroneMode.targeting;

            //do initial targeting 
            UpdateDestination(true);
        }


        IEnumerator FullAttackVolley()
        {
            if (_attacking) yield break;
            _attacking = true;
            
            SpiderSound.MakeSound("Play_Drone_Attack", gameObject);

            int attacks = 0;
            float maxTime = 6;

            while (attacks < volleySize)
            {
                float cooldown = Random.Range(.05f, fireCooldown);

                // Have a max time for the attack routine
                maxTime -= cooldown;
                if (maxTime < 0) break;

                yield return StartCoroutine(Attack(cooldown));
                attacks++;
            }

            _attacking = false;
            UpdateDestination(true);
        }


        IEnumerator Attack(float chargeTime)
        {
            float timer = 0;

            while (timer < 1)
            {
                //modify glow for attack
                glowIntensity = timer * 6 + 1;
                timer += Time.deltaTime / chargeTime;
                yield return null;
            }

            FireWeapon(weaponPoint.transform);

            glowIntensity = 1;
        }
    }
}