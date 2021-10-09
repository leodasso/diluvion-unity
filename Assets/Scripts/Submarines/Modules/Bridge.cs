using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Loot;
using Diluvion.Sonar;
using Sirenix.OdinInspector;
using DUI;
using Diluvion.AI;

/// <summary>
/// Defunct
/// </summary>

[Serializable]
public class ShipSave
{
    public string currentShipType;
    public int healthUpgrades;
    public int depthUpgrades;
    public float currentHealth;
    public string appliedEmblemItem;
    [HideInInspector]
    public float testDepth;             // not used
    [HideInInspector]
    public List<string> knownLandmarks;	// not used

    //Returns true if the ship save is valid, false otherwise
    public bool IsValid()
    {
        return !String.IsNullOrEmpty(currentShipType);
    }
}

namespace Diluvion.Ships
{

    public enum GunArrangement
    {
        Forward,
        Back,
        Top,
        Down,
        Horizontal,
        Vertical
    }
    
    /// <summary>
    /// Bridge is a core component for all ships. Interfaces with movement, weapons, modules, AI and more modules / components. A bridge
    /// should be placed on the root object of the ship.
    /// </summary>
    public class Bridge : Spawnable, IHUDable
    {
        [TabGroup("Main"), DisableInEditorMode]
        public SubChassis chassis;

        [TabGroup("Equipment"), ReadOnly]
        public List<ShipModule> shipModules = new List<ShipModule>();

        [TabGroup("Equipment"), ReadOnly]
        public List<ShipModule> disabledModules = new List<ShipModule>();

        [TabGroup("Equipment"), ReadOnly]
        public List<Forging> bonusChunks = new List<Forging>();

        List<BonusSlot> _bonusSlots = new List<BonusSlot>();

        [TabGroup("Equipment"), ReadOnly]
        public DItemDecal appliedEmblem;

        [TabGroup("Main"), DisableInEditorMode]
        public SonarStats currentTarget;

        [TabGroup("Main"), DisableInEditorMode]
        public SonarStats sonarSignature;

        [TabGroup("Setup")]
        public Hull hull;

        [TabGroup("Setup")]
        public List<Hull> allHulls = new List<Hull>();

        [TabGroup("Setup")]
        public CrewManager crewManager;

        [TabGroup("Setup")]
        public InteriorManager interiorManager;

        [TabGroup("Setup")]
        [SerializeField]
        Inventory shipInv;

        [TabGroup("Setup")] 
        public GunArrangement strongVector = GunArrangement.Forward;

        [TabGroup("Main"), ReadOnly]
        public Vector3 currentAim;

        [TabGroup("Main"), ReadOnly]
        public float maxRange = 300;
        
        
        [TabGroup("Main"), ReadOnly]
        public bool disableWeapons = false;

        [TabGroup("Setup")]
        public List<Renderer> emblemRenders = new List<Renderer>();
         
        Listener _sonarListener;

        int _currentCount = 0;
        bool _inCurrent = false;
        List<StationSlot> _stationSlots = new List<StationSlot>();
        List<DUIPanel> panelInstances = new List<DUIPanel>();
        Vector3 _lastCurrentDirection;

        public List<WeaponSystem> equippedWeaponSystems = new List<WeaponSystem>();
        List<WeaponSystem> _orderedWeaponSystems = new List<WeaponSystem>();


        void OnDrawGizmos()
        {
            if (IsPlayer())
                Gizmos.DrawLine(transform.position, currentAim);
        }


        void Awake()
        {
            GetCrewManager();           
        

            //Interior manager
            interiorManager = GetComponentInChildren<InteriorManager>();

            // Find slots
            if (interiorManager)
            {
                _stationSlots.AddRange(interiorManager.GetComponentsInChildren<StationSlot>());
                _bonusSlots.AddRange(interiorManager.GetComponentsInChildren<BonusSlot>());
            }

            //Find hull
            hull = GetComponent<Hull>();
            allHulls = GetAllHulls();

            if (appliedEmblem == null) ClearEmblem();

            sonarSignature = SpiderWeb.GO.MakeComponent<SonarStats>(gameObject);
        }

        
        public BonusSlot NextAvailableBonusSlot()
        {
            foreach (var slot in _bonusSlots)
            {
                if (slot.Available()) return slot;
            }
            return null;
        }
        
       

