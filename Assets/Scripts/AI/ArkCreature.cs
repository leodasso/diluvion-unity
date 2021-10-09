using UnityEngine;
using HeavyDutyInspector;
using SpiderWeb;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace Diluvion
{

    /// <summary>
    /// Base class for all ark creatures.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ArkCreature : MonoBehaviour, IAlignable
    {

        [FoldoutGroup("Creature", expanded:false)]
        public Transform target;

        [Tooltip("Layers that will block my line of sight to target.")]
        [FoldoutGroup("Creature")]
        public LayerMask moveToLayermask;
        [Tooltip("What layers to check against when raycasting to see if I can move from current point to destination point.")]
        [FoldoutGroup("Movement")]
        public LayerMask raycastLayers;

        [FoldoutGroup("Creature")]
        [Tooltip("If casting from A to B and hits point C, destination will be set to C. How many "
            + "units should the destnation be set away from C? Increase if I hit a lot of walls while approaching a destination.")]
        public float destinationOffset = 1;

        [FoldoutGroup("Creature")]
        [Tooltip("Destination is the point at x radius this object sets around its target." +
            "When it's within the min dist to its destination point, calls OnDestinationReached.")]
        public float minDistToDestination = 6;

        [FoldoutGroup("Creature")]
        [Tooltip("How close must an object be to this before it detects the object?")]
        public float detectionRadius = 50;
        
        [FoldoutGroup("Creature")]
        public bool detectPlayerOnly;

        [FoldoutGroup("Creature")]
        public Material materialBase;

        [FoldoutGroup("Creature")]
        [Tooltip("The full visuals should be placed here. This object will be rotated to face the direction" +
            "that it's going. The parent object isn't rotated such as to not mess up the rigidbody movement.")]
        public GameObject rotator;

        [FoldoutGroup("Movement")]
        public float speed;                     // The max movement speed
        [FoldoutGroup("Movement")]
        public float acceleration;
        [FoldoutGroup("Movement")]
        public float rotationSpeed = 10;
        
        [FoldoutGroup("Attack")]
        public GameObject ammo;                 // The object I shoot from my weapon
        [FoldoutGroup("Attack")]
        public float innaccuracy = 5;           // How wide an angle can the weapon miss by?
        
        [Tooltip("Particle that spawns when I'm damaged")]
        public GameObject damageParticle;       // Particle that spawns when I'm damaged

        public float danger = 1;
        
        [FoldoutGroup("Creature")]
        [Tooltip("Set to true if you want this creature to just be a prop. It will stay kinematic and still.")]
        public bool neverActive;

        protected float maxSpeed;
        protected Material localMat;
        protected Animator animator;
        protected Rigidbody myRigidBody;
        protected float sphereRadius;
        protected float glowIntensity = 0;
        protected Vector3 randomPoint;
        protected Vector3 destination;
        protected Vector3 startingPosition;
        protected bool sleeping;

        float cullDistance = 200;
        protected bool atDestination;
        bool _targetingPlayer;

        protected virtual void Awake()
        {
            // prepare material for color changing
            localMat = new Material(materialBase);
            foreach (Renderer render in GetComponentsInChildren<Renderer>())
                if (render.sharedMaterial.name == localMat.name) render.sharedMaterial = localMat;
            animator = GetComponentInChildren<Animator>();
            myRigidBody = GetComponent<Rigidbody>();
            myRigidBody.useGravity = false;
        }

        // Use this for initialization
        protected virtual void Start()
        {
            MyHull().myDeath += Killed;
        }



        public bool IsTargetLos()
        {
            return Calc.IsLOS(transform.position, target.transform.position, raycastLayers);
        }

        public bool AttackingPlayer()
        {
            return _targetingPlayer;
        }

        protected virtual void InitValues()
        {
            EngineSound();
            startingPosition = transform.position;
            if (neverActive) myRigidBody.isKinematic = true;
            maxSpeed = speed;
        }


        public void SetAnimationSpeed(float speed)
        {
            animator.speed = speed;
        }

        /// <summary>
        /// Puts the behavior to sleep, sleeps the rigidbody, and stops any shooting / music stuff. Use this once the player
        /// is far enough away from this AI
        /// </summary>
        protected virtual void Sleep()
        {
            sleeping = true;
            myRigidBody.velocity = Vector3.zero;
            myRigidBody.angularVelocity = Vector3.zero;
        }

        /// <summary>
        /// Wakes this behavior up from its sleep mode
        /// </summary>
        protected virtual void WakeUp()
        {
            sleeping = false;
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            // apply glow to renderers
            if (localMat != null && !sleeping)
                localMat.SetColor("_EmissionColor", Color.white * glowIntensity);

            // Check if within distance to destination
            float sqDistToDestination = Vector3.SqrMagnitude(transform.position - destination);
            float sqMinDist = minDistToDestination * minDistToDestination;

            if (sqDistToDestination < sqMinDist && !atDestination)
            {
                atDestination = true;
                OnDestinationReached();
            }
        }

        protected virtual void FixedUpdate()
        {
            SleepCheck();

            if (sleeping) return;

            Vector3 toTarget = Vector3.Normalize(destination - transform.position);

            //calculate move force
            float moveForce = acceleration * myRigidBody.mass;

            //Apply force until reaching the max speed
            float normalizedSpeed = 1 - (myRigidBody.velocity.magnitude / maxSpeed);
            myRigidBody.AddForce(toTarget * moveForce * normalizedSpeed);
        }

        /// <summary>
        /// Puts to sleep if out of range, and wakes up if in range of camera.
        /// </summary>
        protected void SleepCheck()
        {
            if (sleeping && InRangeOfPlayer())
            {
                WakeUp();
                return;
            }

            if (!sleeping && !InRangeOfPlayer())
                Sleep();
        }


        protected virtual void EngineSound()
        {
            SpiderSound.MakeSound("Play_Ark_Engine_Loop", gameObject);
        }

        protected virtual void StopEngineSound()
        {
            SpiderSound.MakeSound("Stop_Ark_Engine_Loop", gameObject);
        }

        public virtual void SetTarget(Transform newTarget)
        {
            if (_targetingPlayer) return;
            if (!newTarget.GetComponent<Bridge>()) return;
            if (!newTarget.GetComponent<Bridge>().IsPlayer()) return;
            if (!InRangeOfPlayer()) return;

            WakeUp();

            _targetingPlayer = true;

            CombatMusic.AddCombatant(gameObject);
            CombatMusic.CreatureCombat();
            //if (CombatZone.Get())
            //    CombatZone.Get().AddToCombat(gameObject);
        }

        /// <summary>
        /// Based on the culling range, am I close enough to the main camera to take action?
        /// </summary>
        protected bool InRangeOfPlayer()
        {
            if (!OrbitCam.Exists()) return false;
            return (Calc.WithinDistance(cullDistance, OrbitCam.Get().transform, transform));
        }


        /// <summary>
        /// Rotates the rotator to look at the target using rotation speed.
        /// </summary>
        protected void RotateToTarget()
        {
            if (target == null) return;
            //Get point to look at
            Vector3 lookPoint = target.position;

            //look at that point
            Quaternion look = Quaternion.LookRotation(lookPoint - transform.position);
            rotator.transform.rotation = Quaternion.Slerp(rotator.transform.rotation, look, Time.deltaTime * rotationSpeed);
        }

        /// <summary>
        /// Rotates the rotator to look towards its forward direction using rotation speed.
        /// </summary>
        protected virtual void RotateToForwardMovement()
        {

            //look towards velocity direction
            Vector3 rigibodyvelocity = myRigidBody.velocity;
            if (rigibodyvelocity == Vector3.zero) return;
            Quaternion look = Quaternion.LookRotation(myRigidBody.velocity);
            rotator.transform.rotation = Quaternion.Slerp(rotator.transform.rotation, look, Time.deltaTime * rotationSpeed);
        }


        /// <summary>
        /// Called once when the object gets within MinDistToDestination of the destination
        /// point.
        /// </summary>
        protected virtual void OnDestinationReached()
        {
        }

        Hull _hull;
        protected Hull MyHull()
        {
            if (_hull) return _hull;
            _hull = GetComponent<Hull>();
            return _hull;
        }


        /// <summary>
        /// Fires ammo out of the given weapon point.
        /// </summary>
        protected void FireWeapon(Transform fromPoint, bool straightAtTarget = true)
        {

            if (sleeping) return;

            if (_targetingPlayer)
            {
                CombatMusic.CreatureCombat();
                CombatMusic.ShotsFired(transform.position);
            }

            if (ammo == null) return;

            //instantiate ammo object
            GameObject newAmmo = Instantiate(ammo, fromPoint.position, transform.rotation);

            SpiderSound.MakeSound("Play_Fire_Bolt_Drone", gameObject);

            // Set the direction of the ammo 
            Vector3 ammoDirection = fromPoint.transform.forward;
            if (straightAtTarget && target != null) 
                ammoDirection = (target.transform.position - fromPoint.transform.position).normalized;
            
            // Get the random rotation for innaccuracy of the ammo
            float randAmt = innaccuracy;
            Quaternion randomRot = Quaternion.Euler(new Vector3(Random.Range(-randAmt, randAmt), Random.Range(-randAmt, randAmt), Random.Range(-randAmt, randAmt)));
            
            // add the innacuracy to the ammo
            ammoDirection = randomRot * ammoDirection;
            
            //rotate the ammo to it's direction
            newAmmo.transform.rotation = Quaternion.LookRotation(ammoDirection);
            
            Debug.DrawRay(newAmmo.transform.position, ammoDirection * 50, Color.red, 30);

            //add force
            float force = Random.Range(150, 300);
            newAmmo.GetComponent<Rigidbody>().AddForce(ammoDirection.normalized * force);

            //shake camera
            OrbitCam.ShakeCam(.5f, transform.position);

            //get ammo component
            SimpleAmmo ammoScript = newAmmo.GetComponent<SimpleAmmo>();
            ammoScript.friendHull = MyHull();
            ammoScript.Init();
        }

        public virtual void DamageTaken()
        {
            //spawn damage particle
            GameObject newParticle = Instantiate(damageParticle, transform.position, Quaternion.identity);
            Destroy(newParticle, 2);
        }

        
        public virtual void Killed(Hull hullThatDied, string killer)
        {
            
        }


        protected virtual void AnalyzeTargets(Collider[] colliders)
        {

            if (sleeping) return;

            if (neverActive) return;

            //foreach 
            for (int i = 0; i < colliders.Length; i++)
                AnalyzeTarget(colliders[i]);
        }

        Bridge otherBridge;
        protected void AnalyzeTarget(Collider col)
        {
            //If the thing entering trigger is a ship, awaken and set the ship as the target
            otherBridge = col.GetComponent<Bridge>();
            if (otherBridge)
            {

                if (detectPlayerOnly)
                    if (otherBridge != PlayerManager.pBridge) return;

                FoundBridge(otherBridge);
            }


            //If the other thing is another ark creature, call friend has target
            ArkCreature otherCreature = col.GetComponent<ArkCreature>();

            if (otherCreature)
            {

                // Ignore the 'friend has target' if the other creature is a sentry, so that I don't wake up before
                // the sentry sends out his warning wave
                if (otherCreature as Sentry != null) return;

                if (otherCreature.target)
                    FriendHasTarget(otherCreature.target);
            }
        }



        /// <summary>
        /// A bridge has wondered into my detection radius.
        /// </summary>
        protected virtual void FoundBridge(Bridge bridge) { }

        /// <summary>
        /// Another ark creature in my detection radius has a target.
        /// </summary>
        protected virtual void FriendHasTarget(Transform friendsTarget) { }

        void TargetCheck()
        {
            TargetCheck(detectionRadius);
        }


        /// <summary>
        /// Updates the destination position that I'll try to approach. This position is picked as a random
        /// point on a sphere around the target.
        /// </summary>
        /// <param name="newPointOnSphere"> If true, will pick a new random point on the sphere. Otherwise, keeps the same point.</param>
        protected void UpdateDestination(bool newPointOnSphere, Vector3 sphereCenter)
        {

            if (newPointOnSphere)
            {
                //Get a random point on sphere
                randomPoint = Random.onUnitSphere * sphereRadius;
                maxSpeed = speed;
            }

            //get final position by adding targets position to random point
            destination = randomPoint + sphereCenter;
            Vector3 targetVector = destination - transform.position;
            //Raycast to the destination. If theres an intersection, use the point of intersection as the new destination
            Ray ray = new Ray(transform.position, targetVector);
            RaycastHit hit;
            //Check if its a closer collider
            if (Physics.Raycast(ray, out hit, 300, raycastLayers))
            {
                // Get a position spaced away from the collider so I dont try to run into the obstacle
                Vector3 hitVector = hit.point - transform.position;

                //If the hit spot is closer than our target, something is in the way
                if (hitVector.sqrMagnitude < targetVector.sqrMagnitude)
                {
                    Ray normalRay = new Ray(hit.point, hit.normal);
                    Vector3 newPoint = normalRay.GetPoint(destinationOffset);

                    destination = newPoint;
                    // Debug.Log(gameObject.name + " hit " + hit.collider.name + " at " + hit.point + " when casting towards destination.");
                }

                Debug.DrawLine(hit.point, destination, Color.green, 5);
            }

            atDestination = false;
        }

        protected void DrawDebugCross(Vector3 pos, float radius, float duration)
        {
            Debug.DrawLine(pos, pos + Vector3.up * radius, Color.yellow, duration);
            Debug.DrawLine(pos, pos + Vector3.down * radius, Color.yellow, duration);
            Debug.DrawLine(pos, pos + Vector3.forward * radius, Color.yellow, duration);
            Debug.DrawLine(pos, pos + Vector3.back * radius, Color.yellow, duration);
            Debug.DrawLine(pos, pos + Vector3.left * radius, Color.yellow, duration);
            Debug.DrawLine(pos, pos + Vector3.right * radius, Color.yellow, duration);
        }

        /// <summary>
        /// Updates the destination, assuming the target to be the center of the sphere.
        /// </summary>
        protected void UpdateDestination(bool newPointOnSphere)
        {
            if (!target) return;
            UpdateDestination(newPointOnSphere, target.position);
        }

        /// <summary>
        /// Moves straight for target
        /// </summary>
        protected void MoveToTarget(float distFromTarget)
        {

            if (target == null) return;
            if (!CanSeeTarget(target)) return;

            Ray ray = new Ray(target.position, transform.position - target.transform.position);
            destination = ray.GetPoint(distFromTarget);
        }

        public bool CanSeeTarget(Transform targ)
        {

            if (Calc.IsLOS(transform.position, targ.position, moveToLayermask, 2.0f)) return true;
            return false;
        }

        Collider[] colliders = null;
        protected void TargetCheck(float radius)
        {

            if (sleeping) return;

            //Collider[] colliders = null;
            colliders = Physics.OverlapSphere(transform.position, radius);
            //Physics.OverlapSphereNonAlloc(transform.position, radius, colliders);
            AnalyzeTargets(colliders);
        }

        protected virtual void OnEnable()
        {
            InitValues();
            InvokeRepeating("TargetCheck", Random.Range(0, .5f), .5f);
            _targetingPlayer = false;

        }

        protected virtual void OnDisable()
        {
            if (!Application.isPlaying) return;
            CombatMusic.RemoveCombatant(gameObject);
            StopEngineSound();
            CancelInvoke();
            if (!_targetingPlayer) return;
            _targetingPlayer = false;
        }


        public AlignmentToPlayer getAlignment()
        {
            return AlignmentToPlayer.Hostile;
        }

        public float SafeDistance()
        {
            return sleeping ? 5 : 25;
        }
    }
}