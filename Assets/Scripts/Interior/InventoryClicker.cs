using System.Collections.Generic;
using Diluvion.Roll;
using UnityEngine;
using Quests;
using SpiderWeb;
using Sirenix.OdinInspector;

namespace Diluvion
{
    /// <summary>
    /// Allows for player to select loot / inventory
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(ColliderMatchSprite))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class InventoryClicker : InteriorSwitch
    {
        [ToggleLeft]
        public bool playerStorage;

        [HideIf("playerStorage")]
        public Inventory inventory;
        
        static List<InventoryClicker> all = new List<InventoryClicker>();

        /// <summary>
        /// Calls for each instance of inventory clicker to refresh the sprite. This will reflect the state of the
        /// instance on this frame (hovered, empty, full).
        /// </summary>
        public static void RefreshSprites()
        {
            foreach (var instance in all) 
                instance.RefreshSprite();
        }

        void RefreshSprite()
        {

            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
            if (!spriteRenderer) return;
            if (!inventory)
            {
                if (spriteRenderer)
                    spriteRenderer.enabled = false;
                return;
            }

            spriteRenderer.enabled = true;

            if (!spriteSet) return;
            SetSprite(!inventory.IsEmpty() ? spriteSet.normal : spriteSet.empty);
        }

        void Awake()
        {
            all.Add(this);
        }

        void OnDestroy()
        {
            all.Remove(this);
        }

        protected override void Start()
        {
            base.Start();

            if (playerStorage) inventory = PlayerManager.PlayerStorage();
            else
            {
                //If there's no inventory manually set, check for an inventory component on this object.
                if (inventory == null)
                {
                    Inventory myInv = GetComponent<Inventory>();
                    if (myInv != null) inventory = myInv;
                }
            }
            
            PlayerStorageFailsafeSetup();

            if (inventory)
            {
                inventory.OnInitComplete += SetInitSprite;
            }

            if (spriteSet == null)
                spriteSet = Resources.Load("default treasure") as SpriteSet;
        }

        void SetInitSprite()
        {
            RefreshSprite();
        }

        void PlayerStorageFailsafeSetup()
        {
            if (!inventory) return;
            if (!inventory.invGenerator) return;
            if (inventory.invGenerator != InvGlobal.PlayerStorageInvGen()) return;
            
            Debug.Log("It looks like this inventory, " + gameObject.name + ", was meant to be player storage. Correcting now...", gameObject);
            playerStorage = true;
            inventory = PlayerManager.PlayerStorage();
        }

        
        public override void OnRelease ()
        {
            base.OnRelease();
            
            if (locked || !inventory)           return;
            if (inventory.invGenerator == null) return;

            // Show the inventory UI panel. If this is just a loot, the panel will be the smaller loot panel.
            inventory.CreateUI();
        }

        
        bool ShowsFullTrade()
        {
            if (!inventory) return false;
            if (!inventory.invGenerator) return false;

            InvGenerator gen = inventory.invGenerator;
            return gen.merchant || gen.playerShipInventory || gen.unlimitedSize;
        }

        public override void OnPointerExit ()
        {
            base.OnPointerExit();
            RefreshSprite();
        }
        
        /// <summary>
        /// Remove the UI (if up) when the inventory is de-focused
        /// </summary>
        public override void OnDefocus ()
        {
            base.OnDefocus();
            if (!inventory) return;
            inventory.RemoveUI();

            if (playerStorage) return;
            
            if (inventory.IsEmpty()) if (myCol) myCol.enabled = false;
        }
    }
}