        /// <summary>
        /// Finds the next bonus slot that contains the given chunk
        /// </summary>
        public BonusSlot FindBonusSlotWithChunk(Forging chunk)
        {
            foreach (var slot in _bonusSlots)
            {
                if (slot.bonusChunk == chunk) return slot;
            }
            return null;
        }

        public void AddSonarListener(SonarModule module)
        {
            _sonarListener = SpiderWeb.GO.MakeComponent<Listener>(gameObject);
            _sonarListener.sonarModule = module;
        }

        void Update()
        {
            //if this is a player ship
            if (IsPlayer() && Camera.main)
            {
                Vector3 targetPos;
                //get the point at the center of the screen, 50 units out
                targetPos = Camera.main.ViewportToWorldPoint(new Vector3(.5f, .5f, 50));
                currentAim = targetPos;
            }
        }

        public WeaponSystem LastFiredLeadingWeaponSystem()
        {
            if (_orderedWeaponSystems.Count < 1)
            {
                _orderedWeaponSystems.AddRange(GetComponents<WeaponSystem>());
            }

            if (_orderedWeaponSystems.Count < 1) return null;

            return _orderedWeaponSystems[0];
        }
        
        

        /// <summary>
        /// Puts the weapon systems in order by recently fired.
        /// </summary>
        public void WeaponSystemFired(WeaponSystem ws)
        {
            if (!ws.module.showLeadGUI) return;
            _orderedWeaponSystems.Remove(ws);
            _orderedWeaponSystems.Insert(0, ws);
        }

        Pinger myPinger;
        public Pinger MyPinger
        {
            get
            {
                if (myPinger != null) return myPinger;

                myPinger = GetComponent<Pinger>();

                if (myPinger != null) return myPinger;

                return myPinger = gameObject.AddComponent<Pinger>();
            }
            private set
            {
                myPinger = value;
            }
        }

        /// <summary>
        /// is this the player's bridge?
        /// </summary>
        public bool IsPlayer()
        {
            return (PlayerManager.pBridge == this);
        }

        #region UI

        public void CreateUI()
        {
            // create the basic center point reticule
            DUIReticule reticule = UIManager.Create(UIManager.Get().basicReticule as DUIReticule);
            panelInstances.Add(reticule);
        }

        public void RemoveUI()
        {
            foreach (DUIPanel p in panelInstances) p.End();
            panelInstances.Clear();
        }

        /// <summary>
        /// Creates the UI panel for the given module, unless we already made the UI panel. It's okay to call this multiple times.
        /// </summary>
        public void AddUI(ShipModule module)
        {
            //Debug.Log("Attempting add UI for " + module.name);

            if (GetUIPanel(module) != null) return;
            if (module.UI == null) return;

            panelInstances.Add(module.CreateUI(this));

            //Debug.Log("UI created for " + module.name);
        }

        /// <summary>
        /// Removes the UI for the given ship module
        /// </summary>
        public void RemoveUI(ShipModule module)
        {
            DUIPanel p = GetUIPanel(module);
            if (p)
            {
                p.End();
                panelInstances.Remove(p);
            }
        }

        /// <summary>
        /// Returns the panel instance if this bridge already has one for the given module.
        /// </summary>
        public DUIPanel GetUIPanel(ShipModule m)
        {
            foreach (DUIPanel p in panelInstances)
            {
                if (!p) continue;
                if (p.prefab == m.UI) return p;
            }
            return null;
        }

        #endregion

        #region ship modules

        /// <summary>
        /// Returns true if the given module is equipped to this bridge.
        /// </summary>
        public bool HasModule(ShipModule module)
        {
            return shipModules.Contains(module);
        }


        /// <summary>
        /// Removes all ship modules from this bridge.
        /// </summary>
        public void RemoveAllModules()
        {
            foreach (ShipModule m in shipModules) m.RemoveFromShip(this);
            shipModules.Clear();
        }

        #endregion

