using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using System.Linq;
using Diluvion.Ships;
using Diluvion.Sonar;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using NodeCanvas.Framework;
using NodeCanvas.BehaviourTrees;
using Diluvion.Roll;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Diluvion.AI
{
    [System.Serializable]
    public class Reaction
    {
      
        [EnumFlags]      
        public TargetContext targetContext;

        [EnumFlags]       
        public ActionContext reactionContext;
       
        public AIAction actionType = AIAction.Mission;
      
        public BehaviourTree overrideMission;

        public bool distanceImportance = false;
    
        public float priorityMultiplier = 1;
     
        [SerializeField]
        List<Action> actions = new List<Action>();
        int actionIndex;

        [SerializeField]
        List<Convo> responses = new List<Convo>();
        int responseIndex;

        
     
        

        /// <summary>
        /// Gives a score based on how fitting the contextTarget is for this reaction
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public bool CanReact(ContextTarget ct)
        {        
            return HasTargetContext(ct.MyTargetContext)&& (HasReactionContext(ct.MyActionContext));
        }

        string SignatureContext(List<SignatureValues> signatureRelations)
        {
            string returnString = "";
            foreach (SignatureValues sv in signatureRelations)
                if (sv.signature != null && HasTargetContext(sv.Context(0)))
                    returnString += sv.signature.name + ", ";
            returnString.TrimEnd(new char[2] { ',', ' '});
            return returnString;
        }

        public string FactionDescription(List<Faction> factions, List<SignatureValues> nonfactions)
        {
            string returnString = "";
            List<SignatureValues> sigValues = nonfactions;
            foreach (Faction f in factions)
            {
                if (f == null) continue;
                 sigValues.AddRange(f.signatureRelations);
            }
            returnString += SignatureContext(sigValues);
            return returnString;
            
        }

        public string CaptainDescription()
        {
            string returnString = "I " + ReactionConText() + targetContext + " i will ";        

            return returnString;
        }
        
        public string BuildDescription(List<Faction> factions, List<SignatureValues> nonFactions)
        {
            string returnString = "I " + ReactionConText() +" "+ FactionDescription(factions,nonFactions) + " i will ";        

            return returnString;
        }

       
        string ReactionConText()
        { 
            bool atLeastOneDescription = false;
            
            string  actionString ="";
            
            if(HasReactionContext(ActionContext.Found))
            {
                  actionString += "found ";
                  atLeastOneDescription = true;
            }

            if (HasReactionContext(ActionContext.Attacked))
            {
                if (atLeastOneDescription)
                    actionString += "or, ";
                actionString += "was attacked ";
                atLeastOneDescription = true;
            }
            
            if (HasReactionContext(ActionContext.SOS))
            {
                if (atLeastOneDescription)
                    actionString += "or, ";
                actionString += "heard an SOS ";
                atLeastOneDescription = true;
            }
            
            if (HasReactionContext(ActionContext.Hailed))
            {
                if (atLeastOneDescription)
                    actionString += "or, ";
                actionString += "was hailed ";
                atLeastOneDescription = true;
            }
            
            if (HasReactionContext(ActionContext.Pinged))
            {
                if (atLeastOneDescription)
                    actionString += "or, ";
                actionString += "heard a Ping ";
                atLeastOneDescription = true;
            }

            if (atLeastOneDescription)
                actionString += "by ";
            actionString += "a ";
            return actionString;
        }
        

        public bool HasTargetContext (TargetContext toCheck)
        {
            return (targetContext & toCheck) == toCheck;
        }

        public bool HasReactionContext(ActionContext toCheck)
        {
            return (reactionContext & toCheck) == toCheck;
        }

        public override bool Equals(object obj)
        {
            Reaction re = obj as Reaction;
            if (re?.reactionContext!=reactionContext) return false;
            if (re.targetContext != targetContext) return false;
            if (re.actionType == actionType) return false;
            return re.overrideMission == overrideMission;
        }

        public Action GetAction
        {
            get
            {
                if (actions == null||actions.Count<1) return null;

                return actions[actionIndex];
            }
        }


        public Convo GetResponse
        {
            get
            {
                if (responses == null || responses.Count < 1) return null;
                return responses[responseIndex];
            }
        }        
    }

    public enum CaptainStat
    {
        Patience, 
        Bravery,
        ShipSteering,
        WeaponCrew
    }

    [System.Serializable]
    public struct SignatureValues
    {
        [HorizontalGroup("SigValue")]
        [HideLabel]
        [AssetsOnly]
        public Signature signature;

        [Tooltip("Once gained, this signature is never lost")]
        public bool sticky;
        [Tooltip("Prioritize farther targets as higher priority")]
        public bool inverseDistance;

        [HorizontalGroup("SigValue")] 
        [SerializeField, Range(-10,10)]     
        float baseAttitude;


        public SignatureValues(Signature sig, float attitude)
        {
            signature = sig;
            baseAttitude = attitude;
            inverseDistance = false;
            sticky = false;
        }
        public float Value
        {
            get
            {
                return baseAttitude;
            }
            set
            {
                baseAttitude = value;
            }
        }

        public TargetContext Context(float offset)
        {
            if (Value + offset > 0)
                return TargetContext.Friend;
            else if (Value + offset < 0)
                return TargetContext.Enemy;
            else
                return TargetContext.Interesting;
        }

        public override bool Equals(object obj)
        {
            Signature sig = obj as Signature;
            if (sig == null) return false;
            return sig == signature;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }



    /// <summary>
    /// AI Execution Class, Reads the captainpersonality and runs two state machines
    /// One state machine reads data and makes decisions
    /// The other acts on those decisions.
    /// </summary>
    [CreateAssetMenu(fileName = "new Captain", menuName = "Diluvion/AI/Captain", order = 0)]
    public class Captain : Entry
    {


        [TabGroup("Personality", false, 899)]
        [TextArea(1, 15, order = 899)]
        [ReadOnly]
        public string description = "This is a captain";

        [OnValueChanged("UpdateDescription")]
        public List<Faction> baseFactions = new List<Faction>();    
        
        [OnValueChanged("UpdateDescription")]
        public List<SignatureValues> nonFactionRelations = new List<SignatureValues>();

        [Tooltip("Defines wait time between actions")]
        [TabGroup("Personality", false, 990)]
        [Range(-10,10)]
        [SerializeField]
        float patience = 0;
        public float Patience
        {
            get
            {
                return patience;
            }
            private set
            {
                patience = value;
            }

        }

        [Tooltip("Defines how ballsy a captain is, affects chosen engagement range.")]
        [TabGroup("Personality", false, 990)]
        [Range(-10, 10)]
        [SerializeField]
        float bravery = 0;
        public float Bravery
        {
            get
            {
                return bravery;
            }
            private set
            {
                bravery = value;
            }
        }


        [Tooltip("Defines how well a captain steers")]
        [TabGroup("Personality", false, 990)]
        [Range(-10, 10)]
        [SerializeField]
        float shipSteer = 0;
        public float ShipSteering
        {
            get
            {
                return shipSteer;
            }
            private set
            {
                shipSteer = value;
            }
        }


        [Tooltip("Defines how good a captain is at firing guns")]
        [TabGroup("Personality", false, 990)]
        [Range(-10, 10)]
        [SerializeField]
        float weaponCrew = 0;
        public float WeaponCrew
        {
            get
            {
                return weaponCrew;
            }
            private set
            {
                weaponCrew = value;
            }
        }

        [TabGroup("Actions", false, 2)]
        [SerializeField]
        [OnValueChanged("UpdateDescription")]
        BehaviourTree defaultFight;

        [TabGroup("Actions", false, 2)]
        [SerializeField]
        BehaviourTree defaultFlee;

        [TabGroup("Actions", false, 2)]
        [SerializeField]
        BehaviourTree defaultMission;

        [TabGroup("Actions", false, 2)]
        [OnValueChanged("UpdateDescription")]
        public List<Reaction> reactions = new List<Reaction>();

        public float GetStat(CaptainStat statType)
        {
            switch(statType)
            {
                case CaptainStat.Patience:
                {
                    return Patience;
                }
                case CaptainStat.Bravery:
                {
                    return Bravery;
                }
                case CaptainStat.ShipSteering:
                {
                    return ShipSteering;
                }
                case CaptainStat.WeaponCrew:
                {
                    return WeaponCrew;
                }
                default:
                    {
                        Debug.LogError(statType.ToString() + " not yet implemented");
                        return 0;
                    }
            }
        }

        [TabGroup("Personality", false, 991)]
        [TextArea(1, 4, order = 991)]
        public string designNotes = "This is a captain";

        /// <summary>
        /// Returns a mission connected to a context if it exists, otherwise returns the default
        /// </summary>
        /// <returns></returns>
        public BehaviourTree ReactiveMission(TargetContext tc)
        {
           //  BehaviourTree returnMission = ContextReaction(tc).mission;
           ////   if (returnMission == null)
               // returnMission = defaultMission;

            return null;
        }
        
        
        /// <summary>
        /// Determines if the target is scarier than me or not, and how to react to that
        /// </summary>
        /// <param name="target"></param>
        /// <param name="myDanger"></param>
        /// <returns>Enemy or Scary based on my bravery</returns>
        public TargetContext Enemy(ContextTarget target, float hpPercent) // myDanger) //TODO Yagni For danger calc bravery 
        {
            if(target.target==null) return TargetContext.Enemy;
            
          //  if (Math.Abs(myDanger) < 0.05f) return TargetContext.Enemy; 

            //  float dangerDifferenceRatio = target.Danger()/myDanger -1;
            float lostHPBravery = Mathf.Lerp(10, -10, hpPercent/100); // direct relationship between remaining health and bravery
                                                                  //float dangerRatio = target.Danger()*1.0f / myDanger * 4; //TODO Base danger is 4 times as strong for the captain meaning you need 4 ships of the same type for the captain at BASE bravery to run away

           // Debug.Log("Determening enemy scaryness: Target: " + target.target.name + " attitude: " + target.InstanceAttitude + " MyHP%: <color=green> " + hpPercent + "</color> (HpBravery: <color=yellow>" + lostHPBravery + "</color> ) " + "Captain bravery: <color=red>" + bravery +"</color>");

            //Determines if a target is scary or an enemy
            if (target.InstanceAttitude < 0)
            {
                if (lostHPBravery < bravery)
                {
                    return TargetContext.Enemy;
                }
                else
                {
                    return TargetContext.Scary;
                }
            }
            return TargetContext.Enemy;
        }

        string MissionDescription(BehaviourTree tree)
        {
            return tree.graphComments;
        }

        string ReactionDescription(Reaction reaction)
        {
            if(reaction==null)return "";
            string returnString = reaction.CaptainDescription();

            if (reaction.overrideMission != null)
            {
                returnString += reaction.overrideMission.graphComments;
            }
            else
            {
                if(reaction.actionType==AIAction.Fight)
                    returnString += defaultFight.graphComments;
                if(reaction.actionType==AIAction.Flee)
                    returnString += defaultFlee.graphComments;
            }
            return returnString;
        }
        

        [Button]
        public void UpdateDescription()
        {
            description = "I will " + MissionDescription(defaultMission);

            if (reactions.Count > 0)
                description += " until: \n \n";
            foreach(Reaction r in reactions)
            {
                description += ReactionDescription(r) + ", \n \n";
            }
        }

        
        
        /// <summary>
        /// Picks the first reaction that fits the contextTarget
        /// </summary>
        /// <param name="tc"></param>
        /// <returns></returns>
        public Reaction TargetReaction(ContextTarget tc)
        {
            foreach (Reaction r in reactions)
                if (r.CanReact(tc))
                {
                    return r; 
                }
                       
            return null;
        }

        const float captainDangerFactor = 1;
        private const int captainBaseFactor = 20; //The score for (0,0,0,0) Cant be negative?
        public override int Danger()
        {
            int totalDanger = captainBaseFactor + Mathf.RoundToInt((bravery + shipSteer + patience + weaponCrew)/captainDangerFactor);
          //  Debug.Log("<color=yellow>" + name + "</color> ("+totalDanger+") Danger is 1 + ( bravery: <color=magenta>" + bravery + "</color> + shipSteer: <color=cyan>" + shipSteer + "</color> + patience: <color=blue>" +patience + "</color> + weaponCrew: <color=red>" + weaponCrew + "</color> / captainDangerFactor:  <color=green>" + captainDangerFactor + "</color>");

            return totalDanger;
        }


        #region getters and Setters 

        public BehaviourTree DefaultAttack
        {
            get
            {
                if (defaultFight != null) return defaultFight;
                return defaultFight = Resources.Load<BehaviourTree>("tactic_inOut");   //Default in case we forgot to set it     
            }

            private set
            {
                defaultFight = value;
            }
        }
        public BehaviourTree DefaultFlee
        {
            get
            {
                if (defaultFlee != null) return defaultFlee;
                return defaultFlee = Resources.Load<BehaviourTree>("flee_awayDirection");   //Default in case we forgot to set it     
            }

            private set
            {
                defaultFlee = value;
            }
        }
        public BehaviourTree DefaultMission
        {
            get
            {
                if (defaultMission != null) return defaultMission;
                return defaultMission = Resources.Load<BehaviourTree>("mission_townWander");   //Default in case we forgot to set it     
            }

            private set
            {
                defaultMission = value;
            }
        }
        #endregion
    }
}

#region OldCaptain

/* public CaptainPersonality personality = new CaptainPersonality();
       [Comment("If this is not null, the personality will be overwritten")]
       public CaptainScriptableObject overridePersonality;
       bool personalitySet = false;
       public SonarStats target;

       public AIMission currentMission;
       public bool cullAI = true;

       public AIAction currentAction;
       AIMission previousMission;
       public Hull hull;

       public PatrolPath currentPatrol;
       public List<Vector3> pathToFollow;
       public List<PatrolPath> nearbyPatrolPaths = new List<PatrolPath>();
       public bool attackingPlayer = false;
       public bool noCombatMusic = false;
       public bool debug = false;

    /*    /// <summary>
    /// AI action options, this is the second-to-second actual thing the AI is performing
    /// </summary>
    public enum AIAction
    {
        Moving,
        Firing,
        FiringAndMoving,
        Waiting,
        Pinging
    }

    /// <summary>
    /// Preferred firing range, basically chooses between trying to stay at max torpedo range or bolt attack range
    /// </summary>
    public enum FiringRange
    {
        ShortRange,
        LongRange
    }

    /// <summary>
    /// Represents the different tactics the AI employs when not Idle/Patrolling
    /// </summary>
    public enum AITactics
    {
        Strafe, // Circle back and forth at preferred range inside its patrol area
        DontStop, // Continue patrol path, fire when in range 
        DiveBy, // Heads toward the target's weak spot
        Kite,   // Keeps itself at its preferred Range, 
        Chase   // Follows the target anywhere, following it's EXACT path while it sees the target, returns along the same path back to patrol
    }

    /// <summary>
    /// The high level Job the AI is currently committing to
    /// </summary>
    public enum AIMission
    {
        Idling,
        Patrolling,
        Attacking,
        Fleeing,
        ReturnToPath
    }
    */
/// <summary>
/// Container for behaviour/difficulty of the AI
/// </summary>
/*
[System.Serializable]
public class CaptainPersonality
{


    //SIMULATES % CREW PRESENCE
    public string personalityname = "";
    [SerializeField]
    public List<StationStat> bonusStats = new List<StationStat>();

    public float interestDistance = 30; // First Interest Range, if the target is outside the range they will continue what they are doing (0 =  No Clamping)
    public float carefulNess = 3;
    public float danger = 1;
    //applies a range multiplier to what the AI considers optimal range.
    public float rangeModifier = 1.1f;
    /*  public FiringRange preferredRange = FiringRange.ShortRange;
      public AITactics tactics = AITactics.Strafe;
      [HideConditional(true, "tactics", (int)AITactics.Chase)]
    public bool neverLoseTarget = false;
    public bool alwaysFireAllWeapons = false;


    public bool forceIdle = false;
    public bool forceHoldFire = false;
    public bool waitAtStart = false;

    public float scaredPercentage = 25f;


    //Hostile Tags List of sonarstat tags this cpatain will attack on sight TODO something with this
    public List<Signature> hostileTags = new List<Signature>();
    public List<Signature> excludeTags = new List<Signature>();



    /// <summary>
    /// Returns the stat named statKey, 
    /// Only pass keywords from the Heavydutyinspector Keyword editor
    /// </summary>
    /// <param name="statKey"></param>
    /// <returns></returns>
    public float GetStat(string statKey)
    {
        foreach (StationStat st in bonusStats)
            if (st.IsStat(statKey))
                return st.totalBonus;
        return 0;
    }

    #region constructors

    public CaptainPersonality()
    {
        interestDistance = 30;
        carefulNess = 3;
        // preferredRange = FiringRange.ShortRange;
        //  tactics = AITactics.Strafe;
        neverLoseTarget = false;
        forceIdle = false;
        forceHoldFire = false;
        waitAtStart = false;
        scaredPercentage = 25f;
        hostileTags = new List<Signature>();
        excludeTags = new List<Signature>();
    }
    public CaptainPersonality(string name)
    {
        personalityname = name;
        interestDistance = 30;
        carefulNess = 3;
        //   preferredRange = FiringRange.ShortRange;
        //    tactics = AITactics.Strafe;
        neverLoseTarget = false;
        forceIdle = false;
        forceHoldFire = false;
        waitAtStart = false;
        scaredPercentage = 25f;
        hostileTags = new List<Signature>();
        excludeTags = new List<Signature>();
    }

    public CaptainPersonality(CaptainPersonality person)
    {
        personalityname = person.personalityname;
        bonusStats = new List<StationStat>(person.bonusStats);
        interestDistance = person.interestDistance;
        carefulNess = person.carefulNess;
        //allowedWeapons = new List<WeaponType>(person.allowedWeapons);
        alwaysFireAllWeapons = person.alwaysFireAllWeapons;
        //  preferredRange = person.preferredRange;
        //     tactics = person.tactics;
        neverLoseTarget = person.neverLoseTarget;
        forceIdle = person.forceIdle;
        forceHoldFire = person.forceHoldFire;
        waitAtStart = person.waitAtStart;
        scaredPercentage = person.scaredPercentage;
        hostileTags = new List<Signature>(person.hostileTags);
        excludeTags = new List<Signature>(person.excludeTags);
    }

    #endregion
}



#region private vars
List<WaypointNode> patrolPoints;

       PathFind myPath;
       int moveIndex = 0;
       public int MoveIndex
       {
           get
           {
               return moveIndex;
           }
           private set
           {
               moveIndex = value;
           }
       }
       AIMover aiMover;
       Sonar.Listener sonar;
       //Sonar sonar;
       SonarStats attackerStats;
       //PrefabDicts pfDict;
       PatrolPath defaultPath;

       AIWeapons myWeapons;
       //Helm myHelm;
       float closeMoveDistance = 5;
       Rigidbody myRbody;

       AICuller culler;
       Vector3 patrolOffset;
       Vector3 targetPoint;
       Bridge targetBridge;
       NavigationManager navMan;
       bool waiting = false;
       bool following = false;
       int continueAttackCount = 12;
       bool initialized = false;
       List<Vector3> followPoints;
       LayerMask terrainMask;
       float attackTime = 0;
       //bool attackRun = false; commented out 11/14/2016
       WorldControl wc;
       bool startCoRoutines = false;
       /// <summary>
       /// False until a target is found that's within interest distance
       /// </summary>
       bool foundFirstTarget = false;
       bool targetingPlayer = false;
       Bridge bridge;

       #endregion

       #region Initialization
       void Awake() {

           bridge = GetComponentInParent<Bridge>();
           PreInitCaptain();
       }

       void OnEnable()
       {
           gameObject.layer = LayerMask.NameToLayer("AI");
           if (!initialized)
               PreInitCaptain();
           else
               StartBehaviours();
       }

       void OnDisable()
       {
           if (!Application.isPlaying) return;
           startCoRoutines = false;
           attackingPlayer = false;
           target = null;
           attackerStats = null;

           //StopAllCoroutines();
           if (noCombatMusic) return;
           if (!SceneControl.Get()) return;
           SceneControl.Get().RemoveFromCombat(gameObject);
       }

       /// <summary>
       /// Debug override so we can get Debug messages from one AI at a time
       /// </summary>
       /// <param name="debugString"></param>
       public void Dbug(string debugString)
       {
           if (gameObject == null) return;
           if (debug)
               Debug.Log(debugString, gameObject);
       }

       void OnDrawGizmos()
       {
           if (debug)
           {
               Gizmos.color = Color.red;
               Gizmos.DrawSphere(transform.position, personality.interestDistance);
           }
       }


       //Sets up the init for the captain, makes sure it gets initialized no matter how it is instantiated or in what order.
       void PreInitCaptain()
       {
           if (gameObject.isStatic)
               gameObject.isStatic = false;
           wc = WorldControl.Get();
           terrainMask = Calc.IncludeLayer("Terrain");
           //wc.worldReset += StopCaptain;//Callback to stop coroutines

           /* TODO
           if (!wc.worldInitialized)
               wc.DoneInit += InitializeCaptain;
           else
               InitializeCaptain();

       }

       /// <summary>
       /// Initializes the information about the ship this Captain is attached to
       /// </summary>
       public void GetShipVariables()
       {
           if (this == null) return;
           navMan = NavigationManager.Get();
           hull = GetComponentInParent<Hull>();
           hull.imHit += IGotHit;
           bridge = GetComponentInParent<Bridge>();

           if (nearbyPatrolPaths != null)
               if (nearbyPatrolPaths.Count > 0)
                   if (nearbyPatrolPaths[0] == null)
                       nearbyPatrolPaths = new List<PatrolPath>();
           //Turns off this object if the bridge IsPlayer

           if (bridge.IsPlayer())
           {
               gameObject.SetActive(false);
               return;
           }
           else
           {
               if (bridge.GetComponent<ShipControls>())
                   bridge.GetComponent<ShipControls>().enabled = false;
           }

           //set Waiting to idleAtStart

           myRbody = bridge.GetComponent<Rigidbody>();
           aiMover = GetComponent<AIMover>();
           //sonar = bridge.sonar;
           //myHelm = bridge.helm;
           //myHelm.SetShipMover(bridge.shipMove);//TODO Temporary shipMover null catch

           /* TODO
           if (bridge.Weaponsmanager() != null)
           {
               if (!GetComponent<AIWeapons>())
                   myWeapons = gameObject.AddComponent<AIWeapons>();
               else
                   myWeapons = GetComponent<AIWeapons>();               
           }


           SetPersonality();
           //pfDict = PrefabDicts.Get();

           // TODO defaultPath = pfDict.GameObjectOfKey("defaultpatrol").GetComponent<PatrolPath>();
       }

       #endregion

       #region PersonalitySetup


       //Immidately changes the personality of the captain
       public void ChangePersonality(CaptainScriptableObject person)
       {
           // Dbug("Changing personality to: " + person.personality.personalityname);
           personalitySet = false;
           overridePersonality = person;
           personality = new CaptainPersonality(overridePersonality.personality);
           SetPersonality();
       }

       /// <summary>
       /// Pushes the relevant stats to the various parts of the ship.
       /// </summary>
       public void SetPersonality()
       {
           if (personalitySet) return;
           Dbug("Setting personality:");
           personalitySet = true;
           waiting = personality.waitAtStart;

           //if(sonar)
           //sonar.SetAIStats(personality);

           //if(myHelm) myHelm.SetAIStats(personality);

           if (!bridge) return;
           //if (bridge.Weaponsmanager() == null) return;

           if (myWeapons == null)
               myWeapons = GetComponent<AIWeapons>();
           if (myWeapons == null)
               myWeapons = gameObject.AddComponent<AIWeapons>();
           if (myWeapons != null)
           {
               MyWeapons().SetPersonality(personality);
               MyWeapons().InitAIWeapons(bridge);
           }
       }

       bool cullerInitialzed = false;
       /// <summary>
       /// Order of operations init for AI information. Starts the Behaviuor loops.
       /// </summary>
       void InitializeCaptain()
       {
           if (this == null) return;
           if (initialized) return;
           if (overridePersonality != null)
               personality = new CaptainPersonality(overridePersonality.personality);
           GetShipVariables();

           if (!gameObject.activeInHierarchy) return;

           if (cullAI)
           {
               // Debug.Log("Transform parent " + bridge.transform.parent, gameObject);
               culler = (AICuller)Instantiate(Resources.Load<AICuller>("aiCuller"), transform.position, transform.rotation);
               culler.instanceToCull = bridge.gameObject;
           }
           StartBehaviours();
           initialized = true;
       }

       //Starts the behaviour  loops.
       public void StartBehaviours()
       {
           if (startCoRoutines) return;

           startCoRoutines = true;
           if (!gameObject.activeInHierarchy) return;
           StartCoroutine(CheckMission());
           StartCoroutine(MissionBehaviour());

       }

       /// <summary>
       /// public Get for the private AIweapons module attached to this captain.
       /// </summary>
       public AIWeapons MyWeapons()
       {
           if (myWeapons != null) return myWeapons;
           myWeapons = GetComponent<AIWeapons>();
           return myWeapons;
       }


       /// <summary>
       /// Sets up a offset to the patrol line within the tube, to make sure that we get less traffic jams
       /// </summary>
       Vector3 GetOffsetToWP(Vector3 randomOffset, Vector3 startPoint, WaypointNode wpn)
       {
           Vector3 offsetTemp = randomOffset;
           Vector3 relativeDirection;

           if (wpn == null) relativeDirection = transform.forward * 10;
           else relativeDirection = (wpn.transform.position - transform.position);


           Vector3.OrthoNormalize(ref relativeDirection, ref offsetTemp);

           randomOffset = offsetTemp.normalized * wpn.AllowedMoveDistance() / 2;

           return (wpn.transform.position + randomOffset);
       }


       #endregion

       #region Tools   
       /// <summary>
       /// Adds bulk extra tags to the personality, to give this AI more enemies 
       public void AddHostileTags(List<Signature> tags)
       {
           foreach (Signature s in tags) AddHostileTag(s);
       }

       /// <summary>
       /// Adds a single new tag to be hostile towards needs to be a keyword
       /// </summary>
       public void AddHostileTag(Signature tag)
       {
           if (personality.hostileTags.Contains(tag)) return;
           personality.hostileTags.Add(tag);
       }

       /// <summary>
       /// Removes a hostile tag if it exists in this personality
       /// </summary>
       public void RemoveHostileTag(Signature tag)
       {
           personality.hostileTags.Remove(tag);
       }


       /// <summary>
       /// Adds bulk exclude tags, exclude tags take priority over hostile, meaning if a target has an excluded tag, this ai will ignore all hostile tags on that target.
       /// </summary>
       public void AddExcludeTags(List<Signature> tags)
       {
           foreach (Signature s in tags) AddExcludeTag(s);
       }


       public void AddExcludeTag(Signature tag)
       {
           if (personality.excludeTags.Contains(tag)) return;
           personality.excludeTags.Add(tag);
       }

       public void RemoveExcludeTag(Signature tag)
       {
           personality.excludeTags.Remove(tag);
       }

       /// <summary>
       /// Checks to see if we are attacking the player
       /// </summary>
       bool AmIAttackingPlayer()
       {
           if (!targetBridge) return false;
           return targetBridge.IsPlayer();

       }

       /// <summary>
       /// Checks to see if the target is within tethering range.
       /// </summary>
       bool InRangeOfPatrol(Transform targ)
       {
           bool inRange = false;

           if (currentPatrol != null)
               inRange = currentPatrol.IsWithinRangeOfPatrol(targ, MyWeapons().LongRange());

           return inRange;
       }

       /// <summary>
       /// Turns the patrol direction around, patrols are always circular, so it will go clockwise to counter-clockwise and vise versa
       /// </summary>
       public void ReversePath()
       {
           patrolPoints.Reverse();
           pathToFollow.Reverse();

           MoveIndex = pathToFollow.Count - MoveIndex;
           if (MoveIndex > pathToFollow.Count - 1)
               MoveIndex = 0;

           Dbug("Reversed Path: " + patrolPoints.Count + " / " + pathToFollow.Count + " / " + MoveIndex);
       }

       #region Safe array operators
       /// <summary>
       /// Safely returns a legal vector3 to return to if for some reason the AI has too short of a list of points.
       /// </summary>
       Vector3 SafePrevPos(Vector3 prevPos, List<Vector3> listToCheck, int index, int bottom)
       {
           if (bottom >= index)//if index is too small
               return prevPos + (prevPos - transform.position).normalized * 15;

           return listToCheck[index - 1];
       }


       /// <summary>
       /// Returns a safe next position even if there is none, defaults to the ship forward.
       /// </summary>
       Vector3 SafeNextPos(Vector3 nextPos, List<Vector3> listToCheck, int index, bool patrol)
       {
           if (listToCheck.Count == 1)
               return listToCheck[0];
           if (listToCheck.Count - 1 <= index)//if index is too big
               if (patrol)
                   return listToCheck[0];
               else
                   return nextPos + (nextPos - transform.position).normalized * 10; //return a point in the same direction as you are going as the nextPos

           return listToCheck[index + 1];
       }

       #endregion
       #endregion

       #region Single Actions
       /// <summary>
       /// Performs a ping through the sonar module.
       /// </summary>
       public void Ping()
       {
           // TODO
           //Ping power = duration of holding,defaulting to MAX power
           //if (sonar.PingCharged())
           //sonar.sPing(sonar.MaxPingPower());
       }

       /// <summary>
       /// Caches the input list of waypoints into a simplified vector3 list for easier iteration.
       /// </summary>
       /// <param name="pws"></param>
       public void SetV3Path(List<WaypointNode> pws)
       {
           //TODO Same offset, relative to the previous point 
           pathToFollow = new List<Vector3>();
           foreach (WaypointNode pw in pws)
           {
               pathToFollow.Add(pw.transform.position);
               //if (pws.Last() != pw)
               //Debug.DrawLine(pw.transform.position + Vector3.up, pws[pws.IndexOf(pw) + 1].transform.position + Vector3.up, Color.green, 25);
           }
       }

       #endregion

       #region MissionSelection
       /// <summary>
       /// Compares the health to the personality's scared percentage
       /// </summary>
       bool AmIDying()
       {
           if (hull.CurrentHPPercent() > personality.scaredPercentage) return false;
           return true;
       }

       /// <summary>
       /// Handy function for checking if the target is missing or invalid
       /// </summary>
       bool TargetMissing()
       {
           if (target == null || !target.gameObject.activeInHierarchy) return true;
           return false;
       }

       /// <summary>
       /// Handy function for checking if the target is missing or invalid
       /// </summary>
       bool AttackerMissing()
       {
           if (attackerStats == null || !attackerStats.gameObject.activeInHierarchy) return true;
           return false;
       }


       /// <summary>
       /// Checks the Weapons module to see if this AI has combat capability
       /// </summary>
       bool AmIArmed()
       {
           if (MyWeapons() == null) return false;
           return MyWeapons().Armed();
       }

       /// <summary>
       /// Listens to the hull when it gets hit, identifies if its an enemy or a friendly, and prioritizes attacking its attacker.
       /// </summary>
       void IGotHit(float howHard, GameObject byWhat)
       {
           // Check if the attacker is null or doesn't have the right components
           if (byWhat == null || !byWhat.activeInHierarchy) return;
           SonarStats attackingStats = byWhat.GetComponent<SonarStats>();
           if (attackingStats == null) return;

           // If the attacker's tags are in my list of exclude tags, ignore the attack
           foreach (Signature tag in personality.excludeTags)
               if (attackerStats.signatures.Contains(tag)) return;

           // Memorize who attacked me so I can retailiate
           attackerStats = attackingStats;

           // Remember if the player attacked me and ran away. I'll still fuck him up
           if (byWhat.GetComponent<Bridge>())
               if (byWhat.GetComponent<Bridge>().IsPlayer())
                   AddHostileTag(SonarGlobal.PlayerSignature());
       }

       /// <summary>
       /// pauses the state machine for seconds
       /// </summary>
       void WaitFor(float seconds)
       {
           StartCoroutine(IdleForSeconds(seconds));
       }

       //Stops behaviours for seconds
       IEnumerator IdleForSeconds(float seconds)
       {
           waiting = true;
           yield return new WaitForSeconds(seconds);
           waiting = false;
       }

       /// <summary>
       /// Changes the current mission to the input mission
       /// </summary>
       /// <param name="mission"></param>
       public void SetMission(AIMission mission)
       {
           if (mission == AIMission.Idling)
               waiting = true;
           else
               waiting = false;

           //What?
           if (mission == AIMission.Attacking)
               if (TargetMissing() && AttackerMissing())
                   return;

           currentMission = mission;
       }


       /// <summary>
       /// DecisionMaking Tree, the lower the assignment, the higher the priority. Checks every two seconds.
       /// </summary>
       IEnumerator CheckMission()
       {
           if (!personality.forceIdle)
           {
               if (waiting)
                   currentMission = AIMission.Idling;
               else
                   currentMission = AIMission.Patrolling;//Descending order determines priority, last is most important

               if (following)//If we are on a follow path (ONLY GETS SET TO FALSE WHEN WE HAVE RETURNED TO THE PATROL AREA)
                   currentMission = AIMission.ReturnToPath;

               if (AmIDying())//Switch between most important things, mutually exclusive            
                   currentMission = AIMission.Fleeing;
               else //Actions for if i noticed the player            
                   currentMission = FoundTarget(FoundAggroTarget());

           }
           else
               currentMission = AIMission.Idling;

           Dbug("Mission is:" + currentMission);
           yield return new WaitForSeconds(2);
           StartCoroutine(CheckMission());
           yield break;
       }

       /// <summary>
       /// Mission behaviour state machine, Executes the current mission set by CheckMission(), will continue until it breaks out of its state.
       /// </summary>
       IEnumerator MissionBehaviour()
       {
           Dbug("Starting " + currentMission + " Mission");
           switch (currentMission)
           {
               case AIMission.Attacking:
                   {
                       yield return AttackBehaviour();

                       break;
                   }
               case AIMission.Fleeing://TODO Despawning behaviour if i escape player notice
                   {

                       yield return FleeBehaviour();
                       break;
                   }
               case AIMission.Patrolling: //Patrol
                   {

                       yield return PatrolBehaviour();
                       break;
                   }
               case AIMission.ReturnToPath:
                   {

                       yield return ReturnToPath();
                       break;
                   }
               case AIMission.Idling:
                   {

                       yield return IdleBehaviour();
                       break;

                   }
               default:
                   {
                       yield return IdleBehaviour();
                       break;
                   }
           }

           Dbug("Ending " + previousMission + " Mission");
           StartCoroutine(MissionBehaviour());
       }


       /// <summary>
       /// Iterates through the sonar module contact list, and sets the target if necessary. Returns the visibility of what it found, if it found something, defaults to 0 which means no target.
       /// </summary>
       /// <returns></returns>
       float FoundAggroTarget()
       {
           float closestTargetVisibility = 0;

           SonarStats targetStats;

           targetStats = null;
           //targetStats = sonar.GetClosestStatsWithTags(personality.hostileTags, personality.excludeTags, out closestTargetVisibility, debug);

           //extra init check.
           SetPersonality();

           //  Dbug("ClosestStats : " + targetStats + " is visible: " + closestTargetVisibility + " because he is in the list " + personality.hostileTags.Count + " and not in the list: " + personality.excludeTags.Count);
           if (targetStats == null)
               return 0;
           if (!targetStats.gameObject.activeInHierarchy)
               return 0;


           if (!foundFirstTarget && personality.interestDistance > 0)
           {
               Dbug("have not found first target yet,  : " + targetStats.name + " is within  " + personality.interestDistance + "?");
               if (Calc.WithinDistance(personality.interestDistance, transform, targetStats.transform))
                   foundFirstTarget = true;
               else
                   return 0;
           }

           if (personality.neverLoseTarget)
               closestTargetVisibility = 4;

           waiting = false;
           target = targetStats;
           targetBridge = target.GetComponent<Bridge>();


           return closestTargetVisibility;
       }

       /// <summary>
       /// Main attack decisionmaker, works exclusively with FoundAggroTarget, as we need both a target and a high visisbility in order to act on anything.
       /// <param name="visibility"></param>
       /// <returns></returns>
       AIMission FoundTarget(float visibility)
       {
           ///Lost Target

           if (visibility <= 1f && target != null)//If the visibility of the target is lacking, start counting down to lose interest
           {
               //Attempt to go to the previously set attack position
               Dbug("Lost sight of " + target.name + " at " + visibility + " visibility,  checking again");

               continueAttackCount--;
               if (continueAttackCount <= 0)
               {
                   //Reset if we can't find the target           
                   ClearTarget("Lost Sight");
               }
           }

           //prioritize the attacker if there is one
           SetTarget(attackerStats);


           //Sanity check, will not change mission if we have no valid target
           if (TargetMissing()) return currentMission;


           //The decision for chasing is fundamentally different that most other attack patterns, so we need to set up the chase pathing early.
           if (personality.tactics == AITactics.Chase && !following)
               StartFollowPath(target.transform);

           //Catch for Missing PatrolPath

           if (currentPatrol == null) GetPatrolPath();

           if (AmIArmed())//Behaviour tree for armed ships
           {
               Dbug(bridge.name + " AM ARMED, ATTEMPTING TO HARM " + target.name);
               if (visibility <= 2) // if we are losing target, Ping to find it again
                   Ping();

               //If we have visibility and have not given up, attempt to attack
               if (visibility >= 0 && continueAttackCount > 0)
               {
                   //if the target is not in range of the patrol sausage, return the last valid mission
                   if (!InRangeOfPatrol(target.transform) && !following) return currentMission;

                   return AIMission.Attacking;
               }
               else
                   ClearTarget();

           }
           else if (attackerStats != null)//if i'm being attacked but have no weapons, flee.
               return AIMission.Fleeing;

           return currentMission;
       }

       /// <summary>
       /// Clears the target and sets attack to off
       /// </summary>
       /// <param name="why"></param>
       void ClearTarget(string why = "")
       {
           continueAttackCount = 10;
           target = null;
           attackerStats = null;
           MyWeapons().ClearAllTargets();
           Dbug(why);

       }

       /// <summary>
       /// Sets target to the given sonar stats. If the given new target is null, or not active in hierarchy,
       /// will not take any action.
       /// </summary>
       /// <param name="newTarget"></param>
       void SetTarget(SonarStats newTarget)
       {
           if (newTarget == null) return;
           if (!newTarget.gameObject.activeInHierarchy) return;// catches despawned attackers        

           target = newTarget;
           if (target.GetComponent<Bridge>())
               targetBridge = target.GetComponent<Bridge>();
           Dbug("Setting Target to " + target.name);
       }

       #endregion

       //The actual mission States
       //Consist of a while loop that will continue unless broken or mission changed.
       // anything before the while loop is the "State entry" for caching and init
       //anything after the loop is "State Exit" for cleanup or reset;
       #region States

       #region Attack
       /// <summary>
       /// Attack behaviour state, runs the approach and firing tactic of the AI.
       /// </summary>
       /// <returns></returns>
       IEnumerator AttackBehaviour()
       {
           //Entering State
           //This is now the last mission we tried
           previousMission = currentMission;


           if (TargetMissing()) // if we lost target while entering attack behaviour,  Pause and exit attack.
           {
               WaitFor(1);
               yield break;
           }


           if (aiMover.AmIstuck())//If we have gotten stuck, and relocation has happened, change to chase so we can continue moving
           {
               //  Dbug("Found enemy!, breaking stuck");
               personality.tactics = AITactics.Chase;
               aiMover.BreakStuck();
           }

           Dbug("Started Attacking: " + target.name);

           //Decide the approach path we want to use. (right now only variation is DontStop, any other case is handled in SetApproachPath)
           Vector3 attackSpot = Vector3.one;
           if (personality.tactics != AITactics.DontStop)
               attackSpot = SetApproachPath(target.transform.position);
           else
               GetPatrolPath();

           //At the start of an attack, make sure we are moving fast
           aiMover.SetThrottle(Throttle.full);


           //Give the correct target to the weapons;
           MyWeapons().SetLeadAim(target);
           if (!following) // only reset the list if we are not on a Follow spot
               MoveIndex = 0;

           //Adds combat zone for combat music     
           if (AmIAttackingPlayer() && !noCombatMusic)
               SceneControl.Get().AddToCombat(gameObject);

           //Running State until its changed
           while (currentMission == previousMission)//will exit as soon as CheckMission changes the mission.
           {

               //Always attempt to fire at the target,  will only fire if within reasonable range
               Fire();
               if (personality.tactics == AITactics.DontStop)               //No attack runs if we are on Dontstop mode
                   RunPatrol(Vector3.zero);
               else
               {
                   //If we are close enough to the player or the target location and see him, start attack run
                   if (aiMover.IsAt(attackSpot, target.transform.position, closeMoveDistance) || MyWeapons().InRange(target.transform) || Calc.IsLOS(transform.position, target.transform.position, terrainMask))
                   {
                       //Set currentaction so we can inspect what i'm trying to do
                       currentAction = AIAction.FiringAndMoving;

                       //if (!attackRun)
                       //  {
                       // Before Commenting out: Finish the attackrun before breaking and deferring to the decisiontree to plan another run
                       // After commenting: Will continuously do attackruns back to back if still applicable.

                       //Holds the Attack state until the run is complete.
                       yield return AttackRun();
                       // attackRun = false;
                       Dbug("Fininshing attack, continuing attack loop!");
                       // }                 
                       // break; // should we break?

                   }
                   else// approach the target spot near the player
                   {
                       PathToAttackPoint();
                       currentAction = AIAction.Moving;
                       //Recalculate target spot if target has moved out of range of spot AND is within range of the patrol.
                       if (InRangeOfPatrol(target.transform) && !following)
                       {
                           float minMoveDistance = Mathf.Clamp(myWeapons.OptimalRange(), 25, 100);
                           // GetLegalPos(aiMover.InterceptPosition(target))
                           if (!Calc.WithinDistance(minMoveDistance, GetLegalPos(target.transform.position), attackSpot))
                           {
                               attackSpot = SetApproachPath(target.transform.position);
                               MoveIndex = 0;
                               Dbug("Target moved: " + minMoveDistance + " from old position, recalculating approach");
                           }
                       }
                   }
               }
               yield return new WaitForEndOfFrame();
           }

           //Start the proceess of stopping combat music, only succesful if no one else is in combat range
           if (!noCombatMusic)
               StartCoroutine(WaitAndEndCombatMusic());
           //Exiting State
           aiMover.SetThrottle(Throttle.full);
           Dbug(bridge.name + " is Ending Attack");
           ClearTarget("Ended Attack");

           yield break;
       }

       /// <summary>
       /// Attempts to stop music, if another enemy is added within 3 seconds, we abort stopping the music
       /// </summary>
       /// <returns></returns>
       IEnumerator WaitAndEndCombatMusic()
       {
           yield return new WaitForSeconds(3);
           if (target != null) yield break;

           SceneControl.Get().RemoveFromCombat(gameObject);
       }

       /// <summary>
       /// Attack execution method. Consists of a movement and an attack hold when the movement is done.
       /// </summary>
       /// <returns></returns>
       IEnumerator AttackRun()
       {
           //Pick two normalized Vectors L/R or Up/Down, get a safe position along those two vectors, set points at one and ending at the other.       
           pathToFollow = TacticsPath();

           if (!following)
               MoveIndex = 0;
           attackTime = 0;//reset the minimum attack timer

           float minimumAttackTime = 2;
           aiMover.SetThrottle(Throttle.full);

           //While we are not at our attackpoint yet
           while (!PathToAttackPoint())// Can set attack points through walls, needs fixing
           {
               currentAction = AIAction.FiringAndMoving;

               //aiMover.SetSpeed(0.6f);
               if (currentMission != AIMission.Attacking) break;
               if (!Calc.IsLOS(transform.position, pathToFollow[MoveIndex], terrainMask)) yield break;  // if we set a position that we can't see, break
                                                                                                        // Dbug("Pathing to attackpoint in attackrun");
               Fire();
               //  if (currentAction != AIAction.Firing) break;
               yield return new WaitForEndOfFrame();
           }

           //Start idleAttack
           aiMover.SetThrottle(Throttle.half);

           //When we get to the end of the path, stop for a while
           while (attackTime < minimumAttackTime)
           {
               currentAction = AIAction.Firing;
               if (currentMission != AIMission.Attacking) break;

               //Dbug("At the end of the path: " + pathToFollow.Count + " / " + followPoints.Count);
               Fire();

               attackTime += Time.deltaTime;
               yield return new WaitForEndOfFrame();
           }

           Dbug("Finished Attack Run");

           //If we got new points meanwhile
           /*  if (FoundAggroTarget() > 1 && currentMission == AIMission.Attacking)
             {
                 yield return AttackRun();
                 yield break;
             }   // Commented out 11/14/16
           yield return new WaitForSeconds(0.5f);
           yield break; //returns control to main attack loop

       }

       /// <summary>
       /// Gets the strong vector to attack from the transform, even if it does not have a bridge.
       /// </summary>
       /// <param name="targetTrans"></param>
       /// <returns></returns>
       Vector3 GetStrongVector(Transform targetTrans)
       {
           if (targetTrans.GetComponent<Bridge>())
               return targetTrans.GetComponent<Bridge>().strongVector;
           else
               return targetTrans.forward;

       }


       /// <summary>
       /// Gets the weak vector from the target vector, even if it has no bridge.
       /// </summary>
       /// <param name="targetTrans"></param>
       /// <returns></returns>
       Vector3 GetWeakVector(Transform targetTrans)
       {
           if (targetTrans.GetComponent<Bridge>())
               return targetTrans.GetComponent<Bridge>().weakVector;
           else
               return -targetTrans.forward;

       }

       #region Path Scope Variables
       Vector3 rawVector;
       Vector3 targetPos;
       Vector3 flatTargetVector;
       Vector3 rVector;
       float optimalRange;
       Vector3 myStrongVector;
       Vector3 theirWeakVector;
       Vector3 targetWeakVector;
       Vector3 relativeHeadingDir;
       float attackVectorLength;
       float clampedScale;
       Vector3 relUpVector;
       float RLDot;
       int rightLeftFlip;
       int aboveStrong;
       #endregion


       /// <summary>
       /// Constructs a legal safe path for the AI for his attackrun
       /// </summary>
       /// <returns></returns>
       public List<Vector3> TacticsPath()
       {
           //Init
           List<Vector3> returnpath = new List<Vector3>();

           //if we have no target, just return my position and wait until further orders   
           if (TargetMissing())
           {
               returnpath.Add(transform.position);
               return returnpath;
           }

           //For the two most typical attack patterns, 
           if (personality.tactics == AITactics.DiveBy || personality.tactics == AITactics.Strafe)
           {
               //float hitTime = 0;
               rawVector = (aiMover.InterceptPosition(target) - transform.position); //This vector can be huge sometimes

               //Dbug("Raw Lead Vector to Target is : " + rawVector + " in " + hitTime + " seconds");
               // Debug.DrawRay(transform.position, rawVector, Color.green, 15);

               //ensures the point is inside the patrol sausage, will clamp to the surface of the sausage if not the case
               targetPos = currentPatrol.GetClosestLegalPos(transform.position + rawVector);

               //flatten the vector in the Y to avoid the AI from getting the PID controller in a strange place TODO allow the ship to correct itself if it gets itself into a  non-flat heading
               flatTargetVector = new Vector3(rawVector.x, 0, rawVector.z);

               //relative right to the target
               rVector = Vector3.Cross(rawVector.normalized, Vector3.up).normalized;
               // Vector3 upVector = Vector3.Cross(targetVector.normalized, rVector); //UNUSED

               //Gets the optimal range from the weapons setup 
               optimalRange = MyWeapons().OptimalRange();


               // myStrongVector = transform.rotation * GetStrongVector(transform.parent);
               //reads their current weak vector (Set manually per bridge)
               theirWeakVector = target.transform.rotation * GetWeakVector(target.transform);

               //final point is the target position  + their weakest side
               targetWeakVector = rawVector + theirWeakVector.normalized * optimalRange;

               //A sized relatve right vector for calculating horizontal points on the path
               relativeHeadingDir = rVector * aiMover.GetSpeed();
               Debug.DrawRay(transform.position, relativeHeadingDir * 5, Color.red, 5);

               //cache distance to target;
               attackVectorLength = targetWeakVector.magnitude;

               //clamped scale is for setting an even distribution of points along the way to the target based on the distance
               clampedScale = Mathf.Clamp(attackVectorLength / 4, 3, 15);

               //Choose what side i approach the target ship with
               RLDot = Vector3.Dot(rVector, transform.forward);

               //figures out if we need to flip our vector multipliers, depending on what direction the ships are heading in relation to each other
               rightLeftFlip = 1;
               aboveStrong = -1;
               if (bridge.strongVector.y < 0)//If i'm stronger below deck
                   aboveStrong = 1;
               //If we are heading away from the RightVector(rVector)
               if (RLDot < 0)
                   rightLeftFlip = -1;

               //The Up vector we are using
               relUpVector = Vector3.up * aboveStrong; //Proper up vector
           }
           //Sets the Open water attack path, only two i can think of atm
           switch (personality.tactics)
           {
               //Constantly try going to a point to the left or right of the ship
               case AITactics.Strafe:
                   {
                       Vector3 finalFlank = currentPatrol.GetClosestLegalPos(targetPos + relativeHeadingDir * rightLeftFlip); //+ rVector * rightLeftFlip + relUpVector * clampedScale / 2);
                       returnpath.Add(finalFlank);

                       Debug.DrawLine(transform.position, returnpath[0], Color.magenta, 5);
                       //   Debug.DrawLine(returnpath[0], returnpath[1], Color.red, 5);

                       break;
                   }
               //Make a path that takes the ship above or below the target
               case AITactics.DiveBy:
                   {

                       returnpath.Add(currentPatrol.GetClosestLegalPos(transform.position + targetWeakVector / 3 + relUpVector * clampedScale));
                       returnpath.Add(currentPatrol.GetClosestLegalPos(transform.position + targetWeakVector / 1.5f + relUpVector * clampedScale));
                       returnpath.Add(currentPatrol.GetClosestLegalPos(transform.position + targetWeakVector + relUpVector));

                       Debug.DrawLine(transform.position, returnpath[0], Color.cyan, 15);
                       Debug.DrawLine(returnpath[0], returnpath[1], Color.cyan, 15);
                       Debug.DrawLine(returnpath[1], returnpath[2], Color.cyan, 15);

                       break;
                   }
               case AITactics.Kite://TODO An attack behaviour for the very long-range torpedo ships, ping and stay at max torpedo range while following the patrolpath
                   {
                       break;
                   }
               case AITactics.DontStop:
                   {
                       returnpath = pathToFollow;
                       break;
                   }
               case AITactics.Chase:
                   {
                       returnpath = followPoints;
                       break;
                   }
               default:
                   {
                       // Debug.DrawRay(transform.position, rVector, Color.white, 5);
                       break;
                   }
           }
           //TODO Add a good return point
           return returnpath;
       }

       //Sets the attack path to follow
       //Pathfinds the patrol to find a safe path to approach the target
       public Vector3 SetApproachPath(Vector3 targetPosition)
       {
           Vector3 targetAttackSpot = GetLegalPos(targetPosition);
           pathToFollow = myPath.PathToVector3(targetAttackSpot, transform.position);

           Debug.DrawLine(transform.position, targetAttackSpot, Color.magenta, 0.1f);
           Ray directRay = new Ray(transform.position, targetAttackSpot - transform.position);
           //If the targetposition that the closest PathNode, just return the targetAttackspot
           /* if ((pathToFollow[1] - transform.position).sqrMagnitude > (targetAttackSpot - transform.position).sqrMagnitude|| !Physics.SphereCast(directRay, 5, directRay.direction.magnitude, LayerMask.GetMask("Terrain")))
            {
                pathToFollow = new List<Vector3>();
                pathToFollow.Add(targetAttackSpot);
            }

           for (int i = 0; i < pathToFollow.Count - 1; i++)
               Debug.DrawLine(pathToFollow[i], pathToFollow[i + 1], Color.yellow, 15f);
           return targetAttackSpot;
       }


       /// <summary>
       /// Movement Order method, Sets up a safe path through the AI's patrol sausage, to the target. //TODO Check to see if player is closer than closest WP, in which case set target position towards him
       /// </summary>
       /// <returns></returns>
       bool PathToAttackPoint()
       {
           if (pathToFollow == null) return true;

           //if within distance of new waypoint change the target

           if (pathToFollow.Count < 1) return true;//Check PathToFollow     
           if (MoveIndex > pathToFollow.Count - 1)
               MoveIndex = pathToFollow.Count - 1;
           //  Dbug("Index/PathToFollow: " + MoveIndex + "/" + pathToFollow.Count);

           Vector3 tPos = pathToFollow[MoveIndex];

           Vector3 nPos = SafeNextPos(tPos, pathToFollow, MoveIndex, false);

           Debug.DrawRay(transform.position, nPos - transform.position, Color.red, 0.1f);
           Debug.DrawRay(transform.position, tPos - transform.position, Color.cyan, 0.1f);

           if (!aiMover.Move(tPos, nPos)) return false;//Continue moving as long as we are not at the next waypoint

           if (MoveIndex < pathToFollow.Count - 1)
               MoveIndex++;
           else
           {
               if (!following)
                   MoveIndex = 0;
               else //if we are following, return the the previous point
               {
                   if (MoveIndex > 0)
                       MoveIndex -= 1;
               }
               return true;
           }

           return false;
       }


       /// <summary>
       /// Returns the next few patrol points ahead of the AI, loops if we hit the max count or 0
       /// </summary>
       /// <param name="targetList">"Patrol list to check"</param>
       /// <param name="startIndex">"Where do we start checking?"</param>
       /// <param name="iterations"> How many points do we look for?</param>
       /// <returns></returns>
       public List<Vector3> NextPatrolPositions(List<Vector3> targetList, int startIndex, int iterations)
       {

           List<Vector3> returnList = new List<Vector3>();
           int cachedStartIndex = startIndex;
           for (int i = 0; i < iterations; i++)
           {
               int targetPosIndex = cachedStartIndex + i;//the next position based on starting point and iteration count

               if (targetPosIndex > targetList.Count - 1)//if the targetPosIndex is out of range of the target list, reset the startPoint
               {
                   targetPosIndex = 0;
                   cachedStartIndex = -i;
               }

               returnList.Add(targetList[targetPosIndex]);
           }
           return returnList;
       }


       /// <summary>
       /// Returns the next few patrol points ahead of the AI, returns Up to iteration count if there are enough points left.
       /// </summary>
       /// <param name="targetList">"Patrol list to check"</param>
       /// <param name="startIndex">"Where do we start checking?"</param>
       /// <param name="iterations"> How many points do we look for?</param>
       /// <returns></returns>
       public List<Vector3> NextPathPositions(List<Vector3> targetList, int startIndex, int iterations)
       {
           List<Vector3> returnList = new List<Vector3>();
           int cachedStartIndex = startIndex;
           for (int i = 0; i < iterations; i++)
           {
               int targetPosIndex = cachedStartIndex + i;//the next position based on starting point and iteration count

               if (targetPosIndex > targetList.Count - 1)//if the targetPosIndex is out of range of the target list
               {
                   //if the next Position is out of range, break early without adding
                   break;
               }

               returnList.Add(targetList[targetPosIndex]);
           }
           return returnList;
       }



       /// <summary>
       /// Gets a safe point as close as possible to the input Vector3, 
       /// </summary>
       /// <param name="tgt"></param>
       /// <returns></returns>
       Vector3 GetLegalPos(Vector3 tgt)
       {
           //Vector3 attackvector = tgt - transform.position;
           // Vector3 moveDirection = myRbody.velocity;

           ///Sets a point based on my preferred range on the vector between target and me, this could be further away too       
           Vector3 targetPosition = currentPatrol.GetClosestLegalPos(tgt);// +  attackvector - (attackvector.normalized * optimalRange)
                                                                          //Debug.DrawLine(transform.position+Vector3.up, targetPosition + Vector3.up, Color.white, 5);
           return targetPosition;
       }


       /// <summary>
       /// Firing function, returns true if we were able to fire
       /// </summary>
       /// <returns></returns>
       bool Fire()
       {
           //Sets the attackingplayer bool to true if we are trying to shoot the player
           attackingPlayer = AmIAttackingPlayer();

           if (!personality.forceHoldFire)
               if (myWeapons != null)
               {
                   bool couldFire = MyWeapons().Fire(target);

                   if (couldFire && !noCombatMusic)
                       SceneControl.Get().SetPlayerFiredUpon();
                   return couldFire;
               }
           return false;
       }

       #endregion

       #region Patrol
       //The patrol behaviour State
       /// <summary>
       /// Runs the AI through their patrol, until something else happens.
       /// </summary>
       /// <returns></returns>
       IEnumerator PatrolBehaviour()
       {
           //Entering State
           Dbug("Entering Patorl Behaviour at " + Time.time);
           previousMission = currentMission;

           //Catch for sudden change 
           if (AmIArmed() && personality.tactics != AITactics.DontStop && !TargetMissing())
               ClearTarget("Started Patrol");

           GetPatrolPath();
           //Gets a random offset direction for making the ship set waypoints on an offset in relation to the patrol points
           //used to make ship collisions less frequent if they share a patrol path
           Vector3 flatRandomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);

           MoveIndex = 0;
           aiMover.SetThrottle(Throttle.full);
           //Update Loop for Logic
           while (currentMission == previousMission)//will exit as soon as CheckMission changes the mission.
           {
               currentAction = AIAction.Moving;
               // Dbug("Patrolling");
               RunPatrol(flatRandomOffset);
               yield return new WaitForEndOfFrame();
           }
           Dbug("exiting Patrol at " + Time.time);
           //Exiting State
       }

       /// <summary>
       /// Actual patrol movement method, iterates through the path ahead of me
       /// </summary>
       /// <param name="offset"></param>
       public void RunPatrol(Vector3 offset)
       {
           //if within distance of new waypoint change the target
           if (patrolPoints == null) return;
           if (patrolPoints.Count < 1) return;

           //    Dbug("running patrol with offset: " + offset);
           int lastPoint = MoveIndex - 1;
           if (lastPoint < 0)
               lastPoint = pathToFollow.Count - 1;

           WaypointNode targetNode = patrolPoints[MoveIndex];

           if (targetNode == null) return;

           Vector3 tPos = GetOffsetToWP(offset, pathToFollow[lastPoint], targetNode);
           //   Debug.DrawLine(transform.position, tPos, Color.blue, 0.1f);
           Vector3 nPos = SafeNextPos(tPos, pathToFollow, MoveIndex, true);

           aiMover.SetDivision(targetNode.approachSpeed);

           if (aiMover.Move(tPos, nPos))
           {
               if (MoveIndex < pathToFollow.Count - 1)
                   MoveIndex++;
               else
                   MoveIndex = 0;
           }
       }

       /// <summary>
       /// Resets patrol path
       /// </summary>
       void EmergencyResetPatrol()
       {
           nearbyPatrolPaths = new List<PatrolPath>();
           currentPatrol = null;
           pathToFollow = new List<Vector3>();

       }

       /// <summary>
       /// Gets a nearby patrol path, if i dont have one. (Should be a safe GET instead of a method)
       /// </summary>
       public void GetPatrolPath()
       {
           //does not override patrolPathChanges anymore, 
           //TODO YAGNI Other methods of patrol switching
           if (currentPatrol == null)
           {
               if (nearbyPatrolPaths == null)
                   nearbyPatrolPaths = new List<PatrolPath>();

               if (nearbyPatrolPaths.Count < 1 || nearbyPatrolPaths.First() != null)//Override for if the AI has no preset path(spawned in)
               {
                   /*PatrolPath path = navMan.GetClosestPatrolPath(transform.position);
                   if (path != null)
                       nearbyPatrolPaths.Insert(0, path);
                   else
                       return;
               }
               //Select closest patrol            
               currentPatrol = nearbyPatrolPaths[0];
           }

           //Construct a pathfinding instance with the currentPatrol's waypoints, and the ship's heading

           myPath = new PathFind();
           // Dbug("PathFind = null?: " + (myPath == null).ToString() + " got new pathFind Object from " + currentPatrol.name);
           //Gets the closest Waypoint out of the input allwps.
           WaypointNode patrolTarget = Calc.FindNearestWayPoint(currentPatrol.allWPS, transform.position);
           if (patrolTarget == null) { EmergencyResetPatrol(); return; }
           patrolPoints = new List<WaypointNode>();

           //Gets the waypoints from the pathfinding call (Entered the same location to make sure we patrol in a circle

           foreach (WaypointNode wp in myPath.PatrolToTarget(patrolTarget, patrolTarget))
               patrolPoints.Add(wp);

           // converst the Waypoints into a vector3 list to iterate on
           SetV3Path(patrolPoints);
       }

       /// <summary>
       /// Creates a simple path around this AI
       /// </summary>
       void CreateDefaultPath()
       {
           nearbyPatrolPaths.Add((PatrolPath)Instantiate(defaultPath, transform.position, transform.rotation));
       }

       /// <summary>
       /// Sets a new path as the home path of this ship
       /// </summary>
       /// <param name="path"></param>
       public void FollowNewPath(PatrolPath path)
       {
           if (nearbyPatrolPaths.Contains(path))
               nearbyPatrolPaths.Remove(path);
           nearbyPatrolPaths.Insert(0, path);
           currentPatrol = path;
           GetPatrolPath();
       }


       #endregion

       #region Follow


       /// <summary>
       /// Starts the target path building loop for following off a patrol sausage
       /// </summary>
       /// <param name="targetToFollow"></param>
       void StartFollowPath(Transform targetToFollow)
       {
           if (following) return;
           following = true;
           followPoints = new List<Vector3>();
           MoveIndex = 0;
           StartCoroutine(SetFollowPathLoop(targetToFollow));
       }


       /// <summary>
       /// Sets the points to the target. DONT CALL THIS DIRECTLY. Call StartFollowPath.
       /// </summary>
       /// <param name="targetToFollow"></param>
       /// <returns></returns>
       const float groundBuffer = 25;
       Vector3 yOffset = Vector3.zero;
       IEnumerator SetFollowPathLoop(Transform targetToFollow)
       {
           Vector3 previousPoint = transform.position;
           Vector3 currentPoint = Vector3.zero;
           int lostSightCount = 5;

           //Continuously sets the position of the target,
           while (following)
           {
               yield return new WaitForSeconds(1);
               if (targetToFollow == null) break;
               yOffset = Vector3.zero;
               if (FoundAggroTarget() > 1)//If we can see player, reset sight count and set a new position at his location
               {

                   /* if (lostSightCount < 5)
                        previousPoint = transform.position;
                   lostSightCount = 5;
                   currentPoint = targetToFollow.position - (targetToFollow.position - previousPoint).normalized * MyWeapons().ShortRange() * 0.2f;

                   Ray groundedRay = new Ray(currentPoint, -Vector3.up);
                   Ray ceilingRay = new Ray(currentPoint, Vector3.up);
                   RaycastHit hit;
                   if (Physics.Raycast(groundedRay, out hit, groundBuffer, terrainMask))
                   {
                       Vector3 groundedVector = groundedRay.origin - hit.point;
                       if (groundedVector.sqrMagnitude < groundBuffer * groundBuffer)
                           yOffset = Vector3.up * (groundBuffer - hit.distance);
                   }

                   if (Physics.Raycast(ceilingRay, out hit, groundBuffer, terrainMask))
                   {
                       Vector3 ceilingVector = ceilingRay.origin - hit.point;
                       if (ceilingVector.sqrMagnitude < groundBuffer * groundBuffer)
                           yOffset -= Vector3.up * (groundBuffer - hit.distance);
                   }

                   currentPoint += yOffset;
               }
               else //if not, start counting down to stop behaviour
               {
                   lostSightCount--;
               }

               if (lostSightCount < 1)//if we have less than 1 sightCount, the ship loses interest and stops setting followPathPoints
                   break;

               //If the target is veryclose, skip some step to go straight for it if its not blocked TODO some kind of path reduction tool, looks at all vectors and when applicable will reduce superfulous
               if (!Calc.WithinDistance(MyWeapons().ShortRange() / 2, currentPoint, previousPoint))//only add new points if there has been a significiant difference in the distance
               {
                   if (followPoints.Count > 3 + MoveIndex)
                   {
                       Vector3 directVector = currentPoint - transform.position;
                       Ray directRay = new Ray(transform.position, directVector);
                       if (!Physics.SphereCast(directRay, 3, directVector.magnitude, terrainMask))
                       {
                           //Remove The points between the current one and the target point
                           followPoints.RemoveRange(MoveIndex, (followPoints.Count - MoveIndex) - 1);
                           followPoints.Add(transform.position + (directVector / 1.5f));
                       }
                   }

                   followPoints.Add(currentPoint);
                   previousPoint = currentPoint;
               }

               DebugTools.DrawLinePositions(followPoints, Color.green, 45);

           }
       }

       /// <summary>
       /// Returns to the path
       /// </summary>
       /// <returns></returns>
       IEnumerator ReturnToPath()
       {
           previousMission = currentMission;
           PatrolPath returnPath;
           //Get the index of the best exit point on our followpoint list as well as the path to return to if it's new
           /*int exitIndex = NavigationManager.Get().GetNiceClosePoint(transform.position, followPoints, out returnPath);
           if (returnPath != null)
               currentPatrol = returnPath;

           currentAction = AIAction.Moving;

           for (int i = followPoints.Count - 1; i > exitIndex; i--)
           {
               if (i > 0)
                   Debug.DrawLine(followPoints[i], followPoints[i - 1], Color.blue, 0.1f);
           }

           aiMover.SetThrottle(Throttle.full);
           while (currentMission == previousMission)
           {
               if (MoveIndex >= followPoints.Count) // Catch for short return trip
               {
                   following = false;
                   currentMission = AIMission.Patrolling;
                   break;
               }

               Vector3 tPos = followPoints[MoveIndex];

               Vector3 nPos = SafePrevPos(tPos, followPoints, MoveIndex, exitIndex);

               if (aiMover.Move(tPos, nPos))
               {
                   if (MoveIndex > exitIndex)//If we are not yet at the exit index, keep going
                   {
                       followPoints.RemoveAt(MoveIndex);
                       MoveIndex--;
                   }
                   else
                   {
                       following = false;
                       currentMission = AIMission.Patrolling;
                       break;
                   }
               }

               yield return new WaitForEndOfFrame();
           }
           yield return new WaitForSeconds(1);
           yield break;
       }
       #endregion


       #region Flee
       //behaviour for schmall sons tha t
       IEnumerator FleeBehaviour()
       {
           //Entering State
           previousMission = currentMission;

           yield return new WaitForEndOfFrame();

           if (!noCombatMusic)
               SceneControl.Get().RemoveFromCombat(gameObject);
           GetPatrolPath();
           Vector3 flatRandomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
           Vector3 startFleePosition = transform.position;
           currentAction = AIAction.Moving;
           aiMover.SetThrottle(Throttle.overdrive);

           while (currentMission == previousMission)//will exit as soon as CheckMission changes the mission.
           {
               RunPatrol(flatRandomOffset);
               if (target && RunAway(target.transform))
                   AttemptDespawn();
               else if (!Calc.WithinDistance(150, transform, startFleePosition))
                   AttemptDespawn();

               yield return new WaitForEndOfFrame();
           }
           //Exiting State
           yield break;
       }

       //TODO YAGNI Default escape routes from the patrol route, despawn the ship when its out of sight
       /// <summary>
       ///Picks a good point to run away and does so
       /// </summary>
       /// <returns>True when ran away</returns>
       bool RunAway(Transform from)
       {
           if (Calc.WithinDistance(150, transform, from)) return true;
           return false;
       }

       /// <summary>
       /// Attempting the Despawn, only legal if we are not visible
       /// </summary>
       void AttemptDespawn()
       {
           if (!Cam.CanIsee(Camera.main, transform.position))
               bridge.hull.SelfDestruct();

       }

       #endregion

       #region IdleArea
       /// <summary>
       /// This AI stands still where it is
       /// </summary>   
       IEnumerator IdleBehaviour()
       {
           previousMission = currentMission;
           currentAction = AIAction.Waiting;

           while (currentMission == previousMission)
           {
               //Dbug("Idling");
               aiMover.ForceStop();
               yield return new WaitForSeconds(1);
           }

           yield break;
       }
       #endregion
       #endregion*/
#endregion