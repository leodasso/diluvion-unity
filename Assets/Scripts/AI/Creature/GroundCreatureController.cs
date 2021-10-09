using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using RootMotion.FinalIK;
using Diluvion.Ships;

namespace Diluvion
{

    public class GroundCreatureController : MonoBehaviour
    {   
        ///TODO YAGNI hook headspeed up to the AIMIK, so we cna turn to look towards where we wnat to go before turning the body
        [HideInInspector]
        public float headSpeed = 5;

        [Range(0, 3)]
        public float moveSpeed = 0.5f;
        [Range(1, 8)]
        public float turnSpeed = 0.5f;
        [HideInInspector]
        public float visionDistance = 120;
        [Range(0, 120)]//Temp range while i figure out some other way to check the greater than 90 values
        public float visionCone = 75;//

        public float walkingHeight = 15;
        public float initHeight = .5f;

        public Hull mainHull;

        [Tooltip("After losing sight, time to forget about player and continue behaviour")]
        public float secondsToForget = 3;

        [Tooltip("Delay before activating delayed behaviours at start")]
        public float delayWaitSeconds = 5;

        //[Comment("List of SpiderMonos that we turn on after OntriggerEnter, delayed by delayWaitSeconds")]
        //[ComponentSelection]
        //public List<SpiderMono> delayedBehaviours = new List<SpiderMono>();

        /*
         * An arbitrary secondary axis that we get by simply switching the axes
         * */
        [Tooltip("List of gameobjects to activate with the same delay time")]
        public List<GameObject> delayActivateObjects = new List<GameObject>();

        [HideInInspector]
        public Vector3 secondaryAxis { get { return new Vector3(axis.y, axis.z, axis.x); } }

        /*
         * Cross product of axis and secondaryAxis
         * */
        [HideInInspector]
        public Vector3 crossAxis { get { return Vector3.Cross(axis, secondaryAxis); } }
        [HideInInspector]
        public Vector3 axis = Vector3.forward;

        /// <summary>
        /// the delegate that happens after the creature is ready to use all behaviours
        /// </summary>
        public delegate void DelayedDelegate();
        public DelayedDelegate delayedDelegate;
        /// <summary>
        /// the target we turn our head towards
        /// </summary>
        [HideInInspector]
        public Transform attackTarget;

        /// <summary>
        /// the target we turn our head towards
        /// </summary>
        [HideInInspector]
        public Transform looktarget;

        /// <summary>
        /// the target we move towards
        /// </summary>  
        [HideInInspector]
        public Transform chaseTarget;

        /// <summary>
        /// the head's up vector(pole)
        /// </summary>
        [HideInInspector]
        public Transform headUp;
        /// <summary>
        /// the head with AIM ik, if it has one
        /// </summary>
        [HideInInspector]
        public AimIK head;
        /// <summary>
        /// the dynamically animated body of the creature if it uses IK
        /// </summary>
        [HideInInspector]
        public Transform ikBody;
     
        /// <summary>
        /// All the dynamic colliders
        /// </summary>
        [HideInInspector]
        public Collider[] boneColliders = new Collider[0];
        
        /// <summary>
        /// all the IK components
        /// </summary>
        [HideInInspector]
        public IK[] ikComponents;
        
        [HideInInspector]
        public MechSpider spiderScript;//TODO Dreive Mechaspider from Creature dynamic animation class
        
        [HideInInspector]
        public PatrolPath currentPatrol;


        [SerializeField]
        List<Hull> weaponHull = new List<Hull>();

        float moveSpeedFactor = 1;
        bool gcActive;
        PathFind myPath;
        Vector3 startLerpPos;
        List<WaypointNode> patrolPoints;
        List<Vector3> pathToFollow;
        int pathIndex = 0;
        LayerMask objectLayer;
        bool reactingToAttackTarget;
        RotationLimitAngle rla;
        float headLerpValue = 0;
        Transform nextWP;


        void OnEnable()
        {
            InitCreature();
        }

        void PrepCreature()
        {
            StartCoroutine(PrepTime(4));
        }

