using UnityEngine;
using System.Collections.Generic;
using DUI;
using Loot;
using NodeCanvas.BehaviourTrees;
using Sirenix.OdinInspector;

namespace Diluvion.Ships
{

    [CreateAssetMenu(fileName = "weapon module", menuName = "Diluvion/subs/modules/weapon")]
    public class WeaponModule : ShipModule
    {
        /// <summary>
        /// What are the weapons linked to this module? For example, if this module is 'torpedo station',
        /// then they would be all the torpedo tubes
        /// </summary>
        [Tooltip("What are the weapons linked to this module? For example, if this module is 'torpedo station', then they would be all the torpedo tubes")]
        public List<DItemWeapon> allowedWeapons = new List<DItemWeapon>();

        [AssetsOnly] public DItem ammo;

        public string weaponSwapAxisName;

        [Tooltip("Thickness of spherecast for manual aiming")]
        public float aimCastRadius = .5f;

        [Tooltip("layers to cast against for manual aiming and target acquiring.")]
        public LayerMask targetingLayers;

        [InlineEditor(InlineEditorModes.LargePreview, DrawGUI = false, DrawHeader = false)]
        public Sprite weaponIcon;

        [ToggleLeft]
        public bool showLeadGUI;

        [Tooltip("Firing Pattern")]
        public BehaviourTree attackPattern;

        /// <summary>
        /// Adds this weapon module to the ship, adds a weapon system component, and equips a default weapon.
        /// </summary>
        /// <param name="data">Save data. If the data has a selected weapon, will equip that.</param>
        public override void AddToShip(Bridge bridge, ShipModuleSave data = null)
        {
            if (bridge.shipModules.Contains(this)) return;
            
            // if this is the player ship, add resource display
            if (IsPlayerBridge(bridge) && ammo)
                DUIResources.DisplayItemHUD(ammo);
            
            //Debug.Log("Adding to Ship: " + bridge.name, bridge.gameObject);
            bool hasMount = false;
            foreach (Mount m in bridge.transform.GetComponentsInChildren<Mount>())
                if (m.weaponModule == this)
                    hasMount = true;

            //if the ship does not have mounts setup for this type of weapon, dont set it up
            if (!hasMount) return;

            // Add a weapon system component to the bridge that's installing this module
            WeaponSystem newSystem = bridge.gameObject.AddComponent<WeaponSystem>();
            newSystem.module = this;
            newSystem.enabled = false;

            // equip a weapon
            AttemptWeaponEquip(bridge);
            
            // load the selected weapon from save data
            WeaponModuleSave weaponData = data as WeaponModuleSave;
            if (weaponData != null)
            {
                // Get the weapon object
                DItemWeapon weapon = ItemsGlobal.GetItem(weaponData.selectedWeapon) as DItemWeapon;

                // Equip the weapon
                if (weapon) EquipWeapon(weapon, bridge);
            }
            
            base.AddToShip(bridge, data);
        }

        /// <summary>
        /// Removes one ammo from the player's inventory
        /// </summary>
        protected void RemovePlayerAmmo()
        {
            if (!PlayerManager.PlayerInventory()) return;
            PlayerManager.PlayerInventory().RemoveItem(ammo);
        }

        protected bool IsPlayerBridge(Bridge b)
        {
            return b == PlayerManager.pBridge;
        }

        /// <summary>
        /// Returns a list of all weapon items in the given bridge's inventory that work for this module.
        /// </summary>
        public List<DItemWeapon> WeaponsInInventory(Bridge b)
        {
            return b.GetInventory().GetItemsOfType(allowedWeapons);
        }

        /// <summary>
        /// Searches the given bridge's inventory for a weapon that can be equipped to this module. Equips the first found.
        /// </summary>
        public virtual void AttemptWeaponEquip(Bridge bridge)
        {
            // Attempt to equip a weapon
            foreach (DItemWeapon w in WeaponsInInventory(bridge))
                if (EquipWeapon(w, bridge)) break;
        }

        public override DUIPanel CreateUI(Bridge b)
        {
            DUIPanel UI = base.CreateUI(b);

            WeaponHUD HUD = UI as WeaponHUD;

            if (HUD == null)
            {
                Debug.LogError("UI for " + name + " must be a type of WeaponHUD.", this);
                return null;
            }

            HUD.bridge = b;
            foreach (WeaponSystem ws in b.GetComponents<WeaponSystem>())
            {
                if (ws.module == this)
                {
                    HUD.weaponSystem = ws;
                    break;
                }
            }
            
            HUD.Init(RelatedWeaponSystem(b));

            return HUD;
        }


        public override void RemoveFromShip(Bridge bridge)
        {
            base.RemoveFromShip(bridge);
            WeaponSystem sys = RelatedWeaponSystem(bridge);
            if (sys != null) Destroy(sys);
        }

        public override bool Enable(Bridge bridge)
        {
            if (!base.Enable(bridge)) return false;
            SetModuleEnabled(true, bridge);
            EnableWeaponSystem(bridge, true);
            return true;
        }

        public override bool Disable(Bridge bridge, float disabledTime = 0)
        {
            if (!base.Disable(bridge, disabledTime)) return false;
            SetModuleEnabled(false, bridge);
            EnableWeaponSystem(bridge, false);
            return true;
        }

