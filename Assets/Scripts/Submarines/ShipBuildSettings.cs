using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.AI;
using Diluvion.Sonar;
using Sirenix.OdinInspector;
using Diluvion.Roll;
using Diluvion.Ships;
using Diluvion.SaveLoad;
using HeavyDutyInspector;
using Loot;
using SpiderWeb;

namespace Diluvion
{

    public class SpawnableSettings
    {    
        [BoxGroup("Instance", order: 201), ToggleLeft] public bool overrideName;
        [BoxGroup("Instance", order: 201), ShowIf("overrideName")] public LocTerm newName;
        
        [BoxGroup("Instance", order: 203)]
        public bool dockable = false;

        [BoxGroup("Instance", order: 203)]
        public bool cull = true;

        [BoxGroup("Instance", order: 203)]
        public bool ignoreCurrents = false;

        [BoxGroup("Instance", order: 203)]
        public Entry inventoryGenerator;

        [BoxGroup("Instance", order: 203), AssetsOnly]
        public List<GameObject> toSetAsChild = new List<GameObject>();

        [BoxGroup("Instance", order: 203), AssetsOnly]
        public List<Signature> newSignatures = new List<Signature>();

        [BoxGroup("Instance", order: 203), Tooltip("Overrides objects that spawn when crippled")]
        public List<Transform> crippledModels = new List<Transform>();

        [BoxGroup("Instance", order: 203), Tooltip("Overrides objects that spawn on complete destruction")]
        public List<Transform> destroyedObjects = new List<Transform>();

        public SpawnableSettings() { }
        public SpawnableSettings(SpawnableSettings other)
        {          
            crippledModels = new List<Transform>(other.crippledModels);
            destroyedObjects = new List<Transform>(other.destroyedObjects);
            ignoreCurrents = other.ignoreCurrents;
            inventoryGenerator = other.inventoryGenerator;
            overrideName = other.overrideName;
            newName = other.newName;
            dockable = other.dockable;
            toSetAsChild = other.toSetAsChild;
            newSignatures = other.newSignatures;
        }

        
        public InvGenerator MyinventoryGenerator()
        {
            InvGenerator iGenerator = inventoryGenerator as InvGenerator;

            if (iGenerator == null)
            {
                Table table = inventoryGenerator as Table;

                iGenerator = table.Roll<InvGenerator>() as InvGenerator;
            }
            return iGenerator;
        }
        
        public Transform ChoseCrippledModel()
        {
            if (crippledModels.Count < 1) return null;
            int selectedIndex = Random.Range(0, crippledModels.Count);
            return crippledModels[selectedIndex];
        }

        public Transform ChoseDestroyedModel()
        {
            if (destroyedObjects.Count < 1) return null;
            int selectedIndex = Random.Range(0, destroyedObjects.Count);
            return destroyedObjects[selectedIndex];
        }


        /// <summary>
        /// Inventory getter, dont store becuase this is on a scriptableOBject
        /// </summary>
        protected Inventory MyInventory(GameObject obj)
        {
            Inventory inv = obj.GetComponent<Inventory>();
            if (inv == null)
                inv = obj.AddComponent<Inventory>();
            return inv;
        }


