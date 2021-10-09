using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Sirenix.OdinInspector;
using Diluvion.Ships;
using Diluvion.AI;
using UnityEngine.UI.Extensions;

namespace Diluvion.Roll
{
    //TODO Yagni Add Rolling functionality for all the diffeernet parts of a ship inside the shipBuildSettings
    [CreateAssetMenu(fileName = "new shipEncounter", menuName = "Diluvion/RollTables/Ship Encounter")]
    public class ShipEncounter : RandomEncounter
    {
    
        [Tooltip("If this is not a CHASSIS or a TABLE WITH CHASSIS' in it, it will roll from the GLOBAL CHASSIS list"), OnValueChanged("Danger")]
        public Entry chassis;
      
        [Tooltip("If this is not a LOADOUT or a TABLE WITH LOADOUTS in it, it will roll from the GLOBAL LOADOUT list"), OnValueChanged("Danger")]
        public Entry loadOut;

        [Tooltip("If this is not a CAPTAIN or a TABLE WITH CAPTAINS in it, it will roll from the GLOBAL CAPTAINS list"), OnValueChanged("Danger")]
        public Entry captain;

        [SerializeField, Tooltip("Added Factions for this spawn")]
        private List<Faction> factions = new List<Faction>();
        
        [SerializeField, Tooltip("Added relations for this spawn")]
        private List<SignatureValues> relations = new List<SignatureValues>();

        [Space()] 
        [ToggleGroup("debug")]
        public bool debug = false;
        
        [SerializeField, ToggleGroup("debug")]
        internal SubChassis rolledChassis;
        
        [SerializeField, ToggleGroup("debug")]
        internal SubLoadout rolledLoadout;
        
        [SerializeField, ToggleGroup("debug")]
        internal Captain rolledCaptain;
        
        [SerializeField, ToggleGroup("debug")]
        private int minimumDanger = 0;
              
        [SerializeField, ToggleGroup("debug")]
        private int lastRolledDanger = 0;


        public override GameObject Prefab()
        {
            if (prefab != null) return prefab;
            return prefab = Resources.Load<GameObject>("randoSub");

        }

        void ResetForRoll()
        {
            rolledChassis = null;
            rolledCaptain = null;
            rolledLoadout = null;
        }
        //public AI captainAI;

#region queries
        //Should sum to 1
        private const float chassisCostPercentage = 0.5f;
        private const float captainCostPercentage = 0.3f;
        private const float loadoutCostPercentage = 0.2f;
        
        /// <summary>
        /// Ship Component Query
        /// </summary>
        /// <para>Does not use its own tags for searching its input lists</para>
        public bool CaptainRollQuery(Entry entry)
        {
            if (!entry.CanAfford(resourceCost )) //TODO YAGNI Distribute the danger cost across the chassis, loadout and captain, instead of keeping the same amount for all 3 rolls, needs a failsafe to get lowst danger option if can't afford
                return false;
            return entry.AllTagsTrue(this);
        }
        
        public bool ChassisRollQuery(Entry entry)
        {
            if (!entry.CanAfford(resourceCost))
                return false;
            return entry.AllTagsTrue(this);
        }
        
        public bool LoadoutRollQuery(Entry entry)
        {
            if (!entry.CanAfford(resourceCost))
                return false;
            return entry.AllTagsTrue(this);
        }
        
        
#endregion
        /// <summary>
        /// The shipencounter Process rolls the parts for the prospective ship, and modifies the resource struct accordingly
        /// </summary>
        public override SpawnableEntry Process(PopResources resources)
        {
            Debug.Log("Processing resources on " + this.name + " " +resourceCost+">"+ resources,this);
            resourceCost = new PopResources(0,resources.danger,resources.hazardDanger);
            Roll();
            return this;
        }

        //TODO Do process after Create to ensure the proper cost deduction
        /// <summary>
        /// Builds a ship with the chassis and loadout previously rolled in the Process()
        /// </summary>      
        public override GameObject Create(Vector3 position, Quaternion rotation, Transform parent = null)
        {
          
            SubChassis chassis = RolledChassis;
            ShipBuildSettings sbs = new ShipBuildSettings(chassis.defaultBuild)
            {
                personality = RolledCaptain,
                loadout = RolledLoadout,
            
            };
            sbs.newRelations.AddRange(relations);
            sbs.newFactions.AddRange(factions);
            
            GameObject go = chassis.InstantiateChassis(null, sbs);
            
            if (go.GetComponent<Spawnable>())
                go.GetComponent<Spawnable>().SetDanger(RolledDanger()); 
            
            go.transform.position = position;
            go.transform.rotation = rotation;
           // resourceCost.danger -= Danger();
            ResetForRoll();
            return go;
        }

 
        #region Variable Checks

        /// <summary>
        /// Checks the input variables for legal input
        /// </summary>
        [Button(), ToggleGroup("debug")]
        void Roll()
        {
            //ResetForRoll();
            rolledChassis = Chassis();
            rolledLoadout = LoadOut();
            rolledCaptain = Captain();
     
            Debug.Log("Danger for " + name + " is "  + RolledDanger());
        }

        /// <summary>
        /// Returns the total danger of the chassis, loadout, and AI
        /// </summary>

        /*public virtual float Width()
        {
            return RolledChassis.Width();
        }
        */