        //Initial animation prepping
        IEnumerator PrepTime(float time)
        {
            if (spiderScript == null) spiderScript = GetComponentInChildren<MechSpider>(true);
            spiderScript.enabled = true;
            spiderScript.height = initHeight;
            yield return new WaitForSeconds(time);
            spiderScript.PlantAllLegs();
        }

        //Behaviour delay ticker method
        public void BehaviourWaitTime(float time)
        {
            if (delayedDelegate == null) return;
            StartCoroutine(DelayedBehaviours(time));
        }

        //Delay Wait enumerator
        IEnumerator DelayedBehaviours(float time)
        {
            yield return new WaitForSeconds(time);
            if (delayedDelegate != null)
                delayedDelegate();
        }


        //Creature prepper
        public void InitCreature()
        {
            if (head == null)
                head = GetComponentInChildren<AimIK>();
            objectLayer = Calc.GunsafeLayerAndFriends(gameObject);
            rla = head.GetComponent<RotationLimitAngle>();
            if (looktarget == null)
                looktarget = head.solver.target;
            //lookTargetStartPos = looktarget.localPosition;
            if (spiderScript == null)
                spiderScript = GetComponentInChildren<MechSpider>();

            if (headUp == null)
                headUp = head.solver.poleTarget;
        
            GetPatrol();
            PrepCreature();
            
            //upTargetStartPos = headUp.localPosition;
            
            //Register any SpiderMono's we want to Callback
            /*
            foreach (SpiderMono sm in delayedBehaviours)
            {
                sm.waitForCallback = true;
                delayedDelegate += sm.OnCallBack;
            }
            */
    
            moveSpeedFactor = 1;
            foreach (Hull h in weaponHull)
                h.myDeath += HullDied;
    
            delayedDelegate += ActivateDelayedObjects;
    
            //StartIKs();
            gcActive = false;
        }

        void HullDied(Hull thatdied, string fuck)
        {
            Debug.Log(fuck + " my weapon hull died");
            moveSpeedFactor -= 0.2f;
        }

        [Button]
        void RegisterWeaponHulls()
        {
            weaponHull.Clear();
            /* TODO
            foreach (WeaponRoom wr in GetComponentsInChildren<WeaponRoom>())
                foreach (Hull h in wr.GetComponentsInChildren<Hull>())
                    weaponHull.Add(h);
                    */
        }


        void ActivateDelayedObjects()
        {
            foreach (GameObject GO in delayActivateObjects)
            {
                if (GO == null) continue;
                GO.SetActive(true);
            }
        }

        //Gets the closest patrol path from the patorl manager
        public void GetPatrol()
        {
            myPath = new PathFind();
            PathMono patrolTarget =
                NavigationManager.Get().ClosestLOSPathMonoToPosition(transform.position, transform.position);
            patrolPoints = new List<WaypointNode>();
            foreach (WaypointNode wp in myPath.PatrolToTarget(patrolTarget, patrolTarget))
                patrolPoints.Add(wp);
            chaseTarget = patrolPoints[pathIndex].transform;
        }



        //loops the target patrol points
        public void RunPatrol()
        {
            if (Calc.WithinDistance(15, chaseTarget, transform))
            {
                if (pathIndex < patrolPoints.Count - 1)
                    pathIndex++;
                else
                    pathIndex = 0;
                nextWP = patrolPoints[pathIndex].transform;
                chaseTarget = nextWP;
                if (!reactingToAttackTarget)
                    looktarget = nextWP;
            }
        }


        /// <summary>
        /// Gets all colliders in children and IKs
        /// </summary>
        [Button]
        public void GetColliders()
        {
            boneColliders = GetComponentsInChildren<Collider>();
            ikComponents = GetComponentsInChildren<IK>();
            //   EnableColliders(false);
        }


        public void StartIKs()
        {
            foreach (IK ik in ikComponents)
            {
                /* if (ik.isActiveAndEnabled)
                     ik.enabled = false;*/
                ik.Initiate();
            }
        }

        public void UpdateIKs()
        {
            foreach (IK ik in ikComponents)
                ik.GetIKSolver().Update();
        }

