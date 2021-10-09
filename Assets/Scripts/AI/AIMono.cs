using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Sonar;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using ParadoxNotion;
using NodeCanvas.Framework;
using NodeCanvas.BehaviourTrees;
using System.Linq;
using System.Text;
using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine.UI.Extensions;

//TODO Ship AI Adding tool, All Ship Testing
namespace Diluvion.AI
{
    //TODO Add Factions to AIMONO
    public class AIMono : MonoBehaviour, IAlignable
    {

        [TabGroup("Settings")]
        [SerializeField]
        Captain myCaptain;
        public Captain MyCaptain
        {
            get
            {
                if (myCaptain != null) return myCaptain;
                if (this == null) return null;
                myCaptain = Resources.Load<Captain>("defaultCaptain");
                return myCaptain;
            }
            set
            {
                myCaptain = value;
            }
        }

        [TabGroup("Debug")]
        public AIAction action = AIAction.Idle;

        [TabGroup("Debug")]
        [SerializeField]
        BehaviourTree baseTree;       
        public BehaviourTree BaseTree
        {
            get
            {
                if (baseTree != null) return baseTree;
                baseTree = Resources.Load<BehaviourTree>("shipAITrunk");
                return baseTree;
            }
            private set
            {
                baseTree = value;
            }
        }

        [ReadOnly]
        public List<ContextTarget> currentContextTargets = new List<ContextTarget>();
        [ReadOnly]
        public List<ContextTarget> memoryTargets = new List<ContextTarget>();

        public List<SonarStats> myFriends = new List<SonarStats>();

        [SerializeField]
        private float danger = 0;
        
       
        public DamageRange shortRange;

        public DamageRange longRange;

        public DamageRange optimalRange;

