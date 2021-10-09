using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using Diluvion;
using DUI;
using Sirenix.OdinInspector;

namespace Diluvion.Ships
{

    /// <summary>
    /// A module of the ship, such as cannons, sonar, EMP, torpedoes, etc. Applying this to the ship
    /// installs the station in the interior, adds any necessary components, and allows you to set 
    /// certain values (i.e. set stats) which will then get applied via all the ship modifiers attached to this
    /// module.
    /// </summary>
    [CreateAssetMenu(fileName = "new ship module", menuName = "Diluvion/subs/modules/generic")]
    public class ShipModule : ScriptableObject
    {
        public LocTerm moduleName;


        [ToggleLeft, Tooltip("Does this module install a station on the ship?")]
        public bool installsStation = true;
        
        /// <summary>
        /// the prefab interior station linked to this module
        /// </summary>
        [Tooltip("the prefab interior station linked to this module"), ShowIf("installsStation")]
        public Station stationPrefab;

        /// <summary>
        /// List of all the modifiers related to this module, i.e. speed, fire rate, accuracy, etc
        /// </summary>
        [Tooltip("List of all the modifiers related to this module, i.e. speed, fire rate, accuracy, etc")]
        public List<ShipModifier> shipMods = new List<ShipModifier>();

        /// <summary>
        /// Name of the input used to control this module
        /// </summary>
        [Tooltip("Name of the input used to control this module")]
        public string inputName;

        public float dangerValue = 10;

        [Tooltip("The UI panel for this module.")]
        public DUIPanel UI;

        static Player player;

        public string LocalizedName()
        {
            return moduleName.LocalizedText();
        }

        /// <summary>
        /// Does this module require an officer to enable?
        /// </summary>
        protected bool RequireOfficer()
        {
            if (!installsStation) return false;
            if (stationPrefab == null) return false;
            return stationPrefab.requireOfficer;
        }

        /// <summary>
        /// Called when this module is equipped to the given bridge.
        /// </summary>
        public virtual void AddToShip(Bridge bridge, ShipModuleSave data = null)
        {
           // Debug.Log("attempting to add: " + this.name + " with data:  " + data);
            if (bridge.shipModules.Contains(this)) return;

            bridge.shipModules.Add(this);

            if (bridge.IsPlayer())
            {
                InstallStation(bridge);
                if (RequireOfficer()) Disable(bridge);
                else Enable(bridge);
            }
            else
            {
               // Debug.Log("Installing " + this.name + "  as  not player");
                Enable(bridge);
            }
        }

        /// <summary>
        /// Called when this module is removed from the bridge
        /// </summary>
        public virtual void RemoveFromShip(Bridge bridge)
        {
            if (!bridge.shipModules.Contains(this)) return;

            Disable(bridge);
            bridge.shipModules.Remove(this);
            bridge.disabledModules.Remove(this);
            RemoveStation(bridge);
        }

        /// <summary>
        /// Get the rewired input 'player 1' i.e. our only player
        /// </summary>
        static Player Player()
        {
            if (player != null) return player;
            player = ReInput.players.GetPlayer(0);
            return player;
        }

        protected virtual void OnInputDown(Bridge b) { }
        protected virtual void OnInputUp(Bridge b) { }

        public virtual bool EnabledForBridge(Bridge b)
        {
            if (!b.HasModule(this)) return false;
            if (b.disabledModules.Contains(this)) return false;
            return true;
        }

        /// <summary>
        /// Instantiates and returns the UI panel for this module.
        /// </summary>
        public virtual DUIPanel CreateUI(Bridge b)
        {
            if (UI) return UIManager.Create(UI);
            return null;
        }

        /// <summary>
        /// Enables the module. This is called by OnEquip, but can also be called
        /// at any time after equip and will enable the module if it's disabled.
        /// </summary>
        public virtual bool Enable(Bridge bridge)
        {
            if (!bridge.HasModule(this)) return false;

           // Debug.Log(bridge.name + " enabling module " + name);

            bridge.disabledModules.Remove(this);

            if (bridge.IsPlayer()) bridge.AddUI(this);
            return true;
        }

        /// <summary>
        /// Disables the module. This is called by OnRemove, but can also be called at
        /// any time after equipped and will leave the module installed, but just disabled.
        /// <para>Useful for say, an EMP pulse disabling a module temporarily. If disable time is 0,
        /// won't turn back on.</para>
        /// </summary>
        public virtual bool Disable(Bridge bridge, float disabledTime = 0)
        {
            if (!bridge.HasModule(this)) return false;
            if (bridge.disabledModules.Contains(this)) return false;

            //Debug.Log(bridge.name + " Disabling module " + name);

            if (disabledTime > 0)
                bridge.StartCoroutine(DelayedEnable(disabledTime, bridge));

            bridge.RemoveUI(this);
            bridge.disabledModules.Add(this);
            return true;
        }

        IEnumerator DelayedEnable(float waitTime, Bridge bridge)
        {
            yield return new WaitForSeconds(waitTime);
            Enable(bridge);
        }

        /// <summary>
        /// Sets the stats to the given bridge for a particular station stat.
        /// </summary>
        public virtual void SetStats(Bridge bridge, List<CrewStatValue> crewStats)
        {
            foreach (ShipModifier mod in shipMods) mod.Modify(bridge, crewStats);
        }

        /// <summary>
        /// Sets the stats to the given bridge based on the given crewstats.
        /// </summary>
        public virtual void SetStats(Bridge bridge, ModifierValue modValue)
        {
            foreach (ShipModifier mod in shipMods)
                if (modValue.mod == mod) mod.Modify(bridge, modValue.value);
        }

        /// <summary>
        /// Checks for any player presses of my input. If so, calls the OnInputDown and OnInputUp functions.
        /// </summary>
        /// <param name="b"></param>
        public virtual void CheckPlayerInputs(Bridge b)
        {
            if (string.IsNullOrEmpty(inputName)) return;
          
            if (Player().GetButtonDown(inputName)) OnInputDown(b);
            if (Player().GetButtonUp(inputName)) OnInputUp(b);
        }

        protected virtual bool InstallStation(Bridge bridge)
        {
            if (!installsStation) return false;
            
            StationSlot slot = bridge.GetStationSlot();
            if (!slot)
            {
                Debug.LogError(bridge.name + " has no station slots remaining to install " + name + ".", bridge);
                return false;
            }

            slot.EquipStation(this);
            return true;
        }

        /// <summary>
        /// Removes the station related to this module from the given bridge.
        /// </summary>
        protected virtual void RemoveStation(Bridge bridge)
        {
            foreach (StationSlot slot in bridge.StationSlots())
            {
                if (!slot) continue;
                if (!slot.equippedStation) continue;
                if (slot.equippedStation.linkedModule == this)
                {
                    slot.RemoveStation();
                    return;
                }
            }
        }

        /// <summary>
        /// Returns a new save file for this module on the given bridge
        /// </summary>
        public virtual ShipModuleSave GetSave(Bridge b)
        {
            ShipModuleSave newSave = new ShipModuleSave();
            newSave.shipModuleName = name;
            return newSave;
        }
    }

    /// <summary>
    /// Base class for save file of any ship module
    /// </summary>
    [System.Serializable]
    public class ShipModuleSave
    {
        /// <summary>
        /// Name of the asset to load
        /// </summary>
        public string shipModuleName;

        public ShipModuleSave() { }

        public ShipModule LoadShipModule()
        {
            return ModulesGlobal.GetModule(shipModuleName);
        }
    }
}