        public virtual void ApplySettingsToInstance(GameObject obj, bool asPlayer = false)
        {
            Hull hull = obj.GetComponent<Hull>();
            if (hull != null)
            {
                hull.OverrideHP(SubChassisGlobal.Get().defaultHP);
                hull.testDepth = SubChassisGlobal.Get().defaultCrushDepth;
                
                if (crippledModels.Count > 0)
                    hull.crippledModel = ChoseCrippledModel();

                // Overriding the destroyed model
                if (destroyedObjects.Count > 0)
                    hull.explodedModel = ChoseDestroyedModel();
            }

            DockControl docks = obj.GetComponent<DockControl>();
            if (docks) docks.dockActive = dockable;

            if (!cull)
                if (obj.GetComponentInChildren<AICuller>())
                {
                    GameObject culler = obj.GetComponentInChildren<AICuller>().gameObject;
                    if (culler != null) culler.SetActive(false);
                }

            // Ignore currents
            if (ignoreCurrents && obj)
                foreach (Rigidbody rb in obj.GetComponentsInChildren<Rigidbody>())
                    rb.gameObject.AddComponent<IgnoreCurrent>();

            // Change sonar tags
            SonarStats sonarStats = obj.GetComponent<SonarStats>();
            if (sonarStats)
            {
                if (overrideName) sonarStats.displayName = newName;
                
                foreach (Signature s in newSignatures)
                    sonarStats.AddSignature(s);
            }

            if (toSetAsChild.Count>0)
                foreach (GameObject go in toSetAsChild)
                    GameObject.Instantiate(go, obj.transform.position, obj.transform.rotation, obj.transform);
        }     
    }

    [System.Serializable]
    public class ShipBuildSettings : SpawnableSettings
    {
        [BoxGroup("Ship", order: 200), ValidateInput("ValidateLoadout", "Loadout cannot be null."), AssetsOnly, Tooltip("The Ship's Loadout")]
        public SubLoadout loadout;

        [BoxGroup("Ship", order: 200),ValidateInput("ValidateCaptain", "Captain cannot be null."), AssetsOnly, 
         InlineEditor(),GUIColor(0f, 0.7f, 0.2f, 0.9f),  Tooltip("The AI behaviour.")]
        public Captain personality;

        [BoxGroup("Ship", order: 200), Tooltip("Will change the interior if not null.")]
        public InteriorManager interior; // Get the sideviewerstats off the instance and apply this interior to it (SideViewersStats.SwapInterior())

        [BoxGroup("Ship", order: 200), Tooltip("Will Add a emblem to the spawned ship.")]
        public Loot.DItemDecal shipEmblem;
           
        [BoxGroup("Ship", order: 200), AssetsOnly, Tooltip("Will Add factions to the spawned ship.")]
        public List<Faction> newFactions = new List<Faction>();
                   
        [BoxGroup("Ship", order: 200), Tooltip("Will Add signature relations to the spawned ship.")]
        public List<SignatureValues> newRelations = new List<SignatureValues>();
        
        [BoxGroup("Ship", order: 200), Tooltip("Will Add reactions to the spawned Captain.")]
        public List<Reaction> newReactions = new List<Reaction>();
        

        public ShipBuildSettings() { }
        public ShipBuildSettings (ShipBuildSettings sbs)
        {        
            crippledModels = new List<Transform>(sbs.crippledModels);
            destroyedObjects = new List<Transform>(sbs.destroyedObjects);
            ignoreCurrents = sbs.ignoreCurrents;
            inventoryGenerator = sbs.inventoryGenerator;
            overrideName = sbs.overrideName;
            newName = sbs.newName;
            dockable = sbs.dockable;
            toSetAsChild = sbs.toSetAsChild;
            newSignatures = sbs.newSignatures;
            loadout = sbs.loadout;
            personality = sbs.personality;
            interior = sbs.interior;
            shipEmblem = sbs.shipEmblem;
        }