        public IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);
            InitAI();
        }
        
        SubTree myMissionTree;
        public SubTree MyMissionTree
        {
            get
            {
                if (myMissionTree != null) return myMissionTree;
                return myMissionTree = TreeOwner.graph.GetNodeWithTag<SubTree>("Mission");
            }
            private set
            {
                myMissionTree = value;
            }
        }
        
        
        Bridge bridge;
        public Bridge MyBridge
        {
            get
            {
                if (bridge != null) return bridge;
                return bridge = GetComponent<Bridge>();
            }
        }
        
        List<WeaponSystem> _weaponSystems = new List<WeaponSystem>();

        public List<WeaponSystem> WeaponSystems
        {
            get
            {
                if (_weaponSystems != null && _weaponSystems.Count != 0) return _weaponSystems;
                _weaponSystems =  new List<WeaponSystem>(GetComponents<WeaponSystem>());// Failsafe in case of failed spawn
                return _weaponSystems;
            }
        }

        
        List<Reaction> myReactions = new List<Reaction>();

        public List<Reaction> MyReactions
        {
            get
            {
                if (myReactions != null && myReactions.Count != 0) return myReactions;
                myReactions = MyCaptain.reactions;// Failsafe in case of failed spawn
                return myReactions;
            }
        }


        /// <summary>
        /// Add the trees to blackboard
        /// </summary>
        public void InitAI()
        {
            //Copy the default Blackboard values         
            TreeOwner.blackboard = MyBlackboard;
            foreach (KeyValuePair<string, Variable> v in DefaultBB().variables)
            {
                if (v.Key == null) continue;
                object value = v.Value.value;
                if (value == null)                
                    MyBlackboard.AddVariable(v.Key, v.Value.varType);                
                else
                    MyBlackboard.AddVariable(v.Key, value);
            }
            
            //Set defense to the ship steering capacity of the personality, a range from 0-20
            MyHull.defense = 1 + MyCaptain.ShipSteering+10;
           
            
            //Set up the action subtrees from the personality
            MyBlackboard.AddVariable("Captain", MyCaptain);
            MyBlackboard.AddVariable("st_bravery", MyCaptain.Bravery);
            MyBlackboard.AddVariable("mySignatureRelations", localSignatureRelations);
            MyBlackboard.AddVariable("myFriends", myFriends);
            MyBlackboard.AddVariable("shipLength", MyBridge.width);
            
            MyBlackboard.AddVariable("defaultMission", MyCaptain.DefaultMission);
            MyBlackboard.AddVariable("defaultAttack", MyCaptain.DefaultAttack);
            MyBlackboard.AddVariable("defaultFlee", MyCaptain.DefaultFlee);
           // MyBlackboard.AddVariable("reactions", MyCaptain.reactions);
            MyBlackboard.AddVariable("myStrongVector", MyBridge.strongVector);

            MyBlackboard.AddVariable("missionSubTree", MyCaptain.DefaultMission);
            MyBlackboard.AddVariable("attackSubTree", MyCaptain.DefaultAttack);

            MyBlackboard.AddVariable("contextTargets", currentContextTargets);// Only added for easy inspection
            MyBlackboard.AddVariable("fleeSubTree", MyCaptain.DefaultFlee);
            
            MyBlackboard.AddVariable("navTarget", new Vector3(0,0,0));
       
            MyAvoider.SetMask(MyBridge.chassis.avoidanceMask);

            GetWeapons();
            //Set the tree to the default baseTree
            TreeOwner.SwitchBehaviour(BaseTree);
            MyMissionTree.subTree = MyCaptain.DefaultMission;
         
            //Set up callbacks
            MyDock.docked += DockedToMe;
            MyDock.unDocked += UndockedFromMe;
            MyHull.imHit += Damaged;
            MySonarStats.pinged += Pinged;
            RegisterSonarInterest();
        }
        
        #region weapons
        void GetWeapons()
        {
            List<WeaponSystem> weapons = new List<WeaponSystem>(GetComponents<WeaponSystem>());
            weapons.RemoveAll(item => item.equippedWeapon == null);
            MyBlackboard.AddVariable("weapons", weapons);        
            //Debug.Log("setting up weapons on " + gameObject.name, gameObject);

            int index = 0;
            shortRange = new DamageRange();
            longRange = new DamageRange();
            optimalRange = new DamageRange();
            
            float totalDPS = 0;
            
            foreach (WeaponSystem ws in weapons.OrderBy(ws => ws.Range()))
            {
                //TODO Dont add rangeTypes that are tied to max ranges, we want  SHORT( 10% of shortest weapon) LONG (Max range of longest weapon) and OPTIMAL ( 75% of shortest weapon, addin cg in all other weapons that can reach that range)
                totalDPS += ws.DamagePerSecond();
              
                //Shortest Range (without multiplier for calculation purposes)
                if (index == 0)
                {    
                   // Debug.Log("Adding "+ ws.equippedWeapon.niceName+ " ("+index+") "+ ws.Range() + " as shortRange " );
                    shortRange.rangeType = RangeType.Short;
                    shortRange.range =  ws.Range();
                    shortRange.damage = ws.DamagePerSecond();                      
                }
                
//                Debug.Log("Index: " + index + "/"+(weapons.Count-1));
                
                //Longst Range, can be the samea s the shortest range
                if(index == weapons.Count-1)
                {
                  //  Debug.Log("Adding "+ ws.equippedWeapon.niceName+ "("+index+") "+ ws.Range() + " as longRange " );
                    longRange.rangeType = RangeType.Long;
                    longRange.range = ws.Range() * 0.9f;
                    longRange.damage = ws.DamagePerSecond();
                }
                
                index++;
                ws.fired += FiredWeapon;
            }
          
            optimalRange.range = shortRange.range*0.8f;
           //Debug.Log("adding "+ optimalRange.range + " as optimal " );
            optimalRange.damage = totalDPS;
            optimalRange.rangeType = RangeType.Optimal;

            shortRange.range *= 0.3f;// Set the actual Short Range here
            shortRange.range = Mathf.Clamp(shortRange.range, 15, 100);
            
            MyBlackboard.AddVariable("shortRange", shortRange);
            MyBlackboard.AddVariable("longRange", longRange);
            MyBlackboard.AddVariable("optimalRange", optimalRange);         
        }
        #endregion

        #region faction parser
           
        //Adds a faction to this AI's List of factions, adds signatures with it
        public void ApplyFactions(List<Faction> factions)
        {
            if (factions == null) return;
            foreach (Faction f in factions)
            {
                if (_myFactions.Contains(f)) { continue;}
                MySonarStats.AddSignature(f.factionSignature);
                _myFactions.Add(f);
                AddRelations(f.signatureRelations);
            }
        }

        
        /// <summary>
        /// List of Factions this AI belongs to
        /// </summary>
        [SerializeField]
        List<Faction> _myFactions = new List<Faction>();
        public List<Faction> MyFactions
        {
            get
            {
                if (_myFactions != null&&_myFactions.Count>0) return _myFactions;
                ApplyFactions(MyCaptain.baseFactions);
                return _myFactions;
            }
        }

        public List<SignatureValues> localSignatureRelations = new List<SignatureValues>();
        
        [SerializeField]
        private float opinionValue = 0;


        [Button()]
        public void TestPlayerOpinion()
        {
            Debug.Log(name + " opinion of player: " + OpinionOfPlayer());
        }

        /// <summary>
        /// Returns positive or negative based on the AI's opinion of the player
        /// </summary>
        /// <returns></returns>
        float OpinionOfPlayer()
        {
            if (PlayerManager.PlayerSonarStats() == null) return 0;
            return SignatureOpinionValue(PlayerManager.PlayerSonarStats());
        }

    /// <summary>
    /// Returns a positive or negative value based on this AI's signature relations
    /// </summary>
    /// <param name="target">The sonar which to compare this AI's Relations to</param>
    /// <returns></returns>
        public float SignatureOpinionValue(SonarStats target)
        {
            float signatureOpinion = 0;
            //Signature Matching
            foreach (Signature s in target.signatures)
            {
                float opinion;
                
                if (!HasSignatureRelation(s, out opinion)) continue;
                   
                signatureOpinion += opinion;
            }
            return signatureOpinion;
        }
        
        /// <summary>
        /// Captain checks its signature relations to the input sonar
        /// </summary>
        /// <param name="targetSonar"> the sonar to check </param>
        /// <param name ="whatIsHappening"> What is the context of the interaction with the target</param>
        /// <param name="value"> The resulting value from -10 to 10</param>
        /// <returns>True if we have a relationship to this sonar at all (it is interesting)</returns>
        public TargetContext TargetRelations(ContextTarget targetSonar, ActionContext whatIsHappening, out float priorityValue)
        {
            opinionValue = targetSonar.InstanceAttitude;
            priorityValue = 0;
            bool hasOneMatch = false;
           
            if (targetSonar != null&&targetSonar.target!=null&&targetSonar.target.signatures.Count>0)
            {
                if (whatIsHappening == ActionContext.Attacked)
                {
                    opinionValue -= 2;
                    targetSonar.UpdateContextTarget(opinionValue);
                }

                float signatureValue = SignatureOpinionValue(targetSonar.target);
                opinionValue = signatureValue;
                
                bool ivDistance = false;
                bool isSticky = false;

                //TODO YAGNI might not need an extra loop in here
                InverseAndSticky(targetSonar.target, out ivDistance, out isSticky);
                
                targetSonar.inverseDistanceRelation = ivDistance;
                targetSonar.sticky = isSticky; 
                
                //Debug.Log("Opinion of : " + targetSonar.target.name + " :" + opinionValue);
                priorityValue = Mathf.Abs(opinionValue);
            }
            
            TargetContext returnTargetContext= TargetContext.UnInteresting;
          
            if(opinionValue>0)
                returnTargetContext= TargetContext.Friend;
            else if (opinionValue < 0)
                returnTargetContext = MyCaptain.Enemy(targetSonar, MyHull.CurrentHPPercent());
            else if(hasOneMatch)
                returnTargetContext = TargetContext.Interesting;
                
            
            if (priorityValue > 0) // Reactions Manipulate the placement in the priority list for later, only add bonus if it all fits
            {
                foreach (Reaction r in MyReactions) 
                    if (r.HasReactionContext(whatIsHappening)&&r.HasTargetContext(returnTargetContext))
                        priorityValue += r.priorityMultiplier;
            }
            
           /* 
            if(returnTargetContext==TargetContext.Enemy)
            {
                
            }
            */
            return returnTargetContext;
        }

        float MyDanger()
        {
            danger = MyBridge.Danger();
            foreach (SonarStats ss in myFriends)
            {
                if (ss == null || !ss.gameObject.activeInHierarchy) continue;
                danger += ss.GetDanger();
            }
            return danger;
        }

        private Hull friendHull;
        public void AddFriend(SonarStats ss)
        {
            friendHull = null;
            if (myFriends.Contains(ss)) return;
            friendHull = ss.GetComponent<Hull>();
            if(friendHull&&friendHull.imHit!=null)
                friendHull.imHit += FriendDamaged;
            myFriends.Add(ss);

        }

        public void RemoveFriend(SonarStats ss)
        {
            friendHull = null;
            if (ss == null) return;
            if (!myFriends.Contains(ss)) return;
            friendHull = ss.GetComponent<Hull>();
            if(friendHull&&friendHull.imHit!=null)
               friendHull.imHit -= FriendDamaged;
           
            myFriends.Remove(ss);
        }

        void FriendDamaged(float howHard, GameObject byWho)
        {
            Damaged(howHard, byWho);
        }

        bool InverseAndSticky(SonarStats target, out bool inverseDistance, out bool sticky )
        {
            foreach (Signature s in target.signatures)
            {
                foreach(SignatureValues sv in localSignatureRelations)
                    if (sv.signature.Equals(s))
                    {
                        inverseDistance = sv.inverseDistance;
                        sticky = sv.sticky;
                        return true;
                    }
            }
            inverseDistance = false;
            sticky = false;
            return false;
        }
        
        

        bool HasSignatureRelation(Signature askingSignature, out float howMuch) //TODO Move this to a faction object Cache results on AIMono?
        {
            howMuch = 0;
            foreach(SignatureValues sv in localSignatureRelations)
                if (sv.signature.Equals(askingSignature))
                {
                    howMuch = sv.Value;
                    return true;
                }
           
            return false;
        }
        

        public void AddRelation(Signature s, float amount)
        {
            bool hasSig = false;
           
            foreach (SignatureValues sv in localSignatureRelations)
            { 
//                Debug.Log("attempting to append sig : " + s.name + " with relation " + amount);
                if (!sv.signature.Equals(s)) continue;
                hasSig = true;
                SignatureValues sigvalue = sv;
                sigvalue.Value += amount;
               
                break;
            }
          
            if (!hasSig)
            {
                SignatureValues sv = new SignatureValues
                {
                    Value = amount,
                    signature = s
                };
                
               // Debug.Log("attempting to append sig : " + sv.signature.name + " with relation " + sv.Value);
                localSignatureRelations.Add(sv);
            }
        }
        
        /// <summary>
        /// Multiple Add For sig relations
        /// </summary>
        /// <param name="sigs"></param>
        public void AddRelations(List<SignatureValues> sigs)
        {
            float value = 0;
//            Debug.Log("adding multiple signaturevalues: " + sigs.Count);
            foreach (SignatureValues sv in sigs)
            {
                AddRelation(sv.signature, sv.Value);
            }
        }
        
        /// <summary>
        /// GameObject overload
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        ContextTarget GetContext(GameObject obj)
        {
            if (obj == null) return null;
            return new ContextTarget(obj.GetComponent<SonarStats>());
        }

        /// <summary>
        /// Safe constructor/Getter for new/existing context targets
        /// </summary>
        /// <param name="ss"></param>
        /// <returns></returns>
        ContextTarget GetContext(SonarStats ss)
        {
            if (ss == null) return null;
            foreach(ContextTarget ct in currentContextTargets)
            {
                if (ct.SameTarget(ss))
                    return ct;
            }

            foreach (ContextTarget ct in memoryTargets)
            {
                if (ct == null) continue;
                if (ct.SameTarget(ss))
                    return ct;
            }           

            ContextTarget newCT = new ContextTarget(ss);          
            currentContextTargets.Add(newCT);
            return newCT;
        }

        /// <summary>
        /// Adds a Reaction to the MonoBehaviuor
        /// </summary>
        /// <param name="reac"></param>
        public void AddReaction(Reaction reac)
        {
            if (reac == null) return;
            foreach(Reaction r in myReactions)
                if (r.Equals(reac)) return;
            
            myReactions.Add(reac);
            
        }
        
        /// <summary>
        /// Adds Reactions to the Monobehaviour
        /// </summary>
        /// <param name="toAdd"></param>
        public void AddReactions(List<Reaction> toAdd)
        {
            foreach(Reaction r in toAdd)
                AddReaction(r);
        }
            
        /// <summary>
        /// Picks the first reaction that fits the contextTarget
        /// </summary>
        /// <param name="tc"></param>
        /// <returns></returns>
        public Reaction TargetReaction(ContextTarget tc)
        {
            foreach (Reaction r in MyReactions)
                if (r.CanReact(tc))
                {
                    return r; 
                }
                       
            return null;
        }

        
        #endregion
        
        #region SonarInterestParser

        void RegisterSonarInterest()
        {
            foreach (SonarStats ss in MyListener.inRangeStats)
            {
                AddSonarContext(ss);
            }
            MyListener.contactGained += AddSonarContext; 
            MyListener.contactLost += RemoveFromInterests;         
        }

        /// <summary>
        /// Callback function from the listener to the tree
        /// </summary>
        /// <param name="ss"></param>
        void AddSonarContext(SonarStats ss)
        {
            if (ss == null) return;
            ContextTarget ct = ContextTargetRelation(ss, ActionContext.Found);
            if (ct == null) return;
            TreeOwner.SendEvent("FoundInterest", ct);
        }

        /// <summary>
        /// GAMEOBJECT OVERLOAD Constructs a context with values based on the cap tain's relationship to the target
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        ContextTarget ContextTargetRelation(GameObject go, ActionContext ac)
        {
            if (go == null) return null;
            return ContextTargetRelation(go.GetComponent<SonarStats>(), ac);
        }

        /// <summary>
        /// SONARSTATS OVERLOAD Constructs a context with values based on the captain's relationship to the target
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        ContextTarget ContextTargetRelation(SonarStats ss, ActionContext ac)
        {
            if (ss == null) return null;
            if (this == null) return null;
            
            ContextTarget ct = GetContext(ss);
            //Debug.Log("found contextTarget: " + ct);
            ct.target = ss;
            ct.MyActionContext = ac;
            ct.MyTargetContext = TargetRelations(ct, ac, out ct.relationPriority);
            return ct;
        }

        /// <summary>
        /// Checks the currentContextTargets to see if we already have the targetStats
        /// </summary>
        /// <param name="targetStats"></param>
        /// <returns></returns>
        bool HaveContextTarget(SonarStats targetStats)
        {
            foreach (ContextTarget ct in currentContextTargets)
                if (ct.target == targetStats) return true;
            return false;
        }

    
        void RemoveFromInterests(SonarStats ss)
        {
            foreach (ContextTarget ct in currentContextTargets)
            {
                if (ct.target == ss&&!ct.sticky)
                {
                    RemoveFromInterests(ct);
                    return;
                }
            }               
        }

        void RemoveFromInterests(ContextTarget ct)
        {
            if (currentContextTargets.Contains(ct))
            {
                currentContextTargets.Remove(ct);
                AddMemory(ct);
                TreeOwner.SendEvent("LostInterest", ct);
            }
        }

        bool HaveMemoryOfTarget(ContextTarget context)
        {
            foreach (ContextTarget ct in memoryTargets)
                if (ct == context) return true;
            return false;
        }

        void AddMemory(ContextTarget context)
        {
            if (context == null) return;
            if (HaveMemoryOfTarget(context)) return;
            
            currentContextTargets.Add(context);         
        }

        ContextTarget RemoveFromMemory(ContextTarget ct)
        {
            if (!memoryTargets.Contains(ct)) return null;          
                memoryTargets.Remove(ct);
            return ct;
        }



        #endregion

        #region tree Callbacks  
        /// <summary>
        /// Undock callback function, registered with DockControl and passes the event to the tree
        /// </summary>
        /// <param name="whoDid"></param>
        public void FiredWeapon(WeaponSystem whoDid)
        {
            TreeOwner.SendEvent("ShotFired", whoDid);
        }
        
        /// <summary>
        /// Undock callback function, registered with DockControl and passes the event to the tree
        /// </summary>
        /// <param name="whoDid"></param>
        public void DockedToMe(DockControl whoDid)
        {
            ContextTarget ct = ContextTargetRelation(whoDid.gameObject, ActionContext.Docked);
            if (ct == null) return;        
            TreeOwner.SendEvent("Docked", ct);
        }


        /// <summary>
        /// Undock callback function, registered with DockControl and passes the event to the tree
        /// </summary>
        /// <param name="whoDid"></param>
        public void UndockedFromMe(DockControl whoDid)
        {
            ContextTarget ct = ContextTargetRelation(whoDid.gameObject, ActionContext.UnDocked);
            if (ct == null) return;
            TreeOwner.SendEvent("Undocked", ct);
        }

        /// <summary>
        /// Damaged Callback
        /// </summary>
        public void Damaged(float howHard, GameObject byWho)
        {
            if (this == null) return;
            ContextTarget ct = ContextTargetRelation(byWho, ActionContext.Attacked);
            if (ct == null) return;
          
            TreeOwner.SendEvent("Damaged", ct);
        }

     

        /// <summary>
        /// Ping function, sends the message to the tree about being pinged, registered to this gameObejct's sonarstats 'pinged' delegate
        /// /// </summary>
        /// <param name="byWho"></param>
        /// <param name="why"></param>
        public void Pinged(GameObject byWho, PingResult why)
        {
           
            ActionContext currentContext = ActionContext.Pinged;
            switch (why)
            {
                case PingResult.Hail:
                    {

                        currentContext = ActionContext.Hailed;
                        break;
                    }
                case PingResult.Ping:
                    {
                        currentContext = ActionContext.Pinged;
                        break;
                    }
                case PingResult.SOS:
                    {
                        currentContext = ActionContext.SOS;
                        break;
                    }
                default:
                    {
                        return;
                    }
            }
            ContextTarget ct = ContextTargetRelation(byWho, currentContext);
            if (ct == null) return;
            TreeOwner.SendEvent("Pinged", ct);
        }
        
        /// <summary>
        /// Resets themission node
        /// </summary>
        public void ResetMission()
        {
            if (MyMissionTree == null) return;
            MyMissionTree.Reset(true);       
        }
        #endregion     

        #region SafeReferences


        BehaviourTreeOwner treeOwner;
        BehaviourTreeOwner TreeOwner
        {
            get
            {
                if (treeOwner != null) return treeOwner;
                treeOwner = GetComponent<BehaviourTreeOwner>();
                if (treeOwner == null)
                {
                    treeOwner = gameObject.AddComponent<BehaviourTreeOwner>();
                }
                return treeOwner;
            }
            set
            {
                treeOwner = value;
            }
        }


        Blackboard DefaultBB()
        {
            return Resources.Load<Blackboard>("defaultBB");
        }

        Blackboard myBlackboard;
        Blackboard MyBlackboard
        {
            get
            {
                if (myBlackboard != null) return myBlackboard;
                myBlackboard = GetComponent<Blackboard>();
                if (myBlackboard == null)
                {
                    myBlackboard = gameObject.AddComponent<Blackboard>();
                }
                return myBlackboard;
            }
            set
            {
                myBlackboard = value;
            }
        }

        List<Signature> interestSignatures = new List<Signature>();
        public List<Signature> InterestSignatures
        {
            get
            {
                if (interestSignatures != null) return interestSignatures;
                interestSignatures.Clear();
                foreach (SignatureValues sr in MyCaptain.nonFactionRelations)
                    interestSignatures.Add(sr.signature);               
                return interestSignatures;
            }
            set
            {
                interestSignatures = value;
            }
        }

        SonarStats mySonarStats;
        public SonarStats MySonarStats
        {
            get
            {
                if (mySonarStats != null) return mySonarStats;
                mySonarStats = GetComponent<SonarStats>();
                if (mySonarStats == null) mySonarStats = gameObject.AddComponent<SonarStats>();
                return mySonarStats;
            }
            set
            {
                mySonarStats = value;
            }
        }


        Listener myListener;
        public Listener MyListener
        {
            get
            {
                if (myListener != null) return myListener;
                myListener = GetComponent<Listener>();
                if (myListener == null) myListener = gameObject.AddComponent<Listener>();
                return myListener;
            }
            set
            {
                myListener = value;
            }
        }

        Hull myHull;
        public Hull MyHull
        {
            get
            {
                if (myHull != null) return myHull;
                if (this == null) return null;
                myHull = GetComponent<Hull>();
                if (myHull == null) myHull = gameObject.AddComponent<Hull>();
                return myHull;
            }
            set
            {
                myHull = value;
            }
        }


        DockControl myDock;
        public DockControl MyDock
        {
            get
            {
                if (myDock != null) return myDock;
                myDock = GetComponent<DockControl>();
                if (myDock == null) myDock = gameObject.AddComponent<DockControl>();
                return myDock;
            }
            set
            {
                myDock = value;
            }
        }


        Navigation myNav;
        public Navigation MyNav
        {
            get
            {
                if (myNav != null) return myNav;
                if (GetComponent<Navigation>())
                    return myNav = GetComponent<Navigation>();
                else
                    return myNav = gameObject.AddComponent<Navigation>();
            }
            private set
            {
                myNav = value;
            }
        }

        bool avoiderSetUp = false;
        Avoider myAvoider;
        public Avoider MyAvoider
        {
            get
            {
                if (myAvoider == null)
                {
                    if (GetComponent<Avoider>())
                        myAvoider = GetComponent<Avoider>();
                    else
                        myAvoider = gameObject.AddComponent<Avoider>();
                }
                if (!avoiderSetUp)
                {
                    myAvoider.Setup();
                    avoiderSetUp = true;
                }
                return myAvoider;
            }
            private set
            {
                myAvoider = value;
            }
        }

        ShipMover myShipMover;
        public ShipMover MyShipMover
        {
            get
            {
                if (myShipMover != null) return myShipMover;
                if (!GetComponent<ShipMover>())
                {
                    Debug.LogError("No Shipmover component on:   " + gameObject.name, gameObject);
                    return null;
                }
                return myShipMover = GetComponent<ShipMover>();
            }
            set
            {
                myShipMover = value;
            }
        }

        Pinger myPinger;
        public Pinger MyPinger
        {
            get
            {
                if (myPinger != null) return myPinger;
                myPinger = GetComponent<Pinger>();
                if (myPinger == null) return null;
                return myPinger;                  
            }
            private set
            {
                myPinger = value;
            }
        }

            #endregion


        void OnDisable()
        {
            CombatMusic.RemoveCombatant(gameObject);
            RemoveFriend(GetComponent<SonarStats>());

            currentContextTargets.Clear();
            //CombatZone.Get().RemoveFromCombat(gameObject);
        }

        public AlignmentToPlayer getAlignment()
        {
            float opinion = OpinionOfPlayer();
            if (opinion > 0) return AlignmentToPlayer.Friendly;
            if (opinion < 0) return AlignmentToPlayer.Hostile;
            return AlignmentToPlayer.Neutral;
        }

        public float SafeDistance()
        {
            return 40;
        }
    }
    
    
    #region Data classes for range and damage
    [System.Flags]
    public enum TargetContext
    {
        Enemy = 1,
        Friend = 2,
        Interesting = 4,
        Scary = 8,
        UnInteresting = 16
    }

    [System.Flags]
    public enum ActionContext
    {
        Attacked = 1,
        Found = 2,
        Pinged = 4,
        Hailed = 8,
        SOS = 16,
        Docked = 32,
        UnDocked = 64,
        None = 128
    }

    [System.Serializable]
    public class ContextTarget : IComparable<ContextTarget>
    //TODO Support for group context
    {
        public SonarStats target;
        public List<SonarStats> friends = new List<SonarStats>();
        
        [Tooltip("Who am i Targeting?")]
        TargetContext targetContext = TargetContext.UnInteresting;
        
        [Tooltip("How did I find them?")]
        ActionContext actionContext = ActionContext.None;

        public bool sticky = false;
        public bool inverseDistanceRelation = false;
        bool changedContext = false;
        public float relationPriority = 0;
        public float distanceAndRelationPriority = 0;

        [Tooltip("Modified Emotions since birth")] 
        float instanceAttitude = 0;

        public float InstanceAttitude
        {
            get { return instanceAttitude; }
            private set { instanceAttitude = value; }
        }
        
        public int lastFoundDanger = 0;
        public ContextTarget() { }
        public ContextTarget(SonarStats ss)
        {
            target = ss;
        }

        public TargetContext MyTargetContext
        {
            get { return targetContext; }
            set
            {
                if(value!=targetContext)
                    changedContext = true;
                targetContext = value;
            }
        }

        public ActionContext MyActionContext
        {
            get { return actionContext; }
            set
            {
                if(value!=actionContext)
                    changedContext = true;
                actionContext = value;
            }
        }

        public bool Valid()
        {
            if (target == null) return false;
            return true;
        }
        /// <summary>
        /// Get the current priority distance (smaller values is higher priority)
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public float DistancePriority(float distance)
        {
            float clampedAttitude = Mathf.Clamp(relationPriority, 0.1f, 30);


            if (inverseDistanceRelation)
                distanceAndRelationPriority = clampedAttitude / Mathf.Clamp(distance, 1, 200);
            else
                distanceAndRelationPriority = distance / clampedAttitude; // TODO Add Inverse Distance For certain reactions

         //    Debug.Log("Doing the distance Priority "+ " attitude : " +relationPriority+ " / "  + distanceAndRelationPriority + "for this object at : " + distance  + " / " + clampedAttitude);

            return distanceAndRelationPriority;
        }


        public void UpdateContextTarget(float attitude)
        {
            if (Math.Abs(InstanceAttitude - attitude) < 0.1f) return;
            changedContext = true;
            InstanceAttitude = attitude;
        }
        
        #region friends

        public void GetFriends(List<ContextTarget> targets)
        {
            foreach (ContextTarget ct in targets)
            {
                if (Equals(ct)) continue;
                if (target.HasFactionSignatures(ct.target.signatures))
                { 
                    //Debug.Log(target.name + " has a friend! " + ct.target.name, target.gameObject);
                    AddFriend(ct.target);
                }
            }
        }
        
        
        public void AddFriend(SonarStats friend)
        {
            if (friend == null) return;
            if (friends.Contains(friend)) return;
            friends.Add(friend);
        }

        public void RemoveFriend(SonarStats friend)
        {
            if (friend == null||friends==null||friends.Count<1) return;
            if (!friends.Contains(friend)) return;
            friends.Remove(friend);
        }
        
        List<SonarStats> removeFriends = new List<SonarStats>();
        
        public int Danger()
        {
            if (target == null) return 0;
            lastFoundDanger= target.GetDanger();
            removeFriends.Clear();
            foreach (SonarStats ss in friends)
            {
                if (ss == null || !ss.gameObject.activeInHierarchy){removeFriends.Add(ss); continue;}
                int dan = ss.GetDanger();
                lastFoundDanger += dan;
                //Debug.Log("Adding Danger from my friend:"+ ss.name+": "+dan+"/" + lastFoundDanger);
            }
            
            foreach (var removeS in removeFriends)
                friends.Remove(removeS);
            
            //Debug.Log("Total Danger for: "+ target.name +" = "  + lastFoundDanger);
            return lastFoundDanger;
        }

#endregion
        public int CompareTo(ContextTarget ct1)
        {
            if (ct1 == null)
                return 1;
            if (targetContext == TargetContext.Enemy && ct1.targetContext != TargetContext.Enemy)
                return -1;
            if (targetContext != TargetContext.Enemy && ct1.targetContext == TargetContext.Enemy)
                return 1;
            return distanceAndRelationPriority.CompareTo(ct1.distanceAndRelationPriority);
        }
        

        public bool SameTarget(SonarStats ss)
        {
            return target == ss;
        }

        public void SetUpdated()
        {
            changedContext = false;
        }
        
        public bool Changed()
        {
            return changedContext;
        }
        public bool SameContext(ContextTarget ct)
        {
            return SameActionContext(ct) && SameTargetContext(ct);
        }
        
  
        bool SameTargetContext(ContextTarget ct)
        {
            if (ct == null) return true;
            return targetContext == ct.targetContext;    
        }

        bool SameActionContext(ContextTarget ct)
        {
            if (ct == null) return true;
            return actionContext == ct.actionContext;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (target != null)
                sb.Append(target.name);
            else
                sb.Append("No one ");

            sb.Append(" is " + targetContext.ToString() + " of attitude" + relationPriority);
            return sb.ToString();
        }
    }

    public enum RangeType
    {
        Long,
        Optimal,
        Short,
        None
    }

    /// <summary>
    /// Struct for holding damage/range relationships
    /// </summary>
    public struct DamageRange
    {
        public RangeType rangeType;
        public float range;
        public float damage;

        public DamageRange(float r, float d, RangeType rt = RangeType.None)
        {
            rangeType = rt;
            range = r;
            damage = d;
        }
    }

    public enum AIAction
    {
        Idle,
        Flee,
        Mission,
        Fight
    }
    #endregion

}