        public void EnableColliders(bool enable)
        {
            foreach (Collider vol in boneColliders)
            {
                if (vol == null) continue;
                vol.enabled = enable;
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (gcActive) return;
            if (!other.GetComponent<Bridge>()) return;

            Bridge otherBridge = other.GetComponent<Bridge>();

            if (!otherBridge.IsPlayer()) return;

            ///    Debug.Log(other.gameObject.name + " is turning on the Creature!");

            TurnOnCreature();
            AddAttackTarget(other.transform);
        }

        /// <summary>
        /// Satan for today
        /// </summary>
        /// <param name="Not satan"></param>
        public void AddAttackTarget(Transform target)
        {
            attackTarget = target;
            StartCoroutine(CheckTargetLoop());

        }

        //Does the sightrange check to see if we can see the player and set the behavioru to something like that
        public IEnumerator CheckTargetLoop()
        {
            float goneCount = 0;
            while (gcActive)
            {
                if (CanIReactToAttackTarget())
                {
                    reactingToAttackTarget = true;

                    goneCount = 0;
                }
                else
                {
                    if (goneCount < secondsToForget)
                        goneCount += 0.3f;
                    else
                    {
                        reactingToAttackTarget = false;

                    }
                }

                if (reactingToAttackTarget)
                    looktarget = attackTarget;
                else
                    looktarget = nextWP;

                yield return new WaitForSeconds(0.3f);
            }
        }



        //one stop check to find out if the attackTarget is a viable, visibile target to this creature
        public bool CanIReactToAttackTarget()
        {
            if (attackTarget == null) return false; // We have a target
            if (!Calc.WithinDistance(visionDistance, transform, attackTarget)) return false;//its within a certain reactionDistance
            if (!Calc.IsLOS(transform, attackTarget, objectLayer)) return false;//the target isnt behind anything

            //NEGATIVE Head.transform.right is "forward" due to how the bones are positioned on the model.
            Vector3 targetDir = attackTarget.position - head.transform.position;
            float visionDot = Vector3.Dot(targetDir, -head.transform.right);

            if (visionDot > visionCone) return false; // its within the visionCone of meeee

            return true;

        }
        /// <summary>
        /// Turns the Creature on
        /// </summary>
        [Button]
       void TurnOnCreature(){           

            SpiderSound.MakeSound("Play_YamatoCrab_Awakening", gameObject);
            StartIKs();
            if (spiderScript == null)
                spiderScript = GetComponentInChildren<MechSpider>();
            EnableColliders(true);

            spiderScript.height = walkingHeight;
            spiderScript.enabled = true;
            gcActive = true;
            BehaviourWaitTime(delayWaitSeconds);
        }

        //runs after the update on crab
        public void LateUpdate()
        {
            if (!gcActive) return;

            RunPatrol();

            if (looktarget != null)
                RotateTowardsTarget(looktarget.position);
            if (chaseTarget != null)
                MoveTowardsTarget(chaseTarget.position);
        }

        /// <summary>
        /// Set a look target


        //Rotates the crab towards the target
        void RotateTowardsTarget(Vector3 target)
        {
            Vector3 targetDirection = target - transform.position;
            Quaternion targetRot = Quaternion.LookRotation(targetDirection, head.transform.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        //Moves the creature linearly to the targets
        void MoveTowardsTarget(Vector3 target)
        {
            float factoredMoveSpeed = moveSpeed * moveSpeedFactor;
            float modMovespeed = factoredMoveSpeed;

            if (looktarget == chaseTarget)//if we are moving towards the same target we are looking at
            {
                Vector3 targetDirection = target - transform.position;
                //Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.y);
                Vector3 flatDirection = new Vector3(targetDirection.x, 0, targetDirection.y);
                float forwardDOT = Vector3.Dot(transform.forward.normalized, flatDirection.normalized);
                if (forwardDOT < 0.5f)//if target is not quite in front of us(45degrees)
                {
                    modMovespeed = factoredMoveSpeed / 2;
                    if (forwardDOT < -0.5f)//if target is behind us
                        modMovespeed = factoredMoveSpeed / 4;
                }
            }

            transform.position = Vector3.MoveTowards(transform.position, target, modMovespeed * Time.deltaTime);
        }

        public void SafeDeath()
        {
            mainHull.Damage(10000000, 1, gameObject);
           Debug.Log("DESPAWNING FROM:" + this.name , this); Destroy(gameObject);
        }
    }
}