        /// <summary>
        /// Finds the weapon system component on the given bridge linked to this particular module.
        /// Since there may be multiple weapon systems, it checks for reference to this.
        /// </summary>
        protected WeaponSystem RelatedWeaponSystem(Bridge bridge)
        {
            foreach (WeaponSystem ws in bridge.GetComponents<WeaponSystem>())
                if (ws.module == this) return ws;

            return null;
        }

        void EnableWeaponSystem(Bridge b, bool enabled)
        {
            WeaponSystem ws = RelatedWeaponSystem(b);
            if (!ws) return;
            ws.enabled = enabled;
        }

        /// <summary>
        /// Sets the linked weapon system component on the given bridge as enabled or disabled.
        /// </summary>
        void SetModuleEnabled(bool enabled, Bridge bridge)
        {
            WeaponSystem system = RelatedWeaponSystem(bridge);
            if (system != null) system.enabled = enabled;
        }

        protected override void OnInputDown(Bridge b)
        {
            if (b.disableWeapons) return;
            if (!EnabledForBridge(b)) return;
            base.OnInputDown(b);
            
            // If the weapon system has nothing equipped, check the inventory to see if there's anything that
            // can be equipped.
            if (RelatedWeaponSystem(b).equippedWeapon == null)
            {
                AttemptWeaponEquip(b);
            }
            
            //ShipControls s = b.GetComponent<ShipControls>();
            //if (s) s.PlayerWeaponRequest();

            FireOn(RelatedWeaponSystem(b));
        }

        protected override void OnInputUp(Bridge b)
        {
            if (b.disableWeapons) return;
            if (!EnabledForBridge(b)) return;
            base.OnInputUp(b);
            FireOff(RelatedWeaponSystem(b));
        }

        /// <summary>
        /// Fires the weapon attached to mount m.
        /// </summary>
        public virtual GameObject FireWeapon(Mount m, WeaponSystem ws)
        {
            // Check if it's the player firing
            bool isPlayer = ws.GetBridge().IsPlayer();

            if (isPlayer)
            {
                // Make ship semitransparent
                ShipInvisibler.SetInvisibleIfObstructed(3);
            }
            return m.FireWeapon();
        }
        

        /// <summary>
        /// Attempts to spend an ammunition for given weapon system. If no ammunition are left, returns false.
        /// </summary>
        public bool TrySpendAmmo(WeaponSystem ws)
        {
            if (!HasAmmo(ws))
            {
                ws.FeedbackNoAmmo();
                return false;
            }
                
            // otherwise, subtract an ammo.
            RemovePlayerAmmo();
            return true;
        }

        #region mounts & weapon equipping

        /// <summary>
        /// Equips the given weapon to the given bridge.
        /// </summary>
        /// <param name="weapon">This weapon must be one of my allowed weapons</param>
        /// <returns>Whether the equip was successful or not.</returns>
        public virtual bool EquipWeapon(DItemWeapon weapon, Bridge b)
        {
            if (!allowedWeapons.Contains(weapon))
            {
                Debug.LogWarning("Tried to equip weapon " + weapon.name + " on bridge " +
                    b.name + " but the weapon isn't allowed by this module.", this);
                return false;
            }

            WeaponSystem ws = RelatedWeaponSystem(b);
            if (!ws)
            {
                Debug.LogWarning("Tried to equip weapon " + weapon.name + " on bridge " +
                    b.name + " but the bridge doesn't have a weapon system for this module.", this);
                return false;
            }

            ws.EquipWeapon(weapon);
            return true;
        }


        /// <summary>
        /// Is the given mount okay to fire with the given weapon system?
        /// </summary>
        public virtual bool ValidMount(Mount m)
        {
            return true;
        }


        /// <summary>
        /// Does the inventory attached to the given weapon system have the correct type of ammo?
        /// </summary>
        protected bool HasAmmo(WeaponSystem ws)
        {
            return ws.GetBridge().GetInventory().HasItem(ammo);
        }

        #endregion

        public virtual void UpdateSystem(WeaponSystem ws)
        {
            // Cool down the system
            ws.Reloading();
        }

        /// <summary>
        /// Tells the given weapon system to begin firing behavior
        /// </summary>
        public virtual void FireOn(WeaponSystem ws) { ws.FireOn(); }

        /// <summary>
        /// Tells the given weapon system to stop firing behavior
        /// </summary>
        public virtual void FireOff(WeaponSystem ws) { ws.FireOff(); }



        public override ShipModuleSave GetSave(Bridge b)
        {
            WeaponModuleSave newSave = new WeaponModuleSave();
            newSave.shipModuleName = name;

            // Get the weapon system related to this module from the bridge
            WeaponSystem ws = null;
            if (b) ws = RelatedWeaponSystem(b);

            // Save the currently equipped weapon
            if (ws) if (ws.equippedWeapon)
                    newSave.selectedWeapon = ws.equippedWeapon.name;

            return newSave;
        }
    }

    [System.Serializable]
    public class WeaponModuleSave : ShipModuleSave
    {
        public string selectedWeapon;
        public WeaponModuleSave() { }
    }
}