        #region stations
        /// <summary>
        /// Returns the next available station slot.
        /// </summary>
        public StationSlot GetStationSlot()
        {
            foreach (StationSlot slot in StationSlots())
                if (slot.equippedStation == null && !slot.equipOnAwake) return slot;

            return null;
        }

        /// <summary>
        /// Returns a list of all station slots that can be used for populating. (i.e. aren't reserved)
        /// </summary>
        public List<StationSlot> StationSlots()
        {
            if (!Interior()) return null;
            _stationSlots.Clear();
            _stationSlots.AddRange(Interior().GetComponentsInChildren<StationSlot>());
            _stationSlots = _stationSlots.Where(x => !x.equipOnAwake).ToList();
            return _stationSlots;
        }
        #endregion

        #region currents
        public bool InCurrent()
        {
            if (GetComponent<IgnoreCurrent>()) return false;
            return _inCurrent;
        }

        public Vector3 OceanCurrentDirection()
        {
            if (GetComponent<IgnoreCurrent>()) return transform.forward;
            return _lastCurrentDirection;
        }

        public void EnteredCurrent(Vector3 globalDir)
        {
            if (GetComponent<IgnoreCurrent>()) return;
            _lastCurrentDirection = globalDir;
            _currentCount++;
            _inCurrent = true;
        }

        public void ExitedCurrent()
        {
            if (GetComponent<IgnoreCurrent>()) return;

            _currentCount--;

            if (_currentCount <= 0)
                StartCoroutine(ExitCurrent());
        }

        IEnumerator ExitCurrent()
        {
            yield return new WaitForSeconds(1);
            if (_currentCount > 0) yield break;
            _inCurrent = false;
            _lastCurrentDirection = transform.forward;
        }
        #endregion

        #region component getting

        SideViewerStats _sideViewStats;

        SideViewerStats SideViewStats()
        {
            if (_sideViewStats) return _sideViewStats;
            _sideViewStats = GetComponentInChildren<SideViewerStats>(true);
            return _sideViewStats;
        }
        
        public InteriorManager Interior()
        {
            if (!SideViewStats()) return null;
            return SideViewStats().intMan;
        }

        public CrewManager GetCrewManager()
        {
            if (crewManager) return crewManager;
            if (!Interior()) return null;
            crewManager = Interior().GetComponent<CrewManager>();
            return crewManager;
        }

        public Inventory GetInventory()
        {
            if (shipInv != null) return shipInv;          
            shipInv = GetComponent<Inventory>();
            return shipInv;
        }
        #endregion

        #region emblems

        /// <summary>
        /// Applies the given texture as an emblem on the ship
        /// </summary>
        public void ApplyEmblem(DItemDecal emblem)
        {
            appliedEmblem = emblem;

            if (emblem.emblemTexture == null) return;
            if (!Application.isPlaying) return;
                foreach (Renderer r in emblemRenders)
            {
             
                Material m = r.material;
                if (m == null) continue;
                m.mainTexture = emblem.emblemTexture;
                m.SetTexture("_EmissionMap", emblem.emblemTexture);
                m.color = Color.white;
            }

            //TODO sound, effects
        }

        public void ClearEmblem()
        {
            appliedEmblem = null;
            foreach (Renderer r in emblemRenders)
            {
                Material m = r.material;
                if (m == null) continue;
                m.color = Color.clear;
            }
        }

        #endregion

        #region hull
        //Safe Cached get function for the hull
        public Hull GetHull()
        {
            if (hull != null) return hull;
            return hull = GetComponent<Hull>();
        }

        //Safe Cached Get method for all the hulls in the ship
        public List<Hull> GetAllHulls()
        {
            if (allHulls == null) allHulls = new List<Hull>();
            if (allHulls.Count > 0) return allHulls;
            allHulls = new List<Hull>();

            allHulls.AddRange(GetComponentsInChildren<Hull>());

            foreach (Hull h in allHulls)
                h.myDeath += LostHull;

            return allHulls;
        }

        //SomethingBlewUp
        public void LostHull(Hull hullThatDied, string byWho)
        {
            allHulls.Remove(hullThatDied);
        }
        #endregion
    }
}