        List<ShipModule> currentModules;
        List<Forging> _forgings = new List<Forging>();
        /// <summary>
        /// Loads from shipdata
        /// </summary>
        /// <param name="data"></param>
        /// <param name="b"></param>
        /// <param name="asPlayer"></param>
        public void LoadShipSettings(SubChassisData data,  bool asPlayer)
        {         
            _forgings = new List<Forging>();
            if (shipEmblem == null)
            {
                DItemDecal decalItem = ItemsGlobal.GetItem(data.decalName) as DItemDecal;
                shipEmblem = decalItem;
            }
            
            currentModules = new List<ShipModule>();
            //Debug.Log(data.appliedModules.Count + " dataModules");

            if(data.appliedModules!=null) // if the data has applied modules
                foreach (ShipModuleSave s in data.appliedModules) currentModules.Add(s.LoadShipModule());

            if (asPlayer)
            {
                if (DSave.current != null) DSave.current.SwapPlayerActiveSub(data);            

                inventoryGenerator = InvGlobal.PlayerShipInvGen();             
            }
            else // If we an AI
            {
                if (loadout) // overwrite the data if this SBS has a loadout
                    loadout.AddToChassis(data);
           
                if (inventoryGenerator == null)
                    inventoryGenerator = InvGlobal.GetInventory(data.invName); 
            }

            //override to catch missing inventory
            if (inventoryGenerator == null)
                inventoryGenerator = Resources.Load<InvGenerator>("default invgen");
          
            foreach (DItem i in ItemsGlobal.GetItems(data.appliedSlots)) 
            {
                Forging chunk = i as Forging;
                if (!chunk)
                {
                    Debug.LogError("item not casting to chunk."); continue;
                }
                _forgings.Add(chunk);
            }       
        }

        bool ValidateLoadout(SubLoadout l)
        {
            return l != null;
        }

        bool ValidateCaptain(Captain cap)
        {
            return cap != null;
        }
        public override void ApplySettingsToInstance(GameObject ship, bool asPlayer = false)
        {
            base.ApplySettingsToInstance(ship);

           // Debug.Log("applying ship Settings");
            if (personality != null&&!asPlayer)
            {
                
                AIMono myAI = ship.GetComponentInChildren<AIMono>();
                if (myAI == null) myAI = ship.AddComponent<AIMono>();

              //  Debug.Log("applying factions" + newFactions);
                myAI.MyCaptain = personality;
                myAI.ApplyFactions(personality.baseFactions);
                myAI.AddRelations(personality.nonFactionRelations);
                myAI.AddRelations(newRelations);
                myAI.AddReactions(personality.reactions);
                myAI.AddReactions(newReactions);
                if (newFactions != null && newFactions.Count > 0)
                {
//                    Debug.Log("add New factions " + newFactions.Count);
                    myAI.ApplyFactions(newFactions);
                }
            }

            Bridge shipBridge = ship.GetComponent<Bridge>();
            
            if (shipBridge)
            {
                foreach (Forging chunk in _forgings)
                {
                    chunk.ApplyToShip(shipBridge, shipBridge.chassis);
                }
                
                if (shipEmblem != null) shipBridge.ApplyEmblem(shipEmblem);
                else shipBridge.ClearEmblem();

                if (loadout == null)
                {
                    if(asPlayer)
                        loadout = Resources.Load("defaultPlayerLoadout") as SubLoadout;
                    else
                        loadout = Resources.Load("defaultNPCloadout")as SubLoadout;
                }    

                
                //loadout loading of weapons (Never for player)
                if (MyInventory(ship)&&!asPlayer)
                {
                    MyInventory(ship).Clear();
                   // Debug.Log("Loadout: " + loadout.name, loadout);
             
                    foreach (DItemWeapon diw in loadout.weapons)
                        MyInventory(ship).AddItem(diw);
                }

                //Catch for no modules, adds loadout modules
                if (currentModules == null || currentModules.Count < 1) currentModules = loadout.modules;

                MyinventoryGenerator().InitializeInventory(shipBridge.GetInventory());
                
               // Debug.Log("CurrentModules: " + currentModules, loadout);
                //Initializes the modules to the instance, weapons get equipped, crew gets sorted
                foreach (ShipModule m in currentModules)
                {
                    if (m == null) continue;
                    m.AddToShip(shipBridge);
                }
            }

            if (interior != null)
            {
                SideViewerStats sideView = ship.GetComponent<SideViewerStats>();
                if (sideView != null)
                    sideView.SwapInteriors(interior);
                
                //apply the name text and description to the interior
                sideView.Interior().myName = shipBridge.chassis.locName;
                sideView.Interior().description = shipBridge.chassis.locDetailedDescription;
            }
        }
    }
}