        ///Minimum Rolled Danger (For table purposes)
        public override int Danger()
        {
            int d = 0;

            if (chassis != null)
            {
                int chd = chassis.Danger();
                d += chd;
              //  Debug.Log(chassis.name + " has a chassis value of " + chd );
            }
            if (loadOut != null)
            {
                int ld = loadOut.Danger();
                d += ld;
             //   Debug.Log(loadOut.name + " has a danger value of  " + ld );
            }
            if (captain != null)
            {
                int cd =  captain.Danger();
                d += cd;
                //Debug.Log(captain.name + " has a dangervalue of:  " + cd );
            }
       
            return minimumDanger = d;
        }

        //Current Rolled Danger
        public int RolledDanger()
        {
            int d = 0;

            if (RolledChassis != null)
            {
                int chd = RolledChassis.Danger();
                d += chd;
                Debug.Log(RolledChassis.name + " has a chassis value of " + chd );
            }
            if (RolledLoadout != null)
            {
                int ld = RolledLoadout.Danger();
                d += ld;
                Debug.Log(RolledLoadout.name + " has a danger value of  " + ld );
            }
            if (RolledCaptain != null)
            {
                int cd =  RolledCaptain.Danger();
                d += cd;
                Debug.Log(RolledCaptain.name + " has a dangervalue of:  " + cd );
            }
            if(rolledChassis!=null&&rolledCaptain!=null&&rolledLoadout!=null)
                Debug.Log(" Rolled: <color=cyan>"  + RolledChassis.name + "</color> <color=yellow> " + RolledCaptain.name + "</color>  <color=red>" + RolledLoadout.name + "</color> with a total danger of : " + d);
            
            return lastRolledDanger = d;
        }


        private SubChassis RolledChassis
        {
            get
            {
                if (rolledChassis != null) return rolledChassis;
                return rolledChassis = Chassis();
            }
        }
      
        SubChassis Chassis()
        {
            SubChassis sub = null;
            //if the chassis slot is not null
            if (chassis != null)
            {
                sub = chassis as SubChassis;
            }
            else //If its empty, grab the All Ships table
                chassis = SubChassisGlobal.Get().Table();

            Table table = null;
            //If the entry was not a sub check to see if it was a table
            if (!sub)
                table = chassis as Table;
            else
            {            
                return sub;
            }

            //if the table contains subchassis
            if (table != null&&table.ContainsRollType(typeof(SubChassis))) { }        
            else // its not a chassis or a table            
                table = SubChassisGlobal.Get().Table();

            rolledChassis = table.Roll<SubChassis>(ChassisRollQuery) as SubChassis;

            if (rolledChassis == null) { Debug.LogError("Couldnt get a chassis for: " + name.ToString() + " with " + resourceCost, this); return null; }
            //Debug.Log(rolledChassis + " was rolled");
          
            return rolledChassis;
        }
        
        
        private SubLoadout RolledLoadout
        {
            get
            {
                if (rolledLoadout != null) return rolledLoadout;
                return rolledLoadout = LoadOut();
            }
        }
        
        SubLoadout LoadOut()
        {
            SubLoadout lo = null;
            //if the chassis slot is not null
            if (loadOut != null)            
                lo = loadOut as SubLoadout;            
            else //If its empty, grab the All Ships table
                loadOut = LoadoutsGlobal.Get().Table();

            Table table = null;
            //If the entry was not a sub check to see if it was a table
            if (!lo)
                table = loadOut as Table;
            else           
                return lo;
           
            //if the table contains subchassis
            if (table != null&&table.ContainsRollType(typeof(SubLoadout))) { }        
            else // its not a chassis or a table, replace with the default table            
                table = LoadoutsGlobal.Get().Table();   
            
            rolledLoadout = table.Roll<SubLoadout>(LoadoutRollQuery) as SubLoadout;
            
            if (rolledLoadout == null) { Debug.LogError("Couldnt get a loadout for: " + name.ToString() + " with " + resourceCost, this); return null; }
           // Debug.Log(rolledLoadout + " was rolled");
          
            return rolledLoadout;
        }
        

        private Captain RolledCaptain
        {
            get
            {
                if (rolledCaptain != null) return rolledCaptain;
                return rolledCaptain = Captain();
            }
        }
        
        Captain Captain()
        {
            Captain cap = null;
            //if the chassis slot is not null
            if (captain != null)
            {
                cap = captain as Captain;
            }
            else //If its empty, grab the All Captains table
            {
                captain = CaptainsGlobal.Get().Table();
            }

            Table table = null;
            //If the entry was not a sub check to see if it was a table
            if (!cap)
                table = captain as Table;
            else
            {            
                return cap;
            }

            //if the table contains subchassis
            if (table != null&&table.ContainsRollType(typeof(Captain))) { }        
            else // its not a chassis or a table            
                table = CaptainsGlobal.Get().Table();

            rolledCaptain = table.Roll<Captain>(CaptainRollQuery) as Captain;

            if (rolledCaptain == null) { Debug.LogError("Couldnt get a Captain for: " + name.ToString() + " with " + resourceCost, this); return null; }
           // Debug.Log(rolledCaptain + " was rolled");
           
            return rolledCaptain;
            
        }
        
        #endregion

    }
}
