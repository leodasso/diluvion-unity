using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Loot;
using System;
using Diluvion.Roll;
using Diluvion;
using Diluvion.Ships;
using Diluvion.Sonar;
using Diluvion.SaveLoad;
using Sirenix.OdinInspector;
using Diluvion.AI;
using SpiderWeb;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Diluvion.Ships
{
        
    
    
    /// <summary>
    /// 
    /// A sub chassis. All elements related to this particular chassis; Icon, skin, prefab, etc.
    /// <para>Includes function for instantiation: <see cref="InstantiateChassis"/></para>
    /// For saving data, uses <see cref="SubChassisData"/>
    /// <para><seealso cref="SubChassisGlobal"/></para>
    /// </summary>
    [CreateAssetMenu(fileName = "new sub chassis", menuName = "Diluvion/subs/Chassis", order = 0)]
    public class SubChassis : SpawnableEntry
    {
        //TODO AutoPopulation of SpawnableEntryValues
     
        [InlineEditor(InlineEditorModes.LargePreview), TabGroup("Basics", order: -399)]
        [AssetList(AssetNamePrefix = "ship_", Path = "/Prefabs/Ships/", CustomFilterMethod = "ValidShip")]
        public GameObject shipPrefab
        {
            get {
                return prefab;
            }
            set {
                prefab = value;
            }
        }
    
        [TabGroup("Basics")]
        [InlineEditor(InlineEditorModes.LargePreview)]
        public Sprite shipIcon;

        [TabGroup("Basics"), AssetsOnly] 
        public SkinHolder skin;

        [TabGroup("Basics"), Tooltip("Objects to place as a child of the ship")]
        public List<GameObject> additionalChildren = new List<GameObject>();

        [TabGroup("Basics")] public LayerMask avoidanceMask;

        [TabGroup("Basics")]
        public ShipCosts cost;

        [Tooltip("Modules that are required for this chassis"), AssetsOnly]
        [TabGroup("Basics")] public List<ShipModule> inherentModules;
    
        [TabGroup("Basics")]
        public bool DLC = false;

        [TabGroup("Details"), AssetsOnly]
        public LocTerm locName;
        [TabGroup("Details"), AssetsOnly]
        public LocTerm locDetailedDescription;

    
        [TabGroup("Details")]
        [OnValueChanged("ResetDanger")]
        [InfoBox("Ship prefab contains mounts without a linked weapon type!", InfoMessageType.Error, "MountsError")]
        public List<Mount> armaments = new List<Mount>();
    
        [Range(1, 5)]
        [TabGroup("Basics")]
        public int shipLevel = 1;
    
        [TabGroup("Details")]
        [OnValueChanged("ResetDanger")]
        [InfoBox("Actual number of station slots in prefab doesn't match intended value!", InfoMessageType.Error, "StationSlotsError")]
        public int stationSlots = 3;
    
        [TabGroup("Details")]
        [OnValueChanged("ResetDanger")]
        [InfoBox("Actual number of bonus slots in prefab doesn't match intended value!", InfoMessageType.Error, "BonusSlotsError")]
        public int bonusSlots = 4;
    
        [TabGroup("Details")]
        public Forging chassisBonus;
    
        [TabGroup("Details")]
        [OnValueChanged("ResetDanger")]
        [EnableIf("MultEnabled")]
        public float bonusMultiplier = 1.5f;
    
        [TabGroup("Details")]
        public int danger;
    
        [TabGroup("Details")]
        public ShipBuildSettings defaultBuild;      
        
        [TabGroup("Basics")]
        public bool hasUpgrade;
    
        [TabGroup("Basics"), ShowIf("hasUpgrade")]
        public SubChassis upgrade;
    
        [TabGroup("Basics")]
        [PropertyTooltip("Alternate names are used for converting old save files to new. If the ship prefab has a different name than the " +
            "chassis object, be sure to include the prefab name here.")]
        public List<string> altNames = new List<string>();

    
        #region inspector functions
        bool MultEnabled ()
        {
            return (chassisBonus != null);
        }
        
    
        bool ValidShip (GameObject obj)
        {
            if (obj.GetComponent<Bridge>()) return true;
            return false;
        }
    
    
    #if UNITY_EDITOR
    
        [ButtonGroup("test", order: 2)]
        void InstantiatePrefab ()
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(shipPrefab) as GameObject;
            instance.transform.position = SceneView.lastActiveSceneView.pivot;
            instance.transform.eulerAngles = new Vector3(0, -90, 0);
            instance.name = "ship_" + name;
            Skin s = GO.MakeComponent<Skin>(instance);
            s.ApplySkin(skin.normal);
            Selection.activeGameObject = instance;

            foreach (var GO in additionalChildren) Instantiate(GO, instance.transform);
        }
    
        [ButtonGroup("test", order: 2)]
        void InstantiateAsNPC ()
        {
            GameObject instance = InstantiateChassis(null,defaultBuild, false);
            instance.transform.position = SceneView.lastActiveSceneView.pivot;
            Selection.activeGameObject = instance;
            SceneView.lastActiveSceneView.FrameSelected();
        }
    
        [ButtonGroup("test", order: 2)]
        void InstantiateAsPlayer ()
        {
            TestAsPlayer();
            Selection.activeGameObject = PlayerManager.pBridge.gameObject;
        }
    
        bool StationSlotsError ()
        {
            if (!shipPrefab) return true;
            Bridge b = shipPrefab.GetComponentInChildren<Bridge>(true);
            if (!b) return true;
            var slotsList = b.StationSlots();
            if (slotsList == null) return true;
            return slotsList.Count != stationSlots;
        }
    
        bool BonusSlotsError ()
        {
            int bonusNum = shipPrefab.GetComponentsInChildren<BonusSlot>(true).Length;
            return (bonusNum != bonusSlots);
        }
    
        bool EngineError ()
        {
            ShipMover s = shipPrefab.GetComponentInChildren<ShipMover>(true);
            if (s) if (!s.engine) return true;
            return false;
        }
    
        bool MountsError ()
        {
            bool mountsReady = true;
            foreach (Mount m in shipPrefab.GetComponentsInChildren<Mount>(true))
            {
                if (m.weaponModule == null)
                {
                    mountsReady = false;
                    break;
                }
            }
            return !mountsReady;
        }
    
    #endif
        #endregion
    
    
        /// <summary>
        /// Only for creating a default sub, no parameters
        /// </summary> 
        public override GameObject Create (Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject sub = InstantiateChassis(null, defaultBuild, parent);
            sub.transform.position = position;
            sub.transform.rotation = rotation;
            return sub;
        }
    
        /// <summary>
        /// Instantiates the sub and places it using the same code path as during gameplay.
        /// </summary>
        public void TestAsPlayer ()
        {
            PlayerManager.InstantiatePlayerSub(new SubChassisData(this));
        }   
        
        
        
        /// <summary>
        /// Instantiates the ship with the given data.
        /// </summary>
        public GameObject InstantiateChassis (SubChassisData data, ShipBuildSettings sbs, bool asPlayer = false,  Transform parent = null)
        {
            //  Create and name an instance of the sub
            GameObject instance = Instantiate(Prefab(), parent);
            instance.name = "ship_" + name;

            foreach (var GO in additionalChildren) Instantiate(GO, instance.transform);
            
            SetupShip(instance, data, sbs, asPlayer);
            return instance;
        }
    
        public void SetupShip(GameObject instance, SubChassisData data, ShipBuildSettings sbs, bool asPlayer = false)
        {
            SubChassisGlobal global = SubChassisGlobal.Get();
            Bridge b = instance.GetComponent<Bridge>();
    
            if (b)
            {
                b.chassis = this;
                if (b.GetCrewManager())
                    b.GetCrewManager().maxCrew = global.defaultCrewSize;     
                
                Skin s = GO.MakeComponent<Skin>(instance);
                if (skin) s.ApplySkin(skin.normal);
            }
    
            if (sbs == null) sbs = new ShipBuildSettings(defaultBuild);
            if (data == null) data = new SubChassisData(this); 
                  
            ShipMover sm = instance.GetComponent<ShipMover>();
            ShipAnimator sa = instance.GetComponent<ShipAnimator>();
            sm.enabled = sa.enabled = false;
            
            Inventory inv = instance.GetComponent<Inventory>();
            SonarStats stats = instance.GetComponent<SonarStats>();
            stats.displayName = locName; 
    
            if (asPlayer)
            {
                stats.AddSignature(SonarGlobal.PlayerSignature());
                PlayerManager.pBridge = b;
            }
            else        
                if(instance.GetComponent<ShipControls>())
                    Destroy(instance.GetComponent<ShipControls>());
    
            // overwrite ship build settings with data
            sbs.LoadShipSettings(data, asPlayer);
    
            if (inv != null && sbs != null)
            {           
                inv.invGenerator = sbs.MyinventoryGenerator(); // This starts the inventory setup    
            }
            
            // Set the name and description of the interior
            SideViewerStats svs = instance.GetComponent<SideViewerStats>();
            if (svs)
            {
                if (svs.Interior())
                {
                    svs.Interior().myName = locName;
                    svs.Interior().description = locDetailedDescription;
                }
            }
    
            sbs.ApplySettingsToInstance(instance, asPlayer);

            // Add any modules INHERENT to ship. It's okay if they get added again later; addToShip checks for duplicates
            foreach (var m in inherentModules)
            {
                m.AddToShip(b);
            }
        }
    
        [ButtonGroup("weap")]
        public void RefreshMounts()
        {
            if (shipPrefab == null) return;
            foreach (var mountGroup in shipPrefab.GetComponentsInChildren<MountGroup>())
            {
                mountGroup.ApplyModule();
            }
        }
    
    
        [ButtonGroup("weap")]
        public void FindArmaments()
        {
            if (shipPrefab == null) return;
            armaments.Clear();
            armaments.AddRange(shipPrefab.GetComponentsInChildren<Mount>(true));
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
    
        void ResetDanger ()
        {
            danger = 0;
            Danger();
        }
    
        /// <summary>
        /// Danger inherent to this chassis based on mounts, stations, and bonuses
        /// </summary>
        public override int Danger ()
        {
            if (danger != 0) return danger;
            GameMode m = GameManager.Mode();
    
            danger = armaments.Count * m.dangerPerMount;
            danger += Mathf.RoundToInt((bonusSlots * m.dangerPerBonusSlot*bonusMultiplier));
            danger += stationSlots * m.dangerPerStationSlot;
    
            return danger;
        }
    
        /// <summary>
        /// Returns the ship prefab
        /// </summary>
        public override GameObject Prefab ()
        {
            return shipPrefab;
        }
    }
    
    /// <summary>
    /// Class for saving data of a <see cref="SubChassis"/>. Stores all relavent info for customized subs.
    /// <para><seealso cref="SubChassisGlobal"/></para>
    /// </summary>
    [System.Serializable]
    public class SubChassisData
    {
        public string chassisName;  // used for loading the chassis from the global list
        public string decalName;
        public List<string> appliedSlots = new List<string>();
        public List<ShipModuleSave> appliedModules = new List<ShipModuleSave>();
        public string invName;
        public float air;
    
        public SubChassisData ()
        {
            appliedModules = new List<ShipModuleSave>();
            appliedSlots = new List<string>();
        }
    
        public SubChassisData (SubChassis chassis)
        {
            appliedModules = new List<ShipModuleSave>();
            appliedSlots = new List<string>();
            chassisName = chassis.name;
        }

        public SubChassisData(SubChassisData original)
        {
            chassisName = original.chassisName;
            decalName = original.decalName;
            appliedSlots = new List<string>(original.appliedSlots);
            appliedModules = new List<ShipModuleSave>(original.appliedModules);
            invName = original.invName;
            air = original.air;
        }

        public float CrushDepth()
        {
            float depth = SubChassisGlobal.Get().defaultCrushDepth;

            foreach (DItem i in ItemsGlobal.GetItems(appliedSlots)) 
            {
                Forging chunk = i as Forging;
                if (!chunk)
                {
                    Debug.LogError("Upgrade listed as " + i.name + " is not actually a forging."); 
                    continue;
                }

                BonusDepth depthForge = chunk as BonusDepth;
                if (depthForge == null) continue;

                depth -= depthForge.extraDepth;
            }   
            
            return depth;
        }
    
        /// <summary>
        /// Upgrades THIS chassis data to its next upgrade. Returns false if upgrade was unsuccessful.
        /// </summary>
        public bool Upgrade ()
        {
            SubChassis chassis = ChassisObject();
            if (chassis == null) return false;
            if (chassis.upgrade == null || !chassis.hasUpgrade) return false;
            
            // TODO check if the upgrade has any inherent modules that need to be added (like torpedo)
    
            chassisName = chassis.upgrade.name;
            return true;
        }
    
        public SubChassis ChassisObject ()
        {
            return SubChassisGlobal.GetChassis(chassisName);
        }
    
        /// <summary>
        /// Instantiates the sub for this data, marks it as isPlayer
        /// </summary>
        public GameObject InstantiateSub ()
        {
            return ChassisObject().InstantiateChassis(this, ChassisObject().defaultBuild,true);
        }
    
        public override string ToString ()
        {
            string s = "Chassis name: " + chassisName;
            s += "\n emblem: " + decalName;
            s += "\n bonuses: ";
            foreach (string slot in appliedSlots) s += slot + ", ";
            s += "\n modules: ";
            foreach (ShipModuleSave module in appliedModules) s += module.shipModuleName + ", ";
            s += "\n inventory: " + invName;
            return s;
        }